using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using static System.Math;

namespace boilersGraphics.Helpers;

public static class Intersection
{
    /// <summary>
    ///     http://csharphelper.com/blog/2017/08/calculate-where-a-line-segment-and-an-ellipse-intersect-in-c/
    /// </summary>
    /// <param name="ellipse">ellipse</param>
    /// <param name="pt1">beginPoint of tangent</param>
    /// <param name="pt2">endPoint of tangent</param>
    /// <param name="segment_only"></param>
    /// <returns></returns>
    public static Tuple<Point[], double> FindEllipseSegmentIntersections(NEllipseViewModel ellipse, Point pt1,
        Point pt2, bool segment_only)
    {
        var clone = ellipse.Clone() as NEllipseViewModel;
        // If the ellipse or line segment are empty, return no intersections.
        if (clone.Width.Value == 0 || clone.Height.Value == 0 ||
            (pt1.X == pt2.X && pt1.Y == pt2.Y))
            return new Tuple<Point[], double>(new Point[] { }, double.NaN);

        // Make sure the rectangle has non-negative width and height.
        if (clone.Width.Value < 0)
        {
            clone.Left.Value = clone.Right.CurrentValue;
            clone.Width.Value = -clone.Width.Value;
        }

        if (clone.Height.Value < 0)
        {
            clone.Top.Value = clone.Bottom.CurrentValue;
            clone.Height.Value = -clone.Height.Value;
        }

        // Translate so the ellipse is centered at the origin.
        var cx = clone.CenterX.Value;
        var cy = clone.CenterY.Value;
        clone.Left.Value -= cx;
        clone.Top.Value -= cy;
        pt1.X -= cx;
        pt1.Y -= cy;
        pt2.X -= cx;
        pt2.Y -= cy;

        // Get the semimajor and semiminor axes.
        var a = clone.Width.Value / 2;
        var b = clone.Height.Value / 2;

        var x1 = pt1.X;
        var y1 = pt1.Y;
        var x2 = pt2.X;
        var y2 = pt2.Y;

        // Calculate the quadratic parameters.
        var A = (x2 - x1) * (x2 - x1) / a / a +
                (y2 - y1) * (y2 - y1) / b / b;
        var B = 2 * x1 * (x2 - x1) / a / a +
                2 * y1 * (y2 - y1) / b / b;
        var C = x1 * x1 / a / a + y1 * y1 / b / b - 1;

        // Make a list of t values.
        var t_values = new List<double>();

        // Calculate the discriminant.
        var discriminant = B * B - 4 * A * C;
        LogManager.GetCurrentClassLogger().Debug($"discriminant:{discriminant}");
        if (Abs(discriminant) < 0.1)
        {
            // One real solution.
            t_values.Add(-B / 2 / A);
        }
        else if (discriminant > 0)
        {
            // Two real solutions.
            t_values.Add((-B + Sqrt(discriminant)) / 2 / A);
            t_values.Add((-B - Sqrt(discriminant)) / 2 / A);
        }

        // Convert the t values into points.
        var points = new List<Point>();
        foreach (var t in t_values)
            // If the points are on the segment (or we
            // don't care if they are), add them to the list.
            if (!segment_only || (t >= 0f && t <= 1f))
            {
                var x = pt1.X + (pt2.X - pt1.X) * t + cx;
                var y = pt1.Y + (pt2.Y - pt1.Y) * t + cy;
                points.Add(new Point(x, y));
            }

        // Return the points.
        return new Tuple<Point[], double>(points.ToArray(), discriminant);
    }

    public static Tuple<Point[], double> FindPieSegmentIntersectionsSupportRotationLong(NPieViewModel pie, Point pt1,
        Point pt2, bool segment_only)
    {
        var cloneL = pie.CreateNEllipseViewModelLong();
        // If the ellipse or line segment are empty, return no intersections.
        if (cloneL.Width.Value == 0 || cloneL.Height.Value == 0 ||
            (pt1.X == pt2.X && pt1.Y == pt2.Y))
            return new Tuple<Point[], double>(new Point[] { }, double.NaN);
        double discriminant;
        var points = new List<Point>();
        FindPieSegmentIntersectionsSupportRotationInternal(pie, pt1, pt2, segment_only, cloneL, out discriminant,
            points);

        // Return the points.
        return new Tuple<Point[], double>(points.ToArray(), discriminant);
    }

