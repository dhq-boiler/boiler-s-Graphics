using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class PartDefinitionViewModelTest
{
    [Test]
    public void PartDefinitionViewModel_デフォルトプロパティ()
    {
        using var vm = new PartDefinitionViewModel();
        Assert.That(vm.Model, Is.Not.Null);
        Assert.That(vm.Id.Value, Is.Not.EqualTo(Guid.Empty));
        Assert.That(vm.Name.Value, Is.Null);
        Assert.That(vm.Items, Is.Empty);
        Assert.That(vm.ExposedProperties, Is.Empty);
    }

    [Test]
    public void PartDefinitionViewModel_既存ModelのExposedPropertiesを読み込む()
    {
        var model = new PartDefinition { Name = "同心円リング" };
        model.ExposedProperties.Add(new ExposedProperty { Name = "半径" });
        model.ExposedProperties.Add(new ExposedProperty { Name = "リング数" });

        using var vm = new PartDefinitionViewModel(model);

        Assert.That(vm.Name.Value, Is.EqualTo("同心円リング"));
        Assert.That(vm.ExposedProperties.Count, Is.EqualTo(2));
        Assert.That(vm.ExposedProperties[0].Name.Value, Is.EqualTo("半径"));
        Assert.That(vm.ExposedProperties[1].Name.Value, Is.EqualTo("リング数"));
    }

    [Test]
    public void PartDefinitionViewModel_Name変更がModelに同期される()
    {
        var model = new PartDefinition();
        using var vm = new PartDefinitionViewModel(model);

        vm.Name.Value = "ターゲットスコープ";

        Assert.That(model.Name, Is.EqualTo("ターゲットスコープ"));
    }

    [Test]
    public void PartDefinitionViewModel_ExposedPropertyViewModel追加でModelにも追加される()
    {
        var model = new PartDefinition();
        using var vm = new PartDefinitionViewModel(model);

        var epvm = new ExposedPropertyViewModel(new ExposedProperty { Name = "幅" });
        vm.ExposedProperties.Add(epvm);

        Assert.That(model.ExposedProperties.Count, Is.EqualTo(1));
        Assert.That(model.ExposedProperties[0].Name, Is.EqualTo("幅"));
        Assert.That(model.ExposedProperties[0], Is.SameAs(epvm.Model));
    }

    [Test]
    public void PartDefinitionViewModel_ExposedPropertyViewModel削除でModelからも削除される()
    {
        var model = new PartDefinition();
        using var vm = new PartDefinitionViewModel(model);

        var epvm = new ExposedPropertyViewModel(new ExposedProperty());
        vm.ExposedProperties.Add(epvm);
        vm.ExposedProperties.Remove(epvm);

        Assert.That(model.ExposedProperties, Is.Empty);
    }

    [Test]
    public void PartDefinitionViewModel_nullモデルでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => new PartDefinitionViewModel(null));
    }
}
