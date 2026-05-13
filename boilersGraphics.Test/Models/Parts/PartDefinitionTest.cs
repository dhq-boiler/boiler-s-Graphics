using boilersGraphics.Models.Parts;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.Models.Parts;

[TestFixture]
public class PartDefinitionTest
{
    [Test]
    public void PartDefinition_デフォルトプロパティ()
    {
        var def = new PartDefinition();
        Assert.That(def.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(def.Name, Is.Null);
        Assert.That(def.Items, Is.Not.Null);
        Assert.That(def.Items, Is.Empty);
        Assert.That(def.ExposedProperties, Is.Not.Null);
        Assert.That(def.ExposedProperties, Is.Empty);
    }

    [Test]
    public void PartDefinition_IdはインスタンスごとにユニークになるGuidが振られる()
    {
        var d1 = new PartDefinition();
        var d2 = new PartDefinition();
        Assert.That(d1.Id, Is.Not.EqualTo(d2.Id));
    }

    [Test]
    public void PartDefinition_ExposedPropertiesに追加できる()
    {
        var def = new PartDefinition();
        def.ExposedProperties.Add(new ExposedProperty
        {
            Name = "半径",
            Type = ExposedPropertyType.Double
        });
        def.ExposedProperties.Add(new ExposedProperty
        {
            Name = "リング数",
            Type = ExposedPropertyType.Int
        });
        Assert.That(def.ExposedProperties.Count, Is.EqualTo(2));
        Assert.That(def.ExposedProperties[0].Name, Is.EqualTo("半径"));
    }

    [Test]
    public void PartDefinition_NameのPropertyChangedが発火する()
    {
        var def = new PartDefinition();
        string changedProp = null;
        def.PropertyChanged += (_, e) => changedProp = e.PropertyName;

        def.Name = "同心円リング";

        Assert.That(changedProp, Is.EqualTo(nameof(PartDefinition.Name)));
        Assert.That(def.Name, Is.EqualTo("同心円リング"));
    }
}
