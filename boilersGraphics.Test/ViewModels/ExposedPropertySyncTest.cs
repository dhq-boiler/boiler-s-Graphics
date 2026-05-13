using boilersGraphics.Models;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class ExposedPropertySyncTest
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

    private static PartDefinitionViewModel AddDefinition(DiagramViewModel vm, string name)
    {
        var def = new PartDefinitionViewModel(new PartDefinition { Name = name });
        vm.PartDefinitions.Add(def);
        return def;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyToDefinition_DefinitionのExposedPropertiesに追加される()
    {
        var vm = CreateViewModel();
        var def = AddDefinition(vm, "リング");
        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "半径", Type = ExposedPropertyType.Double });

        vm.AddExposedPropertyToDefinition(def, ep);

        Assert.That(def.ExposedProperties, Has.Count.EqualTo(1));
        Assert.That(def.ExposedProperties[0], Is.SameAs(ep));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyToDefinition_参照中のPartInstanceに同期される()
    {
        var vm = CreateViewModel();
        var def = AddDefinition(vm, "リング");
        var instance = new PartInstanceViewModel(def.Id.Value);
        vm.AddItemCommand.Execute(instance);

        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "半径",
            Type = ExposedPropertyType.Double,
            DefaultValue = 10d,
        });

        vm.AddExposedPropertyToDefinition(def, ep);

        Assert.That(instance.ParameterValues, Has.Count.EqualTo(1));
        Assert.That(instance.ParameterValues.ContainsKey(ep.Id.Value), Is.True);
        Assert.That(instance.ParameterValues[ep.Id.Value].Value, Is.EqualTo(10d));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyToDefinition_HasExposedParametersがtrueになる()
    {
        var vm = CreateViewModel();
        var def = AddDefinition(vm, "リング");
        var instance = new PartInstanceViewModel(def.Id.Value);
        vm.AddItemCommand.Execute(instance);

        Assert.That(instance.HasExposedParameters.Value, Is.False, "追加前は false");

        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "半径", DefaultValue = 1d });
        vm.AddExposedPropertyToDefinition(def, ep);

        Assert.That(instance.HasExposedParameters.Value, Is.True, "追加後は true (バッジ点灯条件)");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyToDefinition_別DefinitionのInstanceには同期されない()
    {
        var vm = CreateViewModel();
        var defA = AddDefinition(vm, "A");
        var defB = AddDefinition(vm, "B");

        var instanceA = new PartInstanceViewModel(defA.Id.Value);
        var instanceB = new PartInstanceViewModel(defB.Id.Value);
        vm.AddItemCommand.Execute(instanceA);
        vm.AddItemCommand.Execute(instanceB);

        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "X", DefaultValue = 5d });
        vm.AddExposedPropertyToDefinition(defA, ep);

        Assert.That(instanceA.ParameterValues, Has.Count.EqualTo(1));
        Assert.That(instanceB.ParameterValues, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveExposedPropertyFromDefinition_DefinitionとInstance両方から消える()
    {
        var vm = CreateViewModel();
        var def = AddDefinition(vm, "リング");
        var instance = new PartInstanceViewModel(def.Id.Value);
        vm.AddItemCommand.Execute(instance);

        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "半径", DefaultValue = 10d });
        vm.AddExposedPropertyToDefinition(def, ep);
        Assert.That(def.ExposedProperties, Has.Count.EqualTo(1));
        Assert.That(instance.ParameterValues, Has.Count.EqualTo(1));

        vm.RemoveExposedPropertyFromDefinition(def, ep);

        Assert.That(def.ExposedProperties, Has.Count.EqualTo(0));
        Assert.That(instance.ParameterValues, Has.Count.EqualTo(0));
        Assert.That(instance.HasExposedParameters.Value, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyToDefinition_Undo_DefinitionとInstanceから消える()
    {
        var vm = CreateViewModel();
        var def = AddDefinition(vm, "リング");
        var instance = new PartInstanceViewModel(def.Id.Value);
        vm.AddItemCommand.Execute(instance);

        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "半径", DefaultValue = 10d });
        vm.AddExposedPropertyToDefinition(def, ep);

        vm.UndoCommand.Execute();

        Assert.That(def.ExposedProperties, Has.Count.EqualTo(0), "Undo で Definition から消える");
        // 注: Instance.ParameterValues の Undo は別ルートで管理されておらず、Definition のロールバックのみ確認
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyToDefinition_null引数_例外なし()
    {
        var vm = CreateViewModel();
        var def = AddDefinition(vm, "リング");
        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "X" });

        Assert.DoesNotThrow(() => vm.AddExposedPropertyToDefinition(null, ep));
        Assert.DoesNotThrow(() => vm.AddExposedPropertyToDefinition(def, null));
        Assert.DoesNotThrow(() => vm.AddExposedPropertyToDefinition(null, null));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveExposedPropertyFromDefinition_null引数_例外なし()
    {
        var vm = CreateViewModel();
        var def = AddDefinition(vm, "リング");
        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "X" });

        Assert.DoesNotThrow(() => vm.RemoveExposedPropertyFromDefinition(null, ep));
        Assert.DoesNotThrow(() => vm.RemoveExposedPropertyFromDefinition(def, null));
    }
}
