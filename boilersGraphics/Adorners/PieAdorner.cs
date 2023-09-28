using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners;

internal class PieAdorner : Adorner
{
    public enum PieCreationStep
    {
        Step1, // 中心点決定→長い半径決定
        Step2, // 角度決定
        Step3 // 短い半径決定
    }

    private double _Angle;
    private readonly AuxiliaryArcBetweenCeilingAndTarget _auxiliaryArcBetweenCeilingAndTarget;
    private DesignerCanvas _designerCanvas;
    private double _DispEndAngle;
    private double _DispStartAngle;
    private double _EndAngle;
    private Point? _firstDragEndPoint;
    private readonly Point? _firstDragStartPoint;
    private readonly NPieViewModel _item;
    private double _MinusRaidus;
    private Vector _RadiusVector;
    private readonly Pen _rectanglePen;
    private readonly SnapAction _snapAction;
    private double _StartAngle;
    private PieCreationStep _step;

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
        _auxiliaryArcBetweenCeilingAndTarget = new AuxiliaryArcBetweenCeilingAndTarget(designerCanvas);
        _item = new NPieViewModel();
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
                    var beginVector = new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X,
                        _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y);
                    _auxiliaryArcBetweenCeilingAndTarget.Render1st(_firstDragStartPoint.Value,
                        new Point(_firstDragStartPoint.Value.X, _firstDragStartPoint.Value.Y - beginVector.Length));
                    var (_, roundDegree) = _auxiliaryArcBetweenCeilingAndTarget.Render2nd(_firstDragEndPoint.Value,
                        Vector.AngleBetween(beginVector, new Vector(0, -1)));
                    _DispStartAngle = roundDegree;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                        Properties.Resources.String_Draw;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = string.Format(
                        Properties.Resources.String_Pie_DetermineLongRadiusAndStartingAngle, _firstDragStartPoint,
                        Math.Round(new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X,
                            _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y).Length), _DispStartAngle);
                    break;
                case PieCreationStep.Step2:
                    currentPosition = e.GetPosition(this);
                    _snapAction.OnMouseMove(ref currentPosition,
                        new List<Tuple<Point, object>> { new(_firstDragStartPoint.Value, _item) });
                    var anotherVector = new Vector(currentPosition.X - _firstDragStartPoint.Value.X,
                        currentPosition.Y - _firstDragStartPoint.Value.Y);
                    _Angle = Vector.AngleBetween(_RadiusVector, anotherVector);
                    beginVector = new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X,
                        _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y);
                    _auxiliaryArcBetweenCeilingAndTarget.Render1st(_firstDragStartPoint.Value,
                        new Point(_firstDragStartPoint.Value.X, _firstDragStartPoint.Value.Y - beginVector.Length));
                    (_, roundDegree) = _auxiliaryArcBetweenCeilingAndTarget.Render2nd(currentPosition,
                        Vector.AngleBetween(anotherVector, new Vector(0, -1)));
                    _DispEndAngle = roundDegree;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                        Properties.Resources.String_Draw;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = string.Format(
                        Properties.Resources.String_Pie_DetermineEndAngleByMouseUp, _firstDragStartPoint,
                        Math.Round(new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X,
                            _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y).Length), _DispStartAngle,
                        _DispEndAngle);
                    break;
                case PieCreationStep.Step3:
                    currentPosition = e.GetPosition(this);
                    _snapAction.OnMouseMove(ref currentPosition,
                        new List<Tuple<Point, object>> { new(_firstDragStartPoint.Value, _item) });
                    var vector = new Vector(currentPosition.X - _firstDragStartPoint.Value.X,
                        currentPosition.Y - _firstDragStartPoint.Value.Y);
                    _MinusRaidus = vector.Length;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                        Properties.Resources.String_Draw;
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = string.Format(
                        Properties.Resources.String_Pie_DetermineShortRadiusByMouseUp, _firstDragStartPoint,
                        Math.Round(new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X,
                            _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y).Length), _DispStartAngle,
                        _DispEndAngle, Math.Round(_MinusRaidus));
                    break;
            }

            if (!IsMouseCaptured)
                CaptureMouse();

            InvalidateVisual();
        }
        else
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
        }

        e.Handled = true;
    }

    private double GetAngle(double angle)
    {
        var angleType = (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AngleType
            .Value;
        var ret = Math.Round(angle);
        if (angleType == AngleType.ZeroTo360)
            return ret % 360;
        // angleType == AngleType.Minus180To180
        return ret % 180;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        switch (_step)
        {
            case PieCreationStep.Step1:
                _step = PieCreationStep.Step2;
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                    Properties.Resources.String_Pie_DetermineEndAngle;
                _auxiliaryArcBetweenCeilingAndTarget.OnMouseUp();
                break;
            case PieCreationStep.Step2:
                _step = PieCreationStep.Step3;
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                    Properties.Resources.String_Pie_DetermineShortRadius;
                _auxiliaryArcBetweenCeilingAndTarget.OnMouseUp();
                break;
            case PieCreationStep.Step3:
                // release mouse capture
                _step = PieCreationStep.Step1;
                if (IsMouseCaptured) ReleaseMouseCapture();
                _item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                _item.EdgeBrush.Value = _item.Owner.EdgeBrush.Value.Clone();
                _item.FillBrush.Value = _item.Owner.FillBrush.Value.Clone();
                _item.EdgeThickness.Value = _item.Owner.EdgeThickness.Value.Value;
                _item.ZIndex.Value = _item.Owner.Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
                var geometry = GeometryCreator.CreateDonut(_firstDragStartPoint.Value,
                    _RadiusVector.Length - _MinusRaidus, _RadiusVector.Length, _StartAngle, _EndAngle,
                    GetSweepDirection());
                _item.PieCenterPoint.Value = _firstDragStartPoint.Value;
                _item.DonutWidth.Value = _RadiusVector.Length - _MinusRaidus;
                _item.Distance.Value = _RadiusVector.Length;
                _item.StartDegree.Value = _StartAngle;
                _item.EndDegree.Value = _EndAngle;
                _item.SweepDirection.Value = GetSweepDirection();
                _item.PathGeometryNoRotate.Value = geometry;
                _item.Left.Value = geometry.Bounds.X;
                _item.Top.Value = geometry.Bounds.Y;
                _item.Width.Value = geometry.Bounds.Width;
                _item.PathGeometryNoRotate.Value = null;
                _item.Height.Value = geometry.Bounds.Height;
                _item.IsVisible.Value = true;
                _item.IsSelected.Value = true;
                _item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(_item);
                _snapAction.OnMouseUp(this);
                UpdateStatisticsCount();
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                    Properties.Resources.String_Pie_DetermineCenterPoint;
                break;
        }

        e.Handled = true;
    }

    private static void UpdateStatisticsCount()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        statistics.NumberOfDrawsOfThePieTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_firstDragStartPoint.HasValue && _firstDragEndPoint.HasValue)
            switch (_step)
            {
                case PieCreationStep.Step1:
                    var standardVector = new Vector(0, -1);
                    _RadiusVector = new Vector(_firstDragEndPoint.Value.X - _firstDragStartPoint.Value.X,
                        _firstDragEndPoint.Value.Y - _firstDragStartPoint.Value.Y);
                    _StartAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90;
                    dc.DrawGeometry(Brushes.Transparent, _rectanglePen,
                        GeometryCreator.CreatePie(_firstDragStartPoint.Value, _RadiusVector.Length, _StartAngle,
                            _StartAngle, GetSweepDirection()));
                    break;
                case PieCreationStep.Step2:
                    standardVector = new Vector(0, -1);
                    _StartAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90;
                    _EndAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90 + _Angle;
                    LogManager.GetCurrentClassLogger().Debug($"{_StartAngle} - {_EndAngle}");
                    dc.DrawGeometry(Brushes.Transparent, _rectanglePen,
                        GeometryCreator.CreatePie(_firstDragStartPoint.Value, _RadiusVector.Length, _StartAngle,
                            _EndAngle, GetSweepDirection()));
                    break;
                case PieCreationStep.Step3:
                    standardVector = new Vector(0, -1);
                    _StartAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90;
                    _EndAngle = Vector.AngleBetween(standardVector, _RadiusVector) - 90 + _Angle;
                    dc.DrawGeometry(Brushes.Transparent, _rectanglePen,
                        GeometryCreator.CreateDonut(_firstDragStartPoint.Value, _RadiusVector.Length - _MinusRaidus,
                            _RadiusVector.Length, _StartAngle, _EndAngle, GetSweepDirection()));
                    break;
            }
    }

    private static SweepDirection GetSweepDirection()
    {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)
            ? SweepDirection.Counterclockwise
            : SweepDirection.Clockwise;
    }
}