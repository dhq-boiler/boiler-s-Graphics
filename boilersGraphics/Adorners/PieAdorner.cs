using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class PieAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _firstDragStartPoint;
        private Point? _firstDragEndPoint;
        private Pen _rectanglePen;
        private SnapAction _snapAction;
        private PieCreationStep _step;
        private Vector _RadiusVector;
        private double _Angle;
        private double _MinusRaidus;
        private double _StartAngle;
        private double _EndAngle;


        public enum PieCreationStep
        {
            Step1, // 中心点決定→長径決定
            Step2, // 角度決定
            Step3, // 短径決定
        }

        public PieAdorner(DesignerCanvas designerCanvas, Point? firstDragStartPoint)
            : base(designerCanvas)
        {
            _step = PieCreationStep.Step1;
            _designerCanvas = designerCanvas;
            _firstDragStartPoint = firstDragStartPoint;
            var brush = new SolidColorBrush(Colors.Blue);
            brush.Opacity = 0.5;
            _rectanglePen = new Pen(brush, 1);
            _snapAction = new SnapAction();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            switch (_step)
            {
                case PieCreationStep.Step1:
                    //ドラッグ終了座標を更新
                    _firstDragEndPoint = e.GetPosition(this);
                    var currentPosition = _firstDragEndPoint.Value;
                    _snapAction.OnMouseMove(ref currentPosition);
                    _firstDragEndPoint = currentPosition;
                    break;
                case PieCreationStep.Step2:
                    currentPosition = e.GetPosition(this);
                    var anotherVector = new Vector(currentPosition.X - _firstDragStartPoint.Value.X, currentPosition.Y - _firstDragStartPoint.Value.Y);
                    _Angle = Vector.AngleBetween(_RadiusVector, anotherVector);
                    break;
                case PieCreationStep.Step3:
                    currentPosition = e.GetPosition(this);
                    var vector = new Vector(currentPosition.X - _firstDragStartPoint.Value.X, currentPosition.Y - _firstDragStartPoint.Value.Y);
                    _MinusRaidus = vector.Length;
                    break;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (_step)
            {
                case PieCreationStep.Step1:
                    _step = PieCreationStep.Step2;
                    break;
                case PieCreationStep.Step2:
                    _step = PieCreationStep.Step3;
                    break;
                case PieCreationStep.Step3:
                    // release mouse capture
                    _step = PieCreationStep.Step1;
                    if (this.IsMouseCaptured) this.ReleaseMouseCapture();
                    var item = new NPieViewModel();
                    item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                    item.EdgeColor.Value = item.Owner.EdgeColors.First();
                    item.FillColor.Value = item.Owner.FillColors.First();
                    item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
                    item.ZIndex.Value = item.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
                    var geometry = GeometryCreator.CreateDonut(_firstDragStartPoint.Value, _RadiusVector.Length - _MinusRaidus, _RadiusVector.Length, _StartAngle, _EndAngle, SweepDirection.Clockwise);
                    item.PieCenterPoint.Value = _firstDragStartPoint.Value;
                    item.DonutWidth.Value = _RadiusVector.Length - _MinusRaidus;
                    item.Distance.Value = _RadiusVector.Length;
                    item.StartDegree.Value = _StartAngle;
                    item.EndDegree.Value = _EndAngle;
                    item.PathGeometry.Value = geometry;
                    item.Left.Value = geometry.Bounds.X;
                    item.Top.Value = geometry.Bounds.Y;
                    item.Width.Value = geometry.Bounds.Width;
                    item.Height.Value = geometry.Bounds.Height;
                    item.IsVisible.Value = true;
                    item.Owner.DeselectAll();
                    ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);
                    _snapAction.OnMouseUp(this);
                    break;
            }

            //if (_firstDragStartPoint.HasValue && _firstDragEndPoint.HasValue)
            //{
            //    _snapAction.OnMouseUp(this);

            //    var sliceRect = new Rect(_firstDragStartPoint.Value, _firstDragEndPoint.Value);

            //    UpdateStatisticsCount();

            //    _firstDragStartPoint = null;
            //    _firstDragEndPoint = null;
            //}

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"{_step}";

            e.Handled = true;
        }

        private static void UpdateStatisticsCount()
        {
            //var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            //var dao = new StatisticsDao();
            //dao.Update(statistics);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            //if (_startPoint.HasValue && _endPoint.HasValue)
            //    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(255 / 2, 0, 0, 255)), _rectanglePen, ShiftEdgeThickness());

            if (_firstDragStartPoint.HasValue && _firstDragEndPoint.HasValue)
            {
                switch (_step)
                {
                    case PieCreationStep.Step1:
                        var standardVector = new Vector(0, -1);
                        _RadiusVector = new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X, _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y);
                        dc.DrawGeometry(Brushes.Transparent, _rectanglePen, PieGeometry(_firstDragStartPoint.Value, _RadiusVector.Length, Vector.AngleBetween(standardVector, _RadiusVector) - 90, Vector.AngleBetween(standardVector, _RadiusVector) - 90, SweepDirection.Clockwise));
                        break;
                    case PieCreationStep.Step2:
                        standardVector = new Vector(0, -1);
                        _StartAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90;
                        _EndAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90 + _Angle;
                        LogManager.GetCurrentClassLogger().Debug($"{_StartAngle} - {_EndAngle}");
                        dc.DrawGeometry(Brushes.Transparent, _rectanglePen, PieGeometry(_firstDragStartPoint.Value, _RadiusVector.Length, _StartAngle, _EndAngle, SweepDirection.Clockwise));
                        break;
                    case PieCreationStep.Step3:
                        standardVector = new Vector(0, -1);
                        _StartAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90;
                        _EndAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90 + _Angle;
                        dc.DrawGeometry(Brushes.Transparent, _rectanglePen, DonutGeometry(_firstDragStartPoint.Value, _RadiusVector.Length - _MinusRaidus, _RadiusVector.Length, _StartAngle, _EndAngle, SweepDirection.Clockwise));
                        break;
                }
            }
        }

        private Rect ShiftEdgeThickness()
        {
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var point1 = _firstDragStartPoint.Value;
            point1.X += parent.EdgeThickness.Value.Value / 2;
            point1.Y += parent.EdgeThickness.Value.Value / 2;
            var point2 = _firstDragEndPoint.Value;
            point2.X -= parent.EdgeThickness.Value.Value / 2;
            point2.Y -= parent.EdgeThickness.Value.Value / 2;
            return new Rect(point1, point2);
        }

        /// <summary>
        /// ドーナツ形、アーチ形のPathGeometry作成
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="width">幅</param>
        /// <param name="distance">中心からの距離</param>
        /// <param name="startDeg">開始角度、0以上360未満</param>
        /// <param name="stopDeg">終了角度、0以上360未満</param>
        /// <param name="direction">回転方向、clockwiseが時計回り</param>
        /// <returns></returns>
        private PathGeometry DonutGeometry(Point center, double width, double distance, double startDeg, double stopDeg, SweepDirection direction)
        {
            //外側の円弧終始点
            Point outSideStart = MakePoint(startDeg, center, distance);
            Point outSideStop = MakePoint(stopDeg, center, distance);

            //内側の円弧終始点は角度と回転方向が外側とは逆になる
            Point inSideStart = MakePoint(stopDeg, center, distance - width);
            Point inSideStop = MakePoint(startDeg, center, distance - width);

            //開始角度から終了角度までが180度を超えているかの判定
            //超えていたらArcSegmentのIsLargeArcをtrue、なければfalseで作成
            double diffDegrees = (direction == SweepDirection.Clockwise) ? stopDeg - startDeg : startDeg - stopDeg;
            if (diffDegrees < 0) { diffDegrees += 360.0; }
            bool isLarge = (diffDegrees > 180) ? true : false;

            //arcSegment作成
            var outSideArc = new ArcSegment(outSideStop, new Size(distance, distance), 0, isLarge, direction, true);
            //内側のarcSegmentは回転方向を逆で作成
            var inDirection = (direction == SweepDirection.Clockwise) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            var inSideArc = new ArcSegment(inSideStop, new Size(distance - width, distance - width), 0, isLarge, inDirection, true);

            //PathFigure作成、外側から内側で作成している
            //2つのarcSegmentは、2本の直線(LineSegment)で繋げる
            var fig = new PathFigure();
            fig.StartPoint = outSideStart;
            fig.Segments.Add(outSideArc);
            fig.Segments.Add(new LineSegment(inSideStart, true));//外側終点から内側始点への直線
            fig.Segments.Add(inSideArc);
            fig.Segments.Add(new LineSegment(outSideStart, true));//内側終点から外側始点への直線
            fig.IsClosed = true;//Pathを閉じる必須

            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
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
