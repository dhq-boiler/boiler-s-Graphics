using boilersGraphics.Helpers;
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

namespace boilersGraphics.Controls
{
    public class DragThumb : Thumb
    {
        public OperationRecorder Recorder { get; } = new OperationRecorder((App.Current.MainWindow.DataContext as MainWindowViewModel).Controller);

        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Recorder.BeginRecode();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";
            Recorder.EndRecode();

            var item = this.DataContext as DesignerItemViewModelBase;
            LogManager.GetCurrentClassLogger().Info($"Move item {item.ShowPropertiesAndFields()}");
        }

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItemViewModelBase designerItem = this.DataContext as DesignerItemViewModelBase;

            if (!designerItem.CanDrag.Value)
                return;

            if (designerItem != null)
            {
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = boilersGraphics.Properties.Resources.String_Move;

                SelectableDesignerItemViewModelBase.Disconnect(designerItem);

                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // we only move DesignerItems
                var designerItems = designerItem.SelectedItems.OfType<DesignerItemViewModelBase>();

                foreach (var item in designerItems)
                {
                    double left = item.Left.Value;
                    double top = item.Top.Value;

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (var item in designerItems)
                {
                    var matrixTransform = (this.Parent as Grid).RenderTransform as MatrixTransform;
                    double left = item.Left.Value;
                    double top = item.Top.Value;

                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;

                    if (matrixTransform != null)
                    {
                        var dragDelta = new Point(e.HorizontalChange, e.VerticalChange);
                        dragDelta = matrixTransform.Transform(dragDelta);
                        Recorder.Current.ExecuteSetProperty(item, "Left.Value", left + dragDelta.X);
                        Recorder.Current.ExecuteSetProperty(item, "Top.Value", top + dragDelta.Y);
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"(x, y) = ({item.Left.Value}, {item.Top.Value}) (x+, y+) = ({dragDelta.X}, {dragDelta.Y})";
                    }
                    else
                    {
                        Recorder.Current.ExecuteSetProperty(item, "Left.Value", left + deltaHorizontal);
                        Recorder.Current.ExecuteSetProperty(item, "Top.Value", top + deltaVertical);
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"(x, y) = ({item.Left.Value}, {item.Top.Value}) (x+, y+) = ({deltaHorizontal}, {deltaVertical})";
                    }
                }
                e.Handled = true;
            }
        }
    }
}
