using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace boilersGraphics.Views
{
    [ContentProperty("Items")]
    internal class ItemsControlItemContainerStyleSelector : StyleSelector
    {
        public List<ItemTypedStyle> Items { get; set; }

        public ItemsControlItemContainerStyleSelector()
        {
            Items = new List<ItemTypedStyle>();
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var styleData = Items.Find(s => item.GetType().Equals(s.DataType));
            if (styleData != null) return styleData.Style;

            return base.SelectStyle(item, container);
        }
    }

    [ContentProperty("Style")]
    public class ItemTypedStyle
    {
        public Type DataType { get; set; }
        public Style Style { get; set; }
    }
}
