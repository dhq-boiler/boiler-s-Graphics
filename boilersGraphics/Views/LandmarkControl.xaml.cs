using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Reactive.Bindings;
using Rulyotano.Math.Interpolation.Bezier;
using Point = Rulyotano.Math.Geometry.Point;

namespace boilersGraphics.Views
{
    /// <summary>
    /// Interaction logic for LandmarkControl.xaml
    /// </summary>
    public partial class LandmarkControl : UserControl
    {
        #region Points

        public IEnumerable Points
        {
            get { return (IEnumerable)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(IEnumerable),
            typeof(LandmarkControl), new PropertyMetadata(null, PropertyChangedCallback));

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
                landmarkControl.SetPathData();
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
            landmarkControl.SetPathData();
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
        }

        private System.Windows.Point ConvertToVisualPoint(Point p)
        {
            return new System.Windows.Point(p.X, p.Y);
        }

        public void SetPathData()
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
                var y = (float)(point.GetType().GetProperty("Y").GetValue(point, new object[] { }) as ReactivePropertySlim<double>).Value; point.GetType().GetProperty("Y").GetValue(point, new object[] { });
                points.Add(new Point(x, y));
            }

            if (points.Count <= 1)
                return;

            var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points.FirstOrDefault()) };


            var myPathSegmentCollection = new PathSegmentCollection();

            var bezierSegments = BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

            if (bezierSegments == null || bezierSegments.Count < 1)
            {
                //Add a line segment <this is generic for more than one line>
                foreach (var point in points.GetRange(1, points.Count - 1))
                {

                    var myLineSegment = new LineSegment { Point = ConvertToVisualPoint(point) };
                    myPathSegmentCollection.Add(myLineSegment);
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
                    CorrectBezierSegment(segment, new Rect(0, 0, 300, 300));
                    myPathSegmentCollection.Add(segment);
                }
            }


            myPathFigure.Segments = myPathSegmentCollection;

            var myPathFigureCollection = new PathFigureCollection { myPathFigure };

            var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

            path.Data = myPathGeometry;
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

            SetPathData();
        }

        private void OnPointPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "X" || e.PropertyName == "Y")
                SetPathData();
        }
    }
}
