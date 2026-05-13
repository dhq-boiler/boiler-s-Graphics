using boilersGraphics.Models;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class DetailPartInstanceViewModelTest
{
    private static DiagramViewModel CreateDiagram()
    {
        boilersGraphics.App.IsTest = true;
        var dlgService = new Mock<IDialogService>();
        var mainWindowVm = new MainWindowViewModel(dlgService.Object);
        var diagram = new DiagramViewModel(mainWindowVm);
        diagram.Layers.Clear();
        var layer = new Layer();
        layer.Name.Value = "L1";
        diagram.Layers.Add(layer);
        layer.IsSelected.Value = true;
        return diagram;
    }

    private static PartDefinitionViewModel AddDefinition(DiagramViewModel diagram, string name)
    {
        var def = new PartDefinitionViewModel(new PartDefinition { Name = name });
        diagram.PartDefinitions.Add(def);
        return def;
    }

    private static DetailPartInstanceViewModel CreateDetailVm()
    {
        return new DetailPartInstanceViewModel(new Mock<IRegionManager>().Object);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SetProperties_ExposedPropertiesが0件ならPropertiesも0件()
    {
        var diagram = CreateDiagram();
        var def = AddDefinition(diagram, "P");
        var instance = new PartInstanceViewModel(def.Id.Value);
        diagram.AddItemCommand.Execute(instance);

        var detail = CreateDetailVm();
        detail.ViewModel.Value = instance;
        detail.SetProperties();

        Assert.That(detail.Properties, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SetProperties_DefinitionのExposedProperties数だけPropertiesに並ぶ()
    {
        var diagram = CreateDiagram();
        var def = AddDefinition(diagram, "リング");
        var instance = new PartInstanceViewModel(def.Id.Value);
        diagram.AddItemCommand.Execute(instance);

        diagram.AddExposedPropertyToDefinition(def,
            new ExposedPropertyViewModel(new ExposedProperty
                { Name = "半径", Type = ExposedPropertyType.Double, DefaultValue = 10d }));
        diagram.AddExposedPropertyToDefinition(def,
            new ExposedPropertyViewModel(new ExposedProperty
                { Name = "リング数", Type = ExposedPropertyType.Int, DefaultValue = 3 }));

        var detail = CreateDetailVm();
        detail.ViewModel.Value = instance;
        detail.SetProperties();

        Assert.That(detail.Properties, Has.Count.EqualTo(2));
        var names = detail.Properties.Select(p => p.PropertyName.Value).ToArray();
        Assert.That(names, Is.EquivalentTo(new[] { "半径", "リング数" }));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SetProperties_既存ParameterValueは新規生成されず既存値を維持する()
    {
        var diagram = CreateDiagram();
        var def = AddDefinition(diagram, "リング");
        var instance = new PartInstanceViewModel(def.Id.Value);
        diagram.AddItemCommand.Execute(instance);

        var ep = new ExposedPropertyViewModel(new ExposedProperty
            { Name = "半径", Type = ExposedPropertyType.Double, DefaultValue = 10d });
        diagram.AddExposedPropertyToDefinition(def, ep);
        instance.GetOrCreateParameterValue(ep.Id.Value).Value = 42d;

        var detail = CreateDetailVm();
        detail.ViewModel.Value = instance;
        detail.SetProperties();

        var opt = detail.Properties.OfType<ExposedParameterValuePropertyOption>().Single();
        Assert.That(opt.PropertyValue.Value, Is.EqualTo(42d));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SetProperties_OwnerがDiagramでない場合_例外なくPropertiesは空()
    {
        var instance = new PartInstanceViewModel(Guid.NewGuid());

        var detail = CreateDetailVm();
        detail.ViewModel.Value = instance;
        Assert.DoesNotThrow(() => detail.SetProperties());
        Assert.That(detail.Properties, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SetProperties_PartDefinitionが見つからない場合_Propertiesは空()
    {
        var diagram = CreateDiagram();
        var instance = new PartInstanceViewModel(Guid.NewGuid());
        diagram.AddItemCommand.Execute(instance);

        var detail = CreateDetailVm();
        detail.ViewModel.Value = instance;
        Assert.DoesNotThrow(() => detail.SetProperties());
        Assert.That(detail.Properties, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SetProperties_PropertyValueを編集するとInstanceのParameterValuesに反映される()
    {
        var diagram = CreateDiagram();
        var def = AddDefinition(diagram, "リング");
        var instance = new PartInstanceViewModel(def.Id.Value);
        diagram.AddItemCommand.Execute(instance);

        var ep = new ExposedPropertyViewModel(new ExposedProperty
            { Name = "半径", Type = ExposedPropertyType.Double, DefaultValue = 10d });
        diagram.AddExposedPropertyToDefinition(def, ep);

        var detail = CreateDetailVm();
        detail.ViewModel.Value = instance;
        detail.SetProperties();
        var opt = detail.Properties.OfType<ExposedParameterValuePropertyOption>().Single();
        opt.PropertyValue.Value = 99d;

        Assert.That(instance.ParameterValues[ep.Id.Value].Value, Is.EqualTo(99d));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SetProperties_Optionの型はExposedPropertyTypeに応じて切り替わる()
    {
        var diagram = CreateDiagram();
        var def = AddDefinition(diagram, "P");
        var instance = new PartInstanceViewModel(def.Id.Value);
        diagram.AddItemCommand.Execute(instance);

        diagram.AddExposedPropertyToDefinition(def, new ExposedPropertyViewModel(
            new ExposedProperty { Name = "幅", Type = ExposedPropertyType.Double, DefaultValue = 0d }));
        diagram.AddExposedPropertyToDefinition(def, new ExposedPropertyViewModel(
            new ExposedProperty { Name = "有効", Type = ExposedPropertyType.Boolean, DefaultValue = false }));
        diagram.AddExposedPropertyToDefinition(def, new ExposedPropertyViewModel(
            new ExposedProperty { Name = "色", Type = ExposedPropertyType.Color, DefaultValue = "" }));

        var detail = CreateDetailVm();
        detail.ViewModel.Value = instance;
        detail.SetProperties();

        var byName = detail.Properties
            .OfType<ExposedParameterValuePropertyOption>()
            .ToDictionary(p => p.PropertyName.Value);
        Assert.That(byName["幅"].Type, Is.EqualTo("TextBox"));
        Assert.That(byName["有効"].Type, Is.EqualTo("CheckBox"));
        Assert.That(byName["色"].Type, Is.EqualTo("ReadOnlyTextBox"));
    }
}
