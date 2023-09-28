using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Controls;

public class RotateThumb : Thumb
{
    private AuxiliaryArcBetweenCeilingAndTarget _ArcBetweenCeilingAndTarget;
    private Canvas _canvas;
    private Point _centerPoint;
    private FrameworkElement _designerItem;
    private Matrix _initialMatrix;
    private MatrixTransform _rotateTransform;

    public RotateThumb()
    {
        DragDelta += RotateThumb_DragDelta;
        DragStarted += RotateThumb_DragStarted;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
            Properties.Resources.String_Rotate;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

        _canvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        if (_ArcBetweenCeilingAndTarget is not null) _ArcBetweenCeilingAndTarget.OnMouseUp();
    }

    private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _designerItem = this.GetParentOfType("selectedGrid") as FrameworkElement;

        if (_designerItem != null)
        {
            _canvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);

            if (_canvas != null)
            {
                _centerPoint = _designerItem.TranslatePoint(
                    new Point(_designerItem.ActualWidth * _designerItem.RenderTransformOrigin.X,
                        _designerItem.ActualHeight * _designerItem.RenderTransformOrigin.Y),
                    _canvas);

                _ArcBetweenCeilingAndTarget = new AuxiliaryArcBetweenCeilingAndTarget(_canvas);
                _ArcBetweenCeilingAndTarget.Render1st(_centerPoint,
                    (sender as RotateThumb).TranslatePoint(new Point(0, 0), _canvas));

                _rotateTransform = _designerItem.RenderTransform as MatrixTransform;
                if (_rotateTransform == null)
                    throw new UnexpectedException();
                _initialMatrix = _rotateTransform.Matrix;
            }
        }
    }

    private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var viewModel = DataContext as DesignerItemViewModelBase;

        if (_designerItem != null && _canvas != null)
        {
            _canvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);

            viewModel.RotationAngle.Value = _ArcBetweenCeilingAndTarget
                .Render2nd((sender as RotateThumb).TranslatePoint(new Point(0, 0), _canvas),
                    viewModel.RotationAngle.Value).Item1;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                $"{Properties.Resources.String_Angle}={viewModel.RotationAngle.Value}°";

            _designerItem.InvalidateMeasure();
        }
    }
}