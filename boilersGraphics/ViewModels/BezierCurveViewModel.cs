using boilersGraphics.Helpers;
using boilersGraphics.Views;
using NLog;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
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

        public ReactiveProperty<double> P1X { get; set; }
        public ReactiveProperty<double> P1Y { get; set; }
        public ReactiveProperty<double> P2X { get; set; }
        public ReactiveProperty<double> P2Y { get; set; }
        public ReactiveProperty<double> C1X { get; set; }
        public ReactiveProperty<double> C1Y { get; set; }
        public ReactiveProperty<double> C2X { get; set; }
        public ReactiveProperty<double> C2Y { get; set; }

        public override bool SupportsPropertyDialog => true;

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

        public BezierCurveViewModel(IDiagramViewModel diagramViewModel, Point p1, Point p2, Point c1, Point c2)
            : base()
        {
            Init();
            AddPoints(diagramViewModel, p1, p2);
            ControlPoint1.Value = c1;
            ControlPoint2.Value = c2;
            P1X = Observable.Return(p1.X).ToReactiveProperty();
            P1Y = Observable.Return(p1.Y).ToReactiveProperty();
            P2X = Observable.Return(p2.X).ToReactiveProperty();
            P2Y = Observable.Return(p2.Y).ToReactiveProperty();
            C1X = Observable.Return(c1.X).ToReactiveProperty();
            C1Y = Observable.Return(c1.Y).ToReactiveProperty();
            C2X = Observable.Return(c2.X).ToReactiveProperty();
            C2Y = Observable.Return(c2.Y).ToReactiveProperty();
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
            LogManager.GetCurrentClassLogger().Trace($"ControlLine1LeftTop={ControlLine1LeftTop.Value}");
        }

        private void SetLeftTopOfControlLine2()
        {
            var point = new Point();
            point.X = Math.Min(Points[1].X, ControlPoint2.Value.X);
            point.Y = Math.Min(Points[1].Y, ControlPoint2.Value.Y);
            ControlLine2LeftTop.Value = point;
            LogManager.GetCurrentClassLogger().Trace($"ControlLine2LeftTop={ControlLine2LeftTop.Value}");
        }

        private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Points.Count >= 2)
            {
                LogManager.GetCurrentClassLogger().Trace($"P1={Points[0]} P2={Points[1]}");
                SetLeftTopOfControlLine1();
                SetLeftTopOfControlLine2();
                SetLeftTop();
            }
            else
                LogManager.GetCurrentClassLogger().Trace($"Points.Count < 2");
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
            var clone = new BezierCurveViewModel(Owner, Points[0], Points[1], ControlPoint1.Value, ControlPoint2.Value);
            clone.Owner = Owner;
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.PathGeometry.Value = GeometryCreator.CreateBezierCurve(clone);
            clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
            clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
            clone.PenLineJoin.Value = PenLineJoin.Value;
            clone.StrokeDashArray.Value = StrokeDashArray.Value;
            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailBezier), new DialogParameters() { { "ViewModel", (BezierCurveViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<BezierCurveViewModel>("ViewModel");
                this.Points[0] = new Point(viewModel.P1X.Value, viewModel.P1Y.Value);
                this.Points[1] = new Point(viewModel.P2X.Value, viewModel.P2Y.Value);
                this.ControlPoint1.Value = new Point(viewModel.C1X.Value, viewModel.C1Y.Value);
                this.ControlPoint2.Value = new Point(viewModel.C2X.Value, viewModel.C2Y.Value);
                this.SnapPoint0VM.Value.Left.Value = viewModel.P1X.Value;
                this.SnapPoint0VM.Value.Top.Value = viewModel.P1Y.Value;
                this.SnapPoint1VM.Value.Left.Value = viewModel.P2X.Value;
                this.SnapPoint1VM.Value.Top.Value = viewModel.P2Y.Value;
                this.StrokeStartLineCap.Value = viewModel.StrokeStartLineCap.Value;
                this.StrokeEndLineCap.Value = viewModel.StrokeEndLineCap.Value;
                this.PenLineJoin.Value = viewModel.PenLineJoin.Value;
                this.StrokeDashArray.Value = viewModel.StrokeDashArray.Value;
            }
        }
    }
}
