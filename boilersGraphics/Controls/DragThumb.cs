using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Controls;

public class DragThumb : Thumb
{
    public DragThumb()
    {
        DragDelta += DragThumb_DragDelta;
    }

    public OperationRecorder Recorder { get; } =
        new((Application.Current.MainWindow.DataContext as MainWindowViewModel).Controller);

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Recorder.BeginRecode();
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";
        Recorder.EndRecode();

        var item = DataContext as DesignerItemViewModelBase;
        LogManager.GetCurrentClassLogger().Info($"Move item {item.ShowPropertiesAndFields()}");
    }

    private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var designerItem = DataContext as DesignerItemViewModelBase;

        if (!designerItem.CanDrag.Value)
            return;

        if (designerItem != null)
        {
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Properties.Resources.String_Move;

            SelectableDesignerItemViewModelBase.Disconnect(designerItem);

            var minLeft = double.MaxValue;
            var minTop = double.MaxValue;

            // we only move DesignerItems
            var designerItems = designerItem.SelectedItems.OfType<DesignerItemViewModelBase>();

            if (designerItem.Owner.BackgroundItem.Value.EdgeBrush.Value == Brushes.Magenta
                && designerItem.Owner.BackgroundItem.Value.EdgeThickness.Value == 10)
                designerItems = designerItems.Union(new DesignerItemViewModelBase[]
                    { designerItem.Owner.BackgroundItem.Value });

            foreach (var item in designerItems)
            {
                var left = item.Left.Value;
                var top = item.Top.Value;

                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
            }

            var deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
            var deltaVertical = Math.Max(-minTop, e.VerticalChange);

            foreach (var item in designerItems)
            {
                var matrixTransform = (Parent as Grid).RenderTransform as MatrixTransform;
                var left = item.Left.Value;
                var top = item.Top.Value;

                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                if (matrixTransform != null)
                {
                    var dragDelta = new Point(e.HorizontalChange, e.VerticalChange);
                    dragDelta = matrixTransform.Transform(dragDelta);
                    Recorder.Current.ExecuteSetProperty(item, "Left.Value", left + dragDelta.X);
                    Recorder.Current.ExecuteSetProperty(item, "Top.Value", top + dragDelta.Y);
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                        $"(x, y) = ({item.Left.Value}, {item.Top.Value}) (x+, y+) = ({dragDelta.X}, {dragDelta.Y})";
                }
                else
                {
                    Recorder.Current.ExecuteSetProperty(item, "Left.Value", left + deltaHorizontal);
                    Recorder.Current.ExecuteSetProperty(item, "Top.Value", top + deltaVertical);
                    (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                        $"(x, y) = ({item.Left.Value}, {item.Top.Value}) (x+, y+) = ({deltaHorizontal}, {deltaVertical})";
                }
            }

            e.Handled = true;
        }
    }
}