using boilersGraphics.Models.Parts;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.Models.Parts;

[TestFixture]
public class ExposedPropertyTest
{
    [Test]
    public void ExposedProperty_デフォルトプロパティ()
    {
        var p = new ExposedProperty();
        Assert.That(p.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(p.Name, Is.Null);
        Assert.That(p.Type, Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(p.IsArray, Is.False);
        Assert.That(p.DefaultValue, Is.Null);
        Assert.That(p.MinValue, Is.Null);
        Assert.That(p.MaxValue, Is.Null);
        Assert.That(p.Step, Is.Null);
        Assert.That(p.Bindings, Is.Not.Null);
        Assert.That(p.Bindings, Is.Empty);
    }

    [Test]
    public void ExposedProperty_IdはインスタンスごとにユニークになるGuidが振られる()
    {
        var p1 = new ExposedProperty();
        var p2 = new ExposedProperty();
        Assert.That(p1.Id, Is.Not.EqualTo(p2.Id));
    }

    [Test]
    public void ExposedProperty_すべての公開パラメータ型を取れる()
    {
        var p = new ExposedProperty();
        foreach (ExposedPropertyType t in Enum.GetValues(typeof(ExposedPropertyType)))
        {
            p.Type = t;
            Assert.That(p.Type, Is.EqualTo(t));
        }
    }

    [Test]
    public void ExposedProperty_IsArrayをtrueにできる()
    {
        var p = new ExposedProperty { IsArray = true };
        Assert.That(p.IsArray, Is.True);
    }

    [Test]
    public void ExposedProperty_BindingsにBindingを追加できる()
    {
        var p = new ExposedProperty();
        p.Bindings.Add(new Binding { TargetProperty = "Radius" });
        p.Bindings.Add(new Binding { TargetProperty = "Stroke" });
        Assert.That(p.Bindings.Count, Is.EqualTo(2));
    }

    [Test]
    public void ExposedProperty_制約Min_Max_Stepを設定できる()
    {
        var p = new ExposedProperty
        {
            MinValue = 0.0,
            MaxValue = 100.0,
            Step = 0.5
        };
        Assert.That(p.MinValue, Is.EqualTo(0.0));
        Assert.That(p.MaxValue, Is.EqualTo(100.0));
        Assert.That(p.Step, Is.EqualTo(0.5));
    }
}
