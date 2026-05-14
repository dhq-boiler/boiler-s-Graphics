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
/// Phase 4-g-2: テーマ追従パーツ用に追加した PaletteSlotName / LineStyleName が
/// ExposedProperty のターゲットになれることを検証する。
/// </summary>
[TestFixture]
public class PartEditorPhase4g2ExposureTest
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
    public void PaletteSlotNameを公開できてString型()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("PaletteSlotName");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.String));
        Assert.That(ep.Bindings.FirstOrDefault()?.TargetProperty.Value, Is.EqualTo("PaletteSlotName"));
        Assert.That(editor.IsPaletteSlotNameExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void LineStyleNameを公開できてString型()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("LineStyleName");

        var ep = definition.ExposedProperties.FirstOrDefault();
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep.Type.Value, Is.EqualTo(ExposedPropertyType.String));
        Assert.That(editor.IsLineStyleNameExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void 両方を公開した状態()
    {
        var (editor, definition) = NewEditorWithDefinition();
        var rect = new NRectangleViewModel();
        editor.SelectItem(rect);

        editor.TogglePropertyExposureCommand.Execute("PaletteSlotName");
        editor.TogglePropertyExposureCommand.Execute("LineStyleName");

        Assert.That(definition.ExposedProperties.Count, Is.EqualTo(2));
        Assert.That(editor.IsPaletteSlotNameExposed.Value, Is.True);
        Assert.That(editor.IsLineStyleNameExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void デフォルト値は空文字()
    {
        var rect = new NRectangleViewModel();
        Assert.That(rect.PaletteSlotName.Value, Is.EqualTo(string.Empty));
        Assert.That(rect.LineStyleName.Value, Is.EqualTo(string.Empty));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void 値を設定できる()
    {
        var rect = new NRectangleViewModel();
        rect.PaletteSlotName.Value = "primary";
        rect.LineStyleName.Value = "Dash";
        Assert.That(rect.PaletteSlotName.Value, Is.EqualTo("primary"));
        Assert.That(rect.LineStyleName.Value, Is.EqualTo("Dash"));
    }
}
