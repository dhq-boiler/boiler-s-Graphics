using boilersGraphics.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    [Obsolete]
    public abstract class AbstractDragStartBehavior : Behavior<FrameworkElement>
    {
        private Point _origin;
        private bool _isButtonDown;
        private IInputElement _dragItem;
        private Point _dragStartPos;
        private AbstractDragAdorner _dragGhost;
        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects),
                    typeof(AbstractDragStartBehavior), new UIPropertyMetadata(DragDropEffects.All));

        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.Register("DragDropData", typeof(object),
                    typeof(AbstractDragStartBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty IsDragEnableProperty =
            DependencyProperty.Register("IsDragEnable", typeof(bool),
                    typeof(AbstractDragStartBehavior), new UIPropertyMetadata(true));

        public DragDropEffects AllowedEffects
        {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }
        public object DragDropData
        {
            get { return GetValue(DragDropDataProperty); }
            set { SetValue(DragDropDataProperty, value); }
        }
        public bool IsDragEnable
        {
            get { return (bool)GetValue(IsDragEnableProperty); }
            set { SetValue(IsDragEnableProperty, value); }
        }


        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove += PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp += PreviewMouseUpHandler;
            this.AssociatedObject.QueryContinueDrag += QueryContinueDragHandler;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDown -= PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove -= PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp -= PreviewMouseUpHandler;
            this.AssociatedObject.QueryContinueDrag -= QueryContinueDragHandler;
            base.OnDetaching();
        }

        private void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            _origin = e.GetPosition(this.AssociatedObject);
            _isButtonDown = true;

            if (sender is IInputElement)
            {
                // マウスダウンされたアイテムを記憶
                _dragItem = sender as IInputElement;
                // マウスダウン時の座標を取得
                _dragStartPos = e.GetPosition(_dragItem);
            }
        }

        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            if (e.LeftButton != MouseButtonState.Pressed || !_isButtonDown)
            {
                return;
            }
            var point = e.GetPosition(this.AssociatedObject);

            if (CheckDistance(point, _origin))
            {
                // アクティブWindowの直下のContentに対して、Adornerを付加する
                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

                if (window != null)
                {
                    var root = window.Content as UIElement;
                    var layer = AdornerLayer.GetAdornerLayer(root);
                    _dragGhost = CreateDragAdorner(root, (UIElement)sender, 0.5, _dragStartPos);
                    layer.Add(_dragGhost);
                    var dragItem = new DraggingItem()
                    {
                        Item = DragDropData,
                        XOffset = point.X,
                        YOffset = point.Y
                    };
                    DragDrop.DoDragDrop(this.AssociatedObject, dragItem, this.AllowedEffects);
                    layer.Remove(_dragGhost);
                }
                else
                {
                    var dragItem = new DraggingItem()
                    {
                        Item = DragDropData,
                        XOffset = point.X,
                        YOffset = point.Y
                    };
                    DragDrop.DoDragDrop(this.AssociatedObject, dragItem, this.AllowedEffects);
                }
                _isButtonDown = false;
                e.Handled = true;
                _dragGhost = null;
                _dragItem = null;
            }
        }

        protected abstract AbstractDragAdorner CreateDragAdorner(UIElement owner, UIElement adornedElement, double opacity, Point dragPos);

        private void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        private bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        private void QueryContinueDragHandler(object sender, QueryContinueDragEventArgs e)
        {
            if (!this.IsDragEnable)
            {
                return;
            }
            if (_dragGhost != null)
            {
                var p = CursorInfo.GetNowPosition((Visual)_dragItem);
                _dragGhost.LeftOffset = p.X;
                _dragGhost.TopOffset = p.Y;
            }
        }
    }
}
