using System.Windows;
using System.Windows.Input;
using boilersGraphics.ViewModels;

namespace boilersGraphics.AttachedProperties;

public static class ItemConnectProps
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

    #region EnabledForConnection

    public static readonly DependencyProperty EnabledForConnectionProperty =
        DependencyProperty.RegisterAttached("EnabledForConnection", typeof(bool), typeof(ItemConnectProps),
            new FrameworkPropertyMetadata(false,
                OnEnabledForConnectionChanged));

    public static bool GetEnabledForConnection(DependencyObject d)
    {
        return (bool)d.GetValue(EnabledForConnectionProperty);
    }

    public static void SetEnabledForConnection(DependencyObject d, bool value)
    {
        d.SetValue(EnabledForConnectionProperty, value);
    }

    private static void OnEnabledForConnectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var fe = (FrameworkElement)d;


        if ((bool)e.NewValue)
        {
            fe.MouseEnter += Fe_MouseEnter;
            fe.MouseLeave += Fe_MouseLeave;
        }
        else
        {
            fe.MouseEnter -= Fe_MouseEnter;
            fe.MouseLeave -= Fe_MouseLeave;
        }
    }

    #endregion
}