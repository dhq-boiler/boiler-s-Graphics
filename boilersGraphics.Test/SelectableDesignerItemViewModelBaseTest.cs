using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class SelectableDesignerItemViewModelBaseTest
    {
        private static NRectangleViewModel NewRect()
        {
            App.IsTest = true;
            return new NRectangleViewModel();
        }

        // ---- Restore ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Restore_action呼び出し()
        {
            var rect = NewRect();
            int called = 0;
            rect.Restore(() => called++);
            Assert.That(called, Is.EqualTo(1));
        }

        // ---- OnNext / OnError / OnCompleted ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void OnNextTransformNotification_例外なし()
        {
            var rect = NewRect();
            Assert.That(() => rect.OnNext(new TransformNotification { Sender = rect }), Throws.Nothing);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void OnError_NotSupportedException()
        {
            var rect = NewRect();
            Assert.That(() => rect.OnError(new Exception("test")), Throws.TypeOf<NotSupportedException>());
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void OnCompleted_NotSupportedException()
        {
            var rect = NewRect();
            Assert.That(() => rect.OnCompleted(), Throws.TypeOf<NotSupportedException>());
        }

        // ---- Subscribe (IObservable<TransformNotification>) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Subscribe_observer登録_即OnNext()
        {
            var rect = NewRect();
            int n = 0;
            var observer = new ManualObserver<TransformNotification>(_ => n++);
            using var d = ((IObservable<TransformNotification>)rect).Subscribe(observer);
            Assert.That(n, Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Subscribe_Dispose_observer解除()
        {
            var rect = NewRect();
            var observer = new ManualObserver<TransformNotification>(_ => { });
            var d = ((IObservable<TransformNotification>)rect).Subscribe(observer);
            Assert.That(() => d.Dispose(), Throws.Nothing);
        }

        private sealed class ManualObserver<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;
            public ManualObserver(Action<T> onNext) { _onNext = onNext; }
            public void OnNext(T value) => _onNext(value);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }

        // ---- IsSameGroup ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void IsSameGroup_ParentIDが一致しEmptyでない_true()
        {
            var a = NewRect();
            var b = NewRect();
            var groupId = Guid.NewGuid();
            a.ParentID = groupId;
            b.ParentID = groupId;
            Assert.That(a.IsSameGroup(b), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void IsSameGroup_異なるParentID_false()
        {
            var a = NewRect();
            var b = NewRect();
            a.ParentID = Guid.NewGuid();
            b.ParentID = Guid.NewGuid();
            Assert.That(a.IsSameGroup(b), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void IsSameGroup_両方GuidEmpty_false()
        {
            var a = NewRect();
            var b = NewRect();
            // ParentID は default = Guid.Empty
            Assert.That(a.IsSameGroup(b), Is.False);
        }

        // ---- Swap ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Swap_異なる型はInvalidOperationException()
        {
            var rect = NewRect();
            var ellipse = new NEllipseViewModel();
            Assert.That(() => rect.Swap(ellipse), Throws.TypeOf<InvalidOperationException>());
        }

        // ---- ShowPropertiesAndFields / ToString ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ShowPropertiesAndFields_型名タグから始まる()
        {
            var rect = NewRect();
            var s = rect.ShowPropertiesAndFields();
            Assert.That(s, Does.StartWith("<NRectangleViewModel>"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ToString_ShowPropertiesAndFieldsと同じ()
        {
            var rect = NewRect();
            var ts = rect.ToString();
            var sp = rect.ShowPropertiesAndFields();
            Assert.That(ts, Is.EqualTo(sp));
        }

        // ---- BeginMonitor ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void BeginMonitor_CompositeDisposableを返す()
        {
            var rect = NewRect();
            using var d = rect.BeginMonitor(() => { });
            Assert.That(d, Is.Not.Null);
            Assert.That(d, Is.InstanceOf<R3.CompositeDisposable>());
        }

        // ---- OpenInstructionDialog (virtual) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void OpenInstructionDialog_仮想実装は例外なし()
        {
            var rect = NewRect();
            Assert.That(() => rect.OpenInstructionDialog(), Throws.Nothing);
        }

        // ---- Id / Owner プロパティ ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Id_Owner_セッターで反映される()
        {
            var dlg = new Moq.Mock<Prism.Services.Dialogs.IDialogService>();
            var mainVM = new MainWindowViewModel(dlg.Object);
            var diagramVM = new DiagramViewModel(mainVM);
            var rect = new NRectangleViewModel();
            rect.Id = 42;
            rect.Owner = diagramVM;
            Assert.That(rect.Id, Is.EqualTo(42));
            Assert.That(rect.Owner, Is.SameAs(diagramVM));
        }

        // ---- Init で EnableForSelection が true、PenLineJoins が 3 件 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Init_EnableForSelectionがtrue_PenLineJoinsは3件()
        {
            var rect = NewRect();
            Assert.That(rect.EnableForSelection.Value, Is.True);
            Assert.That(rect.PenLineJoins.Count, Is.EqualTo(3));
            Assert.That(rect.StrokeDashArray.Value, Is.Not.Null);
        }

        // ---- RotationAngle 360超は wrap ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void RotationAngle_360超で剰余に丸められる()
        {
            var rect = NewRect();
            rect.RotationAngle.Value = 450;
            Assert.That(rect.RotationAngle.Value, Is.EqualTo(90));
        }

        // ---- Owner setter で InitMagnificationBindings 呼ばれる ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Owner_setterで再InitMagnificationBindings()
        {
            var rect = NewRect();
            // Owner なしでもデフォルトで SnapPointSize 等は初期化済み
            Assert.That(rect.SnapPointSize, Is.Not.Null);

            var dlg = new Moq.Mock<Prism.Services.Dialogs.IDialogService>();
            var mainVM = new MainWindowViewModel(dlg.Object);
            var diagramVM = new DiagramViewModel(mainVM);
            rect.Owner = diagramVM;
            Assert.That(rect.SnapPointSize, Is.Not.Null);
        }

        // ---- GetParent ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetParent_ParentIDがEmptyならnull()
        {
            var rect = NewRect();
            // ParentID は default Empty
            Assert.That(rect.GetParent(), Is.Null);
        }
    }
}
