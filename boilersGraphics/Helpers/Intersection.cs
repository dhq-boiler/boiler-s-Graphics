using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Helpers
{
    public static class Intersection
    {
        public static Point[] FindEllipseSegmentIntersections(NEllipseViewModel rect, StraightConnectorViewModel straightConnectorViewModel, bool segment_only)
        {
            var pt1 = straightConnectorViewModel.Points[0];
            var pt2 = straightConnectorViewModel.Points[1];
            // If the ellipse or line segment are empty, return no intersections.
            if ((rect.Width.Value == 0) || (rect.Height.Value == 0) ||
                ((pt1.X == pt2.X) && (pt1.Y == pt2.Y)))
                return new Point[] { };

            // Make sure the rectangle has non-negative width and height.
            if (rect.Width.Value < 0)
            {
                rect.Left.Value = rect.Right.Value;
                rect.Width.Value = -rect.Width.Value;
            }
            if (rect.Height.Value < 0)
            {
                rect.Top.Value = rect.Bottom.Value;
                rect.Height.Value = -rect.Height.Value;
            }

            // Translate so the ellipse is centered at the origin.
            double cx = rect.CenterX.Value;
            double cy = rect.CenterY.Value;
            rect.Left.Value -= cx;
            rect.Top.Value -= cy;
            pt1.X -= cx;
            pt1.Y -= cy;
            pt2.X -= cx;
            pt2.Y -= cy;

            // Get the semimajor and semiminor axes.
            double a = rect.Width.Value / 2;
            double b = rect.Height.Value / 2;

            // Calculate the quadratic parameters.
            double A = (pt2.X - pt1.X) * (pt2.X - pt1.X) / a / a +
                       (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y) / b / b;
            double B = 2 * pt1.X * (pt2.X - pt1.X) / a / a +
                       2 * pt1.Y * (pt2.Y - pt1.Y) / b / b;
            double C = pt1.X * pt1.X / a / a + pt1.Y * pt1.Y / b / b - 1;

            // Make a list of t values.
            List<double> t_values = new List<double>();

            // Calculate the discriminant.
            double discriminant = B * B - 4 * A * C;
            if (discriminant < 0.000001)
            {
                // One real solution.
                t_values.Add(-B / 2 / A);
            }
            else if (discriminant > 0)
            {
                // Two real solutions.
                t_values.Add((double)((-B + Math.Sqrt(discriminant)) / 2 / A));
                t_values.Add((double)((-B - Math.Sqrt(discriminant)) / 2 / A));
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
            return points.ToArray();
        }
    }
}
