using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Views;

public partial class PartEditor : UserControl
{
    public PartEditor()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        };
    }
}
