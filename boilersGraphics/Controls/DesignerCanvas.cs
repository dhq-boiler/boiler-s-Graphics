using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Messenger;
using boilersGraphics.Strategies;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Controls
{
    public class DesignerCanvas : Canvas
    {
        private HashSet<Connector> _connectorsHit = new HashSet<Connector>();

        public DesignerCanvas()
        {
            this.AllowDrop = true;
            LineFactory = new StraightLineFactory();
            Mediator.Instance.Register(this);
        }

        public LineFactory LineFactory { get; set; }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            Mediator.Instance.NotifyColleagues<bool>("DoneDrawingMessage", true);

            _connectorsHit = new HashSet<Connector>();
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var position = e.GetPosition(this);

            (DataContext as DiagramViewModel).CurrentPoint = position;
        }


        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();

            foreach (UIElement element in this.InternalChildren)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }
            // add margin 
            size.Width += 10;
            size.Height += 10;
            return size;
        }

        private void HitTesting(Point hitPoint)
        {
            _connectorsHit.Clear();
            Debug.WriteLine("----------------");
            HitTestResultBehavior callback(HitTestResult result)
            {
                Debug.WriteLine(result.VisualHit);
                var connector = result.VisualHit.GetParentOfType<Connector>();
                if (connector != null)
                {
                    _connectorsHit.Add(connector);
                }
                Debug.WriteLine("----continue----");
                return HitTestResultBehavior.Continue;
            }
            VisualTreeHelper.HitTest(this, null, callback, new GeometryHitTestParameters(new RectangleGeometry(new Rect(hitPoint.X - 2, hitPoint.Y - 2, 4, 4))));

            Debug.WriteLine($"ConnectorHitCount:{_connectorsHit.Count}");
        }


        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            DragObject dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (dragObject != null)
            {
                (DataContext as IDiagramViewModel).ClearSelectedItemsCommand.Execute(null);
                Point position = e.GetPosition(this);
                DesignerItemViewModelBase itemBase = (DesignerItemViewModelBase)Activator.CreateInstance(dragObject.ContentType);
                itemBase.Left.Value = Math.Max(0, position.X - DesignerItemViewModelBase.DefaultWidth / 2);
                itemBase.Top.Value = Math.Max(0, position.Y - DesignerItemViewModelBase.DefaultHeight / 2);
                itemBase.IsSelected = true;
                (DataContext as IDiagramViewModel).AddItemCommand.Execute(itemBase);
            }
            e.Handled = true;
        }
    }
}
