using boilersGraphics.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    class GeometryCreator
    {
        public static PathGeometry CreateEllipse(NEllipseViewModel item)
        {
            return PathGeometry.CreateFromGeometry(new EllipseGeometry(new Point(item.Left.Value + item.Width.Value / 2, item.Top.Value + item.Height.Value / 2), item.Width.Value / 2, item.Height.Value / 2));
        }

        public static PathGeometry CreateEllipse(NEllipseViewModel item, double angle)
        {
            var ellipse = new EllipseGeometry(new Point(item.Left.Value + item.Width.Value / 2, item.Top.Value + item.Height.Value / 2), item.Width.Value / 2, item.Height.Value / 2, new RotateTransform(angle, item.CenterPoint.Value.X, item.CenterPoint.Value.Y));
            return PathGeometry.CreateFromGeometry(ellipse);
        }

        public static PathGeometry CreateRectangle(NRectangleViewModel item)
        {
            return PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(new Point(item.Left.Value, item.Top.Value), new Point(item.Left.Value + item.Width.Value, item.Top.Value + item.Height.Value))));
        }

        public static PathGeometry CreateRectangle(NRectangleViewModel item, double angle)
        {
            return PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(new Point(item.Left.Value, item.Top.Value), new Point(item.Left.Value + item.Width.Value, item.Top.Value + item.Height.Value)), 0, 0, new RotateTransform(angle, item.CenterPoint.Value.X, item.CenterPoint.Value.Y)));
        }

        private static double CalcRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        public static PathGeometry CreateRectangle(NRectangleViewModel item, double offsetX, double offsetY)
        {
            var geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(item.Left.Value - offsetX, item.Top.Value - offsetY), true, true);
                ctx.LineTo(new Point(item.Left.Value - offsetX + item.Width.Value, item.Top.Value - offsetY), true, false);
                ctx.LineTo(new Point(item.Left.Value - offsetX + item.Width.Value, item.Top.Value - offsetY + item.Height.Value), true, false);
                ctx.LineTo(new Point(item.Left.Value - offsetX, item.Top.Value - offsetY + item.Height.Value), true, false);
                ctx.LineTo(new Point(item.Left.Value - offsetX, item.Top.Value - offsetY), true, false);
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
        }

        public static PathGeometry CreateRectangle(NRectangleViewModel item, double offsetX, double offsetY, string propertyName, double oldItem, double newItem)
        {
            double widthRatio = 1;
            double heightRatio = 1;
            if (propertyName == "Width")
            {
                widthRatio = newItem / oldItem;
            }
            else if (propertyName == "Height")
            {
                heightRatio = newItem / oldItem;
            }
            
            //TODO Rectangleを構成する4点に widthRatio と heightRatio を掛ける
            var geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(widthRatio * (item.Left.Value - offsetX), heightRatio * (item.Top.Value - offsetY)), true, true);
                ctx.LineTo(new Point(widthRatio * (item.Left.Value - offsetX + item.Width.Value), heightRatio * (item.Top.Value - offsetY)), true, false);
                ctx.LineTo(new Point(widthRatio * (item.Left.Value - offsetX + item.Width.Value), heightRatio * (item.Top.Value - offsetY + item.Height.Value)), true, false);
                ctx.LineTo(new Point(widthRatio * (item.Left.Value - offsetX), heightRatio * (item.Top.Value - offsetY + item.Height.Value)), true, false);
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
        }

        public static PathGeometry CreateBezierCurve(BezierCurveViewModel item)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(item.Points[0], true, true);
                ctx.BezierTo(item.ControlPoint1.Value, item.ControlPoint2.Value, item.Points[1], true, false);
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
        }

        internal static PathGeometry CreateLine(StraightConnectorViewModel item)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(item.Points[0], true, true);
                ctx.LineTo(item.Points[1], true, false);
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
        }

        public static PathGeometry CreateCombineGeometry<T1, T2>(T1 item1, T2 item2) where T1 : SelectableDesignerItemViewModelBase
                                                                                     where T2 : SelectableDesignerItemViewModelBase
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                if (item1.GetType() == typeof(StraightConnectorViewModel))
                {
                    var item1_ = item1 as StraightConnectorViewModel;
                    if (item2.GetType() == typeof(StraightConnectorViewModel))
                    {
                        var item2_ = item2 as StraightConnectorViewModel;
                        if (item1_.Points[0] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.LineTo(item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[0] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.LineTo(item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                    }
                    else if (item2.GetType() == typeof(BezierCurveViewModel))
                    {
                        var item2_ = item2 as BezierCurveViewModel;
                        if (item1_.Points[0] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.LineTo(item1_.Points[1], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[0] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint2.Value, item2_.ControlPoint1.Value, item2_.Points[0], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.LineTo(item1_.Points[1], true, true);
                            ctx.BezierTo(item2_.ControlPoint2.Value, item2_.ControlPoint1.Value, item2_.Points[0], true, true);
                        }
                    }
                    else if (item2.GetType() == typeof(NRectangleViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(NEllipseViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(NPolygonViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(LetterDesignerItemViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(LetterVerticalDesignerItemViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(CombineGeometryViewModel))
                    {
                        return null;
                    }
                }
                else if (item1.GetType() == typeof(BezierCurveViewModel))
                {
                    var item1_ = item1 as BezierCurveViewModel;
                    if (item2.GetType() == typeof(StraightConnectorViewModel))
                    {
                        var item2_ = item2 as StraightConnectorViewModel;
                        if (item1_.Points[0] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.BezierTo(item1_.ControlPoint1.Value, item1_.ControlPoint2.Value, item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[0] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.BezierTo(item1_.ControlPoint1.Value, item1_.ControlPoint2.Value, item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                    }
                    else if (item2.GetType() == typeof(BezierCurveViewModel))
                    {
                        var item2_ = item2 as BezierCurveViewModel;
                        if (item1_.Points[0] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[0])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.BezierTo(item1_.ControlPoint1.Value, item1_.ControlPoint2.Value, item1_.Points[1], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (item1_.Points[0] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint2.Value, item2_.ControlPoint1.Value, item2_.Points[0], true, true);
                        }
                        else if (item1_.Points[1] == item2_.Points[1])
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.BezierTo(item1_.ControlPoint1.Value, item1_.ControlPoint2.Value, item1_.Points[1], true, true);
                            ctx.BezierTo(item2_.ControlPoint2.Value, item2_.ControlPoint1.Value, item2_.Points[0], true, true);
                        }
                    }
                    else if (item2.GetType() == typeof(NRectangleViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(NEllipseViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(NPolygonViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(LetterDesignerItemViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(LetterVerticalDesignerItemViewModel))
                    {
                        return null;
                    }
                    else if (item2.GetType() == typeof(CombineGeometryViewModel))
                    {
                        return null;
                    }
                }
                else if (item1.GetType() == typeof(NRectangleViewModel))
                {
                    return null;
                }
                else if (item1.GetType() == typeof(NEllipseViewModel))
                {
                    return null;
                }
                else if (item1.GetType() == typeof(NPolygonViewModel))
                {
                    return null;
                }
                else if (item1.GetType() == typeof(LetterDesignerItemViewModel))
                {
                    return null;
                }
                else if (item1.GetType() == typeof(LetterVerticalDesignerItemViewModel))
                {
                    return null;
                }
                else if (item1.GetType() == typeof(CombineGeometryViewModel))
                {
                    return null;
                }
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
        }
    }
}
