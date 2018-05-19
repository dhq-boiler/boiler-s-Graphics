using grapher.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace grapher.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as MainWindowViewModel).Initialize();
        }

        private void ResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {

            var thumb = sender as Thumb;
            if (thumb == null) return;

            //サイズ変更の対象要素を取得する
            var adored = AdornedBy.GetAdornedElementFromTemplateChild(thumb) as FrameworkElement;
            if (adored == null) return;

            var viewModel = adored.DataContext as RenderItemViewModel;

            //サイズ変更処理(横)
            if (thumb.Name == "ResizeThumb_LT" || thumb.Name == "ResizeThumb_LB")
            {
                viewModel.X.Value += e.HorizontalChange;
                viewModel.Width.Value = Math.Max(20, viewModel.Width.Value - e.HorizontalChange);
            }
            else
            {
                viewModel.Width.Value = Math.Max(20, viewModel.Width.Value + e.HorizontalChange);
            }

            //サイズ変更処理(たて)
            if (thumb.Name == "ResizeThumb_LT" || thumb.Name == "ResizeThumb_RT")
            {
                viewModel.Y.Value += e.VerticalChange;
                viewModel.Height.Value = Math.Max(20, viewModel.Height.Value - e.VerticalChange);
            }
            else
            {
                viewModel.Height.Value = Math.Max(20, viewModel.Height.Value + e.VerticalChange);
            }
            e.Handled = true;
        }
    }
}
