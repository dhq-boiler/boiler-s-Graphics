using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
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
            currentBrush.Width.Value = currentBrush.Owner.Width;
            currentBrush.Height.Value = currentBrush.Owner.Height;
            currentBrush.FillColor.Value = currentBrush.Owner.FillColors.First();
            currentBrush.EdgeColor.Value = currentBrush.Owner.EdgeColors.First();
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

        public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, MouseButtonEventArgs e, Point point)
        {
            var selectedDataContext = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            var input = AssociatedObject.InputHitTest(point);
            if (selectedDataContext.Count() > 0)
            {
                var views = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<FrameworkElement>(selectedDataContext.First()).Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (!views.Any())
                {
                    e.MouseDevice.Capture(AssociatedObject);
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
                e.MouseDevice.Capture(AssociatedObject);
                currentBrush = BrushViewModel.CreateInstance();
                AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
            }
        }

        public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, StylusDownEventArgs e, Point point)
        {
            var selectedDataContext = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            var input = AssociatedObject.InputHitTest(point);
            if (selectedDataContext.Count() > 0)
            {
                var views = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<FrameworkElement>(selectedDataContext.First()).Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (!views.Any())
                {
                    e.StylusDevice.Capture(AssociatedObject);
                    AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                }
                else
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        BrushInternal.Draw(mainWindowViewModel, ref currentBrush, point, views);
                    }
                    else
                    {
                        AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                    }
                    e.Handled = true;
                }
            }
            else
            {
                e.StylusDevice.Capture(AssociatedObject);
                AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
            }
        }

        public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, TouchEventArgs e, Point point)
        {
            var selectedDataContext = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            var input = AssociatedObject.InputHitTest(point);
            if (selectedDataContext.Count() > 0)
            {
                var views = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<FrameworkElement>(selectedDataContext.First()).Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (!views.Any())
                {
                    e.TouchDevice.Capture(AssociatedObject);
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
                        AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
                    }
                    e.Handled = true;
                }
            }
            else
            {
                e.TouchDevice.Capture(AssociatedObject);
                AddNewBrushViewModel(AssociatedObject, ref currentBrush, point);
            }
        }
    }
}
