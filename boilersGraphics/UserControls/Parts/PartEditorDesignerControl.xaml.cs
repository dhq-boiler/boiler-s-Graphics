using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using System;
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
            try
            {
                _dragOriginLeft = designerItem.Left.Value;
                _dragOriginTop = designerItem.Top.Value;
            }
            catch (ObjectDisposedException)
            {
                // 先に Dispose された Items は drag 対象外。選択のみ。
                return;
            }

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
        try
        {
            _dragTarget.Left.Value = _dragOriginLeft + (current.X - _dragOriginPoint.X);
            _dragTarget.Top.Value = _dragOriginTop + (current.Y - _dragOriginPoint.Y);
        }
        catch (ObjectDisposedException)
        {
            // Promote / Detach で先に Dispose された Items が SelectItem 経由で
            // _dragTarget になっているケース。drag は静かに諦める。
            EndDrag();
        }
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
