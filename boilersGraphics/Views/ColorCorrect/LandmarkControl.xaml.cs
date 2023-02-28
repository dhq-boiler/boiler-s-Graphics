using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.ColorCorrect;
using Reactive.Bindings;
using Rulyotano.Math.Interpolation.Bezier;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using boilersGraphics.Extensions;
using Point = Rulyotano.Math.Geometry.Point;

namespace boilersGraphics.Views
{
    /// <summary>
    /// Interaction logic for LandmarkControl.xaml
    /// </summary>
    public partial class LandmarkControl : UserControl
    {
        #region Points

        public IEnumerable<ToneCurveViewModel.Point> Points
        {
            get { return (IEnumerable<ToneCurveViewModel.Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(IEnumerable<ToneCurveViewModel.Point>),
            typeof(LandmarkControl), new PropertyMetadata(null, PropertyChangedCallback));

        public ReactiveCollection<InOutPair> AllScales
        {
            get { return (ReactiveCollection<InOutPair>)GetValue(AllScalesProperty); }
            set { SetValue(AllScalesProperty, value); }
        }

        public static readonly DependencyProperty AllScalesProperty =
            DependencyProperty.Register("AllScales", typeof(ReactiveCollection<InOutPair>),
                typeof(LandmarkControl), new PropertyMetadata(null));

        public List<InOutPair> Scales
        {
            get { return (List<InOutPair>)GetValue(ScalesProperty); }
            set { SetValue(ScalesProperty, value); }
        }

        public static readonly DependencyProperty ScalesProperty =
            DependencyProperty.Register("Scales", typeof(List<InOutPair>),
                typeof(LandmarkControl), new PropertyMetadata(null));

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var landmarkControl = dependencyObject as LandmarkControl;
            if (landmarkControl == null)
                return;

            if (dependencyPropertyChangedEventArgs.NewValue is INotifyCollectionChanged)
            {
                (dependencyPropertyChangedEventArgs.NewValue as
                INotifyCollectionChanged).CollectionChanged += landmarkControl.OnPointCollectionChanged;
                landmarkControl.RegisterCollectionItemPropertyChanged
                (dependencyPropertyChangedEventArgs.NewValue as IEnumerable);
            }

            if (dependencyPropertyChangedEventArgs.OldValue is INotifyCollectionChanged)
            {
                (dependencyPropertyChangedEventArgs.OldValue as
                INotifyCollectionChanged).CollectionChanged -= landmarkControl.OnPointCollectionChanged;
                landmarkControl.UnRegisterCollectionItemPropertyChanged
                (dependencyPropertyChangedEventArgs.OldValue as IEnumerable);
            }

            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                var window = System.Windows.Window.GetWindow(landmarkControl);
                landmarkControl.SetPathData(window);
                var toneCurve = window.EnumerateChildOfType<Views.ColorCorrect.ToneCurve>().First();
                var dataContext = toneCurve.DataContext as ToneCurveViewModel;
                (dataContext.ViewModel.Value as ColorCorrectViewModel).Curves = dataContext.Curves;
                (dataContext.ViewModel.Value as ColorCorrectViewModel).Render();
            }
        }

        #endregion

        #region PathColor

        public Brush PathColor
        {
            get { return (Brush)GetValue(PathColorProperty); }
            set { SetValue(PathColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PathColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathColorProperty =
            DependencyProperty.Register("PathColor", typeof(Brush), typeof(LandmarkControl),
                                        new PropertyMetadata(Brushes.Black));

        #endregion

        #region IsClosedCurve

        public static readonly DependencyProperty IsClosedCurveProperty =
            DependencyProperty.Register("IsClosedCurve", typeof(bool), typeof(LandmarkControl),
                                        new PropertyMetadata(default(bool), OnIsClosedCurveChanged));

        private static void OnIsClosedCurveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var landmarkControl = dependencyObject as LandmarkControl;
            if (landmarkControl == null)
                return;
            var window = System.Windows.Window.GetWindow(landmarkControl);
            landmarkControl.SetPathData(window);
            var toneCurve = window.EnumerateChildOfType<Views.ColorCorrect.ToneCurve>().First();
            var dataContext = toneCurve.DataContext as ToneCurveViewModel;
            (dataContext.ViewModel.Value as ColorCorrectViewModel).Render();
        }

        public bool IsClosedCurve
        {
            get { return (bool)GetValue(IsClosedCurveProperty); }
            set { SetValue(IsClosedCurveProperty, value); }
        }

        #endregion


        public LandmarkControl()
        {
            InitializeComponent();

            Scales = new List<InOutPair>();
            AllScales = new ReactiveCollection<InOutPair>();
        }

        private System.Windows.Point ConvertToVisualPoint(Point p)
        {
            return new System.Windows.Point(p.X, p.Y);
        }

        private PathSegmentCollection _myPathSegmentCollection = new PathSegmentCollection();

        public void SetPathData(Window window)
        {
            if (Points == null) return;
            var points = new List<Point>();

            foreach (var point in Points)
            {
                var pointProperties = point.GetType().GetProperties();
                if (pointProperties.All(p => p.Name != "X") ||
                pointProperties.All(p => p.Name != "Y"))
                    continue;
                var x = (float)(point.GetType().GetProperty("X").GetValue(point, new object[] { }) as ReactivePropertySlim<double>).Value;
                var y = (float)(point.GetType().GetProperty("Y").GetValue(point, new object[] { }) as ReactivePropertySlim<double>).Value; 
                points.Add(new Point(x, y));
            }

            if (points.Count <= 1)
                return;

            _myPathSegmentCollection = new PathSegmentCollection();

            var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points.FirstOrDefault()) };

            var bezierSegments = BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

            if (bezierSegments == null || bezierSegments.Count < 1)
            {
                //Add a line segment <this is generic for more than one line>
                foreach (var point in points.GetRange(1, points.Count - 1))
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
                    CorrectBezierSegment(segment, new Rect(0, 0, 255, 255));
                    _myPathSegmentCollection.Add(segment);
                }
            }


