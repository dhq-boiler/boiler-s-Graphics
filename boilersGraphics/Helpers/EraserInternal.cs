using boilersGraphics.Controls;
using boilersGraphics.Extensions;
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
    public static class EraserInternal
    {
        public static void Erase(MainWindowViewModel mainWindowViewModel, ref BrushViewModel currentBrush, Point point, IEnumerable<FrameworkElement> views)
        {
            var item = views.First().DataContext as BrushViewModel;
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(item, "PathGeometry.Value", Geometry.Combine(item.PathGeometry.Value, GeometryCreator.CreateEllipse(point.X, point.Y, item.Thickness.Value), GeometryCombineMode.Exclude, null));
            currentBrush = item;
        }

        public static void Erase(MainWindowViewModel mainWindowViewModel, ref BrushViewModel currentBrush, Point point)
        {
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(currentBrush, "PathGeometry.Value", Geometry.Combine(currentBrush.PathGeometry.Value, GeometryCreator.CreateEllipse(point.X, point.Y, currentBrush.Thickness.Value), GeometryCombineMode.Exclude, null));
        }

        public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject, ref BrushViewModel currentBrush, RoutedEventArgs e, Point point)
        {
            var selectedDataContext = mainWindowViewModel.DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            if (selectedDataContext.Count() > 0)
            {
                var views = AssociatedObject.GetCorrespondingViews<FrameworkElement>(selectedDataContext.First());
                var filtered = views.Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (filtered.Any())
                {
                    Erase(mainWindowViewModel, ref currentBrush, point, views);
                    e.Handled = true;
                }
            }
        }
    }
}
