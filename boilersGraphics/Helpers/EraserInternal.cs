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
        public static void Erase(MainWindowViewModel mainWindowViewModel, ref SelectableDesignerItemViewModelBase currentBrush, Point point, IEnumerable<FrameworkElement> views, Func<Point, PathGeometry> template)
        {
            if (currentBrush is DesignerItemViewModelBase designer)
            {
                point = RotatePoint(point, designer.CenterPoint.Value, -designer.RotationAngle.Value);
            }
            var item = views.First().DataContext as SelectableDesignerItemViewModelBase;
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(item, "PathGeometryNoRotate.Value", Geometry.Combine(item.PathGeometryNoRotate.Value, template.Invoke(point), GeometryCombineMode.Exclude, null));
            if (currentBrush.RotationAngle.Value != 0)
            {
                mainWindowViewModel.Recorder.Current.ExecuteSetProperty(item, "PathGeometryRotate.Value", Geometry.Combine(item.PathGeometryRotate.Value, template.Invoke(point), GeometryCombineMode.Exclude, null));
            }
            currentBrush = item;
        }

        public static void Erase(MainWindowViewModel mainWindowViewModel, ref SelectableDesignerItemViewModelBase currentBrush, Point point, Func<Point, PathGeometry> template)
        {
            if (currentBrush is DesignerItemViewModelBase designer)
            {
                point = RotatePoint(point, designer.CenterPoint.Value, -designer.RotationAngle.Value);
            }
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(currentBrush, "PathGeometryNoRotate.Value", Geometry.Combine(currentBrush.PathGeometryNoRotate.Value, template.Invoke(point), GeometryCombineMode.Exclude, null));
            if (currentBrush.RotationAngle.Value != 0)
            {
                mainWindowViewModel.Recorder.Current.ExecuteSetProperty(currentBrush, "PathGeometryRotate.Value", Geometry.Combine(currentBrush.PathGeometryRotate.Value, template.Invoke(point), GeometryCombineMode.Exclude, null));
            }
        }

        private static Point RotatePoint(Point point, Point origin, double angle)
        {
            angle = angle * Math.PI / 180.0;
            return new Point(Math.Cos(angle) * (point.X - origin.X) - Math.Sin(angle) * (point.Y - origin.Y) + origin.X,
                             Math.Sin(angle) * (point.X - origin.X) + Math.Cos(angle) * (point.Y - origin.Y) + origin.Y);
        }

        public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject, ref SelectableDesignerItemViewModelBase currentBrush, RoutedEventArgs e, Point point)
        {
            var selectedDataContext = mainWindowViewModel.DiagramViewModel.AllItems.Value.Where(x => x.IsSelected.Value == true).Select(x => x);
            if (selectedDataContext.Count() > 0)
            {
                var views = AssociatedObject.GetCorrespondingViews<FrameworkElement>(selectedDataContext.First());
                var filtered = views.Where(x => x.GetType() == selectedDataContext.First().GetViewType());
                if (selectedDataContext.First() is DesignerItemViewModelBase designer && currentBrush is BrushViewModel bvm)
                {
                    if (filtered.Any())
                    {
                        Erase(mainWindowViewModel, ref currentBrush, new Point(point.X - designer.Left.Value, point.Y - designer.Top.Value), views, (p) => GeometryCreator.CreateEllipse(p.X, p.Y, bvm.Thickness.Value));
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
