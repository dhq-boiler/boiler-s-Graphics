using boilersGraphics.ViewModels;
using DependencyPropertyGenerator;
using System.Windows;
using System.Windows.Input;

namespace boilersGraphics.AttachedProperties;

[AttachedDependencyProperty<bool, FrameworkElement>("EnabledForConnection", DefaultValue = false)]
public static partial class ItemConnectProps
{
    private static void Fe_MouseEnter(object sender, MouseEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is DesignerItemViewModelBase)
        {
            var designerItem = (DesignerItemViewModelBase)((FrameworkElement)sender).DataContext;
            designerItem.ShowConnectors = true;
        }
    }

    private static void Fe_MouseLeave(object sender, MouseEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is DesignerItemViewModelBase)
        {
            var designerItem = (DesignerItemViewModelBase)((FrameworkElement)sender).DataContext;

            designerItem.ShowConnectors = false;
        }
    }

    static partial void OnEnabledForConnectionChanged(FrameworkElement sender, bool newValue)
    {
        if (newValue)
        {
            sender.MouseEnter += Fe_MouseEnter;
            sender.MouseLeave += Fe_MouseLeave;
        }
        else
        {
            sender.MouseEnter -= Fe_MouseEnter;
            sender.MouseLeave -= Fe_MouseLeave;
        }
    }
}
