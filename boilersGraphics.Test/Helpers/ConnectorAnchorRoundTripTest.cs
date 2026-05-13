using boilersGraphics.Helpers;
using boilersGraphics.Models.Connectors;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using boilersGraphics.ViewModels.Connectors;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace boilersGraphics.Test.Helpers;

/// <summary>
/// Phase 3-f §6.2: ObjectSerializer.ExtractItem → ObjectDeserializer.Extract* のラウンドトリップで、
/// 新規コネクタ (OrthogonalConnector / AnchorBezierConnector) と AnchorViewModel、
/// DesignerItem.IsNode フラグが完全に復元できることを確認する。
/// </summary>
[TestFixture]
public class ConnectorAnchorRoundTripTest
{
    private static DiagramViewModel _diagram;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Mock<IDialogService>();
        _diagram = new MainWindowViewModel(dlg.Object).DiagramViewModel;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OrthogonalConnector_RoutingMode_CornerRadius_MidPoints_完全ラウンドトリップ()
    {
        var src = new OrthogonalConnectorViewModel(_diagram, new Point(10, 20));
        src.AddPointP2(_diagram, new Point(110, 80));
        src.RoutingMode.Value = OrthogonalRoutingMode.HFirst;
        src.CornerRadius.Value = 5.5;
        src.MidPoints.Add(new Point(60, 20));
        src.MidPoints.Add(new Point(60, 80));
        src.EdgeThickness.Value = 2.0;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (OrthogonalConnectorViewModel)ObjectDeserializer.ExtractConnectorBaseViewModel(_diagram, xml);

        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.Points[0], Is.EqualTo(new Point(10, 20)));
        Assert.That(dst.Points[1], Is.EqualTo(new Point(110, 80)));
        Assert.That(dst.RoutingMode.Value, Is.EqualTo(OrthogonalRoutingMode.HFirst));
        Assert.That(dst.CornerRadius.Value, Is.EqualTo(5.5));
        Assert.That(dst.MidPoints.Count, Is.EqualTo(2));
        Assert.That(dst.MidPoints[0], Is.EqualTo(new Point(60, 20)));
        Assert.That(dst.MidPoints[1], Is.EqualTo(new Point(60, 80)));
        Assert.That(dst.EdgeThickness.Value, Is.EqualTo(2.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OrthogonalConnector_AnchorRef_復元()
    {
        var src = new OrthogonalConnectorViewModel(_diagram, new Point(0, 0));
        src.AddPointP2(_diagram, new Point(100, 100));
        var ownerGuid = Guid.NewGuid();
        var anchorGuid = Guid.NewGuid();
        src.BeginAnchorRef.Value = $"{ownerGuid}#tl";
        src.EndAnchorRef.Value = anchorGuid.ToString();

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (OrthogonalConnectorViewModel)ObjectDeserializer.ExtractConnectorBaseViewModel(_diagram, xml);

        Assert.That(dst.BeginAnchorRef.Value, Is.EqualTo($"{ownerGuid}#tl"));
        Assert.That(dst.EndAnchorRef.Value, Is.EqualTo(anchorGuid.ToString()));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OrthogonalConnector_AnchorRef未設定_復元時も空文字()
    {
        var src = new OrthogonalConnectorViewModel(_diagram, new Point(0, 0));
        src.AddPointP2(_diagram, new Point(100, 100));

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (OrthogonalConnectorViewModel)ObjectDeserializer.ExtractConnectorBaseViewModel(_diagram, xml);

        Assert.That(dst.BeginAnchorRef.Value, Is.EqualTo(string.Empty));
        Assert.That(dst.EndAnchorRef.Value, Is.EqualTo(string.Empty));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AnchorBezierConnector_制御点_完全ラウンドトリップ()
    {
        var src = new AnchorBezierConnectorViewModel(_diagram, new Point(0, 0));
        src.AddPointP2(_diagram, new Point(200, 100));
        src.BeginControlPoint.Value = new Point(50, 0);
        src.EndControlPoint.Value = new Point(150, 100);
        src.EdgeThickness.Value = 3.0;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (AnchorBezierConnectorViewModel)ObjectDeserializer.ExtractConnectorBaseViewModel(_diagram, xml);

        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.Points[0], Is.EqualTo(new Point(0, 0)));
        Assert.That(dst.Points[1], Is.EqualTo(new Point(200, 100)));
        Assert.That(dst.BeginControlPoint.Value, Is.EqualTo(new Point(50, 0)));
        Assert.That(dst.EndControlPoint.Value, Is.EqualTo(new Point(150, 100)));
        Assert.That(dst.EdgeThickness.Value, Is.EqualTo(3.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AnchorBezierConnector_AnchorRef_復元()
    {
        var src = new AnchorBezierConnectorViewModel(_diagram, new Point(0, 0));
        src.AddPointP2(_diagram, new Point(100, 100));
        var ownerGuid = Guid.NewGuid();
        src.BeginAnchorRef.Value = $"{ownerGuid}#c";
        src.EndAnchorRef.Value = $"{ownerGuid}#br";

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (AnchorBezierConnectorViewModel)ObjectDeserializer.ExtractConnectorBaseViewModel(_diagram, xml);

        Assert.That(dst.BeginAnchorRef.Value, Is.EqualTo($"{ownerGuid}#c"));
        Assert.That(dst.EndAnchorRef.Value, Is.EqualTo($"{ownerGuid}#br"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Anchor_主要プロパティ_完全ラウンドトリップ()
    {
        var ownerId = Guid.NewGuid();
        var src = new AnchorViewModel
        {
            Owner = _diagram,
            ID = Guid.NewGuid(),
            ParentID = Guid.NewGuid(),
        };
        src.OwnerId.Value = ownerId;
        src.RelativeX.Value = 0.25;
        src.RelativeY.Value = 0.75;
        src.AnchorName.Value = "top-left-quarter";
        src.ZIndex.Value = 5;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = ObjectDeserializer.ExtractAnchorViewModel(_diagram, xml);

        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.ID, Is.EqualTo(src.ID));
        Assert.That(dst.ParentID, Is.EqualTo(src.ParentID));
        Assert.That(dst.OwnerId.Value, Is.EqualTo(ownerId));
        Assert.That(dst.RelativeX.Value, Is.EqualTo(0.25));
        Assert.That(dst.RelativeY.Value, Is.EqualTo(0.75));
        Assert.That(dst.AnchorName.Value, Is.EqualTo("top-left-quarter"));
        Assert.That(dst.ZIndex.Value, Is.EqualTo(5));
        Assert.That(dst.Owner, Is.SameAs(_diagram));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Anchor_名前未設定_AnchorName要素が出力されない()
    {
        var src = new AnchorViewModel
        {
            Owner = _diagram,
        };
        src.OwnerId.Value = Guid.NewGuid();
        src.RelativeX.Value = 0.5;
        src.RelativeY.Value = 0.5;
        // AnchorName.Value は string.Empty (default)

        var xml = ObjectSerializer.ExtractItem(src);

        Assert.That(xml.Element("AnchorName"), Is.Null,
            "空文字の場合は AnchorName 要素を省略する (古いファイル互換)");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsNode_trueなDesignerItem_ラウンドトリップで復元()
    {
        var src = new NRectangleViewModel();
        src.Left.Value = 0;
        src.Top.Value = 0;
        src.Width.Value = 100;
        src.Height.Value = 100;
        src.IsNode.Value = true;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (NRectangleViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml);

        Assert.That(dst.IsNode.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsNode_falseなDesignerItem_要素が出力されずfalseで復元()
    {
        var src = new NRectangleViewModel();
        src.Left.Value = 0;
        src.Top.Value = 0;
        src.Width.Value = 100;
        src.Height.Value = 100;
        // IsNode.Value は false (default)

        var xml = ObjectSerializer.ExtractItem(src);

        Assert.That(xml.Element("IsNode"), Is.Null,
            "false の場合は IsNode 要素を省略する (古いファイル互換)");

        var dst = (NRectangleViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml);
        Assert.That(dst.IsNode.Value, Is.False);
    }
}
