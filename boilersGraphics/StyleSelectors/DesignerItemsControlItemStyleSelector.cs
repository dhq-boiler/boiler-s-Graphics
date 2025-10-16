using boilersGraphics.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.StyleSelectors;

public class DesignerItemsControlItemStyleSelector : StyleSelector
{
    static DesignerItemsControlItemStyleSelector()
    {
        Instance = new DesignerItemsControlItemStyleSelector();
    }

    public static DesignerItemsControlItemStyleSelector Instance { get; }


    public override Style SelectStyle(object item, DependencyObject container)
    {
        var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
        if (itemsControl == null)
            throw new InvalidOperationException("DesignerItemsControlItemStyleSelector : Could not find ItemsControl");

        if (item is DesignerItemViewModelBase) return (Style)itemsControl.FindResource("designerItemStyle");

        if (item is ConnectorBaseViewModel) return (Style)itemsControl.FindResource("connectorItemStyle");

        if (item is SnapPointViewModel) return (Style)itemsControl.FindResource("snapPointItemStyle");

        return null;
    }
}