    public static Tuple<Point[], double> FindPieSegmentIntersectionsSupportRotationShort(NPieViewModel pie, Point pt1,
        Point pt2, bool segment_only)
    {
        var cloneS = pie.CreateNEllipseViewModelShort();
        // If the ellipse or line segment are empty, return no intersections.
        if (cloneS.Width.Value == 0 || cloneS.Height.Value == 0 ||
            (pt1.X == pt2.X && pt1.Y == pt2.Y))
            return new Tuple<Point[], double>(new Point[] { }, double.NaN);
        double discriminant;
        var points = new List<Point>();
        FindPieSegmentIntersectionsSupportRotationInternal(pie, pt1, pt2, segment_only, cloneS, out discriminant,
            points);

        // Return the points.
        return new Tuple<Point[], double>(points.ToArray(), discriminant);
    }

    private static void FindPieSegmentIntersectionsSupportRotationInternal(NPieViewModel pie, Point pt1, Point pt2,
        bool segment_only, NEllipseViewModel clone, out double discriminant, List<Point> points)
    {
        // Make sure the rectangle has non-negative width and height.
        if (clone.Width.Value < 0)
        {
            clone.Left.Value = clone.Right.CurrentValue;
            clone.Width.Value = -clone.Width.Value;
        }

        if (clone.Height.Value < 0)
        {
            clone.Top.Value = clone.Bottom.CurrentValue;
            clone.Height.Value = -clone.Height.Value;
        }

        // Translate so the ellipse is centered at the origin.
        var cx = clone.CenterX.Value;
        var cy = clone.CenterY.Value;
        pt1.X -= cx;
        pt1.Y -= cy;
        pt2.X -= cx;
        pt2.Y -= cy;

        var x1 = pt1.X;
        var y1 = pt1.Y;
        var x2 = pt2.X;
        var y2 = pt2.Y;

        var θ = -pie.RotationAngle.Value * PI / 180.0;

        // Get the semimajor and semiminor axes.
        var a = clone.Width.Value / 2;
        var b = clone.Height.Value / 2;

        var cosθ = Cos(θ);
        var sinθ = Sin(θ);

        // Calculate the quadratic parameters.
        var A =
            (Pow(cosθ * (x2 - x1), 2) - 2 * cosθ * sinθ * (x2 - x1) * (y2 - y1) + Pow(sinθ * (y2 - y1), 2)) / a / a +
            (Pow(sinθ * (x2 - x1), 2) + 2 * sinθ * cosθ * (x2 - x1) * (y2 - y1) + Pow(cosθ * (y2 - y1), 2)) / b / b;
        var B = (Pow(cosθ, 2) * 2 * x1 * (x2 - x1) - 2 * cosθ * sinθ * (y1 * (x2 - x1) + x1 * (y2 - y1)) +
                 Pow(sinθ, 2) * 2 * y1 * (y2 - y1)) / a / a
                + (Pow(sinθ, 2) * 2 * x1 * (x2 - x1) + 2 * sinθ * cosθ * (y1 * (x2 - x1) + x1 * (y2 - y1)) +
                   Pow(cosθ, 2) * 2 * y1 * (y2 - y1)) / b / b;
        var C = (Pow(cosθ * x1, 2) - 2 * cosθ * sinθ * x1 * y1 + Pow(sinθ * y1, 2)) / a / a
            + (Pow(sinθ * x1, 2) + 2 * sinθ * cosθ * x1 * y1 + Pow(cosθ * y1, 2)) / b / b - 1;

        // Make a list of t values.
        var t_values = new List<double>();

        // Calculate the discriminant.
        discriminant = B * B - 4 * A * C;
        LogManager.GetCurrentClassLogger().Debug($"discriminant:{discriminant}");
        var angle = Vector.AngleBetween(new Vector(pt2.X, pt2.Y), new Vector(0, -1));
        if (Abs(discriminant) < 0.1 && angle >= pie.StartDegree.Value + 90 && angle <= pie.EndDegree.Value + 90)
        {
            // One real solution.
            t_values.Add(-B / 2 / A);
        }
        else if (discriminant > 0)
        {
            // Two real solutions.
            t_values.Add((-B + Sqrt(discriminant)) / 2 / A);
            t_values.Add((-B - Sqrt(discriminant)) / 2 / A);
        }

        // Convert the t values into points.
        foreach (var t in t_values)
            // If the points are on the segment (or we
            // don't care if they are), add them to the list.
            if (!segment_only || (t >= 0f && t <= 1f))
            {
                var x = pt1.X + (pt2.X - pt1.X) * t + cx;
                var y = pt1.Y + (pt2.Y - pt1.Y) * t + cy;
                points.Add(new Point(x, y));
            }
    }

