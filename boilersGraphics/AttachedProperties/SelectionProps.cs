using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using DependencyPropertyGenerator;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.AttachedProperties;

[AttachedDependencyProperty<bool, FrameworkElement>("EnabledForSelection", DefaultValue = false)]
public static partial class SelectionProps
{
    private static void Fe_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var selectableDesignerItemViewModelBase =
            (SelectableDesignerItemViewModelBase)((FrameworkElement)sender).DataContext;

        if (selectableDesignerItemViewModelBase != null)
        {
            if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None)
                    selectableDesignerItemViewModelBase.IsSelected.Value =
                        !selectableDesignerItemViewModelBase.IsSelected.Value;

                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
                    selectableDesignerItemViewModelBase.IsSelected.Value =
                        !selectableDesignerItemViewModelBase.IsSelected.Value;

                selectableDesignerItemViewModelBase.SelectedOrder.Value =
                    SelectableDesignerItemViewModelBase.SelectedOrderCount++ + 1;

                var diagramVM = (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                diagramVM.Layers.AsValueEnumerable().ToList().ForEach(x => x.IsSelected.Value = false);

                var layerItem = diagramVM.Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).AsValueEnumerable()
                    .First(x => x is LayerItem && (x as LayerItem).Item.Value == selectableDesignerItemViewModelBase);
                layerItem.IsSelected.Value = true;
                diagramVM.Layers.AsValueEnumerable().Where(x => layerItem.HasAsAncestor(x)).ToList()
                    .ForEach(x => x.IsSelected.Value = true);

                var owner = selectableDesignerItemViewModelBase.Owner;
                var edgeThicknesses = owner.SelectedItems.Value.AsValueEnumerable().Select(x =>
                {
                    if (x is DesignerItemViewModelBase d)
                        return d.EdgeThickness.Value;
                    if (x is ConnectorBaseViewModel c)
                        return c.EdgeThickness.Value;
                    if (x is SnapPointViewModel s)
                        return s.Parent.Value.EdgeThickness.Value;
                    return 0d;
                });
                if (edgeThicknesses.Count() > 0)
                {
                    var edgeThicknessFirst = edgeThicknesses.First();
                    if (edgeThicknesses.All(x => x == edgeThicknessFirst))
                        owner.EdgeThickness.Value = edgeThicknessFirst;
                }
                else
                    owner.EdgeThickness.Value = null;
            }
            else
            {
                selectableDesignerItemViewModelBase.Owner.DeselectAll();
                selectableDesignerItemViewModelBase.Owner.EdgeThickness.Value = double.NaN;
                selectableDesignerItemViewModelBase.IsSelected.Value = true;
                var view = Application.Current.MainWindow
                    .GetVisualChild<FrameworkElement>(selectableDesignerItemViewModelBase);
                view.Focus();
                var diagramVM = (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                diagramVM.Layers.AsValueEnumerable().ToList().ForEach(x => x.IsSelected.Value = false);
                var layerItem = diagramVM.Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).AsValueEnumerable()
                    .FirstOrDefault(x =>
                        x is LayerItem && (x as LayerItem).Item.Value == selectableDesignerItemViewModelBase);
                if (layerItem == null)
                    return;
                layerItem.IsSelected.Value = true;
                diagramVM.Layers.AsValueEnumerable().Where(x => layerItem.HasAsAncestor(x)).ToList()
                    .ForEach(x => x.IsSelected.Value = true);

                if (selectableDesignerItemViewModelBase is DesignerItemViewModelBase)
                {
                    var viewModel = selectableDesignerItemViewModelBase as DesignerItemViewModelBase;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                        $"(x, y) = ({viewModel.Left.Value}, {viewModel.Top.Value}) (w, h) = ({viewModel.Width.Value}, {viewModel.Height.Value})";
                }
                else if (selectableDesignerItemViewModelBase is ConnectorBaseViewModel)
                {
                    var viewModel = selectableDesignerItemViewModelBase as ConnectorBaseViewModel;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                        $"({viewModel.Points[0].X}, {viewModel.Points[0].Y}) - ({viewModel.Points[1].X}, {viewModel.Points[1].Y})";
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
                var edgeThicknesses = owner.SelectedItems.Value.AsValueEnumerable().Select(x =>
                    {
                        if (x is DesignerItemViewModelBase d)
                            return d.EdgeThickness.Value;
                        if (x is ConnectorBaseViewModel c)
                            return c.EdgeThickness.Value;
                        if (x is SnapPointViewModel s) return s.Parent.Value.EdgeThickness.Value;
                        return 0d;
                    })
                    .Where(x => x != double.NaN);
                if (edgeThicknesses.Count() > 0)
                {
                    var edgeThicknessFirst = edgeThicknesses.First();
                    if (edgeThicknesses.All(x => x == edgeThicknessFirst))
                        owner.EdgeThickness.Value = edgeThicknessFirst;
                }
            }
        }
    }

    static partial void OnEnabledForSelectionChanged(FrameworkElement sender, bool newValue)
    {
        if (newValue)
            sender.PreviewMouseLeftButtonDown += Fe_PreviewMouseLeftButtonDown;
        else
            sender.PreviewMouseLeftButtonDown -= Fe_PreviewMouseLeftButtonDown;
    }
}