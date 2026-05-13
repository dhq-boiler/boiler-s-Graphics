using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels.Parts;
using R3;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace boilersGraphics.UserControls;

/// <summary>
///     DiagramControl.xaml の相互作用ロジック
/// </summary>
public partial class DiagramControl : UserControl
{
    public DiagramControl()
    {
        InitializeComponent();
    }

    private void DesignerCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        var myDesignerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        zoomBox.DesignerCanvas = myDesignerCanvas;
    }

    private void OnDesignerItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 &&
            sender is ContentPresenter { Content: PartInstanceViewModel partInstance } &&
            partInstance.MouseDoubleClickCommand is { } command &&
            command.CanExecute())
        {
            command.Execute(Unit.Default);
            e.Handled = true;
        }
    }
}
