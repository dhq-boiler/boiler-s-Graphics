using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
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
        private SnapAction _snapAction;

        private DesignerCanvas _designerCanvas;

        public PictureAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, string filename, double width, double height)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            _filename = filename;
            _Width = width;
            _Height = height;
            _snapAction = new SnapAction();
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);
                var currentPosition = _endPoint.Value;
                _snapAction.OnMouseMove(ref currentPosition);
                _endPoint = currentPosition;
                
                (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint = currentPosition;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }


        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            _snapAction.OnMouseUp(this);

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
