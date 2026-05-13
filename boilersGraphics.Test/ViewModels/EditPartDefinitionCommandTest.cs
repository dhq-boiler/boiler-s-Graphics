using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class EditPartDefinitionCommandTest
{
    private static (DiagramViewModel viewModel, Layer layer) CreateSingleLayerViewModel()
    {
        boilersGraphics.App.IsTest = true;
        var dlgService = new Mock<IDialogService>();
        var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
        var viewModel = new DiagramViewModel(mainWindowViewModel);
        viewModel.Layers.Clear();

        var layer = new Layer();
        layer.Name.Value = "Layer1";
        viewModel.Layers.Add(layer);
        layer.IsSelected.Value = true;
        return (viewModel, layer);
    }

    private static PartInstanceViewModel CreatePromoted(DiagramViewModel vm, NRectangleViewModel rect)
    {
        vm.AddItemCommand.Execute(rect);
        rect.IsSelected.Value = true;
        vm.PromoteToPartCommand.Execute();
        return vm.Layers
            .SelectMany(l => l.Children)
            .OfType<LayerItem>()
            .Select(li => li.Item.Value)
            .OfType<PartInstanceViewModel>()
            .First();
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void EditPartDefinitionCommand_PartInstance未選択ではCanExecuteがfalse()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        Assert.That(vm.EditPartDefinitionCommand.CanExecute(), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void EditPartDefinitionCommand_PartInstance選択でCanExecuteがtrue()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(10, 20, 30, 40));
        instance.IsSelected.Value = true;

        Assert.That(vm.EditPartDefinitionCommand.CanExecute(), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void EditPartDefinitionCommand_DesignerItemのみ選択ではCanExecuteがfalse()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var rect = new NRectangleViewModel(10, 20, 30, 40);
        vm.AddItemCommand.Execute(rect);
        rect.IsSelected.Value = true;

        Assert.That(vm.EditPartDefinitionCommand.CanExecute(), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void EditPartDefinitionCommand_ExecuteでOpenPartEditorが選択中Instanceの定義で呼ばれる()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(10, 20, 30, 40));
        instance.IsSelected.Value = true;
        var expectedDefinition = vm.PartDefinitions[0];

        vm.EditPartDefinitionCommand.Execute();

        Assert.That(vm.LastRequestedEditorTarget, Is.SameAs(expectedDefinition));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void EditPartDefinitionCommand_対応する定義がない場合は何もしない()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var orphan = new PartInstanceViewModel(System.Guid.NewGuid());
        vm.AddItemCommand.Execute(orphan);
        orphan.IsSelected.Value = true;

        Assert.DoesNotThrow(() => vm.EditPartDefinitionCommand.Execute());
        Assert.That(vm.LastRequestedEditorTarget, Is.Null);
    }
}
