﻿using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Helpers
{
    class SnapAction
    {
        private Dictionary<Point, Adorner> _adorners;

        public SnapAction()
        {
            _adorners = new Dictionary<Point, Adorner>();
        }

        public enum SnapResult
        {
            Snapped,
            NoSnap
        }

        private SnapResult _SnapResult = SnapResult.NoSnap;
        private DesignerItemViewModelBase _SnapTargetDataContext { get; set; }

        public void OnMouseMove(ref Point currentPoint)
        {
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var diagramVM = mainWindowVM.DiagramViewModel;
            if (diagramVM.EnablePointSnap.Value)
            {
                var snapPoints = diagramVM.SnapPoints;
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
                        _SnapTargetDataContext = snapPoint.Item1.DataContext as DesignerItemViewModelBase;
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

        public void OnMouseMove(ref Point currentPoint, SnapPoint movingSnapPoint)
        {
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var diagramVM = mainWindowVM.DiagramViewModel;
            if (diagramVM.EnablePointSnap.Value)
            {
                var snapPoints = diagramVM.GetSnapPoints(new SnapPoint[] { movingSnapPoint });
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
                        _SnapTargetDataContext = snapPoint.Item1.DataContext as DesignerItemViewModelBase;
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
                    adornerLayer.Remove(adorner.Value);

                _adorners.Clear();
            }
        }

        public void PostProcess(SnapPointEdge snapPointEdge, SelectableDesignerItemViewModelBase item)
        {
            if (_SnapResult == SnapResult.Snapped)
            {
                switch (snapPointEdge)
                {
                    case SnapPointEdge.LeftTop:
                        break;
                    case SnapPointEdge.RightTop:
                        break;
                    case SnapPointEdge.LeftBottom:
                        break;
                    case SnapPointEdge.RightBottom:
                        break;
                    case SnapPointEdge.BeginEdge:
                        (item as ConnectorBaseViewModel).SnapPoint0VM.Value.SnapObj = _SnapTargetDataContext.Connect(SnapPointEdge.BeginEdge, item);
                        break;
                    case SnapPointEdge.EndEdge:
                        (item as ConnectorBaseViewModel).SnapPoint1VM.Value.SnapObj = _SnapTargetDataContext.Connect(SnapPointEdge.EndEdge, item);
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
    }
}
