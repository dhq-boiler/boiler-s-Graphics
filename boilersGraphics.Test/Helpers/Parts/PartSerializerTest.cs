using boilersGraphics.Helpers.Parts;
using boilersGraphics.Models.Parts;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Test.Helpers.Parts;

[TestFixture]
public class PartSerializerTest
{
    [Test]
    public void SerializeDefinition_最低限のフィールドが出力される()
    {
        var def = new PartDefinition { Name = "同心円リング" };
        var elm = PartSerializer.SerializeDefinition(def);

        Assert.That(elm.Name.LocalName, Is.EqualTo("PartDefinition"));
        Assert.That(elm.Attribute("Id")?.Value, Is.EqualTo(def.Id.ToString()));
        Assert.That(elm.Element("Name")?.Value, Is.EqualTo("同心円リング"));
        Assert.That(elm.Element("Items"), Is.Not.Null);
        Assert.That(elm.Element("ExposedProperties"), Is.Not.Null);
    }

    [Test]
    public void SerializeExposedProperty_全フィールド出力()
    {
        var ep = new ExposedProperty
        {
            Name = "半径",
            Type = ExposedPropertyType.Double,
            IsArray = false,
            DefaultValue = 50.0,
            MinValue = 0.0,
            MaxValue = 100.0,
            Step = 0.5
        };
        ep.Bindings.Add(new Binding { TargetItemId = Guid.NewGuid(), TargetProperty = "Width" });

        var elm = PartSerializer.SerializeExposedProperty(ep);

        Assert.That(elm.Element("Name")?.Value, Is.EqualTo("半径"));
        Assert.That(elm.Element("Type")?.Value, Is.EqualTo("Double"));
        Assert.That(elm.Element("IsArray")?.Value, Is.EqualTo("false"));
        Assert.That(elm.Element("DefaultValue")?.Attribute("Type")?.Value, Is.EqualTo("Double"));
        Assert.That(elm.Element("DefaultValue")?.Value, Is.EqualTo("50"));
        Assert.That(elm.Element("MinValue")?.Value, Is.EqualTo("0"));
        Assert.That(elm.Element("MaxValue")?.Value, Is.EqualTo("100"));
        Assert.That(elm.Element("Step")?.Value, Is.EqualTo("0.5"));
        Assert.That(elm.Element("Bindings")?.Elements("Binding").Count(), Is.EqualTo(1));
    }

    [Test]
    public void SerializePartFile_ルートとバージョン属性()
    {
        var def = new PartDefinition { Name = "テストパーツ" };
        var elm = PartSerializer.SerializePartFile(def);

        Assert.That(elm.Name.LocalName, Is.EqualTo("BoilersGraphicsPart"));
        Assert.That(elm.Attribute("Version")?.Value, Is.EqualTo(PartSerializer.PartFileVersion));
        Assert.That(elm.Element("PartDefinition"), Is.Not.Null);
    }

    [Test]
    public void SerializePartFile_nullでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => PartSerializer.SerializePartFile(null));
    }

    [Test]
    public void SerializeAll_複数定義をPartDefinitions配下にまとめる()
    {
        var d1 = new PartDefinition { Name = "A" };
        var d2 = new PartDefinition { Name = "B" };

        var elm = PartSerializer.SerializeAll(new[] { d1, d2 });

        Assert.That(elm.Name.LocalName, Is.EqualTo("PartDefinitions"));
        Assert.That(elm.Elements("PartDefinition").Count(), Is.EqualTo(2));
    }

    [Test]
    public void InferType_型推論()
    {
        Assert.That(PartSerializer.InferType(1.5), Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(PartSerializer.InferType(3), Is.EqualTo(ExposedPropertyType.Int));
        Assert.That(PartSerializer.InferType(true), Is.EqualTo(ExposedPropertyType.Boolean));
        Assert.That(PartSerializer.InferType("text"), Is.EqualTo(ExposedPropertyType.String));
        Assert.That(PartSerializer.InferType(new Point(1, 2)), Is.EqualTo(ExposedPropertyType.Point));
        Assert.That(PartSerializer.InferType(Colors.Red), Is.EqualTo(ExposedPropertyType.Color));
    }

    [Test]
    public void SerializeParameterValue_nullでもExposedPropertyId属性は残る()
    {
        var id = Guid.NewGuid();
        var elm = PartSerializer.SerializeParameterValue(id, null);

        Assert.That(elm.Attribute("ExposedPropertyId")?.Value, Is.EqualTo(id.ToString()));
        Assert.That(elm.Attribute("Type"), Is.Null);
        Assert.That(string.IsNullOrEmpty(elm.Value), Is.True);
    }

    [Test]
    public void SerializeParameterValue_型情報を保持する()
    {
        var id = Guid.NewGuid();
        var elm = PartSerializer.SerializeParameterValue(id, 42.5);

        Assert.That(elm.Attribute("Type")?.Value, Is.EqualTo("Double"));
        Assert.That(elm.Value, Is.EqualTo("42.5"));
    }
}
