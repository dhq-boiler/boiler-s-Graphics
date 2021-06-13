using grapher.Extensions;
using grapher.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;
using System.Windows.Shapes;

namespace grapher.Views.Behaviors
{
    internal class StraightLineDragStartBehavior : Behavior<FrameworkElement>
    {
        private Point _origin;
        private bool _isButtonDown;
        private IInputElement _dragItem;
        private Point _dragStartPos;
        private AbstractDragAdorner _dragGhost;
        public static readonly DependencyProperty AllowedEffectsProperty =
            DependencyProperty.Register("AllowedEffects", typeof(DragDropEffects),
                    typeof(StraightLineDragStartBehavior), new UIPropertyMetadata(DragDropEffects.All));

        public static readonly DependencyProperty DragDropDataProperty =
            DependencyProperty.Register("DragDropData", typeof(object),
                    typeof(StraightLineDragStartBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty IsDragEnableProperty =
            DependencyProperty.Register("IsDragEnable", typeof(bool),
                    typeof(StraightLineDragStartBehavior), new UIPropertyMetadata(true));

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
            var canvas = this.AssociatedObject.GetParentOfType<Canvas>();
            canvas.PreviewMouseDown += Canvas_PreviewMouseDown;
            canvas.PreviewMouseMove += Canvas_PreviewMouseMove;
            canvas.PreviewMouseUp += Canvas_PreviewMouseUp;
            canvas.QueryContinueDrag += Canvas_QueryContinueDrag;
            base.OnAttached();
        }

        private void Canvas_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (StraightLineHitTest(sender, e))
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
        }

        private void Canvas_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (StraightLineHitTest(sender, e))
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
                        _dragGhost = CreateDragAdorner(root, AssociatedObject, 0.5, _dragStartPos);
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
        }

        protected AbstractDragAdorner CreateDragAdorner(UIElement owner, UIElement adornedElement, double opacity, Point dragPos)
        {
            return new StraightLineDragAdorner(owner, adornedElement, opacity, dragPos);
        }

        private void Canvas_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (StraightLineHitTest(sender, e))
            {
                _isButtonDown = false;
            }
        }

        private bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        private void Canvas_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
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

        private bool StraightLineHitTest(object sender, MouseEventArgs e)
        {
            var radius = 3;
            var line = AssociatedObject as Line;
            var position = e.GetPosition((UIElement)sender);
            bool result = false;
            var hitRect = new Rect(position.X - radius, position.Y - radius, radius * 2, radius * 2);
            VisualTreeHelper.HitTest(line, null, htr => { result = true; return HitTestResultBehavior.Stop; }, new GeometryHitTestParameters(new RectangleGeometry(hitRect)));
            return result;
        }
    }
}
