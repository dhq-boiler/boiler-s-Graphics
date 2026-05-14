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

        // 選択前は IsHighlighted=false で派生プロパティも元値
        Assert.That(connector.IsHighlighted.Value, Is.False);
        Assert.That(connector.EffectiveEdgeThickness.Value, Is.EqualTo(2.0));

        // node を選択
        diagram.Layers[0].Children.First().IsSelected.Value = true;

        // Phase 3.5: 元の EdgeBrush / EdgeThickness は不変、描画値は EffectiveXxx で反映される
        Assert.That(connector.IsHighlighted.Value, Is.True);
        Assert.That(connector.EdgeThickness.Value, Is.EqualTo(2.0).Within(1e-9),
            "元の EdgeThickness は触らない");
        Assert.That(connector.EffectiveEdgeThickness.Value, Is.EqualTo(3.0).Within(1e-9),
            "EffectiveEdgeThickness は 1.5 倍");
        var origBrush = (SolidColorBrush)connector.EdgeBrush.Value;
        Assert.That(origBrush.Color, Is.EqualTo(Color.FromArgb(255, 10, 20, 30)),
            "元の EdgeBrush は触らない");
        var effective = (SolidColorBrush)connector.EffectiveEdgeBrush.Value;
        Assert.That(effective.Color, Is.EqualTo(Color.FromArgb(255, 245, 235, 225)),
            "EffectiveEdgeBrush は色反転");

        // node の選択解除で派生プロパティが元値に戻る (元プロパティは元々動いていない)
        diagram.Layers[0].Children.First().IsSelected.Value = false;
        Assert.That(connector.IsHighlighted.Value, Is.False);
        Assert.That(connector.EffectiveEdgeThickness.Value, Is.EqualTo(2.0).Within(1e-9));
        var restored = (SolidColorBrush)connector.EffectiveEdgeBrush.Value;
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

        Assert.That(connector.IsHighlighted.Value, Is.False, "IsNode=false なら無視されるべき");
        Assert.That(connector.EffectiveEdgeThickness.Value, Is.EqualTo(2.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Phase3_5_強調表示中のEdgeBrush手動編集はEffectiveEdgeBrushに即反映()
    {
        // Phase 3.5 (#4): 旧 stash 方式では強調表示中に EdgeBrush を編集しても、
        // 解除時に元値で上書きされていた。新 IsHighlighted フラグ方式では編集が即座に
        // EffectiveEdgeBrush に反映され、解除後も維持される。
        var (diagram, _) = CreateDiagram();

        var node = new NRectangleViewModel { ID = System.Guid.NewGuid() };
        node.Width.Value = 100;
        node.Height.Value = 100;
        node.IsNode.Value = true;
        diagram.AddItemCommand.Execute(node);

        var connector = new OrthogonalConnectorViewModel(diagram, new Point(0, 0));
        connector.AddPointP2(diagram, new Point(200, 200));
        connector.BeginAnchorRef.Value = $"{node.ID}#tl";
        connector.EdgeBrush.Value = new SolidColorBrush(Colors.Red); // RGB 255,0,0
        diagram.AddItemCommand.Execute(connector);

        // node 選択 → 強調表示開始
        diagram.Layers[0].Children.First().IsSelected.Value = true;
        var initialEff = (SolidColorBrush)connector.EffectiveEdgeBrush.Value;
        Assert.That(initialEff.Color, Is.EqualTo(Color.FromArgb(255, 0, 255, 255)), "Red の反転 = Cyan");

        // 強調表示中にユーザが EdgeBrush を緑に変更
        connector.EdgeBrush.Value = new SolidColorBrush(Colors.Green); // RGB 0,128,0
        var updatedEff = (SolidColorBrush)connector.EffectiveEdgeBrush.Value;
        Assert.That(updatedEff.Color, Is.EqualTo(Color.FromArgb(255, 255, 127, 255)),
            "新しい EdgeBrush=Green の反転が即座に反映される");

        // 選択解除 → 元の EdgeBrush=Green (旧 stash 方式では Red に戻ってしまっていた)
        diagram.Layers[0].Children.First().IsSelected.Value = false;
        var finalEff = (SolidColorBrush)connector.EffectiveEdgeBrush.Value;
        Assert.That(finalEff.Color, Is.EqualTo(Colors.Green),
            "解除後はユーザが変更した Green がそのまま残る");
    }
}
