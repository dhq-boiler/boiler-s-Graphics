using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class DiagramViewModelExecuteTest
    {
        // 本テスト群の意図:
        // - Group/Ungroup/Order/Align は ToolBar ボタンや Ctrl+G などのキーバインド
        //   で頻繁に叩かれる中核操作。CanExecute だけ通っても、Execute のロジックが
        //   壊れると「グループ化されない」「ZIndex 順がおかしい」「整列が崩れる」と
        //   いった即見えるバグになる。Phase 7 の CanExecute テストを補完する。

        private static (DiagramViewModel diagram, Layer layer) NewDiagram()
        {
            App.IsTest = true;
            var dlg = new Mock<IDialogService>();
            var mainVM = new MainWindowViewModel(dlg.Object);
            var diagram = new DiagramViewModel(mainVM);
            diagram.Layers.Clear();
            var layer = new Layer();
            layer.Name.Value = "L1";
            diagram.Layers.Add(layer);
            layer.IsSelected.Value = true;
            return (diagram, layer);
        }

        private static NRectangleViewModel AddRect(DiagramViewModel d, Layer layer, double left, double top, double width, double height, int zIndex = 0)
        {
            var r = new NRectangleViewModel();
            r.Left.Value = left;
            r.Top.Value = top;
            r.Width.Value = width;
            r.Height.Value = height;
            r.ZIndex.Value = zIndex;
            d.AddItemCommand.Execute(r);
            return r;
        }

        private static void Select(DiagramViewModel d, params SelectableDesignerItemViewModelBase[] items)
        {
            foreach (var layer in d.Layers)
                foreach (var child in layer.Children.OfType<LayerItem>())
                    child.IsSelected.Value = items.Contains(child.Item.Value);
        }

        // ---- Group ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Group_2アイテムをグループ化すると_GroupItemが生成され子のParentIDが設定される()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 10, 10, 50, 60);
            var r2 = AddRect(d, layer, 80, 10, 70, 40);
            Select(d, r1, r2);

            d.GroupCommand.Execute();

            // 元の 2 アイテム + Group + (アイテムは Group 配下の LayerItem.Children に再配置)
            var allItems = d.AllItems.Value.ToList();
            var groups = allItems.OfType<GroupItemViewModel>().ToList();
            Assert.That(groups.Count, Is.EqualTo(1), "Group が 1 つ生成される");

            var group = groups.Single();
            Assert.That(r1.ParentID, Is.EqualTo(group.ID), "r1 の ParentID が Group の ID");
            Assert.That(r2.ParentID, Is.EqualTo(group.ID), "r2 の ParentID が Group の ID");
            Assert.That(r1.EnableForSelection.Value, Is.False, "Group 内アイテムは EnableForSelection=false");
            Assert.That(r2.EnableForSelection.Value, Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Group_BoundingRectangleがGroupのWidth_Heightに反映()
        {
            var (d, layer) = NewDiagram();
            // r1: (10, 10, 50, 60)  → 右下 (60, 70)
            // r2: (80, 10, 70, 40)  → 右下 (150, 50)
            // 結合 BBox: Left=10, Top=10, Right=150, Bottom=70 → W=140, H=60
            var r1 = AddRect(d, layer, 10, 10, 50, 60);
            var r2 = AddRect(d, layer, 80, 10, 70, 40);
            Select(d, r1, r2);

            d.GroupCommand.Execute();

            var group = d.AllItems.Value.OfType<GroupItemViewModel>().Single();
            Assert.That(group.Left.Value, Is.EqualTo(10));
            Assert.That(group.Top.Value, Is.EqualTo(10));
            Assert.That(group.Width.Value, Is.EqualTo(140));
            Assert.That(group.Height.Value, Is.EqualTo(60));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Group_Undo_Redoでグループ状態がトグル()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 10, 10, 50, 60);
            var r2 = AddRect(d, layer, 80, 10, 70, 40);
            Select(d, r1, r2);

            d.GroupCommand.Execute();
            Assert.That(r1.ParentID, Is.Not.EqualTo(Guid.Empty));

            d.UndoCommand.Execute();
            Assert.That(r1.ParentID, Is.EqualTo(Guid.Empty), "Undo で ParentID が Empty に戻る");
            Assert.That(r1.EnableForSelection.Value, Is.True, "Undo で EnableForSelection が復元");

            d.RedoCommand.Execute();
            Assert.That(r1.ParentID, Is.Not.EqualTo(Guid.Empty), "Redo で再びグループ配下に");
        }

        // ---- Ungroup ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ungroup_GroupItemを解除すると_子のParentIDがGuidEmptyに()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 10, 10, 50, 60);
            var r2 = AddRect(d, layer, 80, 10, 70, 40);
            Select(d, r1, r2);
            d.GroupCommand.Execute();

            var group = d.AllItems.Value.OfType<GroupItemViewModel>().Single();
            Select(d, group);

            d.UngroupCommand.Execute();

            Assert.That(r1.ParentID, Is.EqualTo(Guid.Empty));
            Assert.That(r2.ParentID, Is.EqualTo(Guid.Empty));
            Assert.That(r1.EnableForSelection.Value, Is.True);
            Assert.That(d.AllItems.Value.OfType<GroupItemViewModel>().Any(), Is.False,
                "GroupItem 自体は AllItems から消える");
        }

        // ---- Order: BringForward / SendBackward ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void BringForward_真ん中の選択アイテムは次に上のZIndexと入れ替わる()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 0);
            var r2 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 1);
            var r3 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 2);
            Select(d, r2);

            d.BringForwardCommand.Execute();

            // r2 と r3 が swap
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(0), "未関与アイテムは変わらない");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void BringForward_最前面なら何もしない()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 0);
            var r2 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 1);
            Select(d, r2);

            d.BringForwardCommand.Execute();

            // 最前面なので何も変わらない
            Assert.That(r1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SendBackward_真ん中の選択アイテムは次に下のZIndexと入れ替わる()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 0);
            var r2 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 1);
            var r3 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 2);
            Select(d, r2);

            d.SendBackwardCommand.Execute();

            // r2 と r1 が swap
            Assert.That(r2.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(2));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SendBackward_最背面なら何もしない()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 0);
            var r2 = AddRect(d, layer, 0, 0, 10, 10, zIndex: 1);
            Select(d, r1);

            d.SendBackwardCommand.Execute();

            Assert.That(r1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(1));
        }

        // ---- Align ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void AlignTop_全選択アイテムのTopが先頭アイテムに揃う()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 0, 100, 10, 10);  // first → Top=100
            var r2 = AddRect(d, layer, 0, 200, 10, 10);
            var r3 = AddRect(d, layer, 0, 50, 10, 10);
            Select(d, r1, r2, r3);

            d.AlignTopCommand.Execute();

            Assert.That(r1.Top.Value, Is.EqualTo(100));
            Assert.That(r2.Top.Value, Is.EqualTo(100));
            Assert.That(r3.Top.Value, Is.EqualTo(100));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void AlignBottom_全選択アイテムのBottomが先頭の下端に揃う()
        {
            var (d, layer) = NewDiagram();
            // 先頭 r1: Top=100, Height=20 → Bottom=120
            var r1 = AddRect(d, layer, 0, 100, 10, 20);
            var r2 = AddRect(d, layer, 0, 200, 10, 30); // Bottom=120 にしたいので Top = 90
            var r3 = AddRect(d, layer, 0, 50, 10, 40);  // Bottom=120 にしたいので Top = 80
            Select(d, r1, r2, r3);

            d.AlignBottomCommand.Execute();

            Assert.That(r1.Top.Value + r1.Height.Value, Is.EqualTo(120));
            Assert.That(r2.Top.Value + r2.Height.Value, Is.EqualTo(120));
            Assert.That(r3.Top.Value + r3.Height.Value, Is.EqualTo(120));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void AlignLeft_全選択アイテムのLeftが先頭に揃う()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 100, 0, 10, 10);  // first → Left=100
            var r2 = AddRect(d, layer, 200, 0, 10, 10);
            var r3 = AddRect(d, layer, 50, 0, 10, 10);
            Select(d, r1, r2, r3);

            d.AlignLeftCommand.Execute();

            Assert.That(r1.Left.Value, Is.EqualTo(100));
            Assert.That(r2.Left.Value, Is.EqualTo(100));
            Assert.That(r3.Left.Value, Is.EqualTo(100));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void AlignRight_全選択アイテムのRightが先頭の右端に揃う()
        {
            var (d, layer) = NewDiagram();
            // 先頭 r1: Left=100, Width=20 → Right=120
            var r1 = AddRect(d, layer, 100, 0, 20, 10);
            var r2 = AddRect(d, layer, 200, 0, 30, 10);
            var r3 = AddRect(d, layer, 50, 0, 40, 10);
            Select(d, r1, r2, r3);

            d.AlignRightCommand.Execute();

            Assert.That(r1.Left.Value + r1.Width.Value, Is.EqualTo(120));
            Assert.That(r2.Left.Value + r2.Width.Value, Is.EqualTo(120));
            Assert.That(r3.Left.Value + r3.Width.Value, Is.EqualTo(120));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void AlignTop_1アイテムでは何もしない()
        {
            var (d, layer) = NewDiagram();
            var r1 = AddRect(d, layer, 0, 100, 10, 10);
            Select(d, r1);

            d.AlignTopCommand.Execute();

            Assert.That(r1.Top.Value, Is.EqualTo(100), "1 アイテムでは Top 変化なし");
        }
    }
}
