using boilersGraphics.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
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

        public static PathGeometry CreateRectangle(NRectangleViewModel item)
        {
            var geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(item.Left.Value, item.Top.Value), true, true);
                ctx.LineTo(new Point(item.Left.Value + item.Width.Value, item.Top.Value), true, false);
                ctx.LineTo(new Point(item.Left.Value + item.Width.Value, item.Top.Value + item.Height.Value), true, false);
                ctx.LineTo(new Point(item.Left.Value, item.Top.Value + item.Height.Value), true, false);
            }
            geometry.Freeze();
            return PathGeometry.CreateFromGeometry(geometry);
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
    }
}
