using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace grapher.Views
{
    [ContentProperty("Items")]
    class ItemsControlTemplateSelector : DataTemplateSelector
    {
        public List<DataTemplate> Items { get; set; }

        public ItemsControlTemplateSelector()
        {
            Items = new List<DataTemplate>();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var template = Items.Find(s => item.GetType().Equals(s.DataType));
            if (template != null) return template;

            return base.SelectTemplate(item, container);
        }
    }
}
