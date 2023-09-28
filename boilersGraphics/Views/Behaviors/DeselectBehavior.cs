using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors;

public class DeselectBehavior : Behavior<DesignerCanvas>
{
    protected override void OnAttached()
    {
        AssociatedObject.StylusDown += AssociatedObject_StylusDown;
        AssociatedObject.MouseDown += AssociatedObject_PreviewMouseDown;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
        AssociatedObject.MouseDown -= AssociatedObject_PreviewMouseDown;
        base.OnDetaching();
    }

    private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
    {
        Internal_PreviewMouseDown(e);
    }

    private void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Internal_PreviewMouseDown(e);
    }

    private void Internal_PreviewMouseDown(InputEventArgs e)
    {
        var frameworkElement = e.OriginalSource as FrameworkElement;
        if (frameworkElement == null)
            frameworkElement = (e.OriginalSource as Run).Parent as FrameworkElement;
        //PreviewMouseDownイベントで得られる ViewModel を取得する
        var viewModel = frameworkElement.DataContext as SelectableDesignerItemViewModelBase;
        foreach (var item in AssociatedObject.Children)
        {
            //LeftShift or RightShiftを押下している時は以降の処理しない
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) return;

            var vm = (item as FrameworkElement).DataContext as SelectableDesignerItemViewModelBase;

            //item の ViewModel がPreviewMouseDownイベントで得られる ViewModel と異なる場合
            //即ち、選択外の ViewModelの場合
            if (vm != viewModel)
            {
                //非選択状態にする
                vm.IsSelected.Value = false;
                if (vm is ConnectorBaseViewModel connectorBaseViewModel)
                {
                    connectorBaseViewModel.SnapPoint0VM.Value.IsSelected.Value = false;
                    connectorBaseViewModel.SnapPoint1VM.Value.IsSelected.Value = false;
                }
            }
        }

        UpdateStatisticsCount();
    }

    private static void UpdateStatisticsCount()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        statistics.NumberOfClicksWithThePointerTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }
}