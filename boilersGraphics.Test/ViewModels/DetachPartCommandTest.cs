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
public class DetachPartCommandTest
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
    public void DetachPartCommand_PartInstance未選択ではCanExecuteがfalse()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        Assert.That(vm.DetachPartCommand.CanExecute(), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DetachPartCommand_PartInstance選択でCanExecuteがtrue()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(10, 20, 30, 40));
        instance.IsSelected.Value = true;

        Assert.That(vm.DetachPartCommand.CanExecute(), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DetachPartCommand_DesignerItemのみ選択ではCanExecuteがfalse()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var rect = new NRectangleViewModel(0, 0, 10, 10);
        vm.AddItemCommand.Execute(rect);
        rect.IsSelected.Value = true;

        Assert.That(vm.DetachPartCommand.CanExecute(), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DetachPartCommand_実行でPartInstanceがLayerから消えて内部図形が展開される()
    {
        var (vm, layer) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm,
            new NRectangleViewModel(10, 20, 30, 40),
            new NRectangleViewModel(50, 60, 30, 40));
        instance.IsSelected.Value = true;

        vm.DetachPartCommand.Execute();

        var instances = layer.Children
            .OfType<LayerItem>()
            .Select(li => li.Item.Value)
            .OfType<PartInstanceViewModel>()
            .ToArray();
        Assert.That(instances, Is.Empty, "PartInstance が Layer から消える");

        var rects = layer.Children
            .OfType<LayerItem>()
            .Select(li => li.Item.Value)
            .OfType<NRectangleViewModel>()
            .ToArray();
        Assert.That(rects.Length, Is.EqualTo(2), "内部図形 2 件が Layer に展開される");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DetachPartCommand_実行後もPartDefinitionsには残る()
    {
        var (vm, _) = CreateSingleLayerViewModel();
        var instance = CreatePromoted(vm, new NRectangleViewModel(0, 0, 10, 10));
        instance.IsSelected.Value = true;

        vm.DetachPartCommand.Execute();

        Assert.That(vm.PartDefinitions.Count, Is.EqualTo(1),
            "Detach はあくまでインスタンス単位の切り離し、定義そのものは保持");
    }
}
