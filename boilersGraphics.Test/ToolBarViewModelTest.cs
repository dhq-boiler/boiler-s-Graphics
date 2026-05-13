using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;
using System.Windows;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ToolBarViewModelTest
    {
        private static MainWindowViewModel NewMainVM()
        {
            App.IsTest = true;
            var dlg = new Mock<IDialogService>();
            return new MainWindowViewModel(dlg.Object);
        }

        private static ToolBarViewModel ToolBar() => NewMainVM().ToolBarViewModel;

        // ---- ctor / 初期状態 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ctor_ToolItemsとToolItems2が初期化済()
        {
            var bar = ToolBar();
            Assert.That(bar.ToolItems.Count, Is.GreaterThan(0));
            Assert.That(bar.ToolItems2.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ctor_主要Behaviorプロパティが非null()
        {
            var bar = ToolBar();
            Assert.That(bar.DeselectBehavior, Is.Not.Null);
            Assert.That(bar.RubberbandBehavior, Is.Not.Null);
            Assert.That(bar.NDrawStraightLineBehavior, Is.Not.Null);
            Assert.That(bar.NDrawRectangleBehavior, Is.Not.Null);
            Assert.That(bar.NDrawEllipseBehavior, Is.Not.Null);
            Assert.That(bar.LetterBehavior, Is.Not.Null);
            Assert.That(bar.LetterVerticalBehavior, Is.Not.Null);
            Assert.That(bar.MonoTextBlockBehavior, Is.Not.Null);
            Assert.That(bar.NDrawBezierCurveBehavior, Is.Not.Null);
            Assert.That(bar.SetSnapPointBehavior, Is.Not.Null);
            Assert.That(bar.EraserBehavior, Is.Not.Null);
            Assert.That(bar.NDrawPieBehavior, Is.Not.Null);
            Assert.That(bar.DropperBehavior, Is.Not.Null);
            Assert.That(bar.CanvasModifierBehavior, Is.Not.Null);
            Assert.That(bar.MosaicBehavior, Is.Not.Null);
            Assert.That(bar.BlurBehavior, Is.Not.Null);
            Assert.That(bar.ColorCorrectBehavior, Is.Not.Null);
            Assert.That(bar.PolyBezierBehavior, Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ctor_ToolItemsにmonotextが含まれる()
        {
            var bar = ToolBar();
            var item = bar.ToolItems.SingleOrDefault(t => t.Name.Value == "monotext");
            Assert.That(item, Is.Not.Null, "Phase 2-b-2: モノスペーステキストツールが登録されている");
            Assert.That(item!.Tooltip.Value, Is.EqualTo("モノスペーステキスト"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SelectOneToolItem_monotext_他は解除される()
        {
            var bar = ToolBar();
            bar.SelectOneToolItem("monotext");

            var item = bar.ToolItems.Single(t => t.Name.Value == "monotext");
            Assert.That(item.IsChecked, Is.True);

            foreach (var other in bar.ToolItems.Where(t => t.Name.Value != "monotext"))
                Assert.That(other.IsChecked, Is.False, $"{other.Name.Value} should be unchecked");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ctor_CurrentHitTestVisibleStateの初期値はfalse()
        {
            var bar = ToolBar();
            Assert.That(bar.CurrentHitTestVisibleState.Value, Is.False);
        }

        // ---- SelectOneToolItem ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SelectOneToolItem_指定ツールがIsCheckedになり他はfalse()
        {
            var bar = ToolBar();
            // すべて初期化前は IsChecked false の前提
            // 1 つ選択
            bar.SelectOneToolItem("rectangle");

            var rectangleItem = bar.ToolItems.Single(t => t.Name.Value == "rectangle");
            Assert.That(rectangleItem.IsChecked, Is.True);

            // それ以外の全ツールは false
            foreach (var item in bar.ToolItems.Where(t => t.Name.Value != "rectangle"))
                Assert.That(item.IsChecked, Is.False, $"{item.Name.Value} should be unchecked");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SelectOneToolItem_別ツールに切り替えると前のIsCheckedが解除()
        {
            var bar = ToolBar();
            bar.SelectOneToolItem("ellipse");
            bar.SelectOneToolItem("letter");

            Assert.That(bar.ToolItems.Single(t => t.Name.Value == "ellipse").IsChecked, Is.False);
            Assert.That(bar.ToolItems.Single(t => t.Name.Value == "letter").IsChecked, Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SelectOneToolItem_存在しないツール名は誰もIsCheckedにしない()
        {
            var bar = ToolBar();
            bar.SelectOneToolItem("does-not-exist");
            Assert.That(bar.ToolItems.Any(t => t.IsChecked), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SelectOneToolItem_描画ツールはContextMenuVisibilityがVisible()
        {
            var bar = ToolBar();
            var diagramVM = MainWindowViewModel.Instance.DiagramViewModel;
            diagramVM.ContextMenuVisibility.Value = Visibility.Collapsed;

            bar.SelectOneToolItem("rectangle");
            Assert.That(diagramVM.ContextMenuVisibility.Value, Is.EqualTo(Visibility.Visible));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SelectOneToolItem_dropperはContextMenuVisibilityがCollapsed()
        {
            var bar = ToolBar();
            var diagramVM = MainWindowViewModel.Instance.DiagramViewModel;
            diagramVM.ContextMenuVisibility.Value = Visibility.Visible;

            bar.SelectOneToolItem("dropper");
            Assert.That(diagramVM.ContextMenuVisibility.Value, Is.EqualTo(Visibility.Collapsed));
        }

        // ---- FinalizeToolItems / ReinitializeToolItems ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void FinalizeToolItems_ToolItemsを空にする()
        {
            var bar = ToolBar();
            Assert.That(bar.ToolItems.Count, Is.GreaterThan(0));
            bar.FinalizeToolItems();
            Assert.That(bar.ToolItems.Count, Is.EqualTo(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ReinitializeToolItems_ToolItemsを再構築()
        {
            var bar = ToolBar();
            int before = bar.ToolItems.Count;
            bar.ReinitializeToolItems();
            Assert.That(bar.ToolItems.Count, Is.EqualTo(before));
        }
    }
}