            myPathFigure.Segments = _myPathSegmentCollection;

            var myPathFigureCollection = new PathFigureCollection { myPathFigure };

            var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

            path.Data = myPathGeometry;

            var inoutPairs = InOutPairs;
            var allScales = new ReactiveCollection<InOutPair>();
            var ret = new List<InOutPair>();

            foreach (var pair in inoutPairs)
            {
                allScales.Add(new InOutPair(pair.In, byte.MaxValue - pair.Out));
            }

            for (int i = 0; i < inoutPairs.Count; i += 5)
            {
                ret.Add(inoutPairs[i]);
            }

            AllScales = allScales;
            Scales = ret;
            
            var toneCurve = window.EnumerateChildOfType<Views.ColorCorrect.ToneCurve>().First();
            var dataContext = toneCurve.DataContext as ToneCurveViewModel;
            dataContext.TargetCurve.Value.InOutPairs = AllScales;
            dataContext.ViewModel.Value.TargetCurve.Value = dataContext.TargetCurve.Value;
            dataContext.ViewModel.Value.TargetCurve.Value.InOutPairs = dataContext.TargetCurve.Value.InOutPairs;
        }

        public List<InOutPair> InOutPairs
        {
            get
            {
                var myPathGeometry = path.Data as PathGeometry;
                var myPathFigureCollection = myPathGeometry.Figures;
                var myPathFigure = myPathFigureCollection.First();
                var segments = myPathFigure.Segments;

                var ret = new List<InOutPair>();
                for (int x = 0; x <= byte.MaxValue; x++)
                {
                    Point P0 = default(Point);
                    foreach (BezierSegment segment in _myPathSegmentCollection.OfType<BezierSegment>())
                    {
                        if (segment == segments.First())
                        {
                            P0 = Points.First().ToPoint();
                        }
                        else
                        {
                            var index = segments.IndexOf(segment);
                            var previous = segments[index-1] as BezierSegment;
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
                            if (!ret.Any(a => a.In == x))
                            {
                                ret.Add(new InOutPair(x, (int)y));
                                break;
                            }
                        }
                    }
                }
                return ret;
            }
        }

        private double FindT(double x, Point P0, Point P1, Point P2, Point P3)
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

        private void CorrectBezierSegment(BezierSegment bezierSegment, Rect bounds)
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

        private void RegisterCollectionItemPropertyChanged(IEnumerable collection)
        {
            if (collection == null)
                return;
            foreach (INotifyPropertyChanged point in collection)
                point.PropertyChanged += OnPointPropertyChanged;
        }

        private void UnRegisterCollectionItemPropertyChanged(IEnumerable collection)
        {
            if (collection == null)
                return;
            foreach (INotifyPropertyChanged point in collection)
                point.PropertyChanged -= OnPointPropertyChanged;
        }

        private void OnPointCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RegisterCollectionItemPropertyChanged(e.NewItems);

            UnRegisterCollectionItemPropertyChanged(e.OldItems);

            var window = System.Windows.Window.GetWindow(this);

            if (window is null)
                return;

            SetPathData(window);
        }

        private void OnPointPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                var window = System.Windows.Window.GetWindow(this);
                if (window is not null)
                {
                    SetPathData(window);
                    ((this.DataContext as ToneCurveViewModel).ViewModel.Value as ColorCorrectViewModel).Render();
                }
            }
        }
    }
}
