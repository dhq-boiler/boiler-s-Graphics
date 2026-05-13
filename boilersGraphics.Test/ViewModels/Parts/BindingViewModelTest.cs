using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class BindingViewModelTest
{
    [Test]
    public void BindingViewModel_デフォルトコンストラクタは空のModelを生成する()
    {
        using var vm = new BindingViewModel();
        Assert.That(vm.Model, Is.Not.Null);
        Assert.That(vm.TargetItemId.Value, Is.EqualTo(Guid.Empty));
        Assert.That(vm.TargetProperty.Value, Is.Null);
    }

    [Test]
    public void BindingViewModel_既存Modelの値を初期値として読み込む()
    {
        var id = Guid.NewGuid();
        var model = new Binding { TargetItemId = id, TargetProperty = "Width" };

        using var vm = new BindingViewModel(model);

        Assert.That(vm.TargetItemId.Value, Is.EqualTo(id));
        Assert.That(vm.TargetProperty.Value, Is.EqualTo("Width"));
    }

    [Test]
    public void BindingViewModel_VM側の変更がModelに同期される()
    {
        var model = new Binding();
        using var vm = new BindingViewModel(model);

        var id = Guid.NewGuid();
        vm.TargetItemId.Value = id;
        vm.TargetProperty.Value = "Height";

        Assert.That(model.TargetItemId, Is.EqualTo(id));
        Assert.That(model.TargetProperty, Is.EqualTo("Height"));
    }

    [Test]
    public void BindingViewModel_nullモデルでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => new BindingViewModel(null));
    }
}
