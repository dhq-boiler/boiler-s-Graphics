using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    public class DeselectBehavior : Behavior<DesignerCanvas>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.MouseDown += AssociatedObject_PreviewMouseDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseDown -= AssociatedObject_PreviewMouseDown;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //PreviewMouseDownイベントで得られる ViewModel を取得する
            var viewModel = (e.OriginalSource as FrameworkElement).DataContext as SelectableDesignerItemViewModelBase;
            foreach (var item in this.AssociatedObject.Children)
            {
                //LeftShift or RightShiftを押下している時は以降の処理しない
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    return;
                }

                var vm = ((item as FrameworkElement).DataContext as SelectableDesignerItemViewModelBase);

                //item の ViewModel がPreviewMouseDownイベントで得られる ViewModel と異なる場合
                //即ち、選択外の ViewModelの場合
                if (vm != viewModel)
                {
                    //非選択状態にする
                    vm.IsSelected = false;
                }
            }
        }
    }
}
