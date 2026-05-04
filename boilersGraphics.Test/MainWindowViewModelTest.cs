using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class MainWindowViewModelTest
    {
        private static MainWindowViewModel NewVM()
        {
            App.IsTest = true;
            var dlg = new Mock<IDialogService>();
            return new MainWindowViewModel(dlg.Object);
        }

        // ---- InitializeCanvasManagement ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void InitializeCanvasManagement_コマンド作成と初期Canvas1個()
        {
            var vm = NewVM();
            vm.CanvasPages.Clear();
            vm.InitializeCanvasManagement();

            Assert.That(vm.AddCanvasCommand, Is.Not.Null);
            Assert.That(vm.SwitchCanvasCommand, Is.Not.Null);
            Assert.That(vm.RemoveCanvasCommand, Is.Not.Null);
            Assert.That(vm.PlayAnimationCommand, Is.Not.Null);
            Assert.That(vm.StopAnimationCommand, Is.Not.Null);
            Assert.That(vm.ExportAnimationGifCommand, Is.Not.Null);

            Assert.That(vm.CanvasPages.Count, Is.EqualTo(1));
            Assert.That(vm.CanvasPages[0].IsActive, Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void InitializeCanvasManagement_既存Pageありなら新規作成しない()
        {
            var vm = NewVM();
            vm.CanvasPages.Clear();
            vm.CanvasPages.Add(new CanvasPage("existing"));

            vm.InitializeCanvasManagement();
            Assert.That(vm.CanvasPages.Count, Is.EqualTo(1));
            Assert.That(vm.CanvasPages[0].Name, Is.EqualTo("existing"));
        }

        // ---- UpdateActiveStates ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void UpdateActiveStates_ActiveCanvasIndexの位置だけIsActive_true()
        {
            var vm = NewVM();
            vm.CanvasPages.Clear();
            vm.CanvasPages.Add(new CanvasPage("p1"));
            vm.CanvasPages.Add(new CanvasPage("p2"));
            vm.CanvasPages.Add(new CanvasPage("p3"));
            vm.ActiveCanvasIndex.Value = 1;
            vm.UpdateActiveStates();

            Assert.That(vm.CanvasPages[0].IsActive, Is.False);
            Assert.That(vm.CanvasPages[1].IsActive, Is.True);
            Assert.That(vm.CanvasPages[2].IsActive, Is.False);
        }

        // ---- AddCanvas ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void AddCanvas_新規追加してActiveが新ページに()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            int before = vm.CanvasPages.Count;

            vm.AddCanvas();

            Assert.That(vm.CanvasPages.Count, Is.EqualTo(before + 1));
            Assert.That(vm.ActiveCanvasIndex.Value, Is.EqualTo(before));
            Assert.That(vm.CanvasPages[before].IsActive, Is.True);
        }

        // ---- SwitchCanvas ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SwitchCanvas_正常な切り替え()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            vm.AddCanvas();
            vm.AddCanvas();
            // active = 2
            vm.SwitchCanvas(0);
            Assert.That(vm.ActiveCanvasIndex.Value, Is.EqualTo(0));
            Assert.That(vm.CanvasPages[0].IsActive, Is.True);
            Assert.That(vm.CanvasPages[2].IsActive, Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SwitchCanvas_範囲外は何もしない()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            int before = vm.ActiveCanvasIndex.Value;
            vm.SwitchCanvas(99);
            vm.SwitchCanvas(-1);
            Assert.That(vm.ActiveCanvasIndex.Value, Is.EqualTo(before));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SwitchCanvas_同じインデックスは何もしない()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            vm.AddCanvas();
            int before = vm.ActiveCanvasIndex.Value;
            vm.SwitchCanvas(before);
            Assert.That(vm.ActiveCanvasIndex.Value, Is.EqualTo(before));
        }

        // ---- RemoveCanvas ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void RemoveCanvas_最後の1個は削除しない()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            Assert.That(vm.CanvasPages.Count, Is.EqualTo(1));
            vm.RemoveCanvas(0);
            Assert.That(vm.CanvasPages.Count, Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void RemoveCanvas_範囲外は何もしない()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            vm.AddCanvas();
            int before = vm.CanvasPages.Count;
            vm.RemoveCanvas(99);
            vm.RemoveCanvas(-1);
            Assert.That(vm.CanvasPages.Count, Is.EqualTo(before));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void RemoveCanvas_アクティブより前を削除でアクティブが1減()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            vm.AddCanvas();
            vm.AddCanvas(); // active=2
            int beforeActive = vm.ActiveCanvasIndex.Value;
            vm.RemoveCanvas(0);
            Assert.That(vm.ActiveCanvasIndex.Value, Is.EqualTo(beforeActive - 1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void RemoveCanvas_アクティブより後を削除でアクティブは変わらず()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            vm.AddCanvas();
            vm.AddCanvas(); // active=2
            vm.SwitchCanvas(0); // active=0
            vm.RemoveCanvas(2);
            Assert.That(vm.ActiveCanvasIndex.Value, Is.EqualTo(0));
        }

        // ---- SaveCurrentCanvasState ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SaveCurrentCanvasState_範囲外indexなら無動作()
        {
            var vm = NewVM();
            vm.CanvasPages.Clear();
            vm.ActiveCanvasIndex.Value = 0;
            // CanvasPages は空。範囲外
            Assert.That(() => vm.SaveCurrentCanvasState(), Throws.Nothing);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SaveCurrentCanvasState_アクティブPageのSerializedDataにserializeを設定()
        {
            var vm = NewVM();
            vm.InitializeCanvasManagement();
            vm.SaveCurrentCanvasState();
            // SerializeCanvasState は XElement を返す前提
            Assert.That(vm.CanvasPages[0].SerializedData, Is.Not.Null);
        }

        // ---- ClearCurrentOperationAndDetails ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ClearCurrentOperationAndDetails_両プロパティを空文字に()
        {
            var vm = NewVM();
            vm.CurrentOperation.Value = "drag";
            vm.Details.Value = "info";
            vm.ClearCurrentOperationAndDetails();
            Assert.That(vm.CurrentOperation.Value, Is.EqualTo(string.Empty));
            Assert.That(vm.Details.Value, Is.EqualTo(string.Empty));
        }
    }
}
