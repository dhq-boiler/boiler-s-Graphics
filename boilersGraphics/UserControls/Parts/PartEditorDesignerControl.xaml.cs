using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using System.Windows.Controls;
using System.Windows.Input;

namespace boilersGraphics.UserControls.Parts;

public partial class PartEditorDesignerControl : UserControl
{
    public PartEditorDesignerControl()
    {
        InitializeComponent();
    }

    private void OnItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is PartEditorViewModel vm &&
            sender is ContentPresenter { Content: SelectableDesignerItemViewModelBase item })
        {
            vm.SelectItem(item);
            Focus();
        }
    }
}
