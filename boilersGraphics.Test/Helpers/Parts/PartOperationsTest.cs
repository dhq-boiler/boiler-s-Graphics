using boilersGraphics.Helpers.Parts;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Parts;

[TestFixture]
public class PartOperationsTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Moq.Mock<Prism.Services.Dialogs.IDialogService>();
        _ = new MainWindowViewModel(dlg.Object);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_選択図形をPartDefinitionに移管してPartInstanceを返す()
    {
        var r1 = new NRectangleViewModel(10, 20, 30, 40);
        var r2 = new NRectangleViewModel(50, 60, 30, 40);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "リング");

        Assert.That(result.Definition.Name.Value, Is.EqualTo("リング"));
        Assert.That(result.Definition.Items.Count, Is.EqualTo(2));
        Assert.That(result.Instance.DefinitionId.Value, Is.EqualTo(result.Definition.Id.Value));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_PartInstanceの位置サイズは選択図形群のバウンディングボックス()
    {
        var r1 = new NRectangleViewModel(10, 20, 30, 40);
        var r2 = new NRectangleViewModel(50, 60, 30, 40);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "P");

        Assert.That(result.Instance.Left.Value, Is.EqualTo(10));
        Assert.That(result.Instance.Top.Value, Is.EqualTo(20));
        Assert.That(result.Instance.Width.Value, Is.EqualTo(70));
        Assert.That(result.Instance.Height.Value, Is.EqualTo(80));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_空の選択でInvalidOperationExceptionをスローする()
    {
        Assert.Throws<InvalidOperationException>(() =>
            PartOperations.Promote(Array.Empty<DesignerItemViewModelBase>(), "P"));
    }

    [Test]
    public void Promote_nullでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => PartOperations.Promote(null, "P"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_既存ExposedPropertyのDefaultValueがPartInstanceにコピーされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P");

        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "幅",
            Type = ExposedPropertyType.Double,
            DefaultValue = 50.0
        });
        result.Definition.ExposedProperties.Add(ep);

        var promotedAfter = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P2");
        promotedAfter.Definition.ExposedProperties.Add(new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "X",
            Type = ExposedPropertyType.Double,
            DefaultValue = 7.5
        }));

        var thirdPromote = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P3");
        Assert.That(thirdPromote.Instance.ParameterValues.Count, Is.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Detach_PartDefinitionの内部図形をクローンして返す()
    {
        var r1 = new NRectangleViewModel(10, 20, 30, 40);
        var r2 = new NRectangleViewModel(50, 60, 30, 40);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "P");

        var detached = PartOperations.Detach(promoted.Instance, promoted.Definition);

        Assert.That(detached.Count, Is.EqualTo(2));
        Assert.That(detached[0], Is.Not.SameAs(r1), "クローンであり同一インスタンスではない");
        Assert.That(detached[1], Is.Not.SameAs(r2));
        Assert.That(detached[0], Is.TypeOf<NRectangleViewModel>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Detach_DefinitionId不一致でInvalidOperationExceptionをスローする()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P");
        var wrongDef = new PartDefinitionViewModel();

        Assert.Throws<InvalidOperationException>(() =>
            PartOperations.Detach(promoted.Instance, wrongDef));
    }

    [Test]
    public void Detach_nullでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => PartOperations.Detach(null, new PartDefinitionViewModel()));
        Assert.Throws<ArgumentNullException>(() => PartOperations.Detach(new PartInstanceViewModel(), null));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_新Idと新Nameを持つ独立した定義を返す()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元パーツ");

        var clone = PartOperations.Clone(promoted.Definition, "新パーツ");

        Assert.That(clone.Id.Value, Is.Not.EqualTo(promoted.Definition.Id.Value));
        Assert.That(clone.Name.Value, Is.EqualTo("新パーツ"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_ExposedPropertyとBindingがコピーされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元");
        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "幅",
            Type = ExposedPropertyType.Double,
            DefaultValue = 50.0,
        });
        ep.Bindings.Add(new BindingViewModel(new Binding
        {
            TargetItemId = r.ID,
            TargetProperty = "Width"
        }));
        promoted.Definition.ExposedProperties.Add(ep);

        var clone = PartOperations.Clone(promoted.Definition, "新");

        Assert.That(clone.ExposedProperties.Count, Is.EqualTo(1));
        var clonedEp = clone.ExposedProperties[0];
        Assert.That(clonedEp.Name.Value, Is.EqualTo("幅"));
        Assert.That(clonedEp.DefaultValue.Value, Is.EqualTo(50.0));
        Assert.That(clonedEp.Bindings.Count, Is.EqualTo(1));
        Assert.That(clonedEp.Bindings[0].TargetProperty.Value, Is.EqualTo("Width"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_内部図形がディープコピーされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元");

        var clone = PartOperations.Clone(promoted.Definition, "新");

        Assert.That(clone.Items.Count, Is.EqualTo(1));
        Assert.That(clone.Items[0], Is.Not.SameAs(r));
        Assert.That(clone.Items[0], Is.TypeOf<NRectangleViewModel>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_BindingのTargetItemIdがクローンした図形のIdにマッピングされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var originalId = r.ID;
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元");
        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "幅",
            Type = ExposedPropertyType.Double,
        });
        ep.Bindings.Add(new BindingViewModel(new Binding
        {
            TargetItemId = originalId,
            TargetProperty = "Width"
        }));
        promoted.Definition.ExposedProperties.Add(ep);

        var clone = PartOperations.Clone(promoted.Definition, "新");

        var clonedItemId = clone.Items[0].ID;
        Assert.That(clone.ExposedProperties[0].Bindings[0].TargetItemId.Value, Is.EqualTo(clonedItemId));
        Assert.That(clone.ExposedProperties[0].Bindings[0].TargetItemId.Value, Is.Not.EqualTo(originalId));
    }

    [Test]
    public void Clone_nullでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => PartOperations.Clone(null, "新"));
    }
}
