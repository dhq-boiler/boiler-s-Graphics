using boilersGraphics.Adorners;
using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.Controls
{
    public class RotateThumb : Thumb
    {
        private Matrix _initialMatrix;
        private MatrixTransform _rotateTransform;
        private Vector _startVector;
        private double _beginDegree;
        private Point _centerPoint;
        private FrameworkElement _designerItem;
        private Canvas _canvas;
        private double _previousAngleInDegrees;
        private AuxiliaryLine CenterToP1;
        private Vector _endVector;
        private AuxiliaryLine CenterToP2;

        public FrameworkElementAdorner Arc { get; private set; }
        public Adorner DegreeText { get; private set; }

        public RotateThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.RotateThumb_DragDelta);
            DragStarted += new DragStartedEventHandler(this.RotateThumb_DragStarted);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "回転";
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            _canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);

            if (CenterToP1 != null)
            {
                adornerLayer.Remove(CenterToP1);
                CenterToP1 = null;
            }
            if (CenterToP2 != null)
            {
                adornerLayer.Remove(CenterToP2);
                CenterToP2 = null;
            }
            if (Arc != null)
            {
                adornerLayer.Remove(Arc);
                Arc = null;
            }
            if (DegreeText != null)
            {
                adornerLayer.Remove(DegreeText);
                DegreeText = null;
            }
        }

        private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _designerItem = this.GetParentOfType("selectedGrid") as FrameworkElement;

            if (_designerItem != null)
            {
                _canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);

                if (_canvas != null)
                {
                    _centerPoint = _designerItem.TranslatePoint(
                        new Point(_designerItem.ActualWidth * _designerItem.RenderTransformOrigin.X,
                                  _designerItem.ActualHeight * _designerItem.RenderTransformOrigin.Y),
                                  _canvas);

                    Point startPoint = Mouse.GetPosition(_canvas);
                    var endPoint = (sender as RotateThumb).TranslatePoint(new Point(0, 0), _canvas);
                    var vector = new Vector(endPoint.X - _centerPoint.X, endPoint.Y - _centerPoint.Y);
                    _startVector = Point.Subtract(startPoint, _centerPoint);
                    _beginDegree = Vector.AngleBetween(_startVector, new Vector(0, 1));
                    CenterToP1 = new AuxiliaryLine(_canvas, new Point(_centerPoint.X, _centerPoint.Y - vector.Length), _centerPoint);
                    adornerLayer.Add(CenterToP1);

                    _rotateTransform = _designerItem.RenderTransform as MatrixTransform;
                    if (_rotateTransform == null)
                    {
                        throw new UnexpectedException();
                    }
                    else
                    {
                        _initialMatrix = _rotateTransform.Matrix;
                        _previousAngleInDegrees = 0;
                    }
                }
            }
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var viewModel = DataContext as DesignerItemViewModelBase;

            if (_designerItem != null && _canvas != null)
            {
                _canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);

                if (CenterToP2 != null)
                {
                    adornerLayer.Remove(CenterToP2);
                }

                Point currentPoint = Mouse.GetPosition(_canvas);
                Vector deltaVector = Point.Subtract(currentPoint, _centerPoint);
                var endPoint = (sender as RotateThumb).TranslatePoint(new Point(0, 0), _canvas);
                _endVector = Vector.Subtract(new Vector(endPoint.X, endPoint.Y), new Vector(_centerPoint.X, _centerPoint.Y));
                CenterToP2 = new AuxiliaryLine(_canvas, _centerPoint, endPoint);
                adornerLayer.Add(CenterToP2);
                var deltaAngle = Vector.AngleBetween(new Vector(0, 1), deltaVector);
                var beginDegree = MakeDegree(-180);
                var endDegree = MakeDegree(deltaAngle);

                viewModel.RotationAngle.Value = (endDegree + 90);

                if (Arc != null)
                {
                    adornerLayer.Remove(Arc);
                }
                Arc = new FrameworkElementAdorner(_canvas);
                var pie = PieGeometry(viewModel.CenterPoint.Value, 20, beginDegree, endDegree, SweepDirection.Clockwise);
                Arc.Child = new Path()
                {
                    Data = pie,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                };
                adornerLayer.Add(Arc);
                if (DegreeText != null)
                {
                    adornerLayer.Remove(DegreeText);
                }
                DegreeText = new AuxiliaryText(_canvas, $"{viewModel.RotationAngle.Value}°", new Point(_centerPoint.X + 40 * Math.Cos(Radian((viewModel.RotationAngle.Value - 90) / 2)) - 20, _centerPoint.Y + 40 * Math.Sin(Radian((viewModel.RotationAngle.Value - 90) / 2)) - 20));
                adornerLayer.Add(DegreeText);

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"角度 {viewModel.RotationAngle.Value}°";

                _designerItem.InvalidateMeasure();
            }
        }

        private double MakeDegree(double deltaAngle)
        {
            var degree = 270 + (deltaAngle + 180);
            if (degree >= 360)
                degree -= 360;
            return degree;
        }

        //完成形、回転方向を指定できるように
        /// <summary>
        /// 扇(pie)型のPathGeometryを作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="distance">中心点からの距離</param>
        /// <param name="startDegrees">開始角度、0以上360未満で指定</param>
        /// <param name="stopDegrees">終了角度、0以上360未満で指定</param>
        /// <param name="direction">回転方向、Clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry PieGeometry(Point center, double distance, double startDegrees, double stopDegrees, SweepDirection direction)
        {
            Point start = MakePoint(startDegrees, center, distance);//始点座標
            Point stop = MakePoint(stopDegrees, center, distance);//終点座標
            //開始角度から終了角度までが180度を超えているかの判定
            //超えていたらArcSegmentのIsLargeArcをtrue、なければfalseで作成
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDegrees - startDegrees : startDegrees - stopDegrees;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;
            var arc = new ArcSegment(stop, new Size(distance, distance), 0, isLarge, direction, true);

            //PathFigure作成
            //ArcSegmentとその両端と中心点をつなぐ直線LineSegment
            var fig = new PathFigure();
            fig.StartPoint = start;//始点座標
            fig.Segments.Add(arc);//ArcSegment追加
            fig.Segments.Add(new LineSegment(center, true));//円弧の終点から中心への直線
            fig.Segments.Add(new LineSegment(start, true));//中心から円弧の始点への直線
            fig.IsClosed = true;//Pathを閉じる、必須

            //PathGeometryを作成してPathFigureを追加して完成
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }

        /// <summary>
        /// 距離と角度からその座標を返す
        /// </summary>
        /// <param name="degrees">360以上は359.99になる</param>
        /// <param name="center">中心点</param>
        /// <param name="distance">中心点からの距離</param>
        /// <returns></returns>
        private Point MakePoint(double degrees, Point center, double distance)
        {
            if (degrees >= 360) { degrees = 359.99; }
            var rad = Radian(degrees);
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);
            var x = center.X + cos * distance;
            var y = center.Y + sin * distance;
            return new Point(x, y);
        }
        private double Radian(double degree)
        {
            return Math.PI / 180.0 * degree;
        }
    }
}
