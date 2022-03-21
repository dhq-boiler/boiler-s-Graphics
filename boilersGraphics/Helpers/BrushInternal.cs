using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Helpers
{
    public static class BrushInternal
    {
        public static void AddNewBrushViewModel(DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, Point point)
        {
            currentBrush.Owner = (AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel;
            currentBrush.Left.Value = 0;
            currentBrush.Top.Value = 0;
            currentBrush.Width.Value = currentBrush.Owner.BackgroundItem.Value.Width.Value;
            currentBrush.Height.Value = currentBrush.Owner.BackgroundItem.Value.Height.Value;
            currentBrush.FillBrush.Value = currentBrush.Owner.FillBrush.Value.Clone();
            currentBrush.EdgeBrush.Value = currentBrush.Owner.EdgeBrush.Value.Clone();
            currentBrush.EdgeThickness.Value = currentBrush.Owner.EdgeThickness.Value.Value;
            currentBrush.ZIndex.Value = currentBrush.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
            currentBrush.PathGeometry.Value = GeometryCreator.CreateEllipse(point.X, point.Y, currentBrush.Thickness.Value);
            currentBrush.IsSelected.Value = true;
            currentBrush.IsVisible.Value = true;
            currentBrush.CanDrag.Value = false;
            currentBrush.Owner.DeselectAll();
            ((AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(currentBrush);
            currentBrush.SetupTimedMethod();
        }

        public static void Draw(MainWindowViewModel mainWindowViewModel, ref BrushViewModel currentBrush, Point point, IEnumerable<FrameworkElement> views)
        {
            var item = views.First().DataContext as BrushViewModel;
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(item, "PathGeometry.Value", Geometry.Combine(item.PathGeometry.Value, GeometryCreator.CreateEllipse(point.X, point.Y, item.Thickness.Value), GeometryCombineMode.Union, null));
            currentBrush = item;
        }

        public static void Draw(MainWindowViewModel mainWindowViewModel, ref BrushViewModel currentBrush, Point point)
        {
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(currentBrush, "PathGeometry.Value", Geometry.Combine(currentBrush.PathGeometry.Value, GeometryCreator.CreateEllipse(point.X, point.Y, currentBrush.Thickness.Value), GeometryCombineMode.Union, null));
        }

        public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, Action captureAction, RoutedEventArgs e, Point point)
        {
            var selectedDataContext = mainWindowViewModel.DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            if (selectedDataContext.Count() > 0)
            {
                var views = AssociatedObject.GetCorrespondingViews<FrameworkElement>(selectedDataContext.First()).Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (!views.Any())
                {
                    captureAction.Invoke();
                    currentBrush = BrushViewModel.CreateInstance();
                    AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                }
                else
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        Draw(mainWindowViewModel, ref currentBrush, point, views);
                    }
                    else
                    {
                        currentBrush = BrushViewModel.CreateInstance();
                        AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                    }
                    e.Handled = true;
                }
            }
            else
            {
                captureAction.Invoke();
                currentBrush = BrushViewModel.CreateInstance();
                AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
            }
        }
    }
}
