using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Text;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Adorners;

/// <summary>
/// Phase 2-b-2: モノスペーステキストツールでドラッグした矩形プレビュー + 配置確定。
/// LetterAdorner と同じ枠組み (ドラッグ中の矩形プレビュー → MouseUp で MonoTextBlockViewModel を生成して Layer に追加)。
/// </summary>
public class MonoTextBlockAdorner : Adorner
{
    private readonly DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly Pen _rectanglePen;
    private Point? _startPoint;

    public MonoTextBlockAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        var brush = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
        _rectanglePen = new Pen(brush, 1);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured)
                CaptureMouse();

            _endPoint = e.GetPosition(this);
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
        if (IsMouseCaptured) ReleaseMouseCapture();

        var adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
        adornerLayer?.Remove(this);

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var vm = new MonoTextBlockViewModel
            {
                Owner = (AdornedElement as DesignerCanvas)?.DataContext as IDiagramViewModel,
            };
            vm.Left.Value = Math.Max(0, _startPoint.Value.X);
            vm.Top.Value = Math.Max(0, _startPoint.Value.Y);
            vm.Width.Value = Math.Max(1, Math.Abs(_endPoint.Value.X - _startPoint.Value.X));
            vm.Height.Value = Math.Max(1, Math.Abs(_endPoint.Value.Y - _startPoint.Value.Y));
            vm.IsVisible.Value = true;

            if (vm.Owner is not null)
            {
                vm.Owner.DeselectAll();
                vm.IsSelected.Value = true;
                vm.ZIndex.Value = vm.Owner.Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .AsValueEnumerable().Count();
                vm.Owner.AddItemCommand.Execute(vm);
            }

            _startPoint = null;
            _endPoint = null;
        }

        if (Application.Current?.MainWindow?.DataContext is MainWindowViewModel mvm)
        {
            mvm.CurrentOperation.Value = string.Empty;
            mvm.Details.Value = string.Empty;
        }

        e.Handled = true;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
            dc.DrawRectangle(Brushes.Transparent, _rectanglePen, new Rect(_startPoint.Value, _endPoint.Value));
    }
}
