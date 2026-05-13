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
public class UnusedPartDefinitionCleanupTest
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
    public void GetPartInstanceReferenceCount_インスタンスがあれば1以上_なければ0()
    {
        var vm = CreateViewModel();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "A" });
        vm.PartDefinitions.Add(def);

        Assert.That(vm.GetPartInstanceReferenceCount(def.Id.Value), Is.EqualTo(0));

        var instance = new PartInstanceViewModel(def.Id.Value);
        vm.AddItemCommand.Execute(instance);

        Assert.That(vm.GetPartInstanceReferenceCount(def.Id.Value), Is.EqualTo(1));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GetPartInstanceReferenceCount_複数Instanceで件数が増える()
    {
        var vm = CreateViewModel();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "A" });
        vm.PartDefinitions.Add(def);

        for (var i = 0; i < 3; i++)
            vm.AddItemCommand.Execute(new PartInstanceViewModel(def.Id.Value));

        Assert.That(vm.GetPartInstanceReferenceCount(def.Id.Value), Is.EqualTo(3));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GetUnusedPartDefinitions_全て使用中なら空()
    {
        var vm = CreateViewModel();
        var defA = new PartDefinitionViewModel(new PartDefinition { Name = "A" });
        var defB = new PartDefinitionViewModel(new PartDefinition { Name = "B" });
        vm.PartDefinitions.Add(defA);
        vm.PartDefinitions.Add(defB);
        vm.AddItemCommand.Execute(new PartInstanceViewModel(defA.Id.Value));
        vm.AddItemCommand.Execute(new PartInstanceViewModel(defB.Id.Value));

        var unused = vm.GetUnusedPartDefinitions();

        Assert.That(unused, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GetUnusedPartDefinitions_未参照のDefinitionが検出される()
    {
        var vm = CreateViewModel();
        var defUsed = new PartDefinitionViewModel(new PartDefinition { Name = "Used" });
        var defOrphan = new PartDefinitionViewModel(new PartDefinition { Name = "Orphan" });
        vm.PartDefinitions.Add(defUsed);
        vm.PartDefinitions.Add(defOrphan);
        vm.AddItemCommand.Execute(new PartInstanceViewModel(defUsed.Id.Value));

        var unused = vm.GetUnusedPartDefinitions();

        Assert.That(unused, Has.Count.EqualTo(1));
        Assert.That(unused[0], Is.SameAs(defOrphan));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveUnusedPartDefinitions_未参照のみ消えて参照中は残る()
    {
        var vm = CreateViewModel();
        var defUsed = new PartDefinitionViewModel(new PartDefinition { Name = "Used" });
        var defOrphan = new PartDefinitionViewModel(new PartDefinition { Name = "Orphan" });
        vm.PartDefinitions.Add(defUsed);
        vm.PartDefinitions.Add(defOrphan);
        vm.AddItemCommand.Execute(new PartInstanceViewModel(defUsed.Id.Value));

        var removed = vm.RemoveUnusedPartDefinitions();

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(1));
        Assert.That(vm.PartDefinitions[0], Is.SameAs(defUsed));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveUnusedPartDefinitions_未参照がなければ0件()
    {
        var vm = CreateViewModel();
        var defUsed = new PartDefinitionViewModel(new PartDefinition { Name = "Used" });
        vm.PartDefinitions.Add(defUsed);
        vm.AddItemCommand.Execute(new PartInstanceViewModel(defUsed.Id.Value));

        Assert.That(vm.RemoveUnusedPartDefinitions(), Is.EqualTo(0));
        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(1));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveUnusedPartDefinitions_Undo_削除されたDefinitionが復元される()
    {
        var vm = CreateViewModel();
        var defOrphan1 = new PartDefinitionViewModel(new PartDefinition { Name = "O1" });
        var defOrphan2 = new PartDefinitionViewModel(new PartDefinition { Name = "O2" });
        vm.PartDefinitions.Add(defOrphan1);
        vm.PartDefinitions.Add(defOrphan2);

        vm.RemoveUnusedPartDefinitions();
        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(0));

        vm.UndoCommand.Execute();

        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(2),
            "1 トランザクションでまとめて削除しているので、Undo 1 回で全件復元される");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveUnusedPartDefinitionsCommand_App_IsTest_確認スキップして実行される()
    {
        var vm = CreateViewModel();
        var defOrphan = new PartDefinitionViewModel(new PartDefinition { Name = "O" });
        vm.PartDefinitions.Add(defOrphan);

        vm.RemoveUnusedPartDefinitionsCommand.Execute();

        Assert.That(vm.LastUnusedRemovalCount, Is.EqualTo(1));
        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveUnusedPartDefinitionsCommand_未参照0件_LastUnusedRemovalCountは0()
    {
        var vm = CreateViewModel();
        var defUsed = new PartDefinitionViewModel(new PartDefinition { Name = "Used" });
        vm.PartDefinitions.Add(defUsed);
        vm.AddItemCommand.Execute(new PartInstanceViewModel(defUsed.Id.Value));

        vm.RemoveUnusedPartDefinitionsCommand.Execute();

        Assert.That(vm.LastUnusedRemovalCount, Is.EqualTo(0));
        Assert.That(vm.PartDefinitions, Has.Count.EqualTo(1));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GetPartInstanceReferenceCount_存在しないId_0が返る()
    {
        var vm = CreateViewModel();

        Assert.That(vm.GetPartInstanceReferenceCount(Guid.NewGuid()), Is.EqualTo(0));
    }
}
