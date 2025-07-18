using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZLinq;

namespace boilersGraphics.Adorners;

public class PictureAdorner : Adorner
{
    private DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly string _filename;
    private readonly double _Height;
    private readonly SnapAction _snapAction;
    private Point? _startPoint;
    private readonly double _Width;

    public PictureAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, string filename, double width,
        double height)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        _filename = filename;
        _Width = width;
        _Height = height;
        _snapAction = new SnapAction();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured)
                CaptureMouse();

            _endPoint = e.GetPosition(this);
            var currentPosition = _endPoint.Value;
            _snapAction.OnMouseMove(ref currentPosition);
            _endPoint = currentPosition;

            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint =
                currentPosition;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

            InvalidateVisual();
        }
        else
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
        }

        e.Handled = true;
    }


    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        // release mouse capture
        if (IsMouseCaptured) ReleaseMouseCapture();

        _snapAction.OnMouseUp(this);

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var bitmap = BitmapFactory.FromStream(new FileStream(_filename, FileMode.Open, FileAccess.Read));
            var itemBase = new PictureDesignerItemViewModel();
            itemBase.FileName = _filename;
            itemBase.FileWidth = bitmap.Width;
            itemBase.FileHeight = bitmap.Height;
            itemBase.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            itemBase.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
            itemBase.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
            itemBase.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X,
                _endPoint.Value.X - _startPoint.Value.X);
            itemBase.PathGeometryNoRotate.Value = null;
            itemBase.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y,
                _endPoint.Value.Y - _startPoint.Value.Y);
            itemBase.IsSelected.Value = true;
            itemBase.IsVisible.Value = true;
            itemBase.Owner.DeselectAll();
            itemBase.ZIndex.Value = itemBase.Owner.Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).AsValueEnumerable().Count();
            ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(itemBase);

            UpdateStatisticsCount();

            _startPoint = null;
            _endPoint = null;
        }

        e.Handled = true;
    }

    private static void UpdateStatisticsCount()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        statistics.NumberOfDrawsOfTheImageFileTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var brush = new ImageBrush(new BitmapImage(new Uri(_filename)));
        brush.Opacity = 0.5;

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var diff = _endPoint.Value - _startPoint.Value;
            if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down)
            {
                if (diff.X > diff.Y)
                {
                    var y = _startPoint.Value.Y + diff.Y;
                    var x = _startPoint.Value.X + diff.Y / _Height * _Width;
                    _endPoint = new Point(x, y);
                }
                else if (diff.X < diff.Y)
                {
                    var x = _startPoint.Value.X + diff.X;
                    var y = _startPoint.Value.Y + diff.X / _Width * _Height;
                    _endPoint = new Point(x, y);
                }

                dc.DrawRectangle(brush, null, new Rect(_startPoint.Value, _endPoint.Value));
            }
            else
            {
                dc.DrawRectangle(brush, null, new Rect(_startPoint.Value, _endPoint.Value));
            }
        }
    }
}