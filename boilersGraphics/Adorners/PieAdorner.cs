using boilersGraphics.Controls;
using boilersGraphics.Dao;
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
            Step1, // 中心点決定→長い半径決定
            Step2, // 角度決定
            Step3, // 短い半径決定
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
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (_step)
                {
                    case PieCreationStep.Step1:
                        //ドラッグ終了座標を更新
                        _firstDragEndPoint = e.GetPosition(this);
                        var currentPosition = _firstDragEndPoint.Value;
                        _snapAction.OnMouseMove(ref currentPosition);
                        _firstDragEndPoint = currentPosition;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"マウスアップで扇形の開始角度を決定しています。 中心点：{_firstDragStartPoint}";
                        break;
                    case PieCreationStep.Step2:
                        currentPosition = e.GetPosition(this);
                        var anotherVector = new Vector(currentPosition.X - _firstDragStartPoint.Value.X, currentPosition.Y - _firstDragStartPoint.Value.Y);
                        _Angle = Vector.AngleBetween(_RadiusVector, anotherVector);
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"マウスアップで扇形の終了角度を決定しています。 中心点：{_firstDragStartPoint} 開始角度：{_StartAngle} 終了角度：{_EndAngle}";
                        break;
                    case PieCreationStep.Step3:
                        currentPosition = e.GetPosition(this);
                        var vector = new Vector(currentPosition.X - _firstDragStartPoint.Value.X, currentPosition.Y - _firstDragStartPoint.Value.Y);
                        _MinusRaidus = vector.Length;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"マウスアップで扇形の短い半径を決定しています。 中心点：{_firstDragStartPoint} 開始角度：{_StartAngle} 終了角度：{_EndAngle} 短い半径：{_MinusRaidus}";
                        break;
                }

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
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"クリックして扇形の終了角度を決定します。";
                    break;
                case PieCreationStep.Step2:
                    _step = PieCreationStep.Step3;
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"クリックして扇形の短い半径を決定します。";
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
                    var geometry = GeometryCreator.CreateDonut(_firstDragStartPoint.Value, _RadiusVector.Length - _MinusRaidus, _RadiusVector.Length, _StartAngle, _EndAngle, GetSweepDirection());
                    item.PieCenterPoint.Value = _firstDragStartPoint.Value;
                    item.DonutWidth.Value = _RadiusVector.Length - _MinusRaidus;
                    item.Distance.Value = _RadiusVector.Length;
                    item.StartDegree.Value = _StartAngle;
                    item.EndDegree.Value = _EndAngle;
                    item.SweepDirection.Value = GetSweepDirection();
                    item.PathGeometry.Value = geometry;
                    item.Left.Value = geometry.Bounds.X;
                    item.Top.Value = geometry.Bounds.Y;
                    item.Width.Value = geometry.Bounds.Width;
                    item.Height.Value = geometry.Bounds.Height;
                    item.IsVisible.Value = true;
                    item.Owner.DeselectAll();
                    ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);
                    _snapAction.OnMouseUp(this);
                    UpdateStatisticsCount();
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"クリックして扇形の中心点を決定します。";
                    break;
            }

            e.Handled = true;
        }

        private static void UpdateStatisticsCount()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.NumberOfDrawsOfThePieTool++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_firstDragStartPoint.HasValue && _firstDragEndPoint.HasValue)
            {
                switch (_step)
                {
                    case PieCreationStep.Step1:
                        var standardVector = new Vector(0, -1);
                        _RadiusVector = new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X, _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y);
                        dc.DrawGeometry(Brushes.Transparent, _rectanglePen, GeometryCreator.CreatePie(_firstDragStartPoint.Value, _RadiusVector.Length, Vector.AngleBetween(standardVector, _RadiusVector) - 90, Vector.AngleBetween(standardVector, _RadiusVector) - 90, GetSweepDirection()));
                        break;
                    case PieCreationStep.Step2:
                        standardVector = new Vector(0, -1);
                        _StartAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90;
                        _EndAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90 + _Angle;
                        LogManager.GetCurrentClassLogger().Debug($"{_StartAngle} - {_EndAngle}");
                        dc.DrawGeometry(Brushes.Transparent, _rectanglePen, GeometryCreator.CreatePie(_firstDragStartPoint.Value, _RadiusVector.Length, _StartAngle, _EndAngle, GetSweepDirection()));
                        break;
                    case PieCreationStep.Step3:
                        standardVector = new Vector(0, -1);
                        _StartAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90;
                        _EndAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90 + _Angle;
                        dc.DrawGeometry(Brushes.Transparent, _rectanglePen, GeometryCreator.CreateDonut(_firstDragStartPoint.Value, _RadiusVector.Length - _MinusRaidus, _RadiusVector.Length, _StartAngle, _EndAngle, GetSweepDirection()));
                        break;
                }
            }
        }

        private static SweepDirection GetSweepDirection()
        {
            return (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
        }
    }
}
