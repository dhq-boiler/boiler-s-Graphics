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
public class PartDeserializerTest
{
    [Test]
    public void DeserializeDefinition_最低限のフィールドが復元される()
    {
        var original = new PartDefinition { Name = "同心円リング" };
        var roundtrip = PartDeserializer.DeserializeDefinition(
            PartSerializer.SerializeDefinition(original));

        Assert.That(roundtrip.Id, Is.EqualTo(original.Id));
        Assert.That(roundtrip.Name, Is.EqualTo(original.Name));
        Assert.That(roundtrip.ExposedProperties, Is.Empty);
    }

    [Test]
    public void DeserializeExposedProperty_全フィールドのラウンドトリップ()
    {
        var original = new ExposedProperty
        {
            Name = "半径",
            Type = ExposedPropertyType.Double,
            IsArray = false,
            DefaultValue = 50.0,
            MinValue = 0.0,
            MaxValue = 100.0,
            Step = 0.5
        };
        original.Bindings.Add(new Binding { TargetItemId = Guid.NewGuid(), TargetProperty = "Width" });

        var roundtrip = PartDeserializer.DeserializeExposedProperty(
            PartSerializer.SerializeExposedProperty(original));

        Assert.That(roundtrip.Id, Is.EqualTo(original.Id));
        Assert.That(roundtrip.Name, Is.EqualTo(original.Name));
        Assert.That(roundtrip.Type, Is.EqualTo(original.Type));
        Assert.That(roundtrip.IsArray, Is.EqualTo(original.IsArray));
        Assert.That(roundtrip.DefaultValue, Is.EqualTo(original.DefaultValue));
        Assert.That(roundtrip.MinValue, Is.EqualTo(original.MinValue));
        Assert.That(roundtrip.MaxValue, Is.EqualTo(original.MaxValue));
        Assert.That(roundtrip.Step, Is.EqualTo(original.Step));
        Assert.That(roundtrip.Bindings.Count, Is.EqualTo(1));
        Assert.That(roundtrip.Bindings[0].TargetItemId, Is.EqualTo(original.Bindings[0].TargetItemId));
        Assert.That(roundtrip.Bindings[0].TargetProperty, Is.EqualTo("Width"));
    }

    [Test]
    public void DeserializePartFile_BoilersGraphicsPartルートを期待する()
    {
        var def = new PartDefinition { Name = "テスト" };
        var xml = PartSerializer.SerializePartFile(def);

        var roundtrip = PartDeserializer.DeserializePartFile(xml);

        Assert.That(roundtrip.Name, Is.EqualTo("テスト"));
        Assert.That(roundtrip.Id, Is.EqualTo(def.Id));
    }

    [Test]
    public void DeserializePartFile_誤ったルートでInvalidOperationExceptionをスローする()
    {
        var wrong = new XElement("WrongRoot");
        Assert.Throws<InvalidOperationException>(() => PartDeserializer.DeserializePartFile(wrong));
    }

    [Test]
    public void DeserializeAll_PartDefinitionsの全件を返す()
    {
        var d1 = new PartDefinition { Name = "A" };
        var d2 = new PartDefinition { Name = "B" };
        var xml = PartSerializer.SerializeAll(new[] { d1, d2 });

        var result = PartDeserializer.DeserializeAll(xml).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("A"));
        Assert.That(result[1].Name, Is.EqualTo("B"));
    }

    [Test]
    public void DeserializeAll_nullで空列挙を返す()
    {
        var result = PartDeserializer.DeserializeAll(null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ParseValue_全6型のパース()
    {
        Assert.That(PartDeserializer.ParseValue(ExposedPropertyType.Double, "1.5"), Is.EqualTo(1.5));
        Assert.That(PartDeserializer.ParseValue(ExposedPropertyType.Int, "42"), Is.EqualTo(42));
        Assert.That(PartDeserializer.ParseValue(ExposedPropertyType.Boolean, "True"), Is.EqualTo(true));
        Assert.That(PartDeserializer.ParseValue(ExposedPropertyType.String, "hello"), Is.EqualTo("hello"));
        Assert.That(PartDeserializer.ParseValue(ExposedPropertyType.Point, "1.5,2.5"), Is.EqualTo(new Point(1.5, 2.5)));
        Assert.That(PartDeserializer.ParseValue(ExposedPropertyType.Color, "#FFFF0000"), Is.EqualTo(Colors.Red));
    }

    [Test]
    public void ParseValue_RGB6桁色は不透明として解釈する()
    {
        var c = (Color)PartDeserializer.ParseValue(ExposedPropertyType.Color, "00FF00");
        Assert.That(c, Is.EqualTo(Color.FromArgb(0xFF, 0x00, 0xFF, 0x00)));
    }

    [Test]
    public void ParameterValue_値型ごとのラウンドトリップ()
    {
        AssertRoundTrip(1.5, ExposedPropertyType.Double);
        AssertRoundTrip(42, ExposedPropertyType.Int);
        AssertRoundTrip(true, ExposedPropertyType.Boolean);
        AssertRoundTrip("hello", ExposedPropertyType.String);
        AssertRoundTrip(new Point(1.5, 2.5), ExposedPropertyType.Point);
        AssertRoundTrip(Colors.Red, ExposedPropertyType.Color);
    }

    private static void AssertRoundTrip(object value, ExposedPropertyType expectedType)
    {
        var id = Guid.NewGuid();
        var elm = PartSerializer.SerializeParameterValue(id, value);

        Assert.That(elm.Attribute("Type")?.Value, Is.EqualTo(expectedType.ToString()));
        var parsed = PartDeserializer.ParseValue(expectedType, elm.Value);
        Assert.That(parsed, Is.EqualTo(value));
    }
}
