using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using R3;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class PartInstanceMouseDoubleClickTest
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
    public void MouseDoubleClickCommand_PartInstanceDoubleClickでOpenPartEditorが呼ばれる()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(10, 20, 30, 40));
        var expectedDefinition = vm.PartDefinitions[0];

        instance.MouseDoubleClickCommand.Execute(Unit.Default);

        Assert.That(vm.LastRequestedEditorTarget, Is.SameAs(expectedDefinition));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void MouseDoubleClickCommand_対応する定義がない場合は何もしない()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var orphan = new PartInstanceViewModel(System.Guid.NewGuid());
        vm.AddItemCommand.Execute(orphan);

        Assert.DoesNotThrow(() => orphan.MouseDoubleClickCommand.Execute(Unit.Default));
        Assert.That(vm.LastRequestedEditorTarget, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void MouseDoubleClickCommand_Ownerが未設定の場合は例外なし()
    {
        var pi = new PartInstanceViewModel(System.Guid.NewGuid());

        Assert.DoesNotThrow(() => pi.MouseDoubleClickCommand.Execute(Unit.Default));
    }
}
