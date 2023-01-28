using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace boilersGraphics.Views;

[ContentProperty("Items")]
internal class ComboBoxItemTemplateSelector : DataTemplateSelector
{
    public ComboBoxItemTemplateSelector()
    {
        Items = new List<DataTemplate>();
    }

    public List<DataTemplate> Items { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is null)
            return null;

        var template = Items.Find(s => item.GetType().Equals(s.DataType));
        if (template is not null) return template;

        return base.SelectTemplate(item, container);
    }
}