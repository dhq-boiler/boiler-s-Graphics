using grapher.Extensions;
using grapher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;

namespace grapher.Views.Behaviors
{
    class StraightLineDragStartBehavior : Behavior<FrameworkElement>
    {
        private Point origin;
        private bool isButtonDown;
        private IInputElement dragItem;
        private Point dragStartPos;
        private AbstractDragAdorner dragGhost;
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
            var line = AssociatedObject as Line;
            var position = e.GetPosition((UIElement)sender);
            bool result = false;
            var hitRect = new Rect(position.X - 2, position.Y - 2, 4, 4);
            VisualTreeHelper.HitTest(line, null, htr => { result = true; return HitTestResultBehavior.Stop; }, new GeometryHitTestParameters(new RectangleGeometry(hitRect)));

            if (result)
            {
                if (!this.IsDragEnable)
                {
                    return;
                }
                this.origin = e.GetPosition(this.AssociatedObject);
                this.isButtonDown = true;

                if (sender is IInputElement)
                {
                    // マウスダウンされたアイテムを記憶
                    this.dragItem = sender as IInputElement;
                    // マウスダウン時の座標を取得
                    this.dragStartPos = e.GetPosition(this.dragItem);
                }
            }
        }

        private void Canvas_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var line = AssociatedObject as Line;
            var position = e.GetPosition((UIElement)sender);
            bool result = false;
            var hitRect = new Rect(position.X - 2, position.Y - 2, 4, 4);
            VisualTreeHelper.HitTest(line, null, htr => { result = true; return HitTestResultBehavior.Stop; }, new GeometryHitTestParameters(new RectangleGeometry(hitRect)));

            if (result)
            {
                if (!this.IsDragEnable)
                {
                    return;
                }
                if (e.LeftButton != MouseButtonState.Pressed || !this.isButtonDown)
                {
                    return;
                }
                var point = e.GetPosition(this.AssociatedObject);

                if (CheckDistance(point, this.origin))
                {
                    // アクティブWindowの直下のContentに対して、Adornerを付加する
                    var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

                    if (window != null)
                    {
                        var root = window.Content as UIElement;
                        var layer = AdornerLayer.GetAdornerLayer(root);
                        this.dragGhost = CreateDragAdorner(root, AssociatedObject, 0.5, this.dragStartPos);
                        layer.Add(this.dragGhost);
                        var dragItem = new DraggingItem()
                        {
                            Item = DragDropData,
                            XOffset = point.X,
                            YOffset = point.Y
                        };
                        DragDrop.DoDragDrop(this.AssociatedObject, dragItem, this.AllowedEffects);
                        layer.Remove(this.dragGhost);
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
                    this.isButtonDown = false;
                    e.Handled = true;
                    this.dragGhost = null;
                    this.dragItem = null;
                }
            }
        }

        protected AbstractDragAdorner CreateDragAdorner(UIElement owner, UIElement adornedElement, double opacity, Point dragPos)
        {
            return new StraightLineDragAdorner(owner, adornedElement, opacity, dragPos);
        }

        private void Canvas_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var line = AssociatedObject as Line;
            var position = e.GetPosition((UIElement)sender);
            bool result = false;
            var hitRect = new Rect(position.X - 2, position.Y - 2, 4, 4);
            VisualTreeHelper.HitTest(line, null, htr => { result = true; return HitTestResultBehavior.Stop; }, new GeometryHitTestParameters(new RectangleGeometry(hitRect)));

            if (result)
            {
                this.isButtonDown = false;
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
            if (this.dragGhost != null)
            {
                var p = CursorInfo.GetNowPosition((Visual)this.dragItem);
                this.dragGhost.LeftOffset = p.X;
                this.dragGhost.TopOffset = p.Y;
            }
        }
    }
}
