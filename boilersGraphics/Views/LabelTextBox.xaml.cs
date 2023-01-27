using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace boilersGraphics.Views;

/// <summary>
///     LabelTextBox.xaml の相互作用ロジック
/// </summary>
public partial class LabelTextBox : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(LabelTextBox));

    public LabelTextBox()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextBlock TargetTextBlock => LabelTextBox_internal_textblock;

    public TextBox TargetTextBox => LabelTextBox_internal_textbox;

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