using grapher.Extensions;
using grapher.Helpers;
using grapher.Messenger;
using grapher.Strategies;
using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace grapher.Controls
{
    public class DesignerCanvas : Canvas
    {
        private ConnectorBaseViewModel _partialConnection;
        private HashSet<Connector> _connectorsHit = new HashSet<Connector>();
        private Connector _sourceConnector;
        private bool _isDragging;

        public DesignerCanvas()
        {
            this.AllowDrop = true;
            LineFactory = new StraightLineFactory();
            Mediator.Instance.Register(this);
        }

        public LineFactory LineFactory { get; set; }

        public Connector SourceConnector
        {
            get { return _sourceConnector; }
            set
            {
                if (_sourceConnector != value)
                {
                    _sourceConnector = value;
                    _connectorsHit.Add(_sourceConnector);
                    FullyCreatedConnectorInfo sourceDataItem = _sourceConnector.DataContext as FullyCreatedConnectorInfo;


                    Rect rectangleBounds = _sourceConnector.TransformToVisual(this).TransformBounds(new Rect(_sourceConnector.RenderSize));
                    Point point = new Point(rectangleBounds.Left + (rectangleBounds.Width / 2),
                                            rectangleBounds.Bottom + (rectangleBounds.Height / 2));
                    _partialConnection = LineFactory.Create(sourceDataItem.DataItem.Owner, sourceDataItem, new PartCreatedConnectionInfo(point));
                    _partialConnection.EdgeColor = _partialConnection.Owner.EdgeColors.First();
                    sourceDataItem.DataItem.Owner.AddItemCommand.Execute(_partialConnection);
                }
            }
        }

        public ResizeHandle MoveConnector { get; set; }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (MoveConnector != null)
            {
                _isDragging = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            Mediator.Instance.NotifyColleagues<bool>("DoneDrawingMessage", true);

            if (_sourceConnector != null)
            {
                FullyCreatedConnectorInfo sourceDataItem = _sourceConnector.DataContext as FullyCreatedConnectorInfo;
                if (_connectorsHit.Count() == 1)
                {
                    Connector sinkConnector = _connectorsHit.Last();
                    FullyCreatedConnectorInfo sinkDataItem = sinkConnector.DataContext as FullyCreatedConnectorInfo;

                    int indexOfLastTempConnection = sinkDataItem.DataItem.Owner.Items.Count - 1;
                    var lastTempConnection = sinkDataItem.DataItem.Owner.Items[indexOfLastTempConnection];
                    sinkDataItem.DataItem.Owner.RemoveItemCommand.Execute(lastTempConnection);

                    var line = LineFactory.Create(sinkDataItem.DataItem.Owner, sourceDataItem, sinkDataItem);
                    line.EdgeColor = line.Owner.EdgeColors.First();
                    sinkDataItem.DataItem.Owner.AddItemCommand.Execute(line);
                }
                else
                {
                    //Need to remove last item as we did not finish drawing the path
                    int indexOfLastTempConnection = sourceDataItem.DataItem.Owner.Items.Count - 1;
                    var lastTempConnection = sourceDataItem.DataItem.Owner.Items[indexOfLastTempConnection];
                    sourceDataItem.DataItem.Owner.RemoveItemCommand.Execute(lastTempConnection);

                    var line = LineFactory.Create(sourceDataItem.DataItem.Owner, sourceDataItem, new PartCreatedConnectionInfo(e.GetPosition(this)));
                    line.EdgeColor = line.Owner.EdgeColors.First();
                    sourceDataItem.DataItem.Owner.AddItemCommand.Execute(line);
                }
            }

            if (MoveConnector != null)
            {
                _isDragging = false;

                var viewModel = MoveConnector.DataContext as ConnectorBaseViewModel;
                if (_connectorsHit.Count() >= 2)
                {
                    Connector sinkConnector = _connectorsHit.Where(x => x.DataContext is FullyCreatedConnectorInfo).Last();
                    FullyCreatedConnectorInfo sinkDataItem = sinkConnector.DataContext as FullyCreatedConnectorInfo;

                    switch (MoveConnector.Name)
                    {
                        case "ResizeHandle_BeginPoint":
                            viewModel.SourceConnectorInfo = sinkDataItem;
                            break;
                        case "ResizeHandle_EndPoint":
                            viewModel.SinkConnectorInfo = sinkDataItem;
                            break;
                    }
                }
                else if (_connectorsHit.Count() == 1)
                {
                    switch (MoveConnector.Name)
                    {
                        case "ResizeHandle_BeginPoint":
                            viewModel.SourceConnectorInfo = new PartCreatedConnectionInfo(e.GetPosition(this));
                            break;
                        case "ResizeHandle_EndPoint":
                            viewModel.SinkConnectorInfo = new PartCreatedConnectionInfo(e.GetPosition(this));
                            break;
                    }
                }
                viewModel.IsHitTestVisible = true;

                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";
            }

            _connectorsHit = new HashSet<Connector>();
            _sourceConnector = null;
            MoveConnector = null;
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var position = e.GetPosition(this);

            (DataContext as DiagramViewModel).CurrentPoint = position;

            if (SourceConnector != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _partialConnection.SinkConnectorInfo = new PartCreatedConnectionInfo(position);
                    HitTesting(position);
                }
                e.Handled = true;
                return;
            }

            if (MoveConnector != null && _isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var viewModel = MoveConnector.DataContext as ConnectorBaseViewModel;
                switch (MoveConnector.Name)
                {
                    case "ResizeHandle_BeginPoint":
                        viewModel.SourceA = position;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({position.X}, {position.Y}) - ({viewModel.SourceB.X}, {viewModel.SourceB.Y})";
                        break;
                    case "ResizeHandle_EndPoint":
                        viewModel.SourceB = position;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({viewModel.SourceA.X}, {viewModel.SourceA.Y}) - ({position.X}, {position.Y})";
                        break;
                }

                viewModel.IsHitTestVisible = true;
                HitTesting(position);
                viewModel.IsHitTestVisible = false;
                e.Handled = true;
                return;
            }

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _isDragging = false;
            }
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
