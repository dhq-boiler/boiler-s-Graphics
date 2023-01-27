using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using boilersGraphics.Extensions;

namespace boilersGraphics.Views;

/// <summary>
///     SelectableLabel.xaml の相互作用ロジック
/// </summary>
[DesignTimeVisible(true)]
public partial class SelectableLabel : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
        typeof(string),
        typeof(SelectableLabel),
        new FrameworkPropertyMetadata(null, OnTextChanged));

    public SelectableLabel()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Label TargetLabel => Button_Label.GetVisualChild<Label>();

    public TextBox TargetTextBox => Button_Label.GetVisualChild<TextBox>();

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as SelectableLabel;
        if (ctrl != null) ctrl.Button_Label.Content = ctrl.Text;
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