using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace boilersGraphics.Views;

/// <summary>
///     MainWindow.xaml の相互作用ロジック
/// </summary>
public partial class MainWindow : Window
{
    private const int WM_NCHITTEST = 0x0084;
    private const int WM_NCLBUTTONDOWN = 0x00A1;

    /// <summary>DPI Scale for current display</summary>
    private const double DPI_SCALE = 1.0;

    private const int HTMAXBUTTON = 9;
    private SolidColorBrush _bgMouseHover = new(Color.FromArgb(0xff, 0x00, 0xff, 0x00));
    private readonly SolidColorBrush _bgNormal = new(Color.FromArgb(0xff, 0x2a, 0x2a, 0x2a));

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (o, e) =>
        {
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(WndProc);
        };
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_NCHITTEST:
                try
                {
                    var xl = lParam.ToInt32() & 0xffff;
                    var yl = lParam.ToInt32() >> 16;
                    var _btn1 = GetCommandButton();
                    var rectx = new Rect(_btn1.PointToScreen(
                            new Point()),
                        new Size(_btn1.Width * DPI_SCALE, _btn1.Height * DPI_SCALE));
                    if (rectx.Contains(new Point(xl, yl)))
                    {
                        handled = true;
                        var colorAnimationUsingKeyFrames = new ColorAnimationUsingKeyFrames();
                        colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame(Colors.Lime,
                            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))));
                        _btn1.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimationUsingKeyFrames);
                    }
                    else
                    {
                        if (_btn1.Background is null) _btn1.Background = _bgNormal;
                        var colorAnimationUsingKeyFrames = new ColorAnimationUsingKeyFrames();
                        colorAnimationUsingKeyFrames.KeyFrames.Add(new EasingColorKeyFrame(
                            Color.FromArgb(0xff, 0x2a, 0x2a, 0x2a),
                            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
                        _btn1.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimationUsingKeyFrames);
                    }

                    return new nint(HTMAXBUTTON);
                }
                catch (OverflowException)
                {
                    handled = true;
                }

                break;
            case WM_NCLBUTTONDOWN:
                var x = lParam.ToInt32() & 0xffff;
                var y = lParam.ToInt32() >> 16;
                var _btn2 = GetCommandButton();
                var rect = new Rect(_btn2.PointToScreen(
                        new Point()),
                    new Size(_btn2.Width * DPI_SCALE, _btn2.Height * DPI_SCALE));
                if (rect.Contains(new Point(x, y)))
                {
                    handled = true;
                    var invokeProv =
                        new ButtonAutomationPeer(_btn2).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invokeProv?.Invoke();
                }

                break;
            default:
                handled = false;
                break;
        }

        return nint.Zero;
    }

    private Button GetCommandButton()
    {
        return WindowState == WindowState.Maximized
            ? MainWindowInstance.Template.FindName("RestoreButton", MainWindowInstance) as Button
            : MainWindowInstance.Template.FindName("MaximizeButton", MainWindowInstance) as Button;
    }

    private void CommandBinding_CanExecute_Close(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void CommandBinding_CanExecute_Maximize(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MaximizeWindow(this);
    }

    private void CommandBinding_CanExecute_Minimize(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void CommandBinding_CanExecute_Restore(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.RestoreWindow(this);
    }
}