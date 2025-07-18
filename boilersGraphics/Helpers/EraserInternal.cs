using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using TsOperationHistory.Extensions;
using ZLinq;

namespace boilersGraphics.Helpers;

public static class EraserInternal
{
    public static void EraseAtDown(MainWindowViewModel mainWindowViewModel,
        ref SelectableDesignerItemViewModelBase currentBrush, Point point, FrameworkElement views,
        Func<Point, PathGeometry> template)
    {
        var item = views.DataContext as DesignerItemViewModelBase;
        if (currentBrush is DesignerItemViewModelBase designer)
        {
            if (item.RotationAngle.Value == 0)
            {
                point = RotatePoint(point, item.CenterPoint.Value, -item.RotationAngle.Value);
            }
            else
            {
                point = new RotateTransform(-item.RotationAngle.Value, item.CenterPoint.Value.X,
                    item.CenterPoint.Value.Y).Transform(point);
                point = new TranslateTransform(-item.Left.Value, -item.Top.Value).Transform(point);
            }
        }

        mainWindowViewModel.Recorder.Current.ExecuteSetProperty(item, "PathGeometryNoRotate.Value",
            Geometry.Combine(item.PathGeometryNoRotate.Value, template.Invoke(point), GeometryCombineMode.Exclude,
                null));
        if (currentBrush.RotationAngle.Value != 0)
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(item, "PathGeometryRotate.Value",
                Geometry.Combine(item.PathGeometryRotate.Value, template.Invoke(point), GeometryCombineMode.Exclude,
                    null));
        currentBrush = item;
    }

    public static void Erase(MainWindowViewModel mainWindowViewModel,
        ref SelectableDesignerItemViewModelBase currentBrush, Point point, Func<Point, PathGeometry> template)
    {
        if (currentBrush is DesignerItemViewModelBase designer)
        {
            if (designer.RotationAngle.Value == 0)
            {
                point = new Point(point.X - designer.Left.Value, point.Y - designer.Top.Value);
            }
            else
            {
                point = new RotateTransform(-designer.RotationAngle.Value, designer.CenterPoint.Value.X,
                    designer.CenterPoint.Value.Y).Transform(point);
                point = new TranslateTransform(-designer.Left.Value, -designer.Top.Value).Transform(point);
            }
        }

        mainWindowViewModel.Recorder.Current.ExecuteSetProperty(currentBrush, "PathGeometryNoRotate.Value",
            Geometry.Combine(currentBrush.PathGeometryNoRotate.Value, template.Invoke(point),
                GeometryCombineMode.Exclude, null));
        if (currentBrush.RotationAngle.Value != 0)
            mainWindowViewModel.Recorder.Current.ExecuteSetProperty(currentBrush, "PathGeometryRotate.Value",
                Geometry.Combine(currentBrush.PathGeometryRotate.Value, template.Invoke(point),
                    GeometryCombineMode.Exclude, null));
    }

    private static Point RotatePoint(Point point, Point origin, double angle)
    {
        angle = angle * Math.PI / 180.0;
        return new Point(Math.Cos(angle) * (point.X - origin.X) - Math.Sin(angle) * (point.Y - origin.Y) + origin.X,
            Math.Sin(angle) * (point.X - origin.X) + Math.Cos(angle) * (point.Y - origin.Y) + origin.Y);
    }

    public static void Down(MainWindowViewModel mainWindowViewModel, DesignerCanvas AssociatedObject,
        ref SelectableDesignerItemViewModelBase currentBrush, RoutedEventArgs e, Point point)
    {
        var selectedDataContext = mainWindowViewModel.DiagramViewModel.AllItems.Value.AsValueEnumerable().Where(x => x.IsSelected.Value)
            .Select(x => x);
        if (selectedDataContext.Count() > 0)
        {
            var view = AssociatedObject.GetVisualChild<FrameworkElement>(selectedDataContext.First());
            var filtered = view;
            if (selectedDataContext.First() is DesignerItemViewModelBase designer && currentBrush is BrushViewModel bvm)
                if (filtered is not null)
                {
                    var selectable = selectedDataContext.First();
                    EraseAtDown(mainWindowViewModel, ref selectable, point, view,
                        p => GeometryCreator.CreateEllipse(p.X, p.Y, bvm.Thickness.Value));
                    e.Handled = true;
                }
        }
    }
}