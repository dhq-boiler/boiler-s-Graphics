using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace boilersGraphics.Views
{
    [ContentProperty("Items")]
    internal class ItemsControlTemplateSelector : DataTemplateSelector
    {
        public List<DataTemplate> Items { get; set; }

        public ItemsControlTemplateSelector()
        {
            Items = new List<DataTemplate>();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            var template = Items.Find(s => item.GetType().Equals(s.DataType));
            if (template != null) return template;

            return base.SelectTemplate(item, container);
        }
    }
}
