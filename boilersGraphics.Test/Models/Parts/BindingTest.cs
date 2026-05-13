using boilersGraphics.Models.Parts;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.Models.Parts;

[TestFixture]
public class BindingTest
{
    [Test]
    public void Binding_デフォルトプロパティ()
    {
        var b = new Binding();
        Assert.That(b.TargetItemId, Is.EqualTo(Guid.Empty));
        Assert.That(b.TargetProperty, Is.Null);
    }

    [Test]
    public void Binding_プロパティ設定()
    {
        var id = Guid.NewGuid();
        var b = new Binding
        {
            TargetItemId = id,
            TargetProperty = "Width"
        };
        Assert.That(b.TargetItemId, Is.EqualTo(id));
        Assert.That(b.TargetProperty, Is.EqualTo("Width"));
    }

    [Test]
    public void Binding_PropertyChangedが発火する()
    {
        var b = new Binding();
        var changedProps = new System.Collections.Generic.List<string>();
        b.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName);

        b.TargetItemId = Guid.NewGuid();
        b.TargetProperty = "Height";

        Assert.That(changedProps, Does.Contain(nameof(Binding.TargetItemId)));
        Assert.That(changedProps, Does.Contain(nameof(Binding.TargetProperty)));
    }
}
