using System.Windows.Controls;

namespace boilersGraphics.Views;

public partial class PromoteToPart : UserControl
{
    public PromoteToPart()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            PartNameTextBox.Focus();
            PartNameTextBox.SelectAll();
        };
    }
}
