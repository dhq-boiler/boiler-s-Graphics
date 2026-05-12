using boilersGraphics.Models.Parts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Helpers.Parts;

internal static class PartDeserializer
{
    public static IEnumerable<PartDefinition> DeserializeAll(XElement partDefinitionsElement)
    {
        if (partDefinitionsElement is null) yield break;
        foreach (var def in partDefinitionsElement.Elements("PartDefinition"))
            yield return DeserializeDefinition(def);
    }

    public static PartDefinition DeserializePartFile(XElement root)
    {
        if (root is null) throw new ArgumentNullException(nameof(root));
        if (root.Name != PartSerializer.PartFileRoot)
            throw new InvalidOperationException(
                $"Expected root element '{PartSerializer.PartFileRoot}', got '{root.Name}'.");

        var defElement = root.Element("PartDefinition")
                         ?? throw new InvalidOperationException(
                             "Missing <PartDefinition> child in part file.");
        return DeserializeDefinition(defElement);
    }

    public static PartDefinition DeserializeDefinition(XElement element)
    {
        if (element is null) throw new ArgumentNullException(nameof(element));

        var def = new PartDefinition();
        var idAttr = element.Attribute("Id");
        if (idAttr is not null && Guid.TryParse(idAttr.Value, out var id))
            def.Id = id;
        def.Name = element.Element("Name")?.Value;

        var exposedRoot = element.Element("ExposedProperties");
        if (exposedRoot is not null)
        {
            foreach (var ep in exposedRoot.Elements("ExposedProperty"))
                def.ExposedProperties.Add(DeserializeExposedProperty(ep));
        }

        return def;
    }

    public static ExposedProperty DeserializeExposedProperty(XElement element)
    {
        var ep = new ExposedProperty();
        var idAttr = element.Attribute("Id");
        if (idAttr is not null && Guid.TryParse(idAttr.Value, out var id))
            ep.Id = id;
        ep.Name = element.Element("Name")?.Value;

        var typeText = element.Element("Type")?.Value;
        if (!string.IsNullOrEmpty(typeText) &&
            Enum.TryParse<ExposedPropertyType>(typeText, out var type))
            ep.Type = type;

        var isArrayText = element.Element("IsArray")?.Value;
        if (!string.IsNullOrEmpty(isArrayText) && bool.TryParse(isArrayText, out var isArray))
            ep.IsArray = isArray;

        var defaultElm = element.Element("DefaultValue");
        if (defaultElm is not null)
            ep.DefaultValue = ParseTypedValue(defaultElm, ep.Type);

        ep.MinValue = ParseNullableDouble(element.Element("MinValue")?.Value);
        ep.MaxValue = ParseNullableDouble(element.Element("MaxValue")?.Value);
        ep.Step = ParseNullableDouble(element.Element("Step")?.Value);

        var bindingsRoot = element.Element("Bindings");
        if (bindingsRoot is not null)
        {
            foreach (var b in bindingsRoot.Elements("Binding"))
                ep.Bindings.Add(DeserializeBinding(b));
        }

        return ep;
    }

    public static Binding DeserializeBinding(XElement element)
    {
        var b = new Binding();
        var idText = element.Element("TargetItemId")?.Value;
        if (!string.IsNullOrEmpty(idText) && Guid.TryParse(idText, out var id))
            b.TargetItemId = id;
        b.TargetProperty = element.Element("TargetProperty")?.Value;
        return b;
    }

    public static object ParseTypedValue(XElement element, ExposedPropertyType fallback)
    {
        var typeAttr = element.Attribute("Type")?.Value;
        var type = fallback;
        if (!string.IsNullOrEmpty(typeAttr) &&
            Enum.TryParse<ExposedPropertyType>(typeAttr, out var parsed))
            type = parsed;
        return ParseValue(type, element.Value);
    }

    public static object ParseValue(ExposedPropertyType type, string text)
    {
        if (text is null) return null;
        return type switch
        {
            ExposedPropertyType.Double => double.Parse(text, CultureInfo.InvariantCulture),
            ExposedPropertyType.Int => int.Parse(text, CultureInfo.InvariantCulture),
            ExposedPropertyType.Boolean => bool.Parse(text),
            ExposedPropertyType.String => text,
            ExposedPropertyType.Point => ParsePoint(text),
            ExposedPropertyType.Color => ParseColor(text),
            _ => text,
        };
    }

    private static double? ParseNullableDouble(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        return double.Parse(text, CultureInfo.InvariantCulture);
    }

    private static Point ParsePoint(string text)
    {
        var parts = text.Split(',');
        return new Point(
            double.Parse(parts[0], CultureInfo.InvariantCulture),
            double.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    private static Color ParseColor(string text)
    {
        text = text.TrimStart('#');
        byte a = 0xFF, r, g, b;
        if (text.Length == 8)
        {
            a = byte.Parse(text.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            r = byte.Parse(text.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            g = byte.Parse(text.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            b = byte.Parse(text.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        else
        {
            r = byte.Parse(text.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            g = byte.Parse(text.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            b = byte.Parse(text.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        return Color.FromArgb(a, r, g, b);
    }
}
