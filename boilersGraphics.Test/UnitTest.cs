using NUnit.Framework;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TsOperationHistory.Extensions;

namespace TsOperationHistory.Test
{
    internal class Person : Bindable, IDisposable, IRestore
    {
        private string _name;

        public Person()
        {
        }

        public static string StaticValue { get; set; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _age;

        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }

        public ReactivePropertySlim<string> RP { get; set; } = new ReactivePropertySlim<string>();

        private ObservableCollection<Person> _children = new ObservableCollection<Person>();
        private bool disposedValue;

        public ObservableCollection<Person> Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RP.Dispose();
                }

                _name = null;
                _children = null;
                RP = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Restore(Action restorePropertiesAction)
        {
            if (!disposedValue) return;

            disposedValue = false;
            _name = string.Empty;
            _age = 0;
            _children = new ObservableCollection<Person>();
            RP = new ReactivePropertySlim<string>();

            restorePropertiesAction.Invoke();
            GC.ReRegisterForFinalize(this);
        }
    }

    [TestFixture]
    public class UnitTest
    {
        /// <summary>
        /// 基本的なUndoRedoのテスト
        /// </summary>
        [Test]
        [Retry(3)]
        public void BasicTest()
        {
            IOperationController controller = new OperationController();
            var person = new Person()
            {
                Name = "Venus",
            };

            controller.Execute(person.GenerateSetPropertyOperation(x => x.Name, "Yamada"));
            Assert.That(person.Name, Is.EqualTo("Yamada"));

            controller.Execute(person.GenerateSetPropertyOperation(x => x.Name, "Tanaka"));
            Assert.That(person.Name, Is.EqualTo("Tanaka"));

            controller.Undo();
            Assert.That(person.Name, Is.EqualTo("Yamada"));

            controller.Undo();
            Assert.That(person.Name, Is.EqualTo("Venus"));
        }

        /// <summary>
        /// スタティックプロパティのUndoRedoのテスト
        /// </summary>
        [Test]
        [Retry(3)]
        public async Task StaticPropertyTest()
        {
            IOperationController controller = new OperationController();

            // デフォルトのマージ時間を 70msに設定
            Operation.DefaultMergeSpan = TimeSpan.FromMilliseconds(70);

            Person.StaticValue = "Geso";

            controller.ExecuteSetStaticProperty(typeof(Person), "StaticValue", "ika");
            Assert.That(Person.StaticValue, Is.EqualTo("ika"));

            await Task.Delay(75);

            controller.ExecuteSetStaticProperty(typeof(Person), "StaticValue", "tako");
            Assert.That(Person.StaticValue, Is.EqualTo("tako"));

            await Task.Delay(75);

            controller.Undo();
            Assert.That(Person.StaticValue, Is.EqualTo("ika"));

            controller.Undo();
            Assert.That(Person.StaticValue, Is.EqualTo("Geso"));
        }

        /// <summary>
        /// リスト操作のテスト
        /// </summary>
        [Test]
        [Retry(3)]
        public void ListTest()
        {
            IOperationController controller = new OperationController();

            var person = new Person()
            {
               Name = "Root"
            };
            
            controller.ExecuteAdd(person.Children , 
                new Person()
                {
                    Name = "Child1"
                });
            
            controller.ExecuteAdd(person.Children , 
                new Person()
                {
                    Name = "Child2"
                });
            
            Assert.That(person.Children.Count, Is.EqualTo(2));
            
            controller.ExecuteRemoveAt(person.Children,0);
            Assert.That(person.Children.Count, Is.EqualTo(1));
            
            controller.Undo();
            Assert.That(person.Children.Count, Is.EqualTo(2));
            
            controller.Undo();
            Assert.That(person.Children.Count, Is.EqualTo(1));

            controller.Undo();
            Assert.That(person.Children, Is.Empty);
        }

        /// <summary>
        /// PropertyChangedを自動的にOperation化するテスト
        /// </summary>
        [Test]
        [Retry(3)]
        public void ObservePropertyChangedTest()
        {
            IOperationController controller = new OperationController();
            
            var person = new Person()
            {
                Name = "First",
                Age = 0,
            };

            var nameChangedWatcher = controller.BindPropertyChanged<string>(person, nameof(Person.Name),false);
            var ageChangedWatcher = controller.BindPropertyChanged<int>(person, nameof(Person.Age));

            // 変更通知から自動的に Undo / Redo が可能なOperationをスタックに積む
            {
                person.Name = "Yammada";
                person.Name = "Tanaka";
            
                Assert.That(controller.CanUndo, Is.True);
            
                controller.Undo();
                Assert.That(person.Name, Is.EqualTo("Yammada"));

                controller.Undo();
                Assert.That(person.Name, Is.EqualTo("First"));                
            }

            // Dispose後は変更通知が自動的にOperationに変更されないことを確認
            {
                nameChangedWatcher.Dispose();
                person.Name = "Tanaka";
                Assert.That(controller.CanUndo, Is.False);

                controller.Undo();
                Assert.That(person.Name, Is.EqualTo("Tanaka"));
            }

            // Ageは自動マージ有効なため1回のUndoで初期値に戻ることを確認
            {
                for (int i = 1; i < 30; ++i)
                {
                    person.Age = i;
                }
                
                Assert.That(person.Age, Is.EqualTo(29));
                
                controller.Undo();
                Assert.That(person.Age, Is.EqualTo(0));
                
                ageChangedWatcher.Dispose();
            }
        }
        
        
        [Test]
        [Retry(3)]
        public void RecorderTest()
        {
            IOperationController controller = new OperationController();
            
            var person = new Person()
            {
                Name = "Default",
                Age = 5,
            };
            
            var recorder = new OperationRecorder(controller);
            
            // 操作の記録開始
            recorder.BeginRecode();
            {
                recorder.Current.ExecuteAdd(person.Children,new Person()
                {
                    Name = "Child1",
                });
            
                recorder.Current.ExecuteSetProperty(person , nameof(Person.Age) , 14);
            
                recorder.Current.ExecuteSetProperty(person , nameof(Person.Name) , "Changed");
            }
            // 操作の記録完了
            recorder.EndRecode();
            
            // 1回のUndoでレコード前のデータが復元される
            controller.Undo();
            Assert.That(person.Name, Is.EqualTo("Default"));
            Assert.That(person.Age, Is.EqualTo(5));
            Assert.That(person.Children, Is.Empty);
            
            // Redoでレコード終了後のデータが復元される
            controller.Redo();
            Assert.That(person.Name, Is.EqualTo("Changed"));
            Assert.That(person.Age, Is.EqualTo(14));
            Assert.That(person.Children.Count, Is.EqualTo(1));
        }

        [Test]
        [Retry(3)]
        public void DisposeTest()
        {
            IOperationController controller = new OperationController();
            var person = new Person()
            {
                Name = "Venus",
            };

            controller.Execute(person.GenerateSetPropertyOperation(x => x.Name, "Yamada"));
            Assert.That(person.Name, Is.EqualTo("Yamada"));

            controller.Execute(person.GenerateSetPropertyOperation(x => x.Name, "Tanaka"));
            Assert.That(person.Name, Is.EqualTo("Tanaka"));

            controller.ExecuteDispose(person, () => person.Restore(() => person.Name = "Tanaka"));
            Assert.That(person.Name, Is.Null);

            controller.Undo();
            Assert.That(person.Name, Is.EqualTo("Tanaka"));

            controller.Undo();
            Assert.That(person.Name, Is.EqualTo("Yamada"));

            controller.Undo();
            Assert.That(person.Name, Is.EqualTo("Venus"));
        }

        [Test]
        [Retry(3)]
        public async Task MultiLayeredPropertyTest()
        {
            IOperationController controller = new OperationController();
            var person = new Person();
            person.RP.Value = "Value1";

            // デフォルトのマージ時間を 70msに設定
            Operation.DefaultMergeSpan = TimeSpan.FromMilliseconds(70);

            //75ms 待つ
            await Task.Delay(75);

            controller.ExecuteSetProperty(person, "RP.Value", "Value2");
            Assert.That(person.RP.Value, Is.EqualTo("Value2"));

            //75ms 待つ
            await Task.Delay(75);

            controller.ExecuteSetProperty(person, "RP.Value", "Value3");
            Assert.That(person.RP.Value, Is.EqualTo("Value3"));

            controller.Undo();
            Assert.That(person.RP.Value, Is.EqualTo("Value2"));

            controller.Undo();
            Assert.That(person.RP.Value, Is.EqualTo("Value1"));

            Assert.That(controller.CanUndo, Is.False);
        }

        [Test]
        [Retry(3)]
        public void MultiLayeredPropertyTest2()
        {
            IOperationController controller = new OperationController();
            var person = new Person();
            person.RP.Value = "Value1";

            using (var watcher = controller.BindPropertyChanged<string>(person, "RP.Value", false))
            {
                person.RP.Value = "Value2";

                person.RP.Value = "Value3";

                controller.Undo();
                Assert.That(person.RP.Value, Is.EqualTo("Value2"));

                controller.Undo();
                Assert.That(person.RP.Value, Is.EqualTo("Value1"));
            }

            Assert.That(controller.CanUndo, Is.False);
        }
    }
}