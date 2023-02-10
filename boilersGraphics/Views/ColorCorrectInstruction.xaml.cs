using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace boilersGraphics.Views
{
    /// <summary>
    /// ColorCorrectInstruction.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorCorrectInstruction : UserControl
    {
        public ColorCorrectInstruction()
        {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute_Close(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            SystemCommands.CloseWindow(window as Window);
        }

        private void CommandBinding_CanExecute_Maximize(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            SystemCommands.MaximizeWindow(window as Window);
        }

        private void CommandBinding_CanExecute_Minimize(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            SystemCommands.MinimizeWindow(window as Window);
        }

        private void CommandBinding_CanExecute_Restore(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            SystemCommands.RestoreWindow(window as Window);
        }
    }
}
