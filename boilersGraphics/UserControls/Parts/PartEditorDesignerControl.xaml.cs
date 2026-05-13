using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace boilersGraphics.UserControls.Parts;

public partial class PartEditorDesignerControl : UserControl
{
    private DesignerItemViewModelBase _dragTarget;
    private Point _dragOriginPoint;
    private double _dragOriginLeft;
    private double _dragOriginTop;

    public PartEditorDesignerControl()
    {
        InitializeComponent();
    }

    private void OnItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not PartEditorViewModel vm ||
            sender is not ContentPresenter { Content: SelectableDesignerItemViewModelBase item })
            return;

        vm.SelectItem(item);
        Focus();

        if (item is DesignerItemViewModelBase designerItem)
        {
            _dragOriginLeft = designerItem.Left.Value;
            _dragOriginTop = designerItem.Top.Value;
            _dragTarget = designerItem;
            _dragOriginPoint = e.GetPosition(this);
            CaptureMouse();
            e.Handled = true;
        }
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragTarget is null) return;
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            EndDrag();
            return;
        }

        var current = e.GetPosition(this);
        _dragTarget.Left.Value = _dragOriginLeft + (current.X - _dragOriginPoint.X);
        _dragTarget.Top.Value = _dragOriginTop + (current.Y - _dragOriginPoint.Y);
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragTarget is not null) EndDrag();
    }

    private void EndDrag()
    {
        _dragTarget = null;
        if (IsMouseCaptured) ReleaseMouseCapture();
    }
}
