using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    public static class GeometryCreator
    {
        public static PathGeometry CreateEllipse(NEllipseViewModel item)
        {
            return PathGeometry.CreateFromGeometry(new EllipseGeometry(new Point(item.Left.Value + item.Width.Value / 2, item.Top.Value + item.Height.Value / 2), item.Width.Value / 2, item.Height.Value / 2));
        }

        public static PathGeometry CreateEllipse(double centerX, double centerY, Thickness thickness)
        {
            return PathGeometry.CreateFromGeometry(new EllipseGeometry(new Point(centerX - thickness.Left, centerY - thickness.Top), thickness.Left + thickness.Right, thickness.Top + thickness.Bottom));
        }

        public static PathGeometry CreatePolyBezier(PolyBezierViewModel clone)
        {
            var geometry = new PathGeometry();
            var pathFigure = new PathFigure();
            pathFigure.StartPoint = clone.Points.First();
            var pathFigureCollection = new PathFigureCollection();
            var pathSegmentCollection = new PathSegmentCollection();
            pathSegmentCollection.Add(new PolyBezierSegment(clone.Points.Skip(1), true));
            pathFigure.Segments = pathSegmentCollection;
            pathFigureCollection.Add(pathFigure);
            geometry.Figures = pathFigureCollection;
            return geometry;
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

        public static PathGeometry CreateCombineGeometry(PolyBezierViewModel pb)
        {
            Point oneIntersection;
            int beginI = 0;
            int endJ = 0;
            DetectIntersections(pb, ref oneIntersection, ref beginI, ref endJ);

            if (beginI > endJ)
            {
                Swap(ref beginI, ref endJ);
            }

            LogManager.GetCurrentClassLogger().Debug($"oneIntersection:{oneIntersection}");
            LogManager.GetCurrentClassLogger().Debug($"beginI:{beginI}, endJ:{endJ}");

            var segments = ExtractSegment(pb.Points, beginI + 1, endJ);
            LogManager.GetCurrentClassLogger().Debug($"segments count:{segments.Count}");
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(oneIntersection, true, true);
                ctx.PolyBezierTo(segments, true, false);
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
        }

        private static void DetectIntersections(PolyBezierViewModel pb, ref Point oneIntersection, ref int beginI, ref int endJ)
        {
            for (int i = 0; i < pb.Points.Count - 1; i++)
            {
                var pt1 = pb.Points[i];
                var pt2 = pb.Points[i + 1];
                for (int j = 0; j < pb.Points.Count - 1; j++)
                {
                    if (i == j || i + 1 == j || i == j + 1 || (i == endJ && j == beginI)) continue;
                    var pt3 = pb.Points[j];
                    var pt4 = pb.Points[j + 1];
                    if (Intersects(pt1, pt2, pt3, pt4, out var intersection))
                    {
                        oneIntersection = intersection;
                        beginI = i;
                        endJ = j;
                    }
                }
            }
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T _tmp = a;
            a = b;
            b = _tmp;
        }

        private static IList<Point> ExtractSegment(ObservableCollection<Point> points, int beginI, int endJ)
        {
            return points.Skip(beginI).Take(endJ - beginI).ToList();
        }

        static bool Intersects(Point a1, Point a2, Point b1, Point b2, out Point intersection)
        {
            intersection = new Point(0, 0);

            Vector b = a2 - a1;
            Vector d = b2 - b1;
            double bDotDPerp = b.X * d.Y - b.Y * d.X;

            if (bDotDPerp == 0)
                return false;

            Vector c = b1 - a1;
            double t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
            if (t < 0 || t > 1)
                return false;

            double u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
            if (u < 0 || u > 1)
                return false;

            intersection = a1 + t * b;

            return true;
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
                        if (Distance(item1_.Points[0], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.LineTo(item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[0], item2_.Points[1]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[1]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.LineTo(item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                    }
                    else if (item2.GetType() == typeof(BezierCurveViewModel))
                    {
                        var item2_ = item2 as BezierCurveViewModel;
                        if (Distance(item1_.Points[0], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.LineTo(item1_.Points[1], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[0], item2_.Points[1]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.LineTo(item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint2.Value, item2_.ControlPoint1.Value, item2_.Points[0], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[1]) < 5)
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
                        var item2_ = item2 as CombineGeometryViewModel;
                        Point beginPoint = GetBeginPoint(item2_.PathGeometry.Value);
                        ctx.BeginFigure(beginPoint, true, true);
                        foreach (var figure in item2_.PathGeometry.Value.Figures)
                        {
                            foreach (var segment in figure.Segments)
                            {
                                if (segment is ArcSegment arcSegment)
                                {
                                    ctx.ArcTo(arcSegment.Point, arcSegment.Size, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection, true, true);
                                }
                                if (segment is BezierSegment bezierSegment)
                                {
                                    ctx.BezierTo(bezierSegment.Point1, bezierSegment.Point2, bezierSegment.Point3, true, true);
                                }
                                if (segment is LineSegment lineSegment)
                                {
                                    ctx.LineTo(lineSegment.Point, true, true);
                                }
                                if (segment is PolyBezierSegment polyBezierSegment)
                                {
                                    ctx.PolyBezierTo(polyBezierSegment.Points, true, true);
                                }
                                if (segment is PolyLineSegment polyLineSegment)
                                {
                                    ctx.PolyLineTo(polyLineSegment.Points, true, true);
                                }
                                if (segment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
                                {
                                    ctx.PolyQuadraticBezierTo(polyQuadraticBezierSegment.Points, true, true);
                                }
                                if (segment is QuadraticBezierSegment quadraticBezierSegment)
                                {
                                    ctx.QuadraticBezierTo(quadraticBezierSegment.Point1, quadraticBezierSegment.Point2, true, true);
                                }
                            }
                        }
                        ctx.LineTo(item1_.Points[0], true, true);
                        ctx.LineTo(item1_.Points[1], true, true);
                    }
                    else if (item2.GetType() == typeof(PolyBezierViewModel))
                    {
                        var item2_ = item2 as PolyBezierViewModel;
                        Point beginPoint = GetBeginPoint(item1_.PathGeometry.Value);
                        ctx.BeginFigure(beginPoint, true, true);
                        ctx.LineTo(item1_.Points[1], true, false);
                        ctx.PolyBezierTo(item2_.Points.ToList(), true, false);
                    }
                }
                else if (item1.GetType() == typeof(BezierCurveViewModel))
                {
                    var item1_ = item1 as BezierCurveViewModel;
                    if (item2.GetType() == typeof(StraightConnectorViewModel))
                    {
                        var item2_ = item2 as StraightConnectorViewModel;
                        if (Distance(item1_.Points[0], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.BezierTo(item1_.ControlPoint1.Value, item1_.ControlPoint2.Value, item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[0], item2_.Points[1]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[1]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.BezierTo(item1_.ControlPoint1.Value, item1_.ControlPoint2.Value, item1_.Points[1], true, true);
                            ctx.LineTo(item2_.Points[0], true, true);
                        }
                    }
                    else if (item2.GetType() == typeof(BezierCurveViewModel))
                    {
                        var item2_ = item2 as BezierCurveViewModel;
                        if (Distance(item1_.Points[0], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[0]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[0], true, true);
                            ctx.BezierTo(item1_.ControlPoint1.Value, item1_.ControlPoint2.Value, item1_.Points[1], true, true);
                            ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, true);
                        }
                        else if (Distance(item1_.Points[0], item2_.Points[1]) < 5)
                        {
                            ctx.BeginFigure(item1_.Points[1], true, true);
                            ctx.BezierTo(item1_.ControlPoint2.Value, item1_.ControlPoint1.Value, item1_.Points[0], true, true);
                            ctx.BezierTo(item2_.ControlPoint2.Value, item2_.ControlPoint1.Value, item2_.Points[0], true, true);
                        }
                        else if (Distance(item1_.Points[1], item2_.Points[1]) < 5)
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
                    var item1_ = item1 as CombineGeometryViewModel;
                    if (item2.GetType() == typeof(StraightConnectorViewModel))
                    {
                        Point beginPoint = GetBeginPoint(item1_.PathGeometry.Value);
                        ctx.BeginFigure(beginPoint, true, true);
                        foreach (var figure in item1_.PathGeometry.Value.Figures)
                        {
                            foreach (var segment in figure.Segments)
                            {
                                if (segment is ArcSegment arcSegment)
                                {
                                    ctx.ArcTo(arcSegment.Point, arcSegment.Size, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection, true, true);
                                }
                                if (segment is BezierSegment bezierSegment)
                                {
                                    ctx.BezierTo(bezierSegment.Point1, bezierSegment.Point2, bezierSegment.Point3, true, true);
                                }
                                if (segment is LineSegment lineSegment)
                                {
                                    ctx.LineTo(lineSegment.Point, true, true);
                                }
                                if (segment is PolyBezierSegment polyBezierSegment)
                                {
                                    ctx.PolyBezierTo(polyBezierSegment.Points, true, true);
                                }
                                if (segment is PolyLineSegment polyLineSegment)
                                {
                                    ctx.PolyLineTo(polyLineSegment.Points, true, true);
                                }
                                if (segment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
                                {
                                    ctx.PolyQuadraticBezierTo(polyQuadraticBezierSegment.Points, true, true);
                                }
                                if (segment is QuadraticBezierSegment quadraticBezierSegment)
                                {
                                    ctx.QuadraticBezierTo(quadraticBezierSegment.Point1, quadraticBezierSegment.Point2, true, true);
                                }
                            }
                        }
                        var item2_ = item2 as StraightConnectorViewModel;
                        ctx.LineTo(item2_.Points[0], true, true);
                        ctx.LineTo(item2_.Points[1], true, true);
                    }
                    else if (item2.GetType() == typeof(BezierCurveViewModel))
                    {
                        return null;
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
                        return null; //leave it to the Geometry.Combine method
                    }
                }
                else if (item1.GetType() == typeof(PolyBezierViewModel))
                {
                    var item1_ = item1 as PolyBezierViewModel;
                    if (item2.GetType() == typeof(StraightConnectorViewModel))
                    {
                        Point beginPoint = GetBeginPoint(item1_.PathGeometry.Value);
                        ctx.BeginFigure(beginPoint, true, true);
                        foreach (var figure in item1_.PathGeometry.Value.Figures)
                        {
                            foreach (var segment in figure.Segments)
                            {
                                if (segment is ArcSegment arcSegment)
                                {
                                    ctx.ArcTo(arcSegment.Point, arcSegment.Size, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection, true, true);
                                }
                                if (segment is BezierSegment bezierSegment)
                                {
                                    ctx.BezierTo(bezierSegment.Point1, bezierSegment.Point2, bezierSegment.Point3, true, true);
                                }
                                if (segment is LineSegment lineSegment)
                                {
                                    ctx.LineTo(lineSegment.Point, true, true);
                                }
                                if (segment is PolyBezierSegment polyBezierSegment)
                                {
                                    ctx.PolyBezierTo(polyBezierSegment.Points, true, true);
                                }
                                if (segment is PolyLineSegment polyLineSegment)
                                {
                                    ctx.PolyLineTo(polyLineSegment.Points, true, true);
                                }
                                if (segment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
                                {
                                    ctx.PolyQuadraticBezierTo(polyQuadraticBezierSegment.Points, true, true);
                                }
                                if (segment is QuadraticBezierSegment quadraticBezierSegment)
                                {
                                    ctx.QuadraticBezierTo(quadraticBezierSegment.Point1, quadraticBezierSegment.Point2, true, true);
                                }
                            }
                        }
                        var item2_ = item2 as StraightConnectorViewModel;
                        ctx.LineTo(item2_.Points[0], true, true);
                        ctx.LineTo(item2_.Points[1], true, true);
                    }
                    else if (item2.GetType() == typeof(BezierCurveViewModel))
                    {
                        var item2_ = item2 as BezierCurveViewModel;
                        Point beginPoint = GetBeginPoint(item1_.PathGeometry.Value);
                        ctx.BeginFigure(beginPoint, true, true);
                        ctx.PolyBezierTo(item1_.Points.Skip(1).ToList(), true, false);
                        ctx.BezierTo(item2_.ControlPoint1.Value, item2_.ControlPoint2.Value, item2_.Points[1], true, false);
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
                        return null; //leave it to the Geometry.Combine method
                    }
                    else if (item2.GetType() == typeof(PolyBezierViewModel))
                    {
                        var item2_ = item2 as PolyBezierViewModel;
                        Point beginPoint = GetBeginPoint(item1_.PathGeometry.Value);
                        ctx.BeginFigure(beginPoint, true, true);
                        ctx.PolyBezierTo(item1_.Points.Skip(1).ToList(), true, false);
                        ctx.PolyBezierTo(item2_.Points.ToList(), true, false);
                    }
                }
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
        }

        private static double Distance(Point a, Point b)
        {
            var xdiff = a.X - b.X;
            var ydiff = a.Y - b.Y;
            var r = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
            return r;
        }

        private static Point GetBeginPoint(PathGeometry pathGeometry)
        {
            string entire = pathGeometry.ToString();
            string beginPointStr = entire.Substring(entire.IndexOf('M') + 1, GetNextTopCharIndex(entire) - entire.IndexOf('M') - 1);
            string[] split = beginPointStr.Split(',');
            Point beginPoint = new Point(double.Parse(split[0]), double.Parse(split[1]));
            return beginPoint;
        }

        private static int GetNextTopCharIndex(string entire)
        {
            var L = entire.IndexOf('L'); //直線コマンド
            var H = entire.IndexOf('H'); //水平線コマンド
            var V = entire.IndexOf('V'); //垂直線コマンド
            var C = entire.IndexOf('C'); //3 次ベジエ曲線コマンド
            var Q = entire.IndexOf('Q'); //2 次ベジエ曲線コマンド
            var S = entire.IndexOf('S'); //スムーズ 3 次ベジエ曲線コマンド
            var T = entire.IndexOf('T'); //スムーズ 2 次ベジエ曲線コマンド
            var A = entire.IndexOf('A'); //楕円の円弧コマンド
            return Max(L, H, V, C, Q, S, T, A);
        }

        private static int Max(params int[] array)
        {
            var left = array.First();
            foreach (var right in array.Skip(1))
            {
                left = Math.Max(left, right);
            }
            return left;
        }
    }
}
