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
                if (viewModel.Model.Width - e.HorizontalChange < 20)
                { }
                else
                {
                    viewModel.Model.X += e.HorizontalChange;
                    viewModel.Model.Width = Math.Max(20, viewModel.Model.Width - e.HorizontalChange);
                }
            }
            else
            {
                viewModel.Model.Width = Math.Max(20, viewModel.Model.Width + e.HorizontalChange);
            }

            //サイズ変更処理(たて)
            if (thumb.Name == "ResizeThumb_LT" || thumb.Name == "ResizeThumb_RT")
            {
                if (viewModel.Model.Height - e.VerticalChange < 20)
                { }
                else
                {
                    viewModel.Model.Y += e.VerticalChange;
                    viewModel.Model.Height = Math.Max(20, viewModel.Model.Height - e.VerticalChange);
                }
            }
            else
            {
                viewModel.Model.Height = Math.Max(20, viewModel.Model.Height + e.VerticalChange);
            }
            e.Handled = true;
        }
    }
}
