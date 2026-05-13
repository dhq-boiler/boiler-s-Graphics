using boilersGraphics.Helpers.Anchors;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Anchors;

[TestFixture]
public class NodeHighlightControllerTest
{
    [Test, RequiresThread(ApartmentState.STA)]
    public void InvertBrush_SolidColorBrush_各成分が反転する()
    {
        var src = new SolidColorBrush(Color.FromArgb(255, 10, 20, 30));
        var inverted = NodeHighlightController.InvertBrush(src) as SolidColorBrush;
        Assert.That(inverted, Is.Not.Null);
        Assert.That(inverted.Color, Is.EqualTo(Color.FromArgb(255, 245, 235, 225)));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void InvertBrush_Alphaは保持される()
    {
        var src = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        var inverted = NodeHighlightController.InvertBrush(src) as SolidColorBrush;
        Assert.That(inverted.Color.A, Is.EqualTo(128));
        Assert.That(inverted.Color.R, Is.EqualTo(255));
        Assert.That(inverted.Color.G, Is.EqualTo(255));
        Assert.That(inverted.Color.B, Is.EqualTo(255));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void InvertBrush_SolidColorBrush以外は元のBrushを返す()
    {
        var src = new LinearGradientBrush();
        var result = NodeHighlightController.InvertBrush(src);
        // SolidColorBrush 以外は反転不可なので、元のオブジェクトをそのまま返す
        Assert.That(result, Is.SameAs(src));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void InvertBrush_白は黒になる()
    {
        var inverted = NodeHighlightController.InvertBrush(new SolidColorBrush(Colors.White)) as SolidColorBrush;
        Assert.That(inverted.Color, Is.EqualTo(Colors.Black));
    }

    [Test]
    public void ThicknessMultiplier_1_5倍()
    {
        Assert.That(NodeHighlightController.ThicknessMultiplier, Is.EqualTo(1.5));
    }

    private static (DiagramViewModel diagram, Layer layer) CreateDiagram()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Mock<IDialogService>();
        var main = new MainWindowViewModel(dlg.Object);
        var diagram = new DiagramViewModel(main);
        diagram.Layers.Clear();
        var layer = new Layer();
        layer.Name.Value = "L";
        diagram.Layers.Add(layer);
        layer.IsSelected.Value = true;
        return (diagram, layer);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsNodeなnodeを選択すると関連OrthogonalConnectorがハイライトされる()
    {
        var (diagram, _) = CreateDiagram();

        var node = new NRectangleViewModel { ID = System.Guid.NewGuid() };
        node.Left.Value = 0;
        node.Top.Value = 0;
        node.Width.Value = 100;
        node.Height.Value = 100;
        node.IsNode.Value = true;
        node.EdgeBrush.Value = new SolidColorBrush(Colors.Black);
        diagram.AddItemCommand.Execute(node);

        var connector = new OrthogonalConnectorViewModel(diagram, new Point(0, 0));
        connector.AddPointP2(diagram, new Point(200, 200));
        connector.BeginAnchorRef.Value = $"{node.ID}#tl";
        connector.EdgeBrush.Value = new SolidColorBrush(Color.FromArgb(255, 10, 20, 30));
        connector.EdgeThickness.Value = 2.0;
        diagram.AddItemCommand.Execute(connector);

        // 選択前は変更なし
        Assert.That(connector.EdgeThickness.Value, Is.EqualTo(2.0));

        // node を選択
        diagram.Layers[0].Children.First().IsSelected.Value = true;

        // 関連コネクタの太さが 1.5 倍 / Brush 反転
        Assert.That(connector.EdgeThickness.Value, Is.EqualTo(3.0).Within(1e-9),
            "1.5 倍に太くなるはず");
        var brush = (SolidColorBrush)connector.EdgeBrush.Value;
        Assert.That(brush.Color, Is.EqualTo(Color.FromArgb(255, 245, 235, 225)),
            "色が反転するはず");

        // node の選択解除で元に戻る
        diagram.Layers[0].Children.First().IsSelected.Value = false;
        Assert.That(connector.EdgeThickness.Value, Is.EqualTo(2.0).Within(1e-9));
        var restored = (SolidColorBrush)connector.EdgeBrush.Value;
        Assert.That(restored.Color, Is.EqualTo(Color.FromArgb(255, 10, 20, 30)));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsNodeがfalseなら選択してもコネクタはハイライトされない()
    {
        var (diagram, _) = CreateDiagram();

        var node = new NRectangleViewModel { ID = System.Guid.NewGuid() };
        node.Left.Value = 0;
        node.Top.Value = 0;
        node.Width.Value = 100;
        node.Height.Value = 100;
        node.IsNode.Value = false; // not a node
        diagram.AddItemCommand.Execute(node);

        var connector = new OrthogonalConnectorViewModel(diagram, new Point(0, 0));
        connector.AddPointP2(diagram, new Point(200, 200));
        connector.BeginAnchorRef.Value = $"{node.ID}#tl";
        connector.EdgeThickness.Value = 2.0;
        diagram.AddItemCommand.Execute(connector);

        diagram.Layers[0].Children.First().IsSelected.Value = true;

        Assert.That(connector.EdgeThickness.Value, Is.EqualTo(2.0),
            "IsNode=false なら無視されるべき");
    }
}
