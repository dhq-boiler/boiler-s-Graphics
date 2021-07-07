using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Adorners
{
    public class PictureAdorner : Adorner
    {
        private Point? _startPoint;
        private Point? _endPoint;
        private string _filename;
        private double _Width;
        private double _Height;
        private Dictionary<Point, Adorner> _adorners;

        private DesignerCanvas _designerCanvas;

        public PictureAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, string filename, double width, double height)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            _filename = filename;
            _Width = width;
            _Height = height;
            _adorners = new Dictionary<Point, Adorner>();
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);

                var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();

                var diagramVM = mainWindowVM.DiagramViewModel;
                if (diagramVM.EnablePointSnap.Value)
                {
                    var snapPoints = diagramVM.SnapPoints;
                    Point? snapped = null;
                    foreach (var snapPoint in snapPoints)
                    {
                        if (_endPoint.Value.X > snapPoint.X - mainWindowVM.SnapPower.Value
                         && _endPoint.Value.X < snapPoint.X + mainWindowVM.SnapPower.Value
                         && _endPoint.Value.Y > snapPoint.Y - mainWindowVM.SnapPower.Value
                         && _endPoint.Value.Y < snapPoint.Y + mainWindowVM.SnapPower.Value)
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
                        _endPoint = snapped;
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

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
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

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);

                foreach (var adorner in _adorners)
                    adornerLayer.Remove(adorner.Value);

                _adorners.Clear();
            }

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var bitmap = BitmapFactory.FromStream(new FileStream(_filename, FileMode.Open, FileAccess.Read));
                PictureDesignerItemViewModel itemBase = new PictureDesignerItemViewModel();
                itemBase.FileName = _filename;
                itemBase.FileWidth = bitmap.Width;
                itemBase.FileHeight = bitmap.Height;
                itemBase.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                itemBase.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                itemBase.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                itemBase.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                itemBase.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                itemBase.IsSelected = true;
                itemBase.Owner.DeselectAll();
                itemBase.ZIndex.Value = itemBase.Owner.Items.Count;
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(itemBase);

                _startPoint = null;
                _endPoint = null;
            }

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var brush = new ImageBrush(new BitmapImage(new Uri(_filename)));
            brush.Opacity = 0.5;

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var diff = _endPoint.Value - _startPoint.Value;
                if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                    (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down)
                {
                    if (diff.X > diff.Y)
                    {
                        var y = _startPoint.Value.Y + diff.Y;
                        var x = _startPoint.Value.X + (diff.Y / _Height) * _Width;
                        _endPoint = new Point(x, y);
                    }
                    else if (diff.X < diff.Y)
                    {
                        var x = _startPoint.Value.X + diff.X;
                        var y = _startPoint.Value.Y + (diff.X / _Width) * _Height;
                        _endPoint = new Point(x, y);
                    }
                    dc.DrawRectangle(brush, null, new Rect(_startPoint.Value, _endPoint.Value));
                }
                else
                {
                    dc.DrawRectangle(brush, null, new Rect(_startPoint.Value, _endPoint.Value));
                }
            }
        }
    }
}
