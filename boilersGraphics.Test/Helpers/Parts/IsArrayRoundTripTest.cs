using boilersGraphics.Helpers.Parts;
using boilersGraphics.Models.Parts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace boilersGraphics.Test.Helpers.Parts;

[TestFixture]
public class IsArrayRoundTripTest
{
    [Test]
    public void SerializeExposedProperty_IsArrayかつDefaultValueがリストならItem要素で出力()
    {
        var ep = new ExposedProperty
        {
            Name = "半径群",
            Type = ExposedPropertyType.Double,
            IsArray = true,
            DefaultValue = new List<object> { 1.0, 2.0, 3.0 }
        };

        var elm = PartSerializer.SerializeExposedProperty(ep);

        var defaultElm = elm.Element("DefaultValue");
        Assert.That(defaultElm, Is.Not.Null);
        Assert.That(defaultElm!.Attribute("IsArray")?.Value, Is.EqualTo("true"));
        Assert.That(defaultElm.Elements("Item").Count(), Is.EqualTo(3));
        Assert.That(defaultElm.Elements("Item").Select(e => e.Value).ToArray(),
            Is.EqualTo(new[] { "1", "2", "3" }));
    }

    [Test]
    public void DeserializeExposedProperty_IsArrayDefaultValueをListとして復元する()
    {
        var ep = new ExposedProperty
        {
            Name = "半径群",
            Type = ExposedPropertyType.Double,
            IsArray = true,
            DefaultValue = new List<object> { 1.5, 2.5, 3.5 }
        };

        var roundtrip = PartDeserializer.DeserializeExposedProperty(
            PartSerializer.SerializeExposedProperty(ep));

        Assert.That(roundtrip.IsArray, Is.True);
        var list = roundtrip.DefaultValue as IList<object>;
        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Count, Is.EqualTo(3));
        Assert.That(list, Is.EqualTo(new object[] { 1.5, 2.5, 3.5 }));
    }

    [Test]
    public void ParseTypedValue_IsArray無し属性なら従来通り単一値()
    {
        var elm = PartSerializer.SerializeTypedValue("DefaultValue", ExposedPropertyType.Int, 7);

        var value = PartDeserializer.ParseTypedValue(elm, ExposedPropertyType.Int);

        Assert.That(value, Is.EqualTo(7));
        Assert.That(value, Is.TypeOf<int>());
    }

    [Test]
    public void SerializeParameterValue_配列値ならItem要素で出力()
    {
        var id = Guid.NewGuid();
        var list = new List<object> { 1.0, 2.0, 3.0 };

        var elm = PartSerializer.SerializeParameterValue(id, list);

        Assert.That(elm.Attribute("IsArray")?.Value, Is.EqualTo("true"));
        Assert.That(elm.Attribute("Type")?.Value, Is.EqualTo("Double"));
        Assert.That(elm.Elements("Item").Count(), Is.EqualTo(3));
    }

    [Test]
    public void SerializeParameterValue_StringはIEnumerable扱いしない()
    {
        var id = Guid.NewGuid();

        var elm = PartSerializer.SerializeParameterValue(id, "hello");

        Assert.That(elm.Attribute("IsArray"), Is.Null);
        Assert.That(elm.Value, Is.EqualTo("hello"));
    }
}
