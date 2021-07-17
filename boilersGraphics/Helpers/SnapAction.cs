using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public void OnMouseMove(ref Point currentPoint)
        {
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var diagramVM = mainWindowVM.DiagramViewModel;
            if (diagramVM.EnablePointSnap.Value)
            {
                var snapPoints = diagramVM.SnapPoints;
                Point? snapped = null;
                foreach (var snapPoint in snapPoints)
                {
                    if (currentPoint.X > snapPoint.X - mainWindowVM.SnapPower.Value
                     && currentPoint.X < snapPoint.X + mainWindowVM.SnapPower.Value
                     && currentPoint.Y > snapPoint.Y - mainWindowVM.SnapPower.Value
                     && currentPoint.Y < snapPoint.Y + mainWindowVM.SnapPower.Value)
                    {
                        //スナップする座標を一時変数へ保存
                        snapped = snapPoint;
                    }
                }

                //スナップした場合
                if (snapped != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                    RemoveFromAdornerLayerAndDictionary(snapped, adornerLayer);

                    //ドラッグ終了座標を一時変数で上書きしてスナップ
                    currentPoint = snapped.Value;
                    if (adornerLayer != null)
                    {
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
        }

        public void OnMouseUp(Adorner targetAdorner)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(targetAdorner);

                foreach (var adorner in _adorners)
                    adornerLayer.Remove(adorner.Value);

                _adorners.Clear();
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
