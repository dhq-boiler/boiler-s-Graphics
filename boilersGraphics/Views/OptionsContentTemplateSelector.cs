using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Views
{
    public class OptionsContentTemplateSelector : DataTemplateSelector
    {
        public OptionsContentTemplateSelector()
        {
        }
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
            {
                return ReadOnlyComboBoxTemplate;
            }
            else if (propertyOptionsValueCombination.Type == "ReadOnlyTextBox")
            {
                return ReadOnlyTextBoxTemplate;
            }
            else if (propertyOptionsValueCombination.Type == "ReadOnlyCheckBox")
            {
                return ReadOnlyCheckBoxTemplate;
            }
            if (propertyOptionsValueCombination.Type == "ComboBox")
            {
                return ComboBoxTemplate;
            }
            else if (propertyOptionsValueCombination.Type == "TextBox")
            {
                return TextBoxTemplate;
            }
            else if (propertyOptionsValueCombination.Type == "CheckBox")
            {
                return CheckBoxTemplate;
            }
            else
            {
                return null;
            }
        }
    }
}
