using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class PartInstanceViewModelTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test]
    public void PartInstanceViewModel_IsResizableはfalse()
    {
        var vm = new PartInstanceViewModel();
        Assert.That(vm.IsResizable, Is.False);
    }

    [Test]
    public void PartInstanceViewModel_DesignerItemViewModelBase既定のIsResizableはtrue()
    {
        var vm = new boilersGraphics.ViewModels.NRectangleViewModel();
        Assert.That(vm.IsResizable, Is.True);
    }

    [Test]
    public void PartInstanceViewModel_DefinitionIdを設定できる()
    {
        var id = Guid.NewGuid();
        var vm = new PartInstanceViewModel(id);
        Assert.That(vm.DefinitionId.Value, Is.EqualTo(id));
    }

    [Test]
    public void PartInstanceViewModel_GetOrCreateParameterValueは初回でデフォルト値を持つ()
    {
        var vm = new PartInstanceViewModel();
        var epId = Guid.NewGuid();

        var rp = vm.GetOrCreateParameterValue(epId, 42.0);

        Assert.That(rp.Value, Is.EqualTo(42.0));
        Assert.That(vm.ParameterValues.ContainsKey(epId), Is.True);
    }

    [Test]
    public void PartInstanceViewModel_GetOrCreateParameterValueは同じキーで同じインスタンスを返す()
    {
        var vm = new PartInstanceViewModel();
        var epId = Guid.NewGuid();

        var first = vm.GetOrCreateParameterValue(epId, 1.0);
        var second = vm.GetOrCreateParameterValue(epId, 99.0);

        Assert.That(second, Is.SameAs(first));
        Assert.That(second.Value, Is.EqualTo(1.0));
    }

    [Test]
    public void PartInstanceViewModel_TryGetParameterValueは未登録キーに対してfalseを返す()
    {
        var vm = new PartInstanceViewModel();
        var result = vm.TryGetParameterValue(Guid.NewGuid(), out var rp);

        Assert.That(result, Is.False);
        Assert.That(rp, Is.Null);
    }

    [Test]
    public void PartInstanceViewModel_RemoveParameterValueでエントリが消える()
    {
        var vm = new PartInstanceViewModel();
        var epId = Guid.NewGuid();
        vm.GetOrCreateParameterValue(epId, 10.0);

        vm.RemoveParameterValue(epId);

        Assert.That(vm.ParameterValues.ContainsKey(epId), Is.False);
    }

    [Test]
    public void ExposedParameterCount_初期値は0_HasExposedParametersはfalse()
    {
        var vm = new PartInstanceViewModel();
        Assert.That(vm.ExposedParameterCount.Value, Is.EqualTo(0));
        Assert.That(vm.HasExposedParameters.Value, Is.False);
    }

    [Test]
    public void GetOrCreateParameterValue_ExposedParameterCountが増えてHasExposedParametersがtrue()
    {
        var vm = new PartInstanceViewModel();
        vm.GetOrCreateParameterValue(Guid.NewGuid(), 1.0);

        Assert.That(vm.ExposedParameterCount.Value, Is.EqualTo(1));
        Assert.That(vm.HasExposedParameters.Value, Is.True);
    }

    [Test]
    public void GetOrCreateParameterValue_同じキーで再呼び出ししてもCountは増えない()
    {
        var vm = new PartInstanceViewModel();
        var epId = Guid.NewGuid();
        vm.GetOrCreateParameterValue(epId, 1.0);
        vm.GetOrCreateParameterValue(epId, 2.0);

        Assert.That(vm.ExposedParameterCount.Value, Is.EqualTo(1));
    }

    [Test]
    public void RemoveParameterValue_最後の1件を消すとHasExposedParametersはfalseに戻る()
    {
        var vm = new PartInstanceViewModel();
        var epId = Guid.NewGuid();
        vm.GetOrCreateParameterValue(epId, 1.0);
        Assert.That(vm.HasExposedParameters.Value, Is.True);

        vm.RemoveParameterValue(epId);

        Assert.That(vm.ExposedParameterCount.Value, Is.EqualTo(0));
        Assert.That(vm.HasExposedParameters.Value, Is.False);
    }

    [Test]
    public void PartInstanceViewModel_CloneはDefinitionIdとParameterValuesをコピーする()
    {
        var defId = Guid.NewGuid();
        var epId = Guid.NewGuid();
        var vm = new PartInstanceViewModel(defId);
        vm.Left.Value = 10;
        vm.Top.Value = 20;
        vm.Width.Value = 100;
        vm.Height.Value = 50;
        vm.GetOrCreateParameterValue(epId, 7.5);

        var clone = (PartInstanceViewModel)vm.Clone();

        Assert.That(clone.DefinitionId.Value, Is.EqualTo(defId));
        Assert.That(clone.Left.Value, Is.EqualTo(10));
        Assert.That(clone.Top.Value, Is.EqualTo(20));
        Assert.That(clone.Width.Value, Is.EqualTo(100));
        Assert.That(clone.Height.Value, Is.EqualTo(50));
        Assert.That(clone.ParameterValues.ContainsKey(epId), Is.True);
        Assert.That(clone.ParameterValues[epId].Value, Is.EqualTo(7.5));
    }
}
