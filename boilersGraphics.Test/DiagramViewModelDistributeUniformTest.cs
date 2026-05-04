using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class DiagramViewModelDistributeUniformTest
    {
        // 本テスト群の意図 (Phase 13 Group/Order/Align の続き):
        // - Distribute: バラバラに置いた図形を等間隔に整列。両端は動かさず、
        //   中間アイテムが等間隔になるべき。バグると等間隔にならない or
        //   重複時に間隔がマイナスになり「内側に潜り込む」表示崩れ。
        // - Uniform: 選択アイテムの幅/高さを最初の選択アイテムに揃える。
        //   バグると揃わない、もしくは揃えると同時にレイアウトが崩れる。

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

        private static NRectangleViewModel AddRect(DiagramViewModel d, double left, double top, double width, double height)
        {
            var r = new NRectangleViewModel();
            r.Left.Value = left;
            r.Top.Value = top;
            r.Width.Value = width;
            r.Height.Value = height;
            d.AddItemCommand.Execute(r);
            return r;
        }

        private static void Select(DiagramViewModel d, params SelectableDesignerItemViewModelBase[] items)
        {
            foreach (var layer in d.Layers)
                foreach (var child in layer.Children.OfType<LayerItem>())
                    child.IsSelected.Value = items.Contains(child.Item.Value);
        }

        // ---- DistributeHorizontal ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void DistributeHorizontal_3アイテムを等間隔化する()
        {
            var (d, _) = NewDiagram();
            // 3 アイテム W=20、左端 0、右端 200 (left+width=220)
            // 配置: r1 Left=0, r3 Left=200, r2 Left=70 (中途半端)
            // 範囲: left=0, right=220, sumWidth=60 → distance=(220-0-60)/2=80
            // 結果: r1 Left=0, 次=0+20+80=100, r2 Left=100, 次=100+20+80=200, r3 Left=200
            var r1 = AddRect(d, 0, 0, 20, 10);
            var r2 = AddRect(d, 70, 0, 20, 10);
            var r3 = AddRect(d, 200, 0, 20, 10);
            Select(d, r1, r2, r3);

            d.DistributeHorizontalCommand.Execute();

            Assert.That(r1.Left.Value, Is.EqualTo(0).Within(1e-6), "左端は固定");
            Assert.That(r2.Left.Value, Is.EqualTo(100).Within(1e-6), "中央が等間隔位置に");
            Assert.That(r3.Left.Value, Is.EqualTo(200).Within(1e-6), "右端は固定");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DistributeHorizontal_異なる幅でも等間隔距離は同じ()
        {
            var (d, _) = NewDiagram();
            // r1: Left=0, W=20 → Right=20
            // r2: Left=50, W=40 → Right=90
            // r3: Left=200, W=10 → Right=210
            // 範囲: left=0, right=210, sumWidth=70 → distance=(210-0-70)/2=70
            // 並びを Left でソート → r1, r2, r3
            // offset=0: r1 Left=0, offset=0+20+70=90
            //          r2 Left=90, offset=90+40+70=200
            //          r3 Left=200, offset=200+10+70=280
            var r1 = AddRect(d, 0, 0, 20, 10);
            var r2 = AddRect(d, 50, 0, 40, 10);
            var r3 = AddRect(d, 200, 0, 10, 10);
            Select(d, r1, r2, r3);

            d.DistributeHorizontalCommand.Execute();

            Assert.That(r1.Left.Value, Is.EqualTo(0).Within(1e-6));
            Assert.That(r2.Left.Value, Is.EqualTo(90).Within(1e-6));
            Assert.That(r3.Left.Value, Is.EqualTo(200).Within(1e-6));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DistributeHorizontal_合計幅が範囲を超えるとdistance0で密着()
        {
            var (d, _) = NewDiagram();
            // r1: Left=0, W=100 → 単独で範囲 100
            // r2: Left=20, W=80 → 重なって配置
            // r3: Left=50, W=50 → 重なって配置
            // 範囲: left=0, right=100, sumWidth=230 → (100-230)/2 < 0 → distance=Max(0,...)=0
            // offset=0: r1 Left=0, offset=100; r2 Left=100, offset=180; r3 Left=180
            var r1 = AddRect(d, 0, 0, 100, 10);
            var r2 = AddRect(d, 20, 0, 80, 10);
            var r3 = AddRect(d, 50, 0, 50, 10);
            Select(d, r1, r2, r3);

            d.DistributeHorizontalCommand.Execute();

            Assert.That(r1.Left.Value, Is.EqualTo(0).Within(1e-6));
            Assert.That(r2.Left.Value, Is.EqualTo(100).Within(1e-6), "前のRightに密着");
            Assert.That(r3.Left.Value, Is.EqualTo(180).Within(1e-6), "前のRightに密着");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DistributeHorizontal_1アイテムでは何もしない()
        {
            var (d, _) = NewDiagram();
            var r1 = AddRect(d, 50, 0, 20, 10);
            Select(d, r1);

            d.DistributeHorizontalCommand.Execute();

            Assert.That(r1.Left.Value, Is.EqualTo(50));
        }

        // ---- DistributeVertical ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void DistributeVertical_3アイテムを等間隔化する()
        {
            var (d, _) = NewDiagram();
            // 3 アイテム H=10、上端 0、下端 200 (top+height=210)
            // 配置: r1 Top=0, r3 Top=200, r2 Top=70
            // 範囲: top=0, bottom=210, sumHeight=30 → distance=(210-0-30)/2=90
            // offset=0: r1 Top=0, offset=10+90=100, r2 Top=100, offset=110+90=200, r3 Top=200
            var r1 = AddRect(d, 0, 0, 10, 10);
            var r2 = AddRect(d, 0, 70, 10, 10);
            var r3 = AddRect(d, 0, 200, 10, 10);
            Select(d, r1, r2, r3);

            d.DistributeVerticalCommand.Execute();

            Assert.That(r1.Top.Value, Is.EqualTo(0).Within(1e-6));
            Assert.That(r2.Top.Value, Is.EqualTo(100).Within(1e-6));
            Assert.That(r3.Top.Value, Is.EqualTo(200).Within(1e-6));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DistributeVertical_1アイテムでは何もしない()
        {
            var (d, _) = NewDiagram();
            var r1 = AddRect(d, 0, 50, 10, 10);
            Select(d, r1);

            d.DistributeVerticalCommand.Execute();

            Assert.That(r1.Top.Value, Is.EqualTo(50));
        }

        // ---- UniformWidth ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void UniformWidth_全アイテムのWidthがfirstに揃う()
        {
            var (d, _) = NewDiagram();
            // first selected の Width に揃う
            // SelectedItems の順序は Add 順とは限らないが、SelectedOrder に依存。
            // ここでは r1 を最初に選択 (IsSelected=true 順序) → first が r1 になる前提
            var r1 = AddRect(d, 0, 0, 100, 30);
            var r2 = AddRect(d, 0, 50, 50, 30);
            var r3 = AddRect(d, 0, 100, 200, 30);
            // 順番に IsSelected=true → SelectedOrder で先勝ち
            ((LayerItem)d.Layers[0].Children[0]).IsSelected.Value = true;
            ((LayerItem)d.Layers[0].Children[1]).IsSelected.Value = true;
            ((LayerItem)d.Layers[0].Children[2]).IsSelected.Value = true;

            var firstWidth = d.SelectedItems.Value.OfType<DesignerItemViewModelBase>().First().Width.Value;
            d.UniformWidthCommand.Execute();

            // 全アイテムが firstWidth に揃う
            Assert.That(r1.Width.Value, Is.EqualTo(firstWidth));
            Assert.That(r2.Width.Value, Is.EqualTo(firstWidth));
            Assert.That(r3.Width.Value, Is.EqualTo(firstWidth));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void UniformWidth_1アイテムでは何もしない()
        {
            var (d, _) = NewDiagram();
            var r1 = AddRect(d, 0, 0, 100, 30);
            Select(d, r1);

            d.UniformWidthCommand.Execute();

            Assert.That(r1.Width.Value, Is.EqualTo(100), "1 アイテムでは Width 不変");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void UniformWidth_既に同サイズでも例外を出さない()
        {
            var (d, _) = NewDiagram();
            var r1 = AddRect(d, 0, 0, 50, 30);
            var r2 = AddRect(d, 0, 40, 50, 30);
            ((LayerItem)d.Layers[0].Children[0]).IsSelected.Value = true;
            ((LayerItem)d.Layers[0].Children[1]).IsSelected.Value = true;

            Assert.That(() => d.UniformWidthCommand.Execute(), Throws.Nothing);
            Assert.That(r1.Width.Value, Is.EqualTo(50));
            Assert.That(r2.Width.Value, Is.EqualTo(50));
        }

        // ---- UniformHeight ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void UniformHeight_全アイテムのHeightがfirstに揃う()
        {
            var (d, _) = NewDiagram();
            var r1 = AddRect(d, 0, 0, 30, 100);
            var r2 = AddRect(d, 50, 0, 30, 50);
            var r3 = AddRect(d, 100, 0, 30, 200);
            ((LayerItem)d.Layers[0].Children[0]).IsSelected.Value = true;
            ((LayerItem)d.Layers[0].Children[1]).IsSelected.Value = true;
            ((LayerItem)d.Layers[0].Children[2]).IsSelected.Value = true;

            var firstHeight = d.SelectedItems.Value.OfType<DesignerItemViewModelBase>().First().Height.Value;
            d.UniformHeightCommand.Execute();

            Assert.That(r1.Height.Value, Is.EqualTo(firstHeight));
            Assert.That(r2.Height.Value, Is.EqualTo(firstHeight));
            Assert.That(r3.Height.Value, Is.EqualTo(firstHeight));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void UniformHeight_1アイテムでは何もしない()
        {
            var (d, _) = NewDiagram();
            var r1 = AddRect(d, 0, 0, 30, 100);
            Select(d, r1);

            d.UniformHeightCommand.Execute();

            Assert.That(r1.Height.Value, Is.EqualTo(100));
        }

        // ---- Undo round-trip (代表 1 件: DistributeHorizontal) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void DistributeHorizontal_Undoで元の位置に戻る()
        {
            var (d, _) = NewDiagram();
            var r1 = AddRect(d, 0, 0, 20, 10);
            var r2 = AddRect(d, 70, 0, 20, 10);
            var r3 = AddRect(d, 200, 0, 20, 10);
            Select(d, r1, r2, r3);

            d.DistributeHorizontalCommand.Execute();
            Assert.That(r2.Left.Value, Is.EqualTo(100).Within(1e-6));

            d.UndoCommand.Execute();
            Assert.That(r2.Left.Value, Is.EqualTo(70).Within(1e-6), "Undo で元の Left に戻る");
        }
    }
}
