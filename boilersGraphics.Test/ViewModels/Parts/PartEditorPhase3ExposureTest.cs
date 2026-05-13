using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using boilersGraphics.ViewModels.Connectors;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;
using System.Windows;

namespace boilersGraphics.Test.ViewModels.Parts;

/// <summary>
/// Phase 3-h §5.7 / Q-9: Phase 3 で追加された公開可能プロパティ
/// (BeginPoint / EndPoint / BeginControlPoint / EndControlPoint / CornerRadius / RelativeX / RelativeY / IsNode)
/// が ExposedProperty のターゲットになれることを検証する。
/// </summary>
[TestFixture]
public class PartEditorPhase3ExposureTest
{
    private static (PartEditorViewModel editor, PartDefinitionViewModel definition) NewEditorWithDefinition()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Mock<IDialogService>();
        var editor = new PartEditorViewModel(dlg.Object);
        var definition = new PartDefinitionViewModel(new PartDefinition { Name = "test" });
        editor.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, definition },
        });
        return (editor, definition);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OrthogonalConnectorのCornerRadiusを公開できる()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var connector = new OrthogonalConnectorViewModel();
        connector.AddPoints(null, new Point(0, 0), new Point(100, 100));
        editor.SelectItem(connector);

        editor.TogglePropertyExposureCommand.Execute("CornerRadius");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(ep.Bindings.FirstOrDefault()?.TargetProperty.Value, Is.EqualTo("CornerRadius"));
        Assert.That(editor.IsCornerRadiusExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OrthogonalConnectorのBeginPointを公開できてPoint型()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var connector = new OrthogonalConnectorViewModel();
        connector.AddPoints(null, new Point(10, 20), new Point(100, 100));
        editor.SelectItem(connector);

        editor.TogglePropertyExposureCommand.Execute("BeginPoint");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.Point));
        Assert.That(editor.IsBeginPointExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AnchorBezierのControlPointを公開できる()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var connector = new AnchorBezierConnectorViewModel();
        connector.AddPoints(null, new Point(0, 0), new Point(100, 100));
        editor.SelectItem(connector);

        editor.TogglePropertyExposureCommand.Execute("BeginControlPoint");
        editor.TogglePropertyExposureCommand.Execute("EndControlPoint");

        Assert.That(definition.ExposedProperties.Count, Is.EqualTo(2));
        Assert.That(definition.ExposedProperties.All(ep => ep.Type.Value == ExposedPropertyType.Point), Is.True);
        Assert.That(editor.IsBeginControlPointExposed.Value, Is.True);
        Assert.That(editor.IsEndControlPointExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AnchorViewModelのRelativeXYを公開できる()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var anchor = new AnchorViewModel();
        editor.SelectItem(anchor);

        editor.TogglePropertyExposureCommand.Execute("RelativeX");
        editor.TogglePropertyExposureCommand.Execute("RelativeY");

        Assert.That(definition.ExposedProperties.Count, Is.EqualTo(2));
        Assert.That(definition.ExposedProperties.All(ep => ep.Type.Value == ExposedPropertyType.Double), Is.True);
        Assert.That(editor.IsRelativeXExposed.Value, Is.True);
        Assert.That(editor.IsRelativeYExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DesignerItemのIsNodeを公開できてBool型()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("IsNode");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.Boolean));
        Assert.That(editor.IsIsNodeExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void 持たないプロパティのトグルはno_op_ExposedPropertyが追加されない()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        // Rectangle に CornerRadius プロパティはあるが (NRectangle.RadiusX/Y じゃなくて CornerRadius は OrthogonalConnector 固有)
        // ここで対象を確認: NRectangle に CornerRadius プロパティはない (RadiusX/Y のみ)
        editor.TogglePropertyExposureCommand.Execute("CornerRadius");

        Assert.That(definition.ExposedProperties.Count, Is.EqualTo(0),
            "持たないプロパティのトグルでは ExposedProperty を作らない");
        Assert.That(editor.IsCornerRadiusExposed.Value, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OrthogonalConnectorのBeginPointProxy_Points0と双方向同期()
    {
        var connector = new OrthogonalConnectorViewModel();
        connector.AddPoints(null, new Point(0, 0), new Point(100, 100));

        // Points -> BeginPoint
        Assert.That(connector.BeginPoint.Value, Is.EqualTo(new Point(0, 0)));

        // BeginPoint -> Points
        connector.BeginPoint.Value = new Point(50, 60);
        Assert.That(connector.Points[0], Is.EqualTo(new Point(50, 60)));

        // Points -> BeginPoint
        connector.Points[0] = new Point(99, 88);
        Assert.That(connector.BeginPoint.Value, Is.EqualTo(new Point(99, 88)));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AnchorBezierのEndPointProxy_Points1と双方向同期()
    {
        var connector = new AnchorBezierConnectorViewModel();
        connector.AddPoints(null, new Point(0, 0), new Point(100, 100));

        Assert.That(connector.EndPoint.Value, Is.EqualTo(new Point(100, 100)));

        connector.EndPoint.Value = new Point(200, 200);
        Assert.That(connector.Points[1], Is.EqualTo(new Point(200, 200)));
    }
}
