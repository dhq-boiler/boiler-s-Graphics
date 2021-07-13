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
                }
            })
            .AddTo(_CompositeDisposable);
        }

        private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Points.Count >= 2)
            {
                var point = new Point();
                point.X = Math.Min(Points[0].X, Points[1].X);
                point.Y = Math.Min(Points[0].Y, Points[1].Y);
                LeftTop.Value = point;
            }
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
