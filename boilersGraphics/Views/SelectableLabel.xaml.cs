using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using boilersGraphics.Extensions;

namespace boilersGraphics.Views
{
    /// <summary>
    /// SelectableLabel.xaml の相互作用ロジック
    /// </summary>
    [DesignTimeVisible(true)]
    public partial class SelectableLabel : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string),
            typeof(SelectableLabel),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(SelectableLabel.OnTextChanged)));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SelectableLabel ctrl = d as SelectableLabel;
            if (ctrl != null)
            {
                ctrl.Button_Label.Content = ctrl.Text;
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public SelectableLabel()
        {
            InitializeComponent();
        }

        public Label TargetLabel
        {
            get { return Button_Label.GetVisualChild<Label>(); }
        }

        public TextBox TargetTextBox
        {
            get { return Button_Label.GetVisualChild<TextBox>(); }
        }

        private void Button_Label_Click(object sender, RoutedEventArgs e)
        {
            TargetLabel.Visibility = Visibility.Collapsed;
            TargetTextBox.Visibility = Visibility.Visible;
            TargetTextBox.Focus();
            TargetTextBox.SelectAll();
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            TargetLabel.Visibility = Visibility.Visible;
            TargetTextBox.Visibility = Visibility.Collapsed;
        }
    }
}
