using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace boilersGraphics.Helpers
{
    public static class BrushInternal
    {
        public static void AddNewBrushViewModel(DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, Point point)
        {
            var item = new BrushViewModel();
            item.Owner = (AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel;
            item.Left.Value = 0;
            item.Top.Value = 0;
            item.Width.Value = item.Owner.Width;
            item.Height.Value = item.Owner.Height;
            item.FillColor.Value = item.Owner.FillColors.First();
            item.EdgeColor.Value = item.Owner.EdgeColors.First();
            item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
            item.ZIndex.Value = item.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
            item.Thickness.Value = new Thickness(10);
            item.PathGeometry.Value = GeometryCreator.CreateEllipse(point.X, point.Y, item.Thickness.Value);
            item.IsSelected.Value = true;
            item.IsVisible.Value = true;
            item.CanDrag.Value = false;
            item.Owner.DeselectAll();
            ((AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);
            item.SetupTimedMethod();
            currentBrush = item;
        }

        public static void Draw(ref BrushViewModel currentBrush, Point point, IEnumerable<FrameworkElement> views)
        {
            var item = views.First().DataContext as BrushViewModel;
            item.PathGeometry.Value = Geometry.Combine(item.PathGeometry.Value, GeometryCreator.CreateEllipse(point.X, point.Y, item.Thickness.Value), GeometryCombineMode.Union, null);
            currentBrush = item;
        }

        public static void Draw(ref BrushViewModel currentBrush, Point point)
        {
            currentBrush.PathGeometry.Value = Geometry.Combine(currentBrush.PathGeometry.Value, GeometryCreator.CreateEllipse(point.X, point.Y, currentBrush.Thickness.Value), GeometryCombineMode.Union, null);
        }

        public static void Down(DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, MouseButtonEventArgs e, Point point)
        {
            var selectedDataContext = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            var input = AssociatedObject.InputHitTest(point);
            if (selectedDataContext.Count() > 0)
            {
                var views = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<FrameworkElement>(selectedDataContext.First()).Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (!views.Any())
                {
                    e.MouseDevice.Capture(AssociatedObject);
                    BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                }
                else
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        BrushInternal.Draw(ref currentBrush, point, views);
                    }
                    else
                    {
                        BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                    }
                    e.Handled = true;
                }
            }
            else
            {
                e.MouseDevice.Capture(AssociatedObject);
                BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
            }
        }

        public static void Down(DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, StylusDownEventArgs e, Point point)
        {
            var selectedDataContext = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            var input = AssociatedObject.InputHitTest(point);
            if (selectedDataContext.Count() > 0)
            {
                var views = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<FrameworkElement>(selectedDataContext.First()).Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (!views.Any())
                {
                    e.StylusDevice.Capture(AssociatedObject);
                    BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                }
                else
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        BrushInternal.Draw(ref currentBrush, point, views);
                    }
                    else
                    {
                        BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                    }
                    e.Handled = true;
                }
            }
            else
            {
                e.StylusDevice.Capture(AssociatedObject);
                BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
            }
        }

        public static void Down(DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, TouchEventArgs e, Point point)
        {
            var selectedDataContext = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            var input = AssociatedObject.InputHitTest(point);
            if (selectedDataContext.Count() > 0)
            {
                var views = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<FrameworkElement>(selectedDataContext.First()).Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (!views.Any())
                {
                    e.TouchDevice.Capture(AssociatedObject);
                    BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                }
                else
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        BrushInternal.Draw(ref currentBrush, point, views);
                    }
                    else
                    {
                        BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                    }
                    e.Handled = true;
                }
            }
            else
            {
                e.TouchDevice.Capture(AssociatedObject);
                BrushInternal.AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
            }
        }
    }
}
