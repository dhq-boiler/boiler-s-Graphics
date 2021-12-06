using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace boilersGraphics.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (o, e) =>
            {
                var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            };
        }

        const int WM_NCHITTEST = 0x0084;
        const int WM_NCLBUTTONDOWN = 0x00A1;
        /// <summary>DPI Scale for current display</summary>
        private const double DPI_SCALE = 1.0;
        private const int HTMAXBUTTON = 9;
        private SolidColorBrush _bgNormal = new SolidColorBrush(Color.FromArgb(0xff, 0xbe, 0xe6, 0xfe));
        private SolidColorBrush _bgMouseHover = new SolidColorBrush(Color.FromArgb(0xff, 0xdd, 0xdd, 0xdd));
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCHITTEST:
                    try
                    {
                        int xl = lParam.ToInt32() & 0xffff;
                        int yl = lParam.ToInt32() >> 16;
                        Button _btn1 = GetCommandButton();
                        var rectx = new Rect(_btn1.PointToScreen(
                            new Point()),
                            new Size(_btn1.Width * DPI_SCALE, _btn1.Height * DPI_SCALE));
                        if (rectx.Contains(new Point(xl, yl)))
                        {
                            handled = true;
                            _btn1.Background = _bgMouseHover;
                        }
                        else
                        {
                            _btn1.Background = _bgNormal;
                        }
                        return new IntPtr(HTMAXBUTTON);
                    }
                    catch (OverflowException)
                    {
                        handled = true;
                    }
                    break;
                case WM_NCLBUTTONDOWN:
                    int x = lParam.ToInt32() & 0xffff;
                    int y = lParam.ToInt32() >> 16;
                    Button _btn2 = GetCommandButton();
                    var rect = new Rect(_btn2.PointToScreen(
                        new Point()),
                        new Size(_btn2.Width * DPI_SCALE, _btn2.Height * DPI_SCALE));
                    if (rect.Contains(new Point(x, y)))
                    {
                        handled = true;
                        IInvokeProvider invokeProv = new ButtonAutomationPeer(_btn2).GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                        invokeProv?.Invoke();
                    }
                    break;
                default:
                    handled = false;
                    break;
            }
            return IntPtr.Zero;
        }

        private Button GetCommandButton()
        {
            return WindowState == WindowState.Maximized
                            ? this.MainWindowInstance.Template.FindName("RestoreButton", this.MainWindowInstance) as Button
                            : this.MainWindowInstance.Template.FindName("MaximizeButton", this.MainWindowInstance) as Button;
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
}
