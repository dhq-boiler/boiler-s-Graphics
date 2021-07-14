using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class BezierCurveViewModel : ConnectorBaseViewModel
    {
        public ReactiveProperty<Point> ControlPoint1 { get; set; } = new ReactiveProperty<Point>();
        public ReactiveProperty<Point> ControlPoint2 { get; set; } = new ReactiveProperty<Point>();

        public ReactiveProperty<Point> ControlLine1LeftTop { get; set; } = new ReactiveProperty<Point>();
        public ReactiveProperty<Point> ControlLine2LeftTop { get; set; } = new ReactiveProperty<Point>();
        public ReactiveProperty<Point> LeftTop { get; set; } = new ReactiveProperty<Point>();

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
            Points.Add(p1);
            Points.Add(p2);
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
                    var point = new Point();
                    point.X = Math.Min(Points[0].X, ControlPoint1.Value.X);
                    point.Y = Math.Min(Points[0].Y, ControlPoint1.Value.Y);
                    ControlLine1LeftTop.Value = point;
                    SetLeftTop();
                }
            })
            .AddTo(_CompositeDisposable);
            ControlPoint2.Subscribe(x =>
            {
                if (Points.Count > 1)
                {
                    var point = new Point();
                    point.X = Math.Min(Points[1].X, ControlPoint2.Value.X);
                    point.Y = Math.Min(Points[1].Y, ControlPoint2.Value.Y);
                    ControlLine2LeftTop.Value = point;
                    SetLeftTop();
                }
            })
            .AddTo(_CompositeDisposable);
        }

        private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Points.Count >= 2)
            {
                SetLeftTop();
            }
        }

        private void SetLeftTop()
        {
            var tarray = new List<double> { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0 };
            var points = new List<Point>();
            foreach (var t in tarray)
            {
                points.Add(BezierCurve.Evaluate(t, new List<Point>() { this.Points[0], this.ControlPoint1.Value, this.ControlPoint2.Value, this.Points[1] }));
            }
            LeftTop.Value = new Point(points.Select(x => x.X).Min(), points.Select(x => x.Y).Min());
        }

        public override object Clone()
        {
            var clone = new BezierCurveViewModel(Points[0], Points[1], ControlPoint1.Value, ControlPoint2.Value);
            clone.Owner = Owner;
            clone.EdgeColor = EdgeColor;
            clone.EdgeThickness = EdgeThickness;

            return clone;
        }
    }
}
