﻿using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace boilersGraphics.AttachedProperties
{
    public static class ItemConnectProps
    {
        #region EnabledForConnection

        public static readonly DependencyProperty EnabledForConnectionProperty =
            DependencyProperty.RegisterAttached("EnabledForConnection", typeof(bool), typeof(ItemConnectProps),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnEnabledForConnectionChanged)));

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
            FrameworkElement fe = (FrameworkElement)d;


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

        private static void Fe_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is DesignerItemViewModelBase)
            {
                DesignerItemViewModelBase designerItem = (DesignerItemViewModelBase)((FrameworkElement)sender).DataContext;
                designerItem.ShowConnectors = true;
            }
        }

        private static void Fe_MouseLeave(object sender, MouseEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is DesignerItemViewModelBase)
            {
                DesignerItemViewModelBase designerItem = (DesignerItemViewModelBase)((FrameworkElement)sender).DataContext;

                designerItem.ShowConnectors = false;
            }
        }
    }
}
