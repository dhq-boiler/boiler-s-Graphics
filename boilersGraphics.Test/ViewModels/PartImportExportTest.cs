using boilersGraphics.Helpers.Parts;
using boilersGraphics.Models;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.IO;
using System.Threading;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class PartImportExportTest
{
    private static (DiagramViewModel vm, Layer layer) CreateViewModelWithLayer()
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
        return (vm, layer);
    }

    private static PartInstanceViewModel PromoteRectangle(DiagramViewModel vm)
    {
        var rect = new NRectangleViewModel(10, 20, 30, 40);
        vm.AddItemCommand.Execute(rect);
        rect.IsSelected.Value = true;
        vm.PromoteToPartCommand.Execute();

        foreach (var layer in vm.Layers)
            foreach (var child in layer.Children)
                if (child is LayerItem li && li.Item.Value is PartInstanceViewModel pi)
                    return pi;
        throw new InvalidOperationException("Promote did not produce a PartInstance.");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SerializePartFileFromViewModel_BoilersGraphicsPartルートを返す()
    {
        var (vm, _) = CreateViewModelWithLayer();
        PromoteRectangle(vm);
        var def = vm.PartDefinitions[0];

        var xml = PartSerializer.SerializePartFileFromViewModel(def);

        Assert.That(xml.Name.LocalName, Is.EqualTo(PartSerializer.PartFileRoot));
        Assert.That(xml.Attribute("Version")?.Value, Is.EqualTo(PartSerializer.PartFileVersion));
        Assert.That(xml.Element("PartDefinition"), Is.Not.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SerializePartFileFromViewModel_Itemsが書き出される()
    {
        var (vm, _) = CreateViewModelWithLayer();
        PromoteRectangle(vm);
        var def = vm.PartDefinitions[0];

        var xml = PartSerializer.SerializePartFileFromViewModel(def);

        var items = xml.Element("PartDefinition")?.Element("Items");
        Assert.That(items, Is.Not.Null);
        Assert.That(System.Linq.Enumerable.Count(items.Elements("DesignerItem")), Is.GreaterThan(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_FileWritten_LastPartFilePathが設定される()
    {
        var (vm, _) = CreateViewModelWithLayer();
        var instance = PromoteRectangle(vm);
        instance.IsSelected.Value = true;

        var tempPath = Path.Combine(Path.GetTempPath(),
            $"part-export-{Guid.NewGuid():N}.bgpart");
        vm.LastPartFilePath = tempPath;

        try
        {
            vm.ExportPartCommand.Execute();

            Assert.That(File.Exists(tempPath), Is.True);
            Assert.That(vm.LastPartFilePath, Is.EqualTo(tempPath));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ExportImportRoundtrip_PartDefinitionsが1つ増える()
    {
        var (vm, _) = CreateViewModelWithLayer();
        var instance = PromoteRectangle(vm);
        instance.IsSelected.Value = true;
        var beforeImport = vm.PartDefinitions.Count; // 1

        var tempPath = Path.Combine(Path.GetTempPath(),
            $"part-roundtrip-{Guid.NewGuid():N}.bgpart");
        vm.LastPartFilePath = tempPath;

        try
        {
            vm.ExportPartCommand.Execute();
            Assert.That(File.Exists(tempPath), Is.True, "Export で .bgpart が作られる");

            // Import 側もテストフックを通すので path は LastPartFilePath を継続使用。
            vm.ImportPartCommand.Execute();

            Assert.That(vm.PartDefinitions, Has.Count.EqualTo(beforeImport + 1),
                "Import で PartDefinitions が 1 件増える");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Import_assignNewId_Importしたものは新しいIdを持つ()
    {
        var (vm, _) = CreateViewModelWithLayer();
        var instance = PromoteRectangle(vm);
        instance.IsSelected.Value = true;
        var originalDefId = vm.PartDefinitions[0].Id.Value;

        var tempPath = Path.Combine(Path.GetTempPath(),
            $"part-newid-{Guid.NewGuid():N}.bgpart");
        vm.LastPartFilePath = tempPath;

        try
        {
            vm.ExportPartCommand.Execute();
            vm.ImportPartCommand.Execute();

            Assert.That(vm.PartDefinitions, Has.Count.EqualTo(2));
            Assert.That(vm.PartDefinitions[1].Id.Value, Is.Not.EqualTo(originalDefId),
                "Import 後の Definition は元の Id と異なる");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Import_Undo_PartDefinitionsが元の数に戻る()
    {
        var (vm, _) = CreateViewModelWithLayer();
        var instance = PromoteRectangle(vm);
        instance.IsSelected.Value = true;

        var tempPath = Path.Combine(Path.GetTempPath(),
            $"part-undo-{Guid.NewGuid():N}.bgpart");
        vm.LastPartFilePath = tempPath;

        try
        {
            vm.ExportPartCommand.Execute();
            var beforeImport = vm.PartDefinitions.Count;

            vm.ImportPartCommand.Execute();
            Assert.That(vm.PartDefinitions, Has.Count.EqualTo(beforeImport + 1));

            vm.UndoCommand.Execute();

            Assert.That(vm.PartDefinitions, Has.Count.EqualTo(beforeImport),
                "Import Undo で Definition が消える (孤児定義が残らない)");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
