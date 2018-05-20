using grapher.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace grapher.Views.Triggers
{
    class ResizeThumbDragDeltaEventTrigger : System.Windows.Interactivity.EventTrigger
    {
        public ResizeThumbDragDeltaEventTrigger()
            : base("DragDelta")
        { }

        protected override void OnEvent(EventArgs eventArgs)
        {
            var e = eventArgs as DragDeltaEventArgs;
            var sender = AssociatedObject;

            var thumb = sender as Thumb;
            if (thumb == null) return;

            //サイズ変更の対象要素を取得する
            var adored = AdornedBy.GetAdornedElementFromTemplateChild(thumb) as FrameworkElement;
            if (adored == null) return;

            var viewModel = adored.DataContext as RenderItemViewModel;

            //サイズ変更処理(横)
            if (thumb.Name == "ResizeThumb_LT" || thumb.Name == "ResizeThumb_LB")
            {
                if (viewModel.Width.Value - e.HorizontalChange < 20)
                { }
                else
                {
                    viewModel.X.Value += e.HorizontalChange;
                    viewModel.Width.Value = Math.Max(20, viewModel.Width.Value - e.HorizontalChange);
                }
            }
            else
            {
                viewModel.Width.Value = Math.Max(20, viewModel.Width.Value + e.HorizontalChange);
            }

            //サイズ変更処理(たて)
            if (thumb.Name == "ResizeThumb_LT" || thumb.Name == "ResizeThumb_RT")
            {
                if (viewModel.Height.Value - e.VerticalChange < 20)
                { }
                else
                {
                    viewModel.Y.Value += e.VerticalChange;
                    viewModel.Height.Value = Math.Max(20, viewModel.Height.Value - e.VerticalChange);
                }
            }
            else
            {
                viewModel.Height.Value = Math.Max(20, viewModel.Height.Value + e.VerticalChange);
            }
            e.Handled = true;
        }
    }
}
