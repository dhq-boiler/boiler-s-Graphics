using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Math;

namespace boilersGraphics.Helpers
{
    public static class Intersection
    {
        /// <summary>
        /// http://csharphelper.com/blog/2017/08/calculate-where-a-line-segment-and-an-ellipse-intersect-in-c/
        /// </summary>
        /// <param name="ellipse">ellipse</param>
        /// <param name="pt1">beginPoint of tangent</param>
        /// <param name="pt2">endPoint of tangent</param>
        /// <param name="segment_only"></param>
        /// <returns></returns>
        public static Tuple<Point[], double> FindEllipseSegmentIntersections(NEllipseViewModel ellipse, Point pt1, Point pt2, bool segment_only)
        {
            var clone = ellipse.Clone() as NEllipseViewModel;
            // If the ellipse or line segment are empty, return no intersections.
            if ((clone.Width.Value == 0) || (clone.Height.Value == 0) ||
                ((pt1.X == pt2.X) && (pt1.Y == pt2.Y)))
                return new Tuple<Point[], double>(new Point[] { }, double.NaN);

            // Make sure the rectangle has non-negative width and height.
            if (clone.Width.Value < 0)
            {
                clone.Left.Value = clone.Right.Value;
                clone.Width.Value = -clone.Width.Value;
            }
            if (clone.Height.Value < 0)
            {
                clone.Top.Value = clone.Bottom.Value;
                clone.Height.Value = -clone.Height.Value;
            }

            // Translate so the ellipse is centered at the origin.
            double cx = clone.CenterX.Value;
            double cy = clone.CenterY.Value;
            clone.Left.Value -= cx;
            clone.Top.Value -= cy;
            pt1.X -= cx;
            pt1.Y -= cy;
            pt2.X -= cx;
            pt2.Y -= cy;

            // Get the semimajor and semiminor axes.
            double a = clone.Width.Value / 2;
            double b = clone.Height.Value / 2;

            var x1 = pt1.X;
            var y1 = pt1.Y;
            var x2 = pt2.X;
            var y2 = pt2.Y;

            // Calculate the quadratic parameters.
            double A = (x2 - x1) * (x2 - x1) / a / a +
                       (y2 - y1) * (y2 - y1) / b / b;
            double B = 2 * x1 * (x2 - x1) / a / a +
                       2 * y1 * (y2 - y1) / b / b;
            double C = x1 * x1 / a / a + y1 * y1 / b / b - 1;

            // Make a list of t values.
            List<double> t_values = new List<double>();

            // Calculate the discriminant.
            double discriminant = B * B - 4 * A * C;
            LogManager.GetCurrentClassLogger().Debug($"discriminant:{discriminant}");
            if (Abs(discriminant) < 0.1)
            {
                // One real solution.
                t_values.Add(-B / 2 / A);
            }
            else if (discriminant > 0)
            {
                // Two real solutions.
                t_values.Add((double)((-B + Sqrt(discriminant)) / 2 / A));
                t_values.Add((double)((-B - Sqrt(discriminant)) / 2 / A));
            }

            // Convert the t values into points.
            List<Point> points = new List<Point>();
            foreach (double t in t_values)
            {
                // If the points are on the segment (or we
                // don't care if they are), add them to the list.
                if (!segment_only || ((t >= 0f) && (t <= 1f)))
                {
                    double x = pt1.X + (pt2.X - pt1.X) * t + cx;
                    double y = pt1.Y + (pt2.Y - pt1.Y) * t + cy;
                    points.Add(new Point(x, y));
                }
            }

            // Return the points.
            return new Tuple<Point[], double>(points.ToArray(), discriminant);
        }

