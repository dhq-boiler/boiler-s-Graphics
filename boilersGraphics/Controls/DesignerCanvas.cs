using boilersGraphics.Helpers;
using boilersGraphics.Messenger;
using boilersGraphics.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace boilersGraphics.Controls;

public class DesignerCanvas : Canvas
{
    private static DesignerCanvas _instance;

    public DesignerCanvas()
    {
        _instance = this;
        AllowDrop = true;
        Mediator.Instance.Register(this);
    }

    [DebuggerStepThrough]
    public static DesignerCanvas GetInstance()
    {
        return _instance;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        Mediator.Instance.NotifyColleagues("DoneDrawingMessage", true);
    }


    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var position = e.GetPosition(this);

        (DataContext as DiagramViewModel).CurrentPoint = position;
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);
        var dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
        if (dragObject != null)
        {
            (DataContext as IDiagramViewModel).ClearSelectedItemsCommand.Execute(null);
            var position = e.GetPosition(this);
            var itemBase = (DesignerItemViewModelBase)Activator.CreateInstance(dragObject.ContentType);
            itemBase.Left.Value = Math.Max(0, position.X - DesignerItemViewModelBase.DefaultWidth / 2);
            itemBase.Top.Value = Math.Max(0, position.Y - DesignerItemViewModelBase.DefaultHeight / 2);
            itemBase.IsSelected.Value = true;
            (DataContext as IDiagramViewModel).AddItemCommand.Execute(itemBase);
        }

        e.Handled = true;
    }
}