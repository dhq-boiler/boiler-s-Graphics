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
        public static void EraseAtDown(MainWindowViewModel mainWindowViewModel, ref SelectableDesignerItemViewModelBase currentBrush, Point point, IEnumerable<FrameworkElement> views, Func<Point, PathGeometry> template)
        {
            var item = views.First().DataContext as DesignerItemViewModelBase;
            if (currentBrush is DesignerItemViewModelBase designer)
            {
                if (item.RotationAngle.Value == 0)
                {
                    point = RotatePoint(point, item.CenterPoint.Value, -item.RotationAngle.Value);
                }
                else
                {
                    point = new RotateTransform(-item.RotationAngle.Value, item.CenterPoint.Value.X, item.CenterPoint.Value.Y).Transform(point);
                    point = new TranslateTransform(-item.Left.Value, -item.Top.Value).Transform(point);
                }
            }
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
                if (designer.RotationAngle.Value == 0)
                {
                    point = new Point(point.X - designer.Left.Value, point.Y - designer.Top.Value);
                }
                else
                {
                    point = new RotateTransform(-designer.RotationAngle.Value, designer.CenterPoint.Value.X, designer.CenterPoint.Value.Y).Transform(point);
                    point = new TranslateTransform(-designer.Left.Value, -designer.Top.Value).Transform(point);
                }
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
                        EraseAtDown(mainWindowViewModel, ref currentBrush, point, views, (p) => GeometryCreator.CreateEllipse(p.X, p.Y, bvm.Thickness.Value));
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
