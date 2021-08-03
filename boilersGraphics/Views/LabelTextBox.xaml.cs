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
using boilersGraphics.Extensions;

namespace boilersGraphics.Views
{
    /// <summary>
    /// LabelTextBox.xaml の相互作用ロジック
    /// </summary>
    public partial class LabelTextBox : UserControl
    {
        public LabelTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LabelTextBox));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TextBlock TargetTextBlock
        {
            get { return this.LabelTextBox_internal_textblock; }
        }

        public TextBox TargetTextBox
        {
            get { return this.LabelTextBox_internal_textbox; }
        }

        private void textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TargetTextBlock.Visibility = Visibility.Visible;
            TargetTextBox.Visibility = Visibility.Collapsed;
        }

        public void FocusTextBox()
        {
            TargetTextBlock.Visibility = Visibility.Collapsed;
            TargetTextBox.Visibility = Visibility.Visible;
            TargetTextBox.Focus();
            TargetTextBox.SelectAll();
        }

        private void textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Escape:
                    TargetTextBlock.Visibility = Visibility.Visible;
                    TargetTextBox.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}
