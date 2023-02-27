using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.Views;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Rulyotano.Math.Interpolation.Bezier;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels.ColorCorrect
{
    public class ToneCurveViewModel : BindableBase, INavigationAware, IDisposable
    {
        private CompositeDisposable _disposable = new CompositeDisposable();
        private bool _disposedValue;
        public ReactivePropertySlim<ColorCorrectViewModel> ViewModel { get; } = new();
        public ReactiveCommand<Point> CanvasClickCommand { get; } = new();
        public ReactiveCommand<KeyEventArgs> PreviewKeyDownCommand { get; } = new();

        public class Point : BindableBase
        {
            public Point() { }

            public Point(double x, double y)
            {
                X.Value = x;
                Y.Value = y;
            }

            public ReactivePropertySlim<double> X { get; } = new ReactivePropertySlim<double>();
            public ReactivePropertySlim<double> Y { get; } = new ReactivePropertySlim<double>();

            public Rulyotano.Math.Geometry.Point ToPoint()
            {
                return new Rulyotano.Math.Geometry.Point(X.Value, Y.Value);
            }
        }

        public ReactiveCommand<DragDeltaEventArgs> DragDeltaCommand { get; } = new();

        public ReactivePropertySlim<Channel> TargetChannel { get; } = new (Channel.GrayScale);

        public ReactiveCommand<SelectionChangedEventArgs> TargetChannelChangedCommand { get; } = new();

        public ReactiveCollection<Curve> Curves { get; private set; } = new ReactiveCollection<Curve>();

        public ReactivePropertySlim<Curve> TargetCurve { get; } = new ReactivePropertySlim<Curve>();

        public class Curve : BindableBase
        {
            public ReactivePropertySlim<Channel> TargetChannel { get; } = new();

            public ReactiveCollection<Point> Points { get; set; } = new();

            public ReactivePropertySlim<WriteableBitmap> Histogram { get; } = new();

            public ReactiveCollection<InOutPair> InOutPairs { get; set; } = new();

            public ReactivePropertySlim<Brush> Color { get; } = new();
        }

        public class RGBCurve : Curve
        {

        }

        public class BlueCurve : Curve
        {

        }

        public class GreenCurve : Curve
        {

        }

        public class RedCurve : Curve
        {

        }

        public ToneCurveViewModel()
        {
            Curve curve = new RGBCurve();
            curve.TargetChannel.Value = Channel.GrayScale;
            curve.Color.Value = Channel.GrayScale.Brush;
            curve.Points.Add(new Point(0, 255));
            curve.Points.Add(new Point(255 / 2, 255 / 2));
            curve.Points.Add(new Point(255, 0));
            Curves.Add(curve);

            curve = new BlueCurve();
            curve.TargetChannel.Value = Channel.Blue;
            curve.Color.Value = Channel.Blue.Brush;
            curve.Points.Add(new Point(0, 255));
            curve.Points.Add(new Point(255 / 2, 255 / 2));
            curve.Points.Add(new Point(255, 0));
            Curves.Add(curve);

            curve = new GreenCurve();
            curve.TargetChannel.Value = Channel.Green;
            curve.Color.Value = Channel.Green.Brush;
            curve.Points.Add(new Point(0, 255));
            curve.Points.Add(new Point(255 / 2, 255 / 2));
            curve.Points.Add(new Point(255, 0));
            Curves.Add(curve);

            curve = new RedCurve();
            curve.TargetChannel.Value = Channel.Red;
            curve.Color.Value = Channel.Red.Brush;
            curve.Points.Add(new Point(0, 255));
            curve.Points.Add(new Point(255 / 2, 255 / 2));
            curve.Points.Add(new Point(255, 0));
            Curves.Add(curve);

            TargetCurve.Value = Curves.First();

            DragDeltaCommand.Subscribe(x =>
            { 
                Point unboxedPoint = (Point)(x.Source as Thumb).DataContext;
                if (TargetCurve.Value.Points.First() == unboxedPoint || TargetCurve.Value.Points.Last() == unboxedPoint)
                {
                    unboxedPoint.Y.Value = Math.Clamp(unboxedPoint.Y.Value + x.VerticalChange, 0, 255);
                    UpdatePoints(x);
                    return;
                }
                int index = TargetCurve.Value.Points.IndexOf(unboxedPoint);
                var previousPoint = TargetCurve.Value.Points[index - 1];
                var nextPoint = TargetCurve.Value.Points[index + 1];
                unboxedPoint.X.Value = Math.Clamp(unboxedPoint.X.Value + x.HorizontalChange, Math.Min(previousPoint.X.Value, nextPoint.X.Value), Math.Max(previousPoint.X.Value, nextPoint.X.Value));
                unboxedPoint.Y.Value = Math.Clamp(unboxedPoint.Y.Value + x.VerticalChange, 0, 255);
                UpdatePoints(x);
            }).AddTo(_disposable);
            CanvasClickCommand.Subscribe(t =>
            {
                var newPoint = new Point(t.X.Value, t.Y.Value);
                var newPointModel = new Point(t.X.Value, t.Y.Value);
                var pointsList = TargetCurve.Value.Points.Select(it => new Rulyotano.Math.Geometry.Point(it.X.Value, it.Y.Value));
                var insertIndex = Rulyotano.Math.Geometry.Helpers.BestPlaceToInsert(newPoint.ToPoint(), pointsList.ToList());
                if (insertIndex == TargetCurve.Value.Points.Count)
                {
                    TargetCurve.Value.Points.Add(newPointModel);
                    return;
                }
                TargetCurve.Value.Points.Insert(insertIndex, newPointModel);
            }).AddTo(_disposable);
            TargetChannelChangedCommand.Subscribe(x =>
            {
                var window = System.Windows.Window.GetWindow(x.Source as System.Windows.Controls.ComboBox);
                var landmark = window.GetVisualChild<LandmarkControl>();
                landmark.SetPathData();
                ViewModel.Value.TargetCurve.Value = Curves.First(y => y.TargetChannel.Value == (Channel)x.AddedItems[0]);
                ViewModel.Value.TargetCurve.Value.InOutPairs.Clear();
                ViewModel.Value.TargetChannel.Value = (Channel)x.AddedItems[0];
                ViewModel.Value.Render();
                SetHistogram(TargetCurve.Value);
            }).AddTo(_disposable);
            PreviewKeyDownCommand.Subscribe(x =>
            {
                if (x.Key == Key.Delete)
                {
                    var thumb = x.Source as Thumb;
                    var dataContext = thumb.DataContext;
                    var point = dataContext as Point;
                    if (point == new Point(0, 255) || point == new Point(255, 0))
                    {
                        return;
                    }
                    if (TargetCurve.Value.Points.Count() <= 3)
                    {
                        return;
                    }
                    TargetCurve.Value.Points.Remove(point);
                }
            }).AddTo(_disposable);
        }

        private void UpdatePoints(DragDeltaEventArgs x)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(TargetCurve.Value.Points)));
            var window = System.Windows.Window.GetWindow(x.Source as Thumb);
            var landmark = window.GetVisualChild<LandmarkControl>();
            landmark.SetPathData();
        }

        private void SetHistogram(Curve curve)
        {
            using (var histogram = new Mat())
            {
                switch (curve.TargetChannel.Value)
                {
                    case GrayScaleChannel:
                    case BlueChannel:
                        using (var mat = ViewModel.Value.Bitmap.Value.ToMat())
                        {
                            Cv2.CalcHist(new Mat[] { mat }, new int[] { 0 }, null, histogram, 1, new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });
                        }
                        break;
                    case GreenChannel:
                        using (var mat = ViewModel.Value.Bitmap.Value.ToMat())
                        {
                            Cv2.CalcHist(new Mat[] { mat }, new int[] { 1 }, null, histogram, 1, new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });
                        }
                        break;
                    case RedChannel:
                        using (var mat = ViewModel.Value.Bitmap.Value.ToMat())
                        {
                            Cv2.CalcHist(new Mat[] { mat }, new int[] { 2 }, null, histogram, 1, new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });
                        }
                        break;
                }
                Cv2.Normalize(histogram, histogram, 0, 256, NormTypes.MinMax);
                using (var output = DrawHistogram(histogram, curve.TargetChannel.Value))
                {
                    curve.Histogram.Value = output.ToWriteableBitmap();
                }
            }
        }

        private unsafe Mat DrawHistogram(Mat histogram, Channel value)
        {
            var ret = new Mat(256, 256, MatType.CV_8UC4);
            float* p_hist = (float*)histogram.Data.ToPointer();
            byte* p_ret = (byte*)ret.Data.ToPointer();
            long step = ret.Step();
            int width = ret.Width;
            int height = ret.Height;

            histogram.MinMaxLoc(out double min, out double max);

            Parallel.For(0, 256, x =>
            {
                var v = (int)(256 - 256 * *(p_hist + x) / max);
                for (int y = 0; y >= 0 && y <= 255; y++)
                {
                    if (y > v)
                    {
                        if (value == Channel.GrayScale)
                        {
                            *(p_ret + y * step + x * 4 + 0) = (byte)x; //B
                            *(p_ret + y * step + x * 4 + 1) = (byte)x; //G
                            *(p_ret + y * step + x * 4 + 2) = (byte)x; //R
                            *(p_ret + y * step + x * 4 + 3) = 255; //Alpha
                        }
                        if (value == Channel.Blue)
                        {
                            *(p_ret + y * step + x * 4 + 0) = (byte)x; //B
                            *(p_ret + y * step + x * 4 + 1) = 0; //G
                            *(p_ret + y * step + x * 4 + 2) = 0; //R
                            *(p_ret + y * step + x * 4 + 3) = 255; //Alpha
                        }
                        if (value == Channel.Green)
                        {
                            *(p_ret + y * step + x * 4 + 0) = 0; //B
                            *(p_ret + y * step + x * 4 + 1) = (byte)x; //G
                            *(p_ret + y * step + x * 4 + 2) = 0; //R
                            *(p_ret + y * step + x * 4 + 3) = 255; //Alpha
                        }
                        if (value == Channel.Red)
                        {
                            *(p_ret + y * step + x * 4 + 0) = 0; //B
                            *(p_ret + y * step + x * 4 + 1) = 0; //G
                            *(p_ret + y * step + x * 4 + 2) = (byte)x; //R
                            *(p_ret + y * step + x * 4 + 3) = 255; //Alpha
                        }
                    }
                    else
                    {
                        *(p_ret + y * step + x * 4 + 3) = 0; //Alpha
                    }
                }
            });

            return ret;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel.Value = navigationContext.Parameters.GetValue<ColorCorrectViewModel>("ViewModel");
            TargetChannel.Value = ViewModel.Value.TargetChannel.Value;
            //Curves = ViewModel.Value.Curves;
            if (ViewModel.Value.TargetCurve.Value is not null)
            {
                TargetCurve.Value = ViewModel.Value.TargetCurve.Value;
                TargetCurve.Value.Points = ViewModel.Value.TargetCurve.Value.Points;
            }

            foreach (var curve in Curves)
            {
                if (curve.Points.Count <= 2)
                {
                    curve.Points.Clear();
                    curve.Points.Add(new Point(0, 255));
                    curve.Points.Add(new Point(255 / 2, 255 / 2));
                    curve.Points.Add(new Point(255, 0));
                }
                SetHistogram(curve);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ViewModel.Value.Curves = Curves;

            var _Points = TargetCurve.Value.Points.Select(x => new Rulyotano.Math.Geometry.Point(x.X.Value, x.Y.Value)).ToList();


            var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(_Points.FirstOrDefault()) };

            var bezierSegments = BezierInterpolation.PointsToBezierCurves(_Points, false);

            var _myPathSegmentCollection = new PathSegmentCollection();

            if (bezierSegments == null || bezierSegments.Count < 1)
            {
                //Add a line segment <this is generic for more than one line>
                foreach (var point in _Points.GetRange(1, _Points.Count - 1))
                {

                    var myLineSegment = new LineSegment { Point = ConvertToVisualPoint(point) };
                    _myPathSegmentCollection.Add(myLineSegment);
                }
            }
            else
            {
                foreach (var bezierCurveSegment in bezierSegments)
                {
                    var segment = new BezierSegment
                    {
                        Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                        Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                        Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                    };
                    CorrectBezierSegment(segment, new System.Windows.Rect(0, 0, 255, 255));
                    _myPathSegmentCollection.Add(segment);
                }
            }
            myPathFigure.Segments = _myPathSegmentCollection;

            var myPathFigureCollection = new PathFigureCollection { myPathFigure };

            var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };
            
            var segments = myPathFigure.Segments;

            var ret = new List<InOutPair>();
            for (int x = 0; x <= byte.MaxValue; x++)
            {
                var P0 = default(Rulyotano.Math.Geometry.Point);
                foreach (BezierSegment segment in _myPathSegmentCollection.Cast<BezierSegment>())
                {
                    if (segment == segments.First())
                    {
                        P0 = TargetCurve.Value.Points.First().ToPoint();
                    }
                    else
                    {
                        var index = segments.IndexOf(segment);
                        var previous = segments[index - 1] as BezierSegment;
                        P0 = new Rulyotano.Math.Geometry.Point(previous.Point3.X, previous.Point3.Y);
                    }

                    var P1 = new Rulyotano.Math.Geometry.Point(segment.Point1.X, segment.Point1.Y);
                    var P2 = new Rulyotano.Math.Geometry.Point(segment.Point2.X, segment.Point2.Y);
                    var P3 = new Rulyotano.Math.Geometry.Point(segment.Point3.X, segment.Point3.Y);

                    if (x < P0.X || P3.X < x)
                    {
                        continue;
                    }

                    var t = FindT(x, P0, P1, P2, P3);
                    double y = Math.Round(Math.Pow(1 - t, 3) * P0.Y + 3 * Math.Pow(1 - t, 2) * t * P1.Y + 3 * (1 - t) * Math.Pow(t, 2) * P2.Y + Math.Pow(t, 3) * P3.Y);
                    if (y >= byte.MinValue && y <= byte.MaxValue)
                    {
                        if (!ret.Any(a => a.In == x))
                        {
                            ret.Add(new InOutPair(x, (int)y));
                            break;
                        }
                    }
                }
            }

            ViewModel.Value.TargetCurve.Value.InOutPairs = ret.ToObservable().ToReactiveCollection();
        }

        private double FindT(double x, Rulyotano.Math.Geometry.Point P0, Rulyotano.Math.Geometry.Point P1, Rulyotano.Math.Geometry.Point P2, Rulyotano.Math.Geometry.Point P3)
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

        private System.Windows.Point ConvertToVisualPoint(Rulyotano.Math.Geometry.Point p)
        {
            return new System.Windows.Point(p.X, p.Y);
        }

        private void CorrectBezierSegment(BezierSegment bezierSegment, System.Windows.Rect bounds)
        {
            System.Windows.Point point1 = bezierSegment.Point1;
            System.Windows.Point point2 = bezierSegment.Point2;
            System.Windows.Point point3 = bezierSegment.Point3;

            if (point1.X < bounds.Left)
            {
                point1.X = bounds.Left;
            }
            else if (point1.X > bounds.Right)
            {
                point1.X = bounds.Right;
            }

            if (point1.Y < bounds.Top)
            {
                point1.Y = bounds.Top;
            }
            else if (point1.Y > bounds.Bottom)
            {
                point1.Y = bounds.Bottom;
            }

            if (point2.X < bounds.Left)
            {
                point2.X = bounds.Left;
            }
            else if (point2.X > bounds.Right)
            {
                point2.X = bounds.Right;
            }

            if (point2.Y < bounds.Top)
            {
                point2.Y = bounds.Top;
            }
            else if (point2.Y > bounds.Bottom)
            {
                point2.Y = bounds.Bottom;
            }

            if (point3.X < bounds.Left)
            {
                point3.X = bounds.Left;
            }
            else if (point3.X > bounds.Right)
            {
                point3.X = bounds.Right;
            }

            if (point3.Y < bounds.Top)
            {
                point3.Y = bounds.Top;
            }
            else if (point3.Y > bounds.Bottom)
            {
                point3.Y = bounds.Bottom;
            }

            bezierSegment.Point1 = point1;
            bezierSegment.Point2 = point2;
            bezierSegment.Point3 = point3;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposable.Dispose();
                }

                _disposable = null;
                _disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~ToneCureveViewModel()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
