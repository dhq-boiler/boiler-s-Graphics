using boilersGraphics.Models;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Threading;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class PartDefinitionsRegistryTest
{
    private static DiagramViewModel CreateViewModel()
    {
        boilersGraphics.App.IsTest = true;
        var dlgService = new Mock<IDialogService>();
        var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
        var vm = new DiagramViewModel(mainWindowViewModel);
        vm.Layers.Clear();

        var layer = new Layer();
        layer.Name.Value = "Layer1";
        vm.Layers.Add(layer);
        layer.IsSelected.Value = true;
        return vm;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PartDefinitionsById_初期値は空()
    {
        var vm = CreateViewModel();
        Assert.That(vm.PartDefinitionsById, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PartDefinitions_Add_PartDefinitionsByIdに反映される()
    {
        var vm = CreateViewModel();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "A" });

        vm.PartDefinitions.Add(def);

        Assert.That(vm.PartDefinitionsById.ContainsKey(def.Id.Value), Is.True);
        Assert.That(vm.PartDefinitionsById[def.Id.Value], Is.SameAs(def));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PartDefinitions_Remove_PartDefinitionsByIdから消える()
    {
        var vm = CreateViewModel();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "A" });
        vm.PartDefinitions.Add(def);

        vm.PartDefinitions.Remove(def);

        Assert.That(vm.PartDefinitionsById.ContainsKey(def.Id.Value), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PartDefinitions_Clear_PartDefinitionsByIdが空になる()
    {
        var vm = CreateViewModel();
        vm.PartDefinitions.Add(new PartDefinitionViewModel(new PartDefinition { Name = "A" }));
        vm.PartDefinitions.Add(new PartDefinitionViewModel(new PartDefinition { Name = "B" }));

        vm.PartDefinitions.Clear();

        Assert.That(vm.PartDefinitionsById, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGetPartDefinition_登録済みIdでtrue_未登録Idでfalse()
    {
        var vm = CreateViewModel();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "A" });
        vm.PartDefinitions.Add(def);

        var foundResult = vm.TryGetPartDefinition(def.Id.Value, out var found);
        var missingResult = vm.TryGetPartDefinition(Guid.NewGuid(), out var missing);

        Assert.That(foundResult, Is.True);
        Assert.That(found, Is.SameAs(def));
        Assert.That(missingResult, Is.False);
        Assert.That(missing, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ValidatePartReferences_すべてのPartInstanceが登録済みならHasOrphansはfalse()
    {
        var vm = CreateViewModel();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "A" });
        vm.PartDefinitions.Add(def);

        var instance = new PartInstanceViewModel(def.Id.Value);
        vm.AddItemCommand.Execute(instance);

        var result = vm.ValidatePartReferences();

        Assert.That(result.HasOrphans, Is.False);
        Assert.That(result.OrphanedInstances, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ValidatePartReferences_未登録Definitionを参照するPartInstanceは孤児として検出される()
    {
        var vm = CreateViewModel();
        var orphan = new PartInstanceViewModel(Guid.NewGuid());
        vm.AddItemCommand.Execute(orphan);

        var result = vm.ValidatePartReferences();

        Assert.That(result.HasOrphans, Is.True);
        Assert.That(result.OrphanedInstances, Has.Count.EqualTo(1));
        Assert.That(result.OrphanedInstances[0], Is.SameAs(orphan));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PromoteToPart_Undo_PartDefinitionsが空に戻る()
    {
        var vm = CreateViewModel();
        var r = new NRectangleViewModel(10, 20, 30, 40);
        vm.AddItemCommand.Execute(r);
        r.IsSelected.Value = true;

        vm.PromoteToPartCommand.Execute();
        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(1), "Promote 直後は PartDefinitions に 1 件");

        vm.UndoCommand.Execute();

        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(0), "Undo で PartDefinitions が空に戻る (孤児定義が残らない)");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ClonePartDefinition_Undo_PartDefinitionsが1件に戻る()
    {
        var vm = CreateViewModel();
        var r = new NRectangleViewModel(10, 20, 30, 40);
        vm.AddItemCommand.Execute(r);
        r.IsSelected.Value = true;
        vm.PromoteToPartCommand.Execute();

        // Promote 後の唯一の PartInstance を選択 (Clone は PartInstance 選択前提)
        var instance = System.Linq.Enumerable.First(
            System.Linq.Enumerable.OfType<PartInstanceViewModel>(
                System.Linq.Enumerable.Select(
                    System.Linq.Enumerable.SelectMany(vm.Layers, l => l.Children),
                    c => ((LayerItem)c).Item.Value)));
        instance.IsSelected.Value = true;

        var beforeCloneCount = vm.PartDefinitions.Count; // 1
        vm.ClonePartDefinitionCommand.Execute();
        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(beforeCloneCount + 1), "Clone で PartDefinitions が 1 件増える");

        vm.UndoCommand.Execute();

        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(beforeCloneCount), "Undo で Clone 分が消える (孤児定義が残らない)");
    }
}
