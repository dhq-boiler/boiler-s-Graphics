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
        public void BasicTest()
        {
            IOperationController controller = new OperationController();
            var person = new Person()
            {
                Name = "Venus",
            };

            controller.Execute(person.GenerateSetPropertyOperation(x=>x.Name , "Yamada"));
            Assert.AreEqual("Yamada",person.Name);

            controller.Execute(person.GenerateSetPropertyOperation(x=>x.Name , "Tanaka"));
            Assert.AreEqual("Tanaka",person.Name);
            
            controller.Undo();
            Assert.AreEqual("Yamada",person.Name);

            controller.Undo();
            Assert.AreEqual("Venus",person.Name);
        }
        
        /// <summary>
        /// Operationの自動結合テスト
        /// </summary>
        [Test]
        public async Task MergedTest()
        {
            IOperationController controller = new OperationController();

            var person = new Person()
            {
                Age = 14,
            };

            // デフォルトのマージ時間を 70msに設定
            Operation.DefaultMergeSpan = TimeSpan.FromMilliseconds(70);

            //Age = 30
            controller.ExecuteSetProperty(person,nameof(Person.Age),30);
            Assert.AreEqual(30, person.Age );
            
            //10 ms待つ
            await Task.Delay(10);
            
            //Age = 100
            controller.ExecuteSetProperty(person,nameof(Person.Age),100);
            Assert.AreEqual(100, person.Age );

            //100ms 待つ
            await Task.Delay(75);
            
            //Age = 150
            controller.ExecuteSetProperty(person,nameof(Person.Age),150);
            Assert.AreEqual(150, person.Age );
            
            //Age = 100
            controller.Undo();
            Assert.AreEqual(100, person.Age );

            // マージされているので 30には戻らずそのまま14に戻る
            // Age = 14
            controller.Undo();
            Assert.AreEqual(14, person.Age );
        }

        /// <summary>
        /// リスト操作のテスト
        /// </summary>
        [Test]
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
            
            Assert.AreEqual(2 , person.Children.Count);
            
            controller.ExecuteRemoveAt(person.Children,0);
            Assert.That(person.Children.Count, Is.EqualTo(1));
            
            controller.Undo();
            Assert.AreEqual(2 , person.Children.Count);
            
            controller.Undo();
            Assert.That(person.Children.Count, Is.EqualTo(1));

            controller.Undo();
            Assert.IsEmpty(person.Children);
        }

        /// <summary>
        /// PropertyChangedを自動的にOperation化するテスト
        /// </summary>
        [Test]
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
            
                Assert.True(controller.CanUndo);
            
                controller.Undo();
                Assert.AreEqual("Yammada",person.Name);

                controller.Undo();
                Assert.AreEqual("First",person.Name);                
            }

            // Dispose後は変更通知が自動的にOperationに変更されないことを確認
            {
                nameChangedWatcher.Dispose();
                person.Name = "Tanaka";
                Assert.False(controller.CanUndo);

                controller.Undo();
                Assert.AreEqual("Tanaka",person.Name);
            }

            // Ageは自動マージ有効なため1回のUndoで初期値に戻ることを確認
            {
                for (int i = 1; i < 30; ++i)
                {
                    person.Age = i;
                }
                
                Assert.AreEqual(29,person.Age);
                
                controller.Undo();
                Assert.AreEqual(0,person.Age);
                
                ageChangedWatcher.Dispose();
            }
        }
        
        
        [Test]
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
            recorder.EndRecode("Fixed");
            
            // 1回のUndoでレコード前のデータが復元される
            controller.Undo();
            Assert.AreEqual("Default",person.Name);
            Assert.AreEqual(5,person.Age);
            Assert.IsEmpty(person.Children);
            
            // Redoでレコード終了後のデータが復元される
            controller.Redo();
            Assert.AreEqual("Changed",person.Name);
            Assert.AreEqual(14,person.Age);
            Assert.That(person.Children.Count, Is.EqualTo(1));
        }

        [Test]
        public void DisposeTest()
        {
            IOperationController controller = new OperationController();
            var person = new Person()
            {
                Name = "Venus",
            };

            controller.Execute(person.GenerateSetPropertyOperation(x => x.Name, "Yamada"));
            Assert.AreEqual("Yamada", person.Name);

            controller.Execute(person.GenerateSetPropertyOperation(x => x.Name, "Tanaka"));
            Assert.AreEqual("Tanaka", person.Name);

            controller.ExecuteDispose(person, () => person.Restore(() => person.Name = "Tanaka"));
            Assert.That(person.Name, Is.Null);

            controller.Undo();
            Assert.AreEqual("Tanaka", person.Name);

            controller.Undo();
            Assert.AreEqual("Yamada", person.Name);

            controller.Undo();
            Assert.AreEqual("Venus", person.Name);
        }

        [Test]
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
            Assert.AreEqual("Value2", person.RP.Value);

            //75ms 待つ
            await Task.Delay(75);

            controller.ExecuteSetProperty(person, "RP.Value", "Value3");
            Assert.AreEqual("Value3", person.RP.Value);

            controller.Undo();
            Assert.AreEqual("Value2", person.RP.Value);

            controller.Undo();
            Assert.AreEqual("Value1", person.RP.Value);

            Assert.That(controller.CanUndo, Is.False);
        }

        [Test]
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
                Assert.AreEqual("Value2", person.RP.Value);

                controller.Undo();
                Assert.AreEqual("Value1", person.RP.Value);
            }

            Assert.That(controller.CanUndo, Is.False);
        }
    }
}