        public static Tuple<Point[], double> FindEllipseSegmentIntersectionsSupportRotation(NEllipseViewModel ellipse, Point pt1, Point pt2, bool segment_only)
        {
            var clone = ellipse.Clone() as NEllipseViewModel;
            // If the ellipse or line segment are empty, return no intersections.
            if ((clone.Width.Value == 0) || (clone.Height.Value == 0) ||
                ((pt1.X == pt2.X) && (pt1.Y == pt2.Y)))
                return new Tuple<Point[], double>(new Point[] { }, double.NaN);

            // Make sure the rectangle has non-negative width and height.
            if (clone.Width.Value < 0)
            {
                clone.Left.Value = clone.Right.Value;
                clone.Width.Value = -clone.Width.Value;
            }
            if (clone.Height.Value < 0)
            {
                clone.Top.Value = clone.Bottom.Value;
                clone.Height.Value = -clone.Height.Value;
            }

            // Translate so the ellipse is centered at the origin.
            double cx = clone.CenterX.Value;
            double cy = clone.CenterY.Value;
            clone.Left.Value -= cx;
            clone.Top.Value -= cy;
            pt1.X -= cx;
            pt1.Y -= cy;
            pt2.X -= cx;
            pt2.Y -= cy;

            var x1 = pt1.X;
            var y1 = pt1.Y;
            var x2 = pt2.X;
            var y2 = pt2.Y;

            var θ = (ellipse.RotationAngle.Value) * PI / 180.0;

            // Get the semimajor and semiminor axes.
            double a = clone.Width.Value / 2;
            double b = clone.Height.Value / 2;

            // Calculate the quadratic parameters.
            double A = (Pow(Cos(θ) * (x2 - x1), 2) - 2 * Cos(θ) * Sin(θ) * (x2 - x1) * (y2 - y1) + Pow(Sin(θ) * (y2 - y1), 2)) / a / a +
                       (Pow(Sin(θ) * (x2 - x1), 2) + 2 * Sin(θ) * Cos(θ) * (x2 - x1) * (y2 - y1) + Pow(Cos(θ) * (y2 - y1), 2)) / b / b;
            double B = (Pow(Cos(θ), 2) * 2 * x1 * (x2 - x1) - 2 * Cos(θ) * Sin(θ) * (y1 * (x2 - x1) + x1 * (y2 - y1)) + Pow(Sin(θ), 2) * 2 * y1 * (y2 - y1)) / a / a
                     + (Pow(Sin(θ), 2) * 2 * x1 * (x2 - x1) + 2 * Sin(θ) * Cos(θ) * (y1 * (x2 - x1) + x1 * (y2 - y1)) + Pow(Cos(θ), 2) * 2 * y1 * (y2 - y1)) / b / b;
            double C = (Pow(Cos(θ) * x1, 2) - 2 * Cos(θ) * Sin(θ) * x1 * y1 + Pow(Sin(θ) * y1, 2)) / a / a
                     + (Pow(Sin(θ) * x1, 2) + 2 * Sin(θ) * Cos(θ) * x1 * y1 + Pow(Cos(θ) * y1, 2)) / b / b - 1;

            // Make a list of t values.
            List<double> t_values = new List<double>();

            // Calculate the discriminant.
            double discriminant = B * B - 4 * A * C;
            LogManager.GetCurrentClassLogger().Debug($"discriminant:{discriminant}");
            if (Abs(discriminant) < 0.1)
            {
                // One real solution.
                t_values.Add(-B / 2 / A);
            }
            else if (discriminant > 0)
            {
                // Two real solutions.
                t_values.Add((double)((-B + Sqrt(discriminant)) / 2 / A));
                t_values.Add((double)((-B - Sqrt(discriminant)) / 2 / A));
            }

            // Convert the t values into points.
            List<Point> points = new List<Point>();
            foreach (double t in t_values)
            {
                // If the points are on the segment (or we
                // don't care if they are), add them to the list.
                if (!segment_only || ((t >= 0f) && (t <= 1f)))
                {
                    double x = pt1.X + (pt2.X - pt1.X) * t + cx;
                    double y = pt1.Y + (pt2.Y - pt1.Y) * t + cy;
                    points.Add(new Point(x, y));
                }
            }

            // Return the points.
            return new Tuple<Point[], double>(points.ToArray(), discriminant);
        }
    }
}
