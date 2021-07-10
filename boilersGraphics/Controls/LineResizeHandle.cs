using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Controls
{
    public class LineResizeHandle : SnapPoint
    {
        private Dictionary<Point, Adorner> _adorners;

        public LineResizeHandle()
        {
            _adorners = new Dictionary<Point, Adorner>();
        }

        public static readonly DependencyProperty OppositeHandleProperty = DependencyProperty.Register("OppositeHandle", typeof(LineResizeHandle), typeof(LineResizeHandle));

        public static readonly DependencyProperty TargetPointIndexProperty = DependencyProperty.Register("TargetPointIndex", typeof(int), typeof(LineResizeHandle));

        public LineResizeHandle OppositeHandle
        {
            get { return (LineResizeHandle)GetValue(OppositeHandleProperty); }
            set { SetValue(OppositeHandleProperty, value); }
        }

        public int TargetPointIndex
        {
            get { return (int)GetValue(TargetPointIndexProperty); }
            set { SetValue(TargetPointIndexProperty, value); }
        }

        public Point? BeginDragPoint { get; private set; }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            BeginDragPoint = e.GetPosition(this);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "変形";
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (BeginDragPoint.HasValue)
            {
                Point currentPosition = Mouse.GetPosition(this);
                double diffX = currentPosition.X - BeginDragPoint.Value.X;
                double diffY = currentPosition.Y - BeginDragPoint.Value.Y;
                var vm = DataContext as ConnectorBaseViewModel;
                Point point = vm.Points[TargetPointIndex];
                point.X += diffX;
                point.Y += diffY;

                var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var diagramVM = mainWindowVM.DiagramViewModel;
                if (diagramVM.EnablePointSnap.Value)
                {
                    var snapPoints = diagramVM.GetSnapPoints(new List<SnapPoint>() { this, OppositeHandle });
                    Point? snapped = null;

                    foreach (var snapPoint in snapPoints)
                    {
                        if (point.X > snapPoint.X - mainWindowVM.SnapPower.Value
                            && point.X < snapPoint.X + mainWindowVM.SnapPower.Value
                            && point.Y > snapPoint.Y - mainWindowVM.SnapPower.Value
                            && point.Y < snapPoint.Y + mainWindowVM.SnapPower.Value)
                        {
                            //スナップする座標を一時変数へ保存
                            snapped = snapPoint;
                        }
                    }

                    if (snapped != null)
                    {
                        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                        RemoveFromAdornerLayerAndDictionary(snapped, adornerLayer);

                        //ドラッグ終了座標を一時変数で上書きしてスナップ
                        point = snapped.Value;
                        if (adornerLayer != null)
                        {
                            Trace.WriteLine($"Snap={snapped.Value}");
                            if (!_adorners.ContainsKey(snapped.Value))
                            {
                                var adorner = new Adorners.SnapPointAdorner(designerCanvas, snapped.Value);
                                if (adorner != null)
                                {
                                    adornerLayer.Add(adorner);

                                    //ディクショナリに記憶する
                                    _adorners.Add(snapped.Value, adorner);
                                }
                            }
                        }
                    }
                    else //スナップしなかった場合
                    {
                        RemoveAllAdornerFromAdornerLayerAndDictionary(designerCanvas);
                    }
                }

                vm.Points[TargetPointIndex] = point;
            }
        }

        private void RemoveAllAdornerFromAdornerLayerAndDictionary(DesignerCanvas designerCanvas)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
            var removes = _adorners.ToList();

            removes.ForEach(x =>
            {
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(x.Value);
                }
                _adorners.Remove(x.Key);
            });
        }

        private void RemoveFromAdornerLayerAndDictionary(Point? snapped, AdornerLayer adornerLayer)
        {
            var removes = _adorners.Where(x => x.Key != snapped)
                                                       .ToList();
            removes.ForEach(x =>
            {
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(x.Value);
                }
                _adorners.Remove(x.Key);
            });
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            BeginDragPoint = null;

            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            RemoveAllAdornerFromAdornerLayerAndDictionary(designerCanvas);
        }
    }
}
