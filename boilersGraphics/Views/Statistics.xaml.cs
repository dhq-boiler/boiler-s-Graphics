using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Views;

/// <summary>
///     Statistics.xaml の相互作用ロジック
/// </summary>
public partial class Statistics : UserControl
{
    private Window _attachedWindow;
    private KeyBinding _escBinding;

    public Statistics()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // M-1 修正: モーダルダイアログ慣例に従い Esc キーで閉じられるようにする。
    // Prism Dialog の Window は外部スタイル経由で生成されるため、Loaded で Window を取得して
    // 動的に InputBindings に Esc → CloseDialogCommand を登録する。
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _attachedWindow = Window.GetWindow(this);
        if (_attachedWindow != null && DataContext is StatisticsDialogViewModel vm)
        {
            _escBinding = new KeyBinding(vm.CloseDialogCommand, Key.Escape, ModifierKeys.None);
            _attachedWindow.InputBindings.Add(_escBinding);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_attachedWindow != null && _escBinding != null)
        {
            _attachedWindow.InputBindings.Remove(_escBinding);
        }
        _attachedWindow = null;
        _escBinding = null;
    }
}