    public static Tuple<Point[], double> FindEllipseSegmentIntersectionsSupportRotation(NEllipseViewModel ellipse,
        Point pt1, Point pt2, bool segment_only)
    {
        var clone = ellipse.Clone() as NEllipseViewModel;
        // If the ellipse or line segment are empty, return no intersections.
        if (clone.Width.Value == 0 || clone.Height.Value == 0 ||
            (pt1.X == pt2.X && pt1.Y == pt2.Y))
            return new Tuple<Point[], double>(new Point[] { }, double.NaN);

        // Make sure the rectangle has non-negative width and height.
        if (clone.Width.Value < 0)
        {
            clone.Left.Value = clone.Right.CurrentValue;
            clone.Width.Value = -clone.Width.Value;
        }

        if (clone.Height.Value < 0)
        {
            clone.Top.Value = clone.Bottom.CurrentValue;
            clone.Height.Value = -clone.Height.Value;
        }

        // Translate so the ellipse is centered at the origin.
        var cx = clone.CenterX.Value;
        var cy = clone.CenterY.Value;
        pt1.X -= cx;
        pt1.Y -= cy;
        pt2.X -= cx;
        pt2.Y -= cy;

        var x1 = pt1.X;
        var y1 = pt1.Y;
        var x2 = pt2.X;
        var y2 = pt2.Y;

        var θ = -ellipse.RotationAngle.Value * PI / 180.0;

        // Get the semimajor and semiminor axes.
        var a = clone.Width.Value / 2;
        var b = clone.Height.Value / 2;

        var cosθ = Cos(θ);
        var sinθ = Sin(θ);

        // Calculate the quadratic parameters.
        var A =
            (Pow(cosθ * (x2 - x1), 2) - 2 * cosθ * sinθ * (x2 - x1) * (y2 - y1) + Pow(sinθ * (y2 - y1), 2)) / a / a +
            (Pow(sinθ * (x2 - x1), 2) + 2 * sinθ * cosθ * (x2 - x1) * (y2 - y1) + Pow(cosθ * (y2 - y1), 2)) / b / b;
        var B = (Pow(cosθ, 2) * 2 * x1 * (x2 - x1) - 2 * cosθ * sinθ * (y1 * (x2 - x1) + x1 * (y2 - y1)) +
                 Pow(sinθ, 2) * 2 * y1 * (y2 - y1)) / a / a
                + (Pow(sinθ, 2) * 2 * x1 * (x2 - x1) + 2 * sinθ * cosθ * (y1 * (x2 - x1) + x1 * (y2 - y1)) +
                   Pow(cosθ, 2) * 2 * y1 * (y2 - y1)) / b / b;
        var C = (Pow(cosθ * x1, 2) - 2 * cosθ * sinθ * x1 * y1 + Pow(sinθ * y1, 2)) / a / a
            + (Pow(sinθ * x1, 2) + 2 * sinθ * cosθ * x1 * y1 + Pow(cosθ * y1, 2)) / b / b - 1;

        // Make a list of t values.
        var t_values = new List<double>();

        // Calculate the discriminant.
        var discriminant = B * B - 4 * A * C;
        LogManager.GetCurrentClassLogger().Debug($"discriminant:{discriminant}");
        if (Abs(discriminant) < 0.1)
        {
            // One real solution.
            t_values.Add(-B / 2 / A);
        }
        else if (discriminant > 0)
        {
            // Two real solutions.
            t_values.Add((-B + Sqrt(discriminant)) / 2 / A);
            t_values.Add((-B - Sqrt(discriminant)) / 2 / A);
        }

        // Convert the t values into points.
        var points = new List<Point>();
        foreach (var t in t_values)
            // If the points are on the segment (or we
            // don't care if they are), add them to the list.
            if (!segment_only || (t >= 0f && t <= 1f))
            {
                var x = pt1.X + (pt2.X - pt1.X) * t + cx;
                var y = pt1.Y + (pt2.Y - pt1.Y) * t + cy;
                points.Add(new Point(x, y));
            }

        // Return the points.
        return new Tuple<Point[], double>(points.ToArray(), discriminant);
    }
}