using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Parts;

/// <summary>
/// Phase 4-g / Q-11 案 A: Phase 4 で追加された Glow 系公開可能プロパティ
/// (GlowRadius / GlowIntensity / GlowColor) が ExposedProperty のターゲットになれることを検証する。
/// </summary>
[TestFixture]
public class PartEditorPhase4ExposureTest
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
    public void GlowRadiusを公開できてDouble型()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("GlowRadius");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(ep.Bindings.FirstOrDefault()?.TargetProperty.Value, Is.EqualTo("GlowRadius"));
        Assert.That(editor.IsGlowRadiusExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GlowIntensityを公開できてDouble型()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("GlowIntensity");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(editor.IsGlowIntensityExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GlowColorを公開できてColor型()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("GlowColor");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.Color));
        Assert.That(ep.Bindings.FirstOrDefault()?.TargetProperty.Value, Is.EqualTo("GlowColor"));
        Assert.That(editor.IsGlowColorExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Glow3つを全部公開できる()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("GlowRadius");
        editor.TogglePropertyExposureCommand.Execute("GlowIntensity");
        editor.TogglePropertyExposureCommand.Execute("GlowColor");

        Assert.That(definition.ExposedProperties.Count, Is.EqualTo(3));
        Assert.That(editor.IsGlowRadiusExposed.Value, Is.True);
        Assert.That(editor.IsGlowIntensityExposed.Value, Is.True);
        Assert.That(editor.IsGlowColorExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Glow公開後の再Toggleで解除()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("GlowRadius");
        Assert.That(editor.IsGlowRadiusExposed.Value, Is.True);

        editor.TogglePropertyExposureCommand.Execute("GlowRadius");
        Assert.That(editor.IsGlowRadiusExposed.Value, Is.False);
        Assert.That(definition.ExposedProperties.Count, Is.EqualTo(0));
    }
}
