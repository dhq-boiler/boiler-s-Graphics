using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace boilersGraphics.Controls
{
    public class LineResizeHandle : Control
    {
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
                vm.Points[TargetPointIndex] = point;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            BeginDragPoint = null;
        }
    }
}
