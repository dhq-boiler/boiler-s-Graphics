using boilersGraphics.Models.Parts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Helpers.Parts;

internal static class PartSerializer
{
    public const string PartFileVersion = "1";

    public const string PartFileRoot = "BoilersGraphicsPart";

    public static XElement SerializeAll(IEnumerable<PartDefinition> definitions)
    {
        return new XElement("PartDefinitions",
            definitions.Select(SerializeDefinition));
    }

    public static XElement SerializePartFile(PartDefinition definition)
    {
        if (definition is null) throw new ArgumentNullException(nameof(definition));
        return new XElement(PartFileRoot,
            new XAttribute("Version", PartFileVersion),
            SerializeDefinition(definition));
    }

    public static XElement SerializeDefinition(PartDefinition definition)
    {
        if (definition is null) throw new ArgumentNullException(nameof(definition));

        return new XElement("PartDefinition",
            new XAttribute("Id", definition.Id),
            new XElement("Name", definition.Name ?? string.Empty),
            new XElement("Items"),
            new XElement("ExposedProperties",
                definition.ExposedProperties.Select(SerializeExposedProperty)));
    }

    public static XElement SerializeExposedProperty(ExposedProperty property)
    {
        var element = new XElement("ExposedProperty",
            new XAttribute("Id", property.Id),
            new XElement("Name", property.Name ?? string.Empty),
            new XElement("Type", property.Type.ToString()),
            new XElement("IsArray", property.IsArray));

        if (property.DefaultValue is not null)
            element.Add(SerializeTypedValue("DefaultValue", property.Type, property.DefaultValue, property.IsArray));

        if (property.MinValue.HasValue)
            element.Add(new XElement("MinValue", property.MinValue.Value.ToString("R", CultureInfo.InvariantCulture)));
        if (property.MaxValue.HasValue)
            element.Add(new XElement("MaxValue", property.MaxValue.Value.ToString("R", CultureInfo.InvariantCulture)));
        if (property.Step.HasValue)
            element.Add(new XElement("Step", property.Step.Value.ToString("R", CultureInfo.InvariantCulture)));

        element.Add(new XElement("Bindings",
            property.Bindings.Select(SerializeBinding)));

        return element;
    }

    public static XElement SerializeBinding(Binding binding)
    {
        return new XElement("Binding",
            new XElement("TargetItemId", binding.TargetItemId),
            new XElement("TargetProperty", binding.TargetProperty ?? string.Empty));
    }

    public static XElement SerializeTypedValue(string elementName, ExposedPropertyType type, object value, bool isArray = false)
    {
        var elm = new XElement(elementName, new XAttribute("Type", type.ToString()));
        if (isArray)
        {
            elm.SetAttributeValue("IsArray", true);
            if (value is IEnumerable items && value is not string)
            {
                foreach (var item in items)
                    elm.Add(new XElement("Item", FormatValue(type, item)));
            }
        }
        else
        {
            elm.Value = FormatValue(type, value);
        }
        return elm;
    }

    public static string FormatValue(ExposedPropertyType type, object value)
    {
        if (value is null) return string.Empty;
        return type switch
        {
            ExposedPropertyType.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture)
                                            .ToString("R", CultureInfo.InvariantCulture),
            ExposedPropertyType.Int => Convert.ToInt32(value, CultureInfo.InvariantCulture)
                                            .ToString(CultureInfo.InvariantCulture),
            ExposedPropertyType.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                                            .ToString(CultureInfo.InvariantCulture),
            ExposedPropertyType.String => value.ToString() ?? string.Empty,
            ExposedPropertyType.Point => FormatPoint((Point)value),
            ExposedPropertyType.Color => FormatColor((Color)value),
            _ => value.ToString() ?? string.Empty,
        };
    }

    private static string FormatPoint(Point p)
        => string.Format(CultureInfo.InvariantCulture, "{0:R},{1:R}", p.X, p.Y);

    private static string FormatColor(Color c)
        => string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);

    public static ExposedPropertyType InferType(object value) => value switch
    {
        double => ExposedPropertyType.Double,
        int => ExposedPropertyType.Int,
        bool => ExposedPropertyType.Boolean,
        Point => ExposedPropertyType.Point,
        Color => ExposedPropertyType.Color,
        _ => ExposedPropertyType.String,
    };

    public static XElement SerializeParameterValue(Guid exposedPropertyId, object value)
    {
        var elm = new XElement("ParameterValue",
            new XAttribute("ExposedPropertyId", exposedPropertyId));
        if (value is null) return elm;

        if (value is IEnumerable items && value is not string)
        {
            var list = items.Cast<object>().ToList();
            var elementType = list.Count > 0
                ? InferType(list[0])
                : ExposedPropertyType.String;
            elm.SetAttributeValue("Type", elementType.ToString());
            elm.SetAttributeValue("IsArray", true);
            foreach (var item in list)
                elm.Add(new XElement("Item", FormatValue(elementType, item)));
            return elm;
        }

        var type = InferType(value);
        elm.SetAttributeValue("Type", type.ToString());
        elm.Value = FormatValue(type, value);
        return elm;
    }
}
