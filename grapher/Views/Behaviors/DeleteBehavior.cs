using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace grapher.Views.Behaviors
{
    class DeleteBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                var mainwindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                var removes = mainwindowViewModel.RenderItems.Where(i => i.IsSelected.Value).ToList();
                foreach (var viewModel in removes)
                {
                    mainwindowViewModel.RenderItems.Remove(viewModel);
                }
            }
        }
    }
}
