using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.AttachedProperties
{
    public static class SelectionProps
    {
        #region EnabledForSelection

        public static readonly DependencyProperty EnabledForSelectionProperty =
            DependencyProperty.RegisterAttached("EnabledForSelection", typeof(bool), typeof(SelectionProps),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnEnabledForSelectionChanged)));

        public static bool GetEnabledForSelection(DependencyObject d)
        {
            return (bool)d.GetValue(EnabledForSelectionProperty);
        }

        public static void SetEnabledForSelection(DependencyObject d, bool value)
        {
            d.SetValue(EnabledForSelectionProperty, value);
        }

        private static void OnEnabledForSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)d;
            if ((bool)e.NewValue)
            {
                fe.PreviewMouseLeftButtonDown += Fe_PreviewMouseLeftButtonDown;
            }
            else
            {
                fe.PreviewMouseLeftButtonDown -= Fe_PreviewMouseLeftButtonDown;
            }
        }

        #endregion

        private static void Fe_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectableDesignerItemViewModelBase selectableDesignerItemViewModelBase =
                (SelectableDesignerItemViewModelBase)((FrameworkElement)sender).DataContext;

            if (selectableDesignerItemViewModelBase != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                {
                    if ((Keyboard.Modifiers & (ModifierKeys.Shift)) != ModifierKeys.None)
                    {
                        selectableDesignerItemViewModelBase.IsSelected.Value = !selectableDesignerItemViewModelBase.IsSelected.Value;
                    }

                    if ((Keyboard.Modifiers & (ModifierKeys.Control)) != ModifierKeys.None)
                    {
                        selectableDesignerItemViewModelBase.IsSelected.Value = !selectableDesignerItemViewModelBase.IsSelected.Value;
                    }

                    selectableDesignerItemViewModelBase.SelectedOrder.Value = SelectableDesignerItemViewModelBase.SelectedOrderCount++ + 1;

                    var diagramVM = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                    diagramVM.Layers.ToList().ForEach(x => x.IsSelected.Value = false);
                    
                    var layerItem = diagramVM.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                                    .First(x => x is LayerItem && (x as LayerItem).Item.Value == selectableDesignerItemViewModelBase);
                    layerItem.IsSelected.Value = true;
                    diagramVM.Layers.Where(x => layerItem.HasAsAncestor(x)).ToList().ForEach(x => x.IsSelected.Value = true);

                    var owner = selectableDesignerItemViewModelBase.Owner;
                    var edgeThicknesses = owner.SelectedItems.Value.Select(x =>
                    {
                        if (x is DesignerItemViewModelBase d)
                        {
                            return d.EdgeThickness.Value;
                        }
                        else if (x is ConnectorBaseViewModel c)
                        {
                            return c.EdgeThickness.Value;
                        }
                        else if (x is SnapPointViewModel s)
                        {
                            return s.Parent.Value.EdgeThickness.Value;
                        }
                        else
                            return 0d;
                    });
                    if (edgeThicknesses.Count() > 0 && edgeThicknesses.All(x => x == edgeThicknesses.First()))
                    {
                        owner.EdgeThickness.Value = edgeThicknesses.First();
                    }
                    else
                    {
                        owner.EdgeThickness.Value = null;
                    }
                }
                else
                {
                    selectableDesignerItemViewModelBase.Owner.DeselectAll();
                    selectableDesignerItemViewModelBase.Owner.EdgeThickness.Value = double.NaN;
                    selectableDesignerItemViewModelBase.IsSelected.Value = true;
                    var view = App.Current.MainWindow.GetCorrespondingViews<FrameworkElement>(selectableDesignerItemViewModelBase)
                                                     .Where(x => x.GetType() == selectableDesignerItemViewModelBase.GetViewType()).First();
                    view.Focus();
                    var diagramVM = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                    diagramVM.Layers.ToList().ForEach(x => x.IsSelected.Value = false);
                    var layerItem = diagramVM.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value)
                                                    .First(x => x is LayerItem && (x as LayerItem).Item.Value == selectableDesignerItemViewModelBase);
                    layerItem.IsSelected.Value = true;
                    diagramVM.Layers.Where(x => layerItem.HasAsAncestor(x)).ToList().ForEach(x => x.IsSelected.Value = true);

                    if (selectableDesignerItemViewModelBase is DesignerItemViewModelBase)
                    {
                        var viewModel = selectableDesignerItemViewModelBase as DesignerItemViewModelBase;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"(x, y) = ({viewModel.Left.Value}, {viewModel.Top.Value}) (w, h) = ({viewModel.Width.Value}, {viewModel.Height.Value})";
                    }
                    else if (selectableDesignerItemViewModelBase is ConnectorBaseViewModel)
                    {
                        var viewModel = selectableDesignerItemViewModelBase as ConnectorBaseViewModel;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({viewModel.Points[0].X}, {viewModel.Points[0].Y}) - ({viewModel.Points[1].X}, {viewModel.Points[1].Y})";
                    }

                    Brush edgeBrush = Brushes.Transparent;
                    Brush fillBrush = Brushes.Transparent;
                    if (selectableDesignerItemViewModelBase is DesignerItemViewModelBase)
                    {
                        edgeBrush = (selectableDesignerItemViewModelBase as DesignerItemViewModelBase).EdgeBrush.Value;
                        fillBrush = (selectableDesignerItemViewModelBase as DesignerItemViewModelBase).FillBrush.Value;
                    }
                    else if (selectableDesignerItemViewModelBase is ConnectorBaseViewModel)
                    {
                        edgeBrush = (selectableDesignerItemViewModelBase as ConnectorBaseViewModel).EdgeBrush.Value;
                    }
                    selectableDesignerItemViewModelBase.Owner.EdgeBrush.Value = edgeBrush;
                    selectableDesignerItemViewModelBase.Owner.FillBrush.Value = fillBrush;

                    var owner = selectableDesignerItemViewModelBase.Owner;
                    var edgeThicknesses = owner.SelectedItems.Value.Select(x =>
                    {
                        if (x is DesignerItemViewModelBase d)
                        {
                            return d.EdgeThickness.Value;
                        }
                        else if (x is ConnectorBaseViewModel c)
                        {
                            return c.EdgeThickness.Value;
                        }
                        else if (x is SnapPointViewModel s)
                        {
                            return s.Parent.Value.EdgeThickness.Value;
                        }
                        return 0d;
                    })
                    .Where(x => x != double.NaN);
                    if (edgeThicknesses.Count() > 0 && edgeThicknesses.All(x => x == edgeThicknesses.First()))
                    {
                        owner.EdgeThickness.Value = edgeThicknesses.First();
                    }
                }
            }
        }
    }
}
