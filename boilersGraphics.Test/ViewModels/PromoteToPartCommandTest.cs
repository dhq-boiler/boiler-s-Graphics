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
public class PromoteToPartCommandTest
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

    [Test, RequiresThread(ApartmentState.STA)]
    public void PromoteToPartCommand_選択無しではCanExecuteがfalse()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        Assert.That(vm.PromoteToPartCommand.CanExecute(), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PromoteToPartCommand_DesignerItem選択でCanExecuteがtrue()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var rect = new NRectangleViewModel(10, 20, 30, 40);
        vm.AddItemCommand.Execute(rect);
        rect.IsSelected.Value = true;

        Assert.That(vm.PromoteToPartCommand.CanExecute(), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PromoteToPartCommand_PartInstanceのみ選択ではCanExecuteがfalse()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var pi = new PartInstanceViewModel();
        vm.AddItemCommand.Execute(pi);
        pi.IsSelected.Value = true;

        Assert.That(vm.PromoteToPartCommand.CanExecute(), Is.False,
            "既存PartInstanceを更にパーツ化するのは禁止");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PromoteToPartCommand_ExecuteでPartDefinitionsに1件追加される()
    {
        var (vm, layer) = CreateSingleLayerViewModel();
        var rect = new NRectangleViewModel(10, 20, 30, 40);
        vm.AddItemCommand.Execute(rect);
        rect.IsSelected.Value = true;

        vm.PromoteToPartCommand.Execute();

        Assert.That(vm.PartDefinitions.Count, Is.EqualTo(1));
        Assert.That(vm.PartDefinitions[0].Name.Value, Is.EqualTo("パーツ1"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PromoteToPartCommand_実行でPartInstanceがレイヤーに追加される()
    {
        var (vm, layer) = CreateSingleLayerViewModel();
        var rect = new NRectangleViewModel(10, 20, 30, 40);
        vm.AddItemCommand.Execute(rect);
        rect.IsSelected.Value = true;

        vm.PromoteToPartCommand.Execute();

        var hasInstance = layer.Children
            .OfType<LayerItem>()
            .Any(li => li.Item.Value is PartInstanceViewModel);
        Assert.That(hasInstance, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PromoteToPartCommand_連番でパーツ名が振られる()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var r1 = new NRectangleViewModel(0, 0, 10, 10);
        vm.AddItemCommand.Execute(r1);
        r1.IsSelected.Value = true;
        vm.PromoteToPartCommand.Execute();

        var r2 = new NRectangleViewModel(0, 0, 10, 10);
        vm.AddItemCommand.Execute(r2);
        r2.IsSelected.Value = true;
        vm.PromoteToPartCommand.Execute();

        Assert.That(vm.PartDefinitions.Count, Is.EqualTo(2));
        Assert.That(vm.PartDefinitions[0].Name.Value, Is.EqualTo("パーツ1"));
        Assert.That(vm.PartDefinitions[1].Name.Value, Is.EqualTo("パーツ2"));
    }
}
