using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class BezierCurveViewModel : ConnectorBaseViewModel
    {
        public ReactivePropertySlim<Point> ControlPoint1 { get; set; } = new ReactivePropertySlim<Point>();
        public ReactivePropertySlim<Point> ControlPoint2 { get; set; } = new ReactivePropertySlim<Point>();

        public ReactivePropertySlim<Point> ControlLine1LeftTop { get; set; } = new ReactivePropertySlim<Point>();
        public ReactivePropertySlim<Point> ControlLine2LeftTop { get; set; } = new ReactivePropertySlim<Point>();

        public BezierCurveViewModel(int id, IDiagramViewModel parent)
            : base(id, parent)
        {
            Init();
        }

        public BezierCurveViewModel()
            : base()
        {
            Init();
        }

        public BezierCurveViewModel(Point p1, Point p2, Point c1, Point c2)
            : base()
        {
            Init();
            AddPoints(p1, p2);
            ControlPoint1.Value = c1;
            ControlPoint2.Value = c2;
        }

        private void Init()
        {
            Points.CollectionChanged += Points_CollectionChanged;
            ControlPoint1.Subscribe(x =>
            {
                if (Points.Count > 0)
                {
                    SetLeftTopOfControlLine1();
                    SetLeftTop();
                }
            })
            .AddTo(_CompositeDisposable);
            ControlPoint2.Subscribe(x =>
            {
                if (Points.Count > 1)
                {
                    SetLeftTopOfControlLine2();
                    SetLeftTop();
                }
            })
            .AddTo(_CompositeDisposable);
            EnablePathGeometryUpdate.Value = false;
        }

        private void SetLeftTopOfControlLine1()
        {
            var point = new Point();
            point.X = Math.Min(Points[0].X, ControlPoint1.Value.X);
            point.Y = Math.Min(Points[0].Y, ControlPoint1.Value.Y);
            ControlLine1LeftTop.Value = point;
            Trace.WriteLine($"ControlLine1LeftTop={ControlLine1LeftTop.Value}");
        }

        private void SetLeftTopOfControlLine2()
        {
            var point = new Point();
            point.X = Math.Min(Points[1].X, ControlPoint2.Value.X);
            point.Y = Math.Min(Points[1].Y, ControlPoint2.Value.Y);
            ControlLine2LeftTop.Value = point;
            Trace.WriteLine($"ControlLine2LeftTop={ControlLine2LeftTop.Value}");
        }

        private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Points.Count >= 2)
            {
                Trace.WriteLine($"P1={Points[0]} P2={Points[1]}");
                SetLeftTopOfControlLine1();
                SetLeftTopOfControlLine2();
                SetLeftTop();
            }
            else
                Trace.WriteLine("Points.Count < 2");
        }

        private void SetLeftTop()
        {
            if (Points.Count != 2)
                throw new Exception("Points.Count == 2");
            var minX = Math.Min(this.Points[0].X, this.Points[1].X);
            var minY = Math.Min(this.Points[0].Y, this.Points[1].Y);
            var points = new Point[] { this.Points[0], this.ControlPoint1.Value, this.ControlPoint2.Value, this.Points[1] };
            var diffT = 1.0 / 64.0;
            for (var t = diffT; t < 1.0; t += diffT)
            {
                var result = BezierCurve.Evaluate(t, points);
                minX = Math.Min(minX, result.X);
                minY = Math.Min(minY, result.Y);
            }
            LeftTop.Value = new Point(minX, minY);
        }

        public override object Clone()
        {
            var clone = new BezierCurveViewModel(Points[0], Points[1], ControlPoint1.Value, ControlPoint2.Value);
            clone.Owner = Owner;
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.PathGeometry.Value = GeometryCreator.CreateBezierCurve(clone);

            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }
    }
}
