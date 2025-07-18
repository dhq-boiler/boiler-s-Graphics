using boilersGraphics.Adorners;
using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NLog;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZLinq;
using static boilersGraphics.Helpers.SnapAction;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;

namespace boilersGraphics.Controls;

public class ResizeThumb : SnapPoint
{
    private const double MIN_ONE_SIDE_LENGTH = 10;
    private readonly Dictionary<Point, Adorner> _adorners;
    private SnapResult _SnapResult = SnapResult.NoSnap;

    private SnapPointPosition _SnapToEdge;
    private DesignerItemViewModelBase _SnapTargetDataContext { get; set; }

    public ResizeThumb()
    {
        _adorners = new Dictionary<Point, Adorner>();
        DragDelta += ResizeThumb_DragDelta;
    }


    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
            Properties.Resources.String_Resize;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_SnapResult == SnapResult.Snapped)
        {
            var designerItem = DataContext as DesignerItemViewModelBase;
            var connector = DataContext as ConnectorBaseViewModel;
            switch (SnapPointPosition)
            {
                case SnapPointPosition.LeftTop:
                case SnapPointPosition.RightTop:
                case SnapPointPosition.LeftBottom:
                case SnapPointPosition.RightBottom:
                    designerItem.SnapObjs.Add(_SnapTargetDataContext.Connect(_SnapToEdge, SnapPointPosition,
                        designerItem));
                    break;
                case SnapPointPosition.BeginEdge:
                    connector.SnapPoint0VM.Value.SnapObjs.Add(
                        _SnapTargetDataContext.Connect(_SnapToEdge, SnapPointPosition.BeginEdge, connector));
                    break;
                case SnapPointPosition.EndEdge:
                    connector.SnapPoint1VM.Value.SnapObjs.Add(
                        _SnapTargetDataContext.Connect(_SnapToEdge, SnapPointPosition.EndEdge, connector));
                    break;
            }
        }

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is not DesignerItemViewModelBase designerItem || !designerItem.IsSelected.Value) return;
        SelectableDesignerItemViewModelBase.Disconnect(designerItem);

        // only resize DesignerItems
        var selectedDesignerItems = designerItem.Owner.SelectedItems.Value.AsValueEnumerable()
            .Where(item => item is DesignerItemViewModelBase).ToArray();

        if (designerItem.Owner.BackgroundItem.Value.EdgeBrush.Value == Brushes.Magenta
            && designerItem.Owner.BackgroundItem.Value.EdgeThickness.Value == 10)
            selectedDesignerItems = selectedDesignerItems.AsValueEnumerable().Union(new SelectableDesignerItemViewModelBase[]
                { designerItem.Owner.BackgroundItem.Value }).ToArray();

        CalculateDragLimits(selectedDesignerItems, out double minLeft, out var minTop,
            out var minDeltaHorizontal, out var minDeltaVertical);

        var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var correspondingViews = designerCanvas.EnumVisualChildren<ResizeThumb>(DataContext);
        var diagramVM = mainWindowVM.DiagramViewModel;

        foreach (var item in selectedDesignerItems)
            if (item is DesignerItemViewModelBase viewModel)
            {
                double dragDeltaVertical;
                double dragDeltaHorizontal;
                if (viewModel is PictureDesignerItemViewModel pictureDesignerItemViewModel &&
                    ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                     (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down))
                {
                    if (VerticalAlignment == VerticalAlignment.Top && HorizontalAlignment == HorizontalAlignment.Left)
                    {
                        var left = pictureDesignerItemViewModel.Left.Value;
                        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                        pictureDesignerItemViewModel.Left.Value = left + dragDeltaHorizontal;
                        pictureDesignerItemViewModel.Width.Value =
                            pictureDesignerItemViewModel.Width.Value - dragDeltaHorizontal;
                        pictureDesignerItemViewModel.Height.Value =
                            pictureDesignerItemViewModel.Width.Value / pictureDesignerItemViewModel.FileWidth *
                            pictureDesignerItemViewModel.FileHeight;
                        pictureDesignerItemViewModel.Top.Value = pictureDesignerItemViewModel.Bottom.Value -
                                                                 pictureDesignerItemViewModel.Height.Value;
                    }
                    else if (VerticalAlignment == VerticalAlignment.Top &&
                             HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        var top = pictureDesignerItemViewModel.Top.Value;
                        dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                        pictureDesignerItemViewModel.Top.Value = top + dragDeltaVertical;
                        pictureDesignerItemViewModel.Height.Value =
                            pictureDesignerItemViewModel.Height.Value - dragDeltaVertical;
                        pictureDesignerItemViewModel.Width.Value =
                            pictureDesignerItemViewModel.Height.Value / pictureDesignerItemViewModel.FileHeight *
                            pictureDesignerItemViewModel.FileWidth;
                    }
                    else if (VerticalAlignment == VerticalAlignment.Bottom &&
                             HorizontalAlignment == HorizontalAlignment.Left)
                    {
                        var left = pictureDesignerItemViewModel.Left.Value;
                        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                        pictureDesignerItemViewModel.Left.Value = left + dragDeltaHorizontal;
                        pictureDesignerItemViewModel.Width.Value =
                            pictureDesignerItemViewModel.Width.Value - dragDeltaHorizontal;
                        pictureDesignerItemViewModel.Height.Value =
                            pictureDesignerItemViewModel.Width.Value / pictureDesignerItemViewModel.FileWidth *
                            pictureDesignerItemViewModel.FileHeight;
                    }
                    else if (VerticalAlignment == VerticalAlignment.Bottom &&
                             HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                        pictureDesignerItemViewModel.Height.Value =
                            pictureDesignerItemViewModel.Height.Value - dragDeltaVertical;
                        pictureDesignerItemViewModel.Width.Value =
                            pictureDesignerItemViewModel.Height.Value / pictureDesignerItemViewModel.FileHeight *
                            pictureDesignerItemViewModel.FileWidth;
                    }
                }
                else if (viewModel is NEllipseViewModel ellipseViewModel &&
                         ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                          (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down))
                {
                    if (VerticalAlignment == VerticalAlignment.Top && HorizontalAlignment == HorizontalAlignment.Left)
                    {
                        var left = ellipseViewModel.Left.Value;
                        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                        ellipseViewModel.Left.Value = left + dragDeltaHorizontal;
                        ellipseViewModel.Width.Value = ellipseViewModel.Width.Value - dragDeltaHorizontal;
                        ellipseViewModel.Height.Value = ellipseViewModel.Width.Value - dragDeltaHorizontal;
                        ellipseViewModel.Top.Value = ellipseViewModel.Bottom.Value - ellipseViewModel.Height.Value;
                    }
                    else if (VerticalAlignment == VerticalAlignment.Top &&
                             HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        var top = ellipseViewModel.Top.Value;
                        dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                        ellipseViewModel.Top.Value = top + dragDeltaVertical;
                        ellipseViewModel.Height.Value = ellipseViewModel.Height.Value - dragDeltaVertical;
                        ellipseViewModel.Width.Value = ellipseViewModel.Height.Value - dragDeltaVertical;
                    }
                    else if (VerticalAlignment == VerticalAlignment.Bottom &&
                             HorizontalAlignment == HorizontalAlignment.Left)
                    {
                        var left = ellipseViewModel.Left.Value;
                        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                        ellipseViewModel.Left.Value = left + dragDeltaHorizontal;
                        ellipseViewModel.Width.Value = ellipseViewModel.Width.Value - dragDeltaHorizontal;
                        ellipseViewModel.Height.Value = ellipseViewModel.Width.Value - dragDeltaHorizontal;
                    }
                    else if (VerticalAlignment == VerticalAlignment.Bottom &&
                             HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                        ellipseViewModel.Height.Value = ellipseViewModel.Height.Value - dragDeltaVertical;
                        ellipseViewModel.Width.Value = ellipseViewModel.Height.Value - dragDeltaVertical;
                    }
                }
                else
                {
                    var rect = new Rect(viewModel.Left.Value, viewModel.Top.Value, viewModel.Width.Value,
                        viewModel.Height.Value);
                    dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                    dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                    Sum(ref rect, dragDeltaHorizontal, dragDeltaVertical, HorizontalAlignment, VerticalAlignment);

                    if (diagramVM.EnablePointSnap.Value)
                    {
                        var snapPoints = diagramVM.GetSnapPoints(new List<SnapPoint>(correspondingViews));
                        Tuple<SnapPoint, Point> snapped = null;

                        foreach (var snapPoint in snapPoints)
                        {
                            var p = GetPosition(rect, VerticalAlignment, HorizontalAlignment);
                            var oppositeP = GetPosition(rect, OppositeVertical(VerticalAlignment),
                                OppositeHorizontal(HorizontalAlignment));
                            if (!(p.X > snapPoint.Item2.X - mainWindowVM.SnapPower.Value)
                                || !(p.X < snapPoint.Item2.X + mainWindowVM.SnapPower.Value)
                                || !(p.Y > snapPoint.Item2.Y - mainWindowVM.SnapPower.Value)
                                || !(p.Y < snapPoint.Item2.Y + mainWindowVM.SnapPower.Value)) continue;
                            //スナップする座標を一時変数へ保存
                            snapped = snapPoint;
                            _SnapToEdge = snapPoint.Item1.SnapPointPosition;
                            _SnapTargetDataContext = snapPoint.Item1.DataContext as DesignerItemViewModelBase;
                        }

                        //スナップした場合
                        if (snapped != null)
                        {
                            var adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                            RemoveFromAdornerLayerAndDictionary(snapped.Item2, adornerLayer);

                            //ドラッグ終了座標を一時変数で上書きしてスナップ
                            SetRect(ref rect, snapped.Item2, VerticalAlignment, HorizontalAlignment);

                            viewModel.Left.Value = rect.X;
                            viewModel.Top.Value = rect.Y;
                            viewModel.Width.Value = rect.Width;
                            viewModel.Height.Value = rect.Height;

                            _SnapResult = SnapResult.Snapped;

                            if (adornerLayer != null)
                            {
                                LogManager.GetCurrentClassLogger().Trace($"Snap={snapped.Item2}");
                                if (!_adorners.ContainsKey(snapped.Item2))
                                {
                                    var adorner = new SnapPointAdorner(designerCanvas, snapped.Item2,
                                        viewModel.SnapPointSize.Value, viewModel.ThumbThickness.Value);
                                    if (adorner != null)
                                    {
                                        adornerLayer.Add(adorner);

                                        //ディクショナリに記憶する
                                        _adorners.Add(snapped.Item2, adorner);
                                    }
                                }
                            }
                        }
                        else //スナップしなかった場合
                        {
                            _SnapResult = SnapResult.NoSnap;

                            RemoveAllAdornerFromAdornerLayerAndDictionary(designerCanvas);

                            viewModel.snapPointPosition =
                                GetSnapPointPosition(VerticalAlignment, HorizontalAlignment);
                            dragDeltaHorizontal = AffectHorizontal(e, HorizontalAlignment, minLeft,
                                minDeltaHorizontal, viewModel);
                            dragDeltaVertical = AffectVertical(e, VerticalAlignment, minTop, minDeltaVertical,
                                viewModel);
                        }
                    }
                    else
                    {
                        if (VerticalAlignment == VerticalAlignment.Bottom)
                        {
                            dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                            var old = viewModel.Top.Value;
                            viewModel.Height.Value = viewModel.Height.Value - dragDeltaVertical;
                            viewModel.UpdatePathGeometryIfEnable("Height", viewModel.Height.Value, old);
                        }
                        else if (VerticalAlignment == VerticalAlignment.Top)
                        {
                            var top = viewModel.Top.Value;
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            var old = viewModel.Top.Value;
                            viewModel.Top.Value = top + dragDeltaVertical;
                            viewModel.UpdatePathGeometryIfEnable("Top", viewModel.Top.Value, old);
                            old = viewModel.Height.Value;
                            viewModel.Height.Value = viewModel.Height.Value - dragDeltaVertical;
                            viewModel.UpdatePathGeometryIfEnable("Height", viewModel.Height.Value, old);
                        }

                        if (HorizontalAlignment == HorizontalAlignment.Left)
                        {
                            var left = viewModel.Left.Value;
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange),
                                minDeltaHorizontal);
                            var old = viewModel.Left.Value;
                            viewModel.Left.Value = left + dragDeltaHorizontal;
                            viewModel.UpdatePathGeometryIfEnable("Left", viewModel.Left.Value, old);
                            old = viewModel.Width.Value;
                            viewModel.Width.Value = viewModel.Width.Value - dragDeltaHorizontal;
                            viewModel.UpdatePathGeometryIfEnable("Width", viewModel.Width.Value, old);
                        }
                        else if (HorizontalAlignment == HorizontalAlignment.Right)
                        {
                            dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                            var old = viewModel.Width.Value;
                            viewModel.Width.Value = viewModel.Width.Value - dragDeltaHorizontal;
                            viewModel.UpdatePathGeometryIfEnable("Width", viewModel.Width.Value, old);
                        }
                    }
                }

                (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                    $"(w, h) = ({viewModel.Width.Value}, {viewModel.Height.Value})";
            }

        e.Handled = true;
    }

    public static double AffectHorizontal(DragDeltaEventArgs e, HorizontalAlignment horizontalAlignment, double minLeft,
        double minDeltaHorizontal, DesignerItemViewModelBase? viewModel)
    {
        var dragDeltaHorizontal = default(double);
        switch (horizontalAlignment)
        {
            case HorizontalAlignment.Left:
                var left = viewModel.Left.Value;
                dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                viewModel.Pool.Value = "Left";
                viewModel.Left.Value = left + dragDeltaHorizontal;
                viewModel.Width.Value = viewModel.Width.Value - dragDeltaHorizontal;
                viewModel.Pool.Value = string.Empty;
                break;
            case HorizontalAlignment.Right:
                dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                viewModel.Width.Value = viewModel.Width.Value - dragDeltaHorizontal;
                break;
        }

        return dragDeltaHorizontal;
    }

    public static double AffectVertical(DragDeltaEventArgs e, VerticalAlignment verticalAlignment, double minTop,
        double minDeltaVertical, DesignerItemViewModelBase? viewModel)
    {
        var dragDeltaVertical = default(double);
        switch (verticalAlignment)
        {
            case VerticalAlignment.Bottom:
                dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                viewModel.Height.Value = viewModel.Height.Value - dragDeltaVertical;
                break;
            case VerticalAlignment.Top:
                dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                viewModel.Pool.Value = "Top";
                viewModel.Top.Value += dragDeltaVertical;
                viewModel.Height.Value = viewModel.Height.Value - dragDeltaVertical;
                viewModel.Pool.Value = string.Empty;
                break;
        }

        return dragDeltaVertical;
    }

    private VerticalAlignment OppositeVertical(VerticalAlignment verticalAlignment)
    {
        if (verticalAlignment == VerticalAlignment.Top)
            return VerticalAlignment.Bottom;
        // verticalAlignment == VerticalAlignment.Bottom
        return VerticalAlignment.Top;
    }

    private HorizontalAlignment OppositeHorizontal(HorizontalAlignment horizontalAlignment)
    {
        if (horizontalAlignment == HorizontalAlignment.Left)
            return HorizontalAlignment.Right;
        // verticalAlignment == VerticalAlignment.Right
        return HorizontalAlignment.Left;
    }

    private SnapPointPosition GetSnapPointPosition(VerticalAlignment verticalAlignment,
        HorizontalAlignment horizontalAlignment)
    {
        switch (verticalAlignment)
        {
            case VerticalAlignment.Center:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        return SnapPointPosition.Left;
                    case HorizontalAlignment.Center:
                        return SnapPointPosition.Center;
                    case HorizontalAlignment.Right:
                        return SnapPointPosition.Right;
                    default:
                        throw new UnexpectedException(horizontalAlignment.ToString());
                }
            case VerticalAlignment.Top:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        return SnapPointPosition.LeftTop;
                    case HorizontalAlignment.Center:
                        return SnapPointPosition.Top;
                    case HorizontalAlignment.Right:
                        return SnapPointPosition.RightTop;
                    default:
                        throw new UnexpectedException(horizontalAlignment.ToString());
                }
            case VerticalAlignment.Bottom:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        return SnapPointPosition.LeftBottom;
                    case HorizontalAlignment.Center:
                        return SnapPointPosition.Bottom;
                    case HorizontalAlignment.Right:
                        return SnapPointPosition.RightBottom;
                    default:
                        throw new UnexpectedException(horizontalAlignment.ToString());
                }
            default:
                throw new UnexpectedException(verticalAlignment.ToString());
        }
    }

    [Conditional("DEBUG")]
    private void DebugPrint(string windowName, Rect rect, Point? value = null)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new RenderTargetBitmap((int)designerCanvas.ActualWidth, (int)designerCanvas.ActualHeight, 96, 96,
            PixelFormats.Pbgra32);

        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var brush = new VisualBrush(designerCanvas);
            context.DrawRectangle(brush, null,
                new Rect(new Point(), new Size(designerCanvas.Width, designerCanvas.Height)));

            context.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Blue, 1), rect);

            context.DrawText(
                new FormattedText($"({rect.X}, {rect.Y})", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("メイリオ"), 12, Brushes.Blue, VisualTreeHelper.GetDpi(designerCanvas).PixelsPerDip),
                new Point(rect.X + 10, rect.Y + 10));
            context.DrawText(
                new FormattedText(rect.Height.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("メイリオ"), 12, Brushes.Blue, VisualTreeHelper.GetDpi(designerCanvas).PixelsPerDip),
                new Point(rect.X, rect.Y + rect.Height / 2));

            if (value != null)
                context.DrawEllipse(Brushes.Transparent, new Pen(Brushes.Red, 1), value.Value, 2, 2);
        }

        rtb.Render(visual);

        //OpenCvSharp.Cv2.ImShow()するためには src_depth != CV_16F && src_depth != CV_32S である必要があるから、予めBgr24に変換しておく
        var newFormattedBitmapSource = new FormatConvertedBitmap();
        newFormattedBitmapSource.BeginInit();
        newFormattedBitmapSource.Source = rtb;
        newFormattedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
        newFormattedBitmapSource.EndInit();

        var mat = newFormattedBitmapSource.ToMat();
        Cv2.ImShow(windowName, mat);
    }

    private void Sum(ref Rect rect, double dragDeltaHorizontal, double dragDeltaVertical,
        HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
    {
        switch (verticalAlignment)
        {
            case VerticalAlignment.Top:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        rect.X += dragDeltaHorizontal;
                        rect.Y += dragDeltaVertical;
                        return;
                    case HorizontalAlignment.Center:
                        rect.Y += dragDeltaVertical;
                        return;
                    case HorizontalAlignment.Right:
                        rect.Width += SafeValue(rect.Width, MIN_ONE_SIDE_LENGTH, dragDeltaHorizontal);
                        rect.Y += dragDeltaVertical;
                        return;
                }

                break;
            case VerticalAlignment.Center:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        rect.X += dragDeltaHorizontal;
                        return;
                    case HorizontalAlignment.Center:
                        return;
                    case HorizontalAlignment.Right:
                        rect.Width += SafeValue(rect.Width, MIN_ONE_SIDE_LENGTH, dragDeltaHorizontal);
                        return;
                }

                break;
            case VerticalAlignment.Bottom:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        rect.X += dragDeltaHorizontal;
                        LogManager.GetCurrentClassLogger().Trace($"rect.Y(a)={rect.Y}");
                        rect.Height += SafeValue(rect.Height, MIN_ONE_SIDE_LENGTH, dragDeltaVertical);
                        LogManager.GetCurrentClassLogger().Trace($"rect.Y(b)={rect.Y}");
                        return;
                    case HorizontalAlignment.Center:
                        rect.Height += SafeValue(rect.Height, MIN_ONE_SIDE_LENGTH, dragDeltaVertical);
                        return;
                    case HorizontalAlignment.Right:
                        rect.Width += SafeValue(rect.Width, MIN_ONE_SIDE_LENGTH, dragDeltaHorizontal);
                        rect.Height += SafeValue(rect.Height, MIN_ONE_SIDE_LENGTH, dragDeltaVertical);
                        return;
                }

                break;
        }

        throw new Exception("alignment combination is wrong");
    }

    private double SafeValue(double target, double min, double delta)
    {
        if (target + delta < min)
            return min - target;
        return delta;
    }

    private void SetRect(ref Rect rect, Point snapPoint, VerticalAlignment verticalAlignment,
        HorizontalAlignment horizontalAlignment)
    {
        switch (verticalAlignment)
        {
            case VerticalAlignment.Top:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        rect.X = snapPoint.X;
                        rect.Y = snapPoint.Y;
                        return;
                    case HorizontalAlignment.Center:
                        rect.X = snapPoint.X - rect.Width / 2;
                        rect.Y = snapPoint.Y;
                        return;
                    case HorizontalAlignment.Right:
                        rect.Width = snapPoint.X - rect.X;
                        rect.Y = snapPoint.Y;
                        return;
                }

                break;
            case VerticalAlignment.Center:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        rect.X = snapPoint.X;
                        rect.Y = snapPoint.Y - rect.Height / 2;
                        return;
                    case HorizontalAlignment.Center:
                        rect.X = snapPoint.X - rect.Width / 2;
                        rect.Y = snapPoint.Y - rect.Height / 2;
                        return;
                    case HorizontalAlignment.Right:
                        rect.Width = snapPoint.X - rect.X;
                        rect.Y = snapPoint.Y - rect.Height / 2;
                        return;
                }

                break;
            case VerticalAlignment.Bottom:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        rect.X = snapPoint.X;
                        rect.Height = snapPoint.Y - rect.Top;
                        return;
                    case HorizontalAlignment.Center:
                        rect.X = snapPoint.X - rect.Width / 2;
                        rect.Height = snapPoint.Y - rect.Top;
                        return;
                    case HorizontalAlignment.Right:
                        rect.Width = snapPoint.X - rect.X;
                        rect.Height = snapPoint.Y - rect.Top;
                        return;
                }

                break;
        }

        throw new Exception("alignment conbination is wrong");
    }

    private Point GetPosition(Rect rect, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        switch (verticalAlignment)
        {
            case VerticalAlignment.Top:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        return new Point(rect.X, rect.Top);
                    case HorizontalAlignment.Center:
                        return new Point(rect.X + rect.Width / 2, rect.Top);
                    case HorizontalAlignment.Right:
                        return new Point(rect.X + rect.Width, rect.Top);
                }

                break;
            case VerticalAlignment.Center:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        return new Point(rect.X, rect.Top + rect.Height / 2);
                    case HorizontalAlignment.Center:
                        return new Point(rect.X + rect.Width / 2, rect.Top + rect.Height / 2);
                    case HorizontalAlignment.Right:
                        return new Point(rect.X + rect.Width, rect.Top + rect.Height / 2);
                }

                break;
            case VerticalAlignment.Bottom:
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        return new Point(rect.X, rect.Top + rect.Height);
                    case HorizontalAlignment.Center:
                        return new Point(rect.X + rect.Width / 2, rect.Top + rect.Height);
                    case HorizontalAlignment.Right:
                        return new Point(rect.X + rect.Width, rect.Top + rect.Height);
                }

                break;
        }

        throw new Exception("alignment conbination is wrong");
    }

    private static void CalculateDragLimits(IEnumerable<SelectableDesignerItemViewModelBase> selectedDesignerItems,
        out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
    {
        minLeft = double.MaxValue;
        minTop = double.MaxValue;
        minDeltaHorizontal = double.MaxValue;
        minDeltaVertical = double.MaxValue;

        // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
        // calculate min value for each parameter for each item
        foreach (var item in selectedDesignerItems)
            switch (item)
            {
                case DesignerItemViewModelBase designerItemViewModel:
                {
                    var left = designerItemViewModel.Left.Value;
                    var top = designerItemViewModel.Top.Value;

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                    minDeltaVertical = Math.Min(minDeltaVertical, designerItemViewModel.Height.Value - designerItemViewModel.MinHeight);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, designerItemViewModel.Width.Value - designerItemViewModel.MinWidth);
                    break;
                }
                case ConnectorBaseViewModel connectorBaseViewModel:
                {
                    var left = Math.Min(connectorBaseViewModel.Points[0].X, connectorBaseViewModel.Points[1].X);
                    var top = Math.Min(connectorBaseViewModel.Points[0].Y, connectorBaseViewModel.Points[1].Y);

                    var width = Math.Max(connectorBaseViewModel.Points[0].X, connectorBaseViewModel.Points[1].X) -
                                Math.Min(connectorBaseViewModel.Points[0].X, connectorBaseViewModel.Points[1].X);
                    var height = Math.Max(connectorBaseViewModel.Points[0].Y, connectorBaseViewModel.Points[1].Y) -
                                 Math.Min(connectorBaseViewModel.Points[0].Y, connectorBaseViewModel.Points[1].Y);

                    minDeltaVertical = Math.Min(minDeltaVertical, height);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, width);
                    break;
                }
            }
    }

    private void RemoveAllAdornerFromAdornerLayerAndDictionary(DesignerCanvas designerCanvas)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
        var removes = _adorners.AsValueEnumerable().ToList();

        removes.ForEach(x =>
        {
            adornerLayer?.Remove(x.Value);
            _adorners.Remove(x.Key);
        });
    }

    private void RemoveFromAdornerLayerAndDictionary(Point? snapped, AdornerLayer? adornerLayer)
    {
        var removes = _adorners.AsValueEnumerable().Where(x => x.Key != snapped)
            .ToList();
        removes.ForEach(x =>
        {
            adornerLayer?.Remove(x.Value);
            _adorners.Remove(x.Key);
        });
    }

    public override string ToString()
    {
        return base.ToString() + $" Margin={Margin}";
    }
}