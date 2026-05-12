using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class ExposedPropertyViewModelTest
{
    [Test]
    public void ExposedPropertyViewModel_デフォルトプロパティ()
    {
        using var vm = new ExposedPropertyViewModel();
        Assert.That(vm.Model, Is.Not.Null);
        Assert.That(vm.Id.Value, Is.Not.EqualTo(Guid.Empty));
        Assert.That(vm.Name.Value, Is.Null);
        Assert.That(vm.Type.Value, Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(vm.IsArray.Value, Is.False);
        Assert.That(vm.DefaultValue.Value, Is.Null);
        Assert.That(vm.MinValue.Value, Is.Null);
        Assert.That(vm.MaxValue.Value, Is.Null);
        Assert.That(vm.Step.Value, Is.Null);
        Assert.That(vm.Bindings, Is.Empty);
    }

    [Test]
    public void ExposedPropertyViewModel_既存Modelの値とBindingsを読み込む()
    {
        var model = new ExposedProperty
        {
            Name = "半径",
            Type = ExposedPropertyType.Double,
            IsArray = false,
            DefaultValue = 50.0,
            MinValue = 0.0,
            MaxValue = 100.0,
            Step = 0.5
        };
        model.Bindings.Add(new Binding { TargetProperty = "Width" });
        model.Bindings.Add(new Binding { TargetProperty = "Height" });

        using var vm = new ExposedPropertyViewModel(model);

        Assert.That(vm.Name.Value, Is.EqualTo("半径"));
        Assert.That(vm.Type.Value, Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(vm.DefaultValue.Value, Is.EqualTo(50.0));
        Assert.That(vm.MinValue.Value, Is.EqualTo(0.0));
        Assert.That(vm.MaxValue.Value, Is.EqualTo(100.0));
        Assert.That(vm.Step.Value, Is.EqualTo(0.5));
        Assert.That(vm.Bindings.Count, Is.EqualTo(2));
        Assert.That(vm.Bindings[0].TargetProperty.Value, Is.EqualTo("Width"));
        Assert.That(vm.Bindings[1].TargetProperty.Value, Is.EqualTo("Height"));
    }

    [Test]
    public void ExposedPropertyViewModel_VM側の変更がModelに同期される()
    {
        var model = new ExposedProperty();
        using var vm = new ExposedPropertyViewModel(model);

        vm.Name.Value = "リング数";
        vm.Type.Value = ExposedPropertyType.Int;
        vm.IsArray.Value = true;
        vm.MinValue.Value = 1.0;
        vm.MaxValue.Value = 10.0;
        vm.Step.Value = 1.0;

        Assert.That(model.Name, Is.EqualTo("リング数"));
        Assert.That(model.Type, Is.EqualTo(ExposedPropertyType.Int));
        Assert.That(model.IsArray, Is.True);
        Assert.That(model.MinValue, Is.EqualTo(1.0));
        Assert.That(model.MaxValue, Is.EqualTo(10.0));
        Assert.That(model.Step, Is.EqualTo(1.0));
    }

    [Test]
    public void ExposedPropertyViewModel_BindingViewModel追加でModelのBindingsにも追加される()
    {
        var model = new ExposedProperty();
        using var vm = new ExposedPropertyViewModel(model);

        var bvm = new BindingViewModel(new Binding { TargetProperty = "Stroke" });
        vm.Bindings.Add(bvm);

        Assert.That(model.Bindings.Count, Is.EqualTo(1));
        Assert.That(model.Bindings[0].TargetProperty, Is.EqualTo("Stroke"));
        Assert.That(model.Bindings[0], Is.SameAs(bvm.Model));
    }

    [Test]
    public void ExposedPropertyViewModel_BindingViewModel削除でModelのBindingsからも削除される()
    {
        var model = new ExposedProperty();
        using var vm = new ExposedPropertyViewModel(model);

        var bvm = new BindingViewModel(new Binding());
        vm.Bindings.Add(bvm);
        vm.Bindings.Remove(bvm);

        Assert.That(model.Bindings, Is.Empty);
    }

    [Test]
    public void ExposedPropertyViewModel_Clearでも同期される()
    {
        var model = new ExposedProperty();
        model.Bindings.Add(new Binding());
        model.Bindings.Add(new Binding());

        using var vm = new ExposedPropertyViewModel(model);
        vm.Bindings.Clear();

        Assert.That(model.Bindings, Is.Empty);
    }

    [Test]
    public void ExposedPropertyViewModel_nullモデルでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => new ExposedPropertyViewModel(null));
    }
}
