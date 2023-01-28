using System.Windows;
using System.Windows.Controls;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Views;

public class OptionsContentTemplateSelector : DataTemplateSelector
{
    public DataTemplate ComboBoxTemplate { get; set; }
    public DataTemplate TextBoxTemplate { get; set; }
    public DataTemplate CheckBoxTemplate { get; set; }
    public DataTemplate ReadOnlyComboBoxTemplate { get; set; }
    public DataTemplate ReadOnlyTextBoxTemplate { get; set; }
    public DataTemplate ReadOnlyCheckBoxTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        // Null value can be passed by IDE designer
        if (item == null) return null;

        var propertyOptionsValueCombination = item as PropertyOptionsValueCombination;

        if (propertyOptionsValueCombination.Type == "ReadOnlyComboBox")
            return ReadOnlyComboBoxTemplate;
        if (propertyOptionsValueCombination.Type == "ReadOnlyTextBox")
            return ReadOnlyTextBoxTemplate;
        if (propertyOptionsValueCombination.Type == "ReadOnlyCheckBox") return ReadOnlyCheckBoxTemplate;
        if (propertyOptionsValueCombination.Type == "ComboBox")
            return ComboBoxTemplate;
        if (propertyOptionsValueCombination.Type == "TextBox")
            return TextBoxTemplate;
        if (propertyOptionsValueCombination.Type == "CheckBox")
            return CheckBoxTemplate;
        return null;
    }
}