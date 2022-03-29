using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Helpers
{
    public static class EraserInternal
    {
        public static void Erase(MainWindowViewModel mainWindowViewModel, ref SelectableDesignerItemViewModelBase currentBrush, Point point, IEnumerable<FrameworkElement> views, Func<PathGeometry> template)
        {
            var item = views.First().DataContext as SelectableDesignerItemViewModelBase;
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(item, "PathGeometry.Value", Geometry.Combine(item.PathGeometry.Value, template.Invoke(), GeometryCombineMode.Exclude, null));
            currentBrush = item;
        }

        public static void Erase(MainWindowViewModel mainWindowViewModel, ref SelectableDesignerItemViewModelBase currentBrush, Point point, Func<PathGeometry> template)
        {
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(currentBrush, "PathGeometry.Value", Geometry.Combine(currentBrush.PathGeometry.Value, template.Invoke(), GeometryCombineMode.Exclude, null));
        }

        public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject, ref SelectableDesignerItemViewModelBase currentBrush, RoutedEventArgs e, Point point)
        {
            var selectedDataContext = mainWindowViewModel.DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            if (selectedDataContext.Count() > 0)
            {
                var views = AssociatedObject.GetCorrespondingViews<FrameworkElement>(selectedDataContext.First());
                var filtered = views.Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (currentBrush is BrushViewModel bvm)
                {
                    if (filtered.Any())
                    {
                        Erase(mainWindowViewModel, ref currentBrush, point, views, () => GeometryCreator.CreateEllipse(point.X - bvm.Width.Value / 2 - bvm.EdgeThickness.Value, point.Y - bvm.Height.Value / 2 - bvm.EdgeThickness.Value, bvm.Thickness.Value));
                        e.Handled = true;
                    }
                }
                else
                {
                    var tempBrush = currentBrush;
                    if (filtered.Any())
                    {
                        Erase(mainWindowViewModel, ref currentBrush, point, views, () => tempBrush.PathGeometry.Value);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
