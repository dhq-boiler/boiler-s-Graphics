using boilersGraphics.Models;
using boilersGraphics.ViewModels.ColorCorrect;
using boilersGraphics.Views;
using Rulyotano.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Helpers
{
    internal static class Curve
    {
        public static List<InOutPair> CalcInOutPairs(LandmarkControl landmarkControl)
        {
            return landmarkControl.InOutPairs;
        }

        public static List<InOutPair> CalcInOutPairs(PathGeometry myPathGeometry, PathSegmentCollection _myPathSegmentCollection, ToneCurveViewModel.Point beginPoint)
        {
                var myPathFigureCollection = myPathGeometry.Figures;
                var myPathFigure = myPathFigureCollection.AsValueEnumerable().First();
                var segments = myPathFigure.Segments;

                var ret = new List<InOutPair>();
                for (int x = 0; x <= byte.MaxValue; x++)
                {
                    Point P0 = default(Point);
                    foreach (BezierSegment segment in _myPathSegmentCollection.AsValueEnumerable().OfType<BezierSegment>())
                    {
                        if (segment == segments.AsValueEnumerable().First())
                        {
                            P0 = beginPoint.ToPoint();
                        }
                        else
                        {
                            var index = segments.IndexOf(segment);
                            var previous = segments[index - 1] as BezierSegment;
                            P0 = new Point(previous.Point3.X, previous.Point3.Y);
                        }

                        Point P1 = new Point(segment.Point1.X, segment.Point1.Y);
                        Point P2 = new Point(segment.Point2.X, segment.Point2.Y);
                        Point P3 = new Point(segment.Point3.X, segment.Point3.Y);

                        if (x < P0.X || P3.X < x)
                        {
                            continue;
                        }

                        var t = FindT(x, P0, P1, P2, P3);
                        double y = Math.Round(Math.Pow(1 - t, 3) * P0.Y + 3 * Math.Pow(1 - t, 2) * t * P1.Y + 3 * (1 - t) * Math.Pow(t, 2) * P2.Y + Math.Pow(t, 3) * P3.Y);
                        if (y >= byte.MinValue && y <= byte.MaxValue)
                        {
                            if (!ret.AsValueEnumerable().Any(a => a.In == x))
                            {
                                ret.Add(new InOutPair(x, (int)y));
                                break;
                            }
                        }
                    }
                }
                return ret;
        }

        private static double FindT(double x, Point P0, Point P1, Point P2, Point P3)
        {
            double t0 = 0;
            double t1 = 1;
            double precision = 0.0001;

            while (Math.Abs(t1 - t0) > precision)
            {
                double t = (t0 + t1) / 2;
                double xValue = Math.Pow(1 - t, 3) * P0.X + 3 * Math.Pow(1 - t, 2) * t * P1.X + 3 * (1 - t) * Math.Pow(t, 2) * P2.X + Math.Pow(t, 3) * P3.X;

                if (xValue < x)
                {
                    t0 = t;
                }
                else
                {
                    t1 = t;
                }
            }

            return (t0 + t1) / 2;
        }
    }
}
