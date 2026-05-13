using boilersGraphics.Models.Parts;
using R3;
using System;
using System.Windows;

namespace boilersGraphics.ViewModels.Parts;

public class ExposedParameterValuePropertyOption : PropertyOptionsValueCombination
{
    public Guid ExposedPropertyId { get; }
    public ExposedPropertyType ExposedType { get; }
    public bool IsArray { get; }
    public BindableReactiveProperty<object> PropertyValue { get; }
    public BindableReactiveProperty<HorizontalAlignment> HorizontalContentAlignment { get; } = new();

    public ExposedParameterValuePropertyOption(
        string name,
        Guid exposedPropertyId,
        ExposedPropertyType exposedType,
        bool isArray,
        BindableReactiveProperty<object> propertyValue)
        : base(name)
    {
        ExposedPropertyId = exposedPropertyId;
        ExposedType = exposedType;
        IsArray = isArray;
        PropertyValue = propertyValue ?? throw new ArgumentNullException(nameof(propertyValue));
        HorizontalContentAlignment.Value = exposedType switch
        {
            ExposedPropertyType.Double => System.Windows.HorizontalAlignment.Right,
            ExposedPropertyType.Int => System.Windows.HorizontalAlignment.Right,
            _ => System.Windows.HorizontalAlignment.Left,
        };
    }

    public override string Type
    {
        get
        {
            if (IsArray) return "ReadOnlyTextBox";
            return ExposedType switch
            {
                ExposedPropertyType.Boolean => "CheckBox",
                ExposedPropertyType.Double => "TextBox",
                ExposedPropertyType.Int => "TextBox",
                ExposedPropertyType.String => "TextBox",
                _ => "ReadOnlyTextBox",
            };
        }
    }
}
