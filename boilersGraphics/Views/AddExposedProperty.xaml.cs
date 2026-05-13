using System.Windows.Controls;

namespace boilersGraphics.Views;

public partial class AddExposedProperty : UserControl
{
    public AddExposedProperty()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        };
    }
}
