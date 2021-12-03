using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace boilersGraphics.Helpers
{
    class SnapAction
    {
        private MultiDictionary<Point, Adorner> _adorners;

        public SnapAction()
        {
            _adorners = new MultiDictionary<Point, Adorner>();
        }

        public enum SnapResult
        {
            Snapped,
            NoSnap
        }

        private SnapPointPosition _SnapToEdge;
        private SnapResult _SnapResult = SnapResult.NoSnap;
        private SelectableDesignerItemViewModelBase _SnapTargetDataContext { get; set; }

        public void SnapIntersectionOfEllipseAndTangent(IEnumerable<NEllipseViewModel> ellipses, Point beginPoint, Point endPoint, List<Tuple<Point, NEllipseViewModel>> appendIntersectionPoints)
        {
            foreach (var ellipse in ellipses)
            {
                var snapPower = (App.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value;
                var array = new ConcurrentBag<Tuple<Point[], double>>();
                Parallel.For((int)-snapPower, (int)snapPower, (y) =>
                {
                    for (double x = -snapPower; x < snapPower; x++)
                    {
                        var tuple = Intersection.FindEllipseSegmentIntersectionsSupportRotation(ellipse, beginPoint, new Point(endPoint.X + x, endPoint.Y + y), false);
                        array.Add(tuple);
                    }
                });
                var minDiscriminant = array.FirstOrDefault(x => Math.Abs(x.Item2) == array.Min(x => Math.Abs(x.Item2)));
                if (minDiscriminant != null && minDiscriminant.Item1.Count() == 1)
                {
                    appendIntersectionPoints.Add(new Tuple<Point, NEllipseViewModel>(minDiscriminant.Item1.First(), ellipse));
                }
            }
        }

        public void OnMouseMove(ref Point currentPoint, List<Tuple<Point, NEllipseViewModel>> appendIntersectionPoints = null)
        {
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var diagramVM = mainWindowVM.DiagramViewModel;
            if (diagramVM.EnablePointSnap.Value)
            {
                var snapPoints = diagramVM.GetSnapPoints(currentPoint).ToList();
                var hitTestResult = designerCanvas.InputHitTest(currentPoint);
                if (!(hitTestResult is FrameworkElement)) return;
                var dataContext = (hitTestResult as FrameworkElement).DataContext;
                if (appendIntersectionPoints != null)
                    snapPoints.AddRange(from x in appendIntersectionPoints select new Tuple<SnapPoint, Point>(CreateSnapPoint(x.Item1, x.Item2), x.Item1));
                Tuple<SnapPoint, Point> snapped = null;
                foreach (var snapPoint in snapPoints)
                {
                    if (currentPoint.X > snapPoint.Item2.X - mainWindowVM.SnapPower.Value
                     && currentPoint.X < snapPoint.Item2.X + mainWindowVM.SnapPower.Value
                     && currentPoint.Y > snapPoint.Item2.Y - mainWindowVM.SnapPower.Value
                     && currentPoint.Y < snapPoint.Item2.Y + mainWindowVM.SnapPower.Value)
                    {
                        //スナップする座標を一時変数へ保存
                        snapped = snapPoint;
                        _SnapToEdge = snapPoint.Item1.SnapPointPosition;
                        _SnapTargetDataContext = snapPoint.Item1.DataContext as SelectableDesignerItemViewModelBase;
                        break;
                    }
                }

                //スナップした場合
                if (snapped != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                    RemoveFromAdornerLayerAndDictionary(snapped.Item2, adornerLayer);

                    //ドラッグ終了座標を一時変数で上書きしてスナップ
                    currentPoint = snapped.Item2;
                    if (adornerLayer != null)
                    {
                        if (!_adorners.ContainsKey(snapped.Item2))
                        {
                            var adorner = new Adorners.SnapPointAdorner(designerCanvas, snapped.Item2);
                            if (adorner != null)
                            {
                                adornerLayer.Add(adorner);

                                //ディクショナリに記憶する
                                _adorners.Add(snapped.Item2, adorner);
                            }

                            if (snapped.Item1.SnapPointPosition == SnapPointPosition.Intersection)
                            {
                                var auxiliaryLine = new Adorners.AuxiliaryLine(designerCanvas, (snapped.Item1.DataContext as NEllipseViewModel).CenterPoint.Value, snapped.Item2);
                                if (auxiliaryLine != null)
                                {
                                    adornerLayer.Add(auxiliaryLine);

                                    //ディクショナリに記憶する
                                    _adorners.Add(snapped.Item2, auxiliaryLine);
                                }
                            }
                        }
                    }
                    _SnapResult = SnapResult.Snapped;
                }
                else //スナップしなかった場合
                {
                    RemoveAllAdornerFromAdornerLayerAndDictionary(designerCanvas);
                    _SnapResult = SnapResult.NoSnap;

                    if (appendIntersectionPoints != null && appendIntersectionPoints.Count() > 0)
                    {
                        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                        var adorner = new Adorners.SnapPointAdorner(designerCanvas, appendIntersectionPoints.First().Item1);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);

                            //ディクショナリに記憶する
                            _adorners.Add(appendIntersectionPoints.First().Item1, adorner);
                        }
                    }
                }
            }
        }

        public void OnMouseMove(ref Point currentPoint, Vector vec, List<Tuple<Point, NEllipseViewModel>> appendIntersectionPoints = null)
        {
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var diagramVM = mainWindowVM.DiagramViewModel;
            if (diagramVM.EnablePointSnap.Value)
            {
                var snapPoints = diagramVM.GetSnapPoints(currentPoint).ToList();
                var dataContext = (designerCanvas.InputHitTest(currentPoint) as FrameworkElement).DataContext;
                if (appendIntersectionPoints != null)
                    snapPoints.AddRange(from x in appendIntersectionPoints select new Tuple<SnapPoint, Point>(CreateSnapPoint(x.Item1, x.Item2), x.Item1));
                Tuple<SnapPoint, Point> snapped = null;
                foreach (var snapPoint in snapPoints)
                {
                    if (currentPoint.X > snapPoint.Item2.X - mainWindowVM.SnapPower.Value
                     && currentPoint.X < snapPoint.Item2.X + mainWindowVM.SnapPower.Value
                     && currentPoint.Y > snapPoint.Item2.Y - mainWindowVM.SnapPower.Value
                     && currentPoint.Y < snapPoint.Item2.Y + mainWindowVM.SnapPower.Value)
                    {
                        //スナップする座標を一時変数へ保存
                        snapped = snapPoint;
                        _SnapToEdge = snapPoint.Item1.SnapPointPosition;
                        _SnapTargetDataContext = snapPoint.Item1.DataContext as SelectableDesignerItemViewModelBase;
                        break;
                    }
                }

                //スナップした場合
                if (snapped != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                    RemoveFromAdornerLayerAndDictionary(snapped.Item2, adornerLayer);

                    //ドラッグ終了座標を一時変数で上書きしてスナップ
                    currentPoint = snapped.Item2;
                    if (adornerLayer != null)
                    {
                        if (!_adorners.ContainsKey(snapped.Item2))
                        {
                            var adorner = new Adorners.SnapPointAdorner(designerCanvas, snapped.Item2);
                            if (adorner != null)
                            {
                                adornerLayer.Add(adorner);

                                //ディクショナリに記憶する
                                _adorners.Add(snapped.Item2, adorner);
                            }

                            if (snapped.Item1.SnapPointPosition == SnapPointPosition.Intersection)
                            {
                                var gradient = -1 / (vec.Y / vec.X);
                                var x0 = snapped.Item2.X;
                                var y0 = snapped.Item2.Y;
                                Func<double, double> f = (x) => gradient * (x - x0) + y0;
                                var auxiliaryLine = new Adorners.AuxiliaryLine(designerCanvas, new Point(x0 - 50, f(x0 - 50)), new Point(x0 + 50, f(x0 + 50)));
                                if (auxiliaryLine != null)
                                {
                                    adornerLayer.Add(auxiliaryLine);

                                    //ディクショナリに記憶する
                                    _adorners.Add(snapped.Item2, auxiliaryLine);
                                }
                            }
                        }
                    }
                    _SnapResult = SnapResult.Snapped;
                }
                else //スナップしなかった場合
                {
                    RemoveAllAdornerFromAdornerLayerAndDictionary(designerCanvas);
                    _SnapResult = SnapResult.NoSnap;

                    if (appendIntersectionPoints != null && appendIntersectionPoints.Count() > 0)
                    {
                        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                        var adorner = new Adorners.SnapPointAdorner(designerCanvas, appendIntersectionPoints.First().Item1);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);

                            //ディクショナリに記憶する
                            _adorners.Add(appendIntersectionPoints.First().Item1, adorner);
                        }
                    }
                }
            }
        }

        public void OnMouseMove(ref Point currentPoint, SnapPoint movingSnapPoint, Vector vec, List<Tuple<Point, NEllipseViewModel>> appendIntersectionPoints = null)
        {
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var diagramVM = mainWindowVM.DiagramViewModel;
            if (diagramVM.EnablePointSnap.Value)
            {
                var snapPoints = diagramVM.GetSnapPoints(new SnapPoint[] { movingSnapPoint }).ToList();
                if (appendIntersectionPoints != null)
                    snapPoints.AddRange(from x in appendIntersectionPoints select new Tuple<SnapPoint, Point>(CreateSnapPoint(x.Item1, x.Item2), x.Item1));
                Tuple<SnapPoint, Point> snapped = null;
                foreach (var snapPoint in snapPoints)
                {
                    if (currentPoint.X > snapPoint.Item2.X - mainWindowVM.SnapPower.Value
                     && currentPoint.X < snapPoint.Item2.X + mainWindowVM.SnapPower.Value
                     && currentPoint.Y > snapPoint.Item2.Y - mainWindowVM.SnapPower.Value
                     && currentPoint.Y < snapPoint.Item2.Y + mainWindowVM.SnapPower.Value)
                    {
                        //スナップする座標を一時変数へ保存
                        snapped = snapPoint;
                        _SnapToEdge = snapPoint.Item1.SnapPointPosition;
                        _SnapTargetDataContext = snapPoint.Item1.DataContext as SelectableDesignerItemViewModelBase;
                        break;
                    }
                }

                //スナップした場合
                if (snapped != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                    RemoveFromAdornerLayerAndDictionary(snapped.Item2, adornerLayer);

                    //ドラッグ終了座標を一時変数で上書きしてスナップ
                    currentPoint = snapped.Item2;
                    Canvas.SetLeft(movingSnapPoint, currentPoint.X - movingSnapPoint.Width / 2);
                    Canvas.SetTop(movingSnapPoint, currentPoint.Y - movingSnapPoint.Height / 2);

                    if (adornerLayer != null)
                    {
                        if (!_adorners.ContainsKey(snapped.Item2))
                        {
                            var adorner = new Adorners.SnapPointAdorner(designerCanvas, snapped.Item2);
                            if (adorner != null)
                            {
                                adornerLayer.Add(adorner);

                                //ディクショナリに記憶する
                                _adorners.Add(snapped.Item2, adorner);
                            }

                            if (snapped.Item1.SnapPointPosition == SnapPointPosition.Intersection)
                            {
                                var gradient = -1 / (vec.Y / vec.X);
                                var x0 = snapped.Item2.X;
                                var y0 = snapped.Item2.Y;
                                Func<double, double> f = (x) => gradient * (x - x0) + y0;
                                var auxiliaryLine = new Adorners.AuxiliaryLine(designerCanvas, new Point(x0 - 50, f(x0 - 50)), new Point(x0 + 50, f(x0 + 50)));
                                if (auxiliaryLine != null)
                                {
                                    adornerLayer.Add(auxiliaryLine);

                                    //ディクショナリに記憶する
                                    _adorners.Add(snapped.Item2, auxiliaryLine);
                                }
                            }
                        }
                    }
                    _SnapResult = SnapResult.Snapped;
                }
                else //スナップしなかった場合
                {
                    RemoveAllAdornerFromAdornerLayerAndDictionary(designerCanvas);
                    _SnapResult = SnapResult.NoSnap;
                }
            }
        }

        private SnapPoint CreateSnapPoint(Point point, object dataContext)
        {
            var snapPoint = new SnapPoint();
            snapPoint.DataContext = dataContext;
            snapPoint.SetValue(Canvas.LeftProperty, point.X);
            snapPoint.SetValue(Canvas.TopProperty, point.Y);
            snapPoint.SnapPointPosition = SnapPointPosition.Intersection;
            return snapPoint;
        }

        public void OnMouseUp(Adorner targetAdorner)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
            if (adornerLayer != null)
            {
                if (targetAdorner != null)
                {
                    adornerLayer.Remove(targetAdorner);
                }

                foreach (var adorner in _adorners)
                    foreach (var remove in adorner.Value)
                        adornerLayer.Remove(remove);

                _adorners.Clear();
            }
        }

        public void PostProcess(SnapPointPosition snapPointEdge, SelectableDesignerItemViewModelBase item)
        {
            if (_SnapTargetDataContext == null)
            {
                LogManager.GetCurrentClassLogger().Debug("_SnapTargetDataContext == null");
                return;
            }

            if (_SnapResult == SnapResult.Snapped)
            {
                switch (snapPointEdge)
                {
                    case SnapPointPosition.Left:
                    case SnapPointPosition.LeftTop:
                    case SnapPointPosition.Top:
                    case SnapPointPosition.RightTop:
                    case SnapPointPosition.Right:
                    case SnapPointPosition.RightBottom:
                    case SnapPointPosition.Bottom:
                    case SnapPointPosition.LeftBottom:
                        (item as DesignerItemViewModelBase).SnapObjs.Add(_SnapTargetDataContext.Connect(_SnapToEdge, snapPointEdge, item));
                        break;
                    case SnapPointPosition.BeginEdge:
                        (item as ConnectorBaseViewModel).SnapPoint0VM.Value.SnapObjs.Add(_SnapTargetDataContext.Connect(_SnapToEdge, SnapPointPosition.BeginEdge, item));
                        break;
                    case SnapPointPosition.EndEdge:
                        (item as ConnectorBaseViewModel).SnapPoint1VM.Value.SnapObjs.Add(_SnapTargetDataContext.Connect(_SnapToEdge, SnapPointPosition.EndEdge, item));
                        break;
                }
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
                    foreach (var remove in x.Value)
                        adornerLayer.Remove(remove);
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
                    foreach (var remove in x.Value)
                        adornerLayer.Remove(remove);
                }
                _adorners.Remove(x.Key);
            });
        }
    }
}
