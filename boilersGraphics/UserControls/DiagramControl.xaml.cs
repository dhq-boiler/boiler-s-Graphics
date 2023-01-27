using System.Windows;
using System.Windows.Controls;
using boilersGraphics.Controls;
using boilersGraphics.Extensions;

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
}