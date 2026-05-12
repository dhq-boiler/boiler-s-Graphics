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
public class ClonePartDefinitionCommandTest
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

    private static PartInstanceViewModel CreatePromoted(DiagramViewModel vm, params NRectangleViewModel[] items)
    {
        foreach (var item in items)
        {
            vm.AddItemCommand.Execute(item);
            item.IsSelected.Value = true;
        }
        vm.PromoteToPartCommand.Execute();
        return vm.Layers
            .SelectMany(l => l.Children)
            .OfType<LayerItem>()
            .Select(li => li.Item.Value)
            .OfType<PartInstanceViewModel>()
            .First();
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinitionCommand_PartInstance未選択ではCanExecuteがfalse()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        Assert.That(vm.ClonePartDefinitionCommand.CanExecute(), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinitionCommand_PartInstance選択でCanExecuteがtrue()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(10, 20, 30, 40));
        instance.IsSelected.Value = true;

        Assert.That(vm.ClonePartDefinitionCommand.CanExecute(), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinitionCommand_実行でPartDefinitionsに新規追加される()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(0, 0, 10, 10));
        instance.IsSelected.Value = true;
        var initialCount = vm.PartDefinitions.Count;

        vm.ClonePartDefinitionCommand.Execute();

        Assert.That(vm.PartDefinitions.Count, Is.EqualTo(initialCount + 1));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinitionCommand_新規定義名は_のコピー_になる()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(0, 0, 10, 10));
        instance.IsSelected.Value = true;

        vm.ClonePartDefinitionCommand.Execute();

        var clone = vm.PartDefinitions.Last();
        Assert.That(clone.Name.Value, Is.EqualTo("パーツ1のコピー"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinitionCommand_2回実行で連番が振られる()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(0, 0, 10, 10));
        instance.IsSelected.Value = true;

        vm.ClonePartDefinitionCommand.Execute();
        vm.ClonePartDefinitionCommand.Execute();

        var names = vm.PartDefinitions.Select(d => d.Name.Value).ToArray();
        Assert.That(names, Does.Contain("パーツ1のコピー"));
        Assert.That(names, Does.Contain("パーツ1のコピー2"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinitionCommand_新Idは元と異なる()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(0, 0, 10, 10));
        instance.IsSelected.Value = true;
        var originalId = vm.PartDefinitions[0].Id.Value;

        vm.ClonePartDefinitionCommand.Execute();

        var cloneId = vm.PartDefinitions.Last().Id.Value;
        Assert.That(cloneId, Is.Not.EqualTo(originalId));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinitionCommand_実行後に新クローンでPartEditorを開く要求が出る()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(0, 0, 10, 10));
        instance.IsSelected.Value = true;

        vm.ClonePartDefinitionCommand.Execute();

        var clone = vm.PartDefinitions.Last();
        Assert.That(vm.LastRequestedEditorTarget, Is.SameAs(clone));
    }
}
