using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NLog;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Controls;

public class LineResizeHandle : SnapPoint
{
    public static readonly DependencyProperty OppositeHandleProperty =
        DependencyProperty.Register("OppositeHandle", typeof(LineResizeHandle), typeof(LineResizeHandle));

    public static readonly DependencyProperty TargetPointIndexProperty =
        DependencyProperty.Register("TargetPointIndex", typeof(int), typeof(LineResizeHandle));

    private readonly SnapAction snapAction;

    public LineResizeHandle()
    {
        snapAction = new SnapAction();
    }

    public OperationRecorder Recorder { get; } =
        new((Application.Current.MainWindow.DataContext as MainWindowViewModel).Controller);

    public LineResizeHandle OppositeHandle
    {
        get => (LineResizeHandle)GetValue(OppositeHandleProperty);
        set => SetValue(OppositeHandleProperty, value);
    }

    public int TargetPointIndex
    {
        get => (int)GetValue(TargetPointIndexProperty);
        set => SetValue(TargetPointIndexProperty, value);
    }

    public Point? BeginDragPoint { get; private set; }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        BeginDragPoint = e.GetPosition(Application.Current.MainWindow.GetChildOfType<DesignerCanvas>());

        var vm = DataContext as ConnectorBaseViewModel ??
                 (DataContext as SnapPointViewModel).Parent.Value as ConnectorBaseViewModel;
        var point = vm.Points[TargetPointIndex];

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
            Properties.Resources.String_Deform;

        Recorder.BeginRecode();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (BeginDragPoint.HasValue)
        {
            var snapPointVM = DataContext as SnapPointViewModel;
            var connectorVM = DataContext as ConnectorBaseViewModel ??
                              (DataContext as SnapPointViewModel).Parent.Value as ConnectorBaseViewModel;

            SelectableDesignerItemViewModelBase.Disconnect(snapPointVM);

            snapPointVM.IsSelected.Value = true;

            var currentPosition = Mouse.GetPosition(Application.Current.MainWindow.GetChildOfType<DesignerCanvas>());

            var point = currentPosition;
            Recorder.Current.ExecuteSetProperty(snapPointVM, "Left.Value", currentPosition.X);
            Recorder.Current.ExecuteSetProperty(snapPointVM, "Top.Value", currentPosition.Y);
            Canvas.SetLeft(this, currentPosition.X - Width / 2);
            Canvas.SetTop(this, currentPosition.Y - Height / 2);

            var ellipses = (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems
                .Value.OfType<NEllipseViewModel>();
            var pies = (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems
                .Value.OfType<NPieViewModel>();
            var appendIntersectionPoints = new List<Tuple<Point, object>>();

            var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var oppositePoint = OppositeHandle.TransformToAncestor(designerCanvas).Transform(new Point(0, 0));
            snapAction.SnapIntersectionOfEllipseAndTangent(ellipses, oppositePoint, point, appendIntersectionPoints);
            snapAction.SnapIntersectionOfPieAndTangent(pies, oppositePoint, point, appendIntersectionPoints);
            var vec = new Vector();
            vec = Point.Subtract(oppositePoint, point);
            snapAction.OnMouseMove(ref point, this, vec, appendIntersectionPoints);

            Recorder.Current.ExecuteSetProperty(connectorVM, $"Points[{TargetPointIndex}]", point);
            if ((string)Tag == "始点")
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                    $"({point}) - ({oppositePoint}) (w, h) = ({oppositePoint.X - point.X}, {oppositePoint.Y - point.Y})";
            else if ((string)Tag == "終点")
                (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                    $"({oppositePoint}) - ({point}) (w, h) = ({point.X - oppositePoint.X}, {point.Y - oppositePoint.Y})";
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        var snapPointVM = DataContext as SnapPointViewModel;
        var connectorVM = DataContext as ConnectorBaseViewModel ??
                          (DataContext as SnapPointViewModel).Parent.Value as ConnectorBaseViewModel;

        base.OnMouseUp(e);

        BeginDragPoint = null;

        snapAction.OnMouseUp(null);

        snapAction.PostProcess(SnapPointPosition, connectorVM);

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

        Recorder.EndRecode();

        LogManager.GetCurrentClassLogger().Info($"Deform item {connectorVM.ShowPropertiesAndFields()}");
    }
}