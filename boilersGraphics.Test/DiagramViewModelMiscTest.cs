using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class DiagramViewModelMiscTest
    {
        private static (DiagramViewModel diagram, Layer layer) NewDiagram(bool initialize = false)
        {
            App.IsTest = true;
            var dlg = new Mock<IDialogService>();
            var mainVM = new MainWindowViewModel(dlg.Object);
            var diagram = new DiagramViewModel(mainVM);
            if (initialize) diagram.Initialize();
            diagram.Layers.Clear();
            var layer = new Layer();
            layer.Name.Value = "L1";
            diagram.Layers.Add(layer);
            layer.IsSelected.Value = true;
            return (diagram, layer);
        }

        // ---- DeselectAll ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void DeselectAll_全LayerItemとItemのIsSelectedをfalseに()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            d.AddItemCommand.Execute(new NEllipseViewModel());
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(layer.Children[0].IsSelected.Value, Is.True);

            d.DeselectAll();

            Assert.That(layer.Children[0].IsSelected.Value, Is.False);
            Assert.That(layer.Children[1].IsSelected.Value, Is.False);
            // 内側 Item の IsSelected も false
            Assert.That(((LayerItem)layer.Children[0]).Item.Value.IsSelected.Value, Is.False);
        }

        // NOTE: Connector の SnapPoint0VM/SnapPoint1VM 経路は ctor だけでは
        // 初期化されない (DiagramViewModel.Initialize と Behavior 経由で生成される)
        // ため、純粋単体テストでは触りにくい。後ほど vs-mcp UI 経由で検証予定。

        // ---- OverwriteColorSpot ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void OverwriteColorSpot_最初の白スロット0番に上書き()
        {
            var (d, _) = NewDiagram();
            var red = new SolidColorBrush(Colors.Red);
            d.OverwriteColorSpot(red);

            // ColorSpot0 が Red に置き換わる
            Assert.That(((SolidColorBrush)d.ColorSpots.Value.ColorSpot0).Color, Is.EqualTo(Colors.Red));
            // ColorSpot1 はまだ White
            Assert.That(((SolidColorBrush)d.ColorSpots.Value.ColorSpot1).Color, Is.EqualTo(Colors.White));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void OverwriteColorSpot_2回呼ぶと0と1にそれぞれ入る()
        {
            var (d, _) = NewDiagram();
            d.OverwriteColorSpot(new SolidColorBrush(Colors.Red));
            d.OverwriteColorSpot(new SolidColorBrush(Colors.Blue));

            Assert.That(((SolidColorBrush)d.ColorSpots.Value.ColorSpot0).Color, Is.EqualTo(Colors.Red));
            Assert.That(((SolidColorBrush)d.ColorSpots.Value.ColorSpot1).Color, Is.EqualTo(Colors.Blue));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void OverwriteColorSpot_白を渡すと既存の白と一致なので何もしない()
        {
            var (d, _) = NewDiagram();
            d.OverwriteColorSpot(new SolidColorBrush(Colors.White));
            // どれも白のまま
            Assert.That(((SolidColorBrush)d.ColorSpots.Value.ColorSpot0).Color, Is.EqualTo(Colors.White));
            Assert.That(((SolidColorBrush)d.ColorSpots.Value.ColorSpot99).Color, Is.EqualTo(Colors.White));
        }

        // ---- SerializeCanvasState / RestoreCanvasState ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SerializeCanvasState_CanvasルートとLayersBackgroundが含まれる()
        {
            var (d, _) = NewDiagram(initialize: true);
            var x = d.SerializeCanvasState();
            Assert.That(x.Name.LocalName, Is.EqualTo("Canvas"));
            Assert.That(x.Element("Layers"), Is.Not.Null);
            Assert.That(x.Element("Background"), Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SerializeCanvasState_BackgroundのLeftTopWidthHeightが直列化される()
        {
            var (d, _) = NewDiagram(initialize: true);
            var bg = d.BackgroundItem.Value;
            bg.Left.Value = 1;
            bg.Top.Value = 2;
            bg.Width.Value = 300;
            bg.Height.Value = 400;
            var x = d.SerializeCanvasState();
            var bgEl = x.Element("Background");
            Assert.That(bgEl.Element("Left").Value, Is.EqualTo("1"));
            Assert.That(bgEl.Element("Top").Value, Is.EqualTo("2"));
            Assert.That(bgEl.Element("Width").Value, Is.EqualTo("300"));
            Assert.That(bgEl.Element("Height").Value, Is.EqualTo("400"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void RestoreCanvasState_SerializeしたXMLからBackgroundが復元()
        {
            var (d, _) = NewDiagram(initialize: true);
            var bg = d.BackgroundItem.Value;
            bg.Left.Value = 5;
            bg.Top.Value = 7;
            bg.Width.Value = 800;
            bg.Height.Value = 600;
            var x = d.SerializeCanvasState();

            // 値を変えてから restore
            bg.Left.Value = 0;
            bg.Top.Value = 0;
            bg.Width.Value = 1;
            bg.Height.Value = 1;

            d.RestoreCanvasState(x);

            Assert.That(bg.Left.Value, Is.EqualTo(5));
            Assert.That(bg.Top.Value, Is.EqualTo(7));
            Assert.That(bg.Width.Value, Is.EqualTo(800));
            Assert.That(bg.Height.Value, Is.EqualTo(600));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void RestoreCanvasState_LayersがなければデフォルトLayerを1個追加()
        {
            var (d, _) = NewDiagram(initialize: true);
            // 中身ほぼ空の XElement
            var x = new System.Xml.Linq.XElement("Canvas");
            d.RestoreCanvasState(x);
            Assert.That(d.Layers.Count, Is.GreaterThanOrEqualTo(1));
        }
    }
}
