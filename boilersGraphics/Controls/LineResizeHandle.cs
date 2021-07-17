using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
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
        private SnapAction snapAction;

        public LineResizeHandle()
        {
            snapAction = new SnapAction();
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

                snapAction.OnMouseMove(ref point);

                vm.Points[TargetPointIndex] = point;

                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                if ((string)Tag == "始点")
                {
                    var oppositePoint = OppositeHandle.TransformToAncestor(designerCanvas).Transform(new Point(0, 0));
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({point}) - ({oppositePoint}) (w, h) = ({oppositePoint.X - point.X}, {oppositePoint.Y - point.Y})";
                }
                else if ((string)Tag == "終点")
                {
                    var oppositePoint = OppositeHandle.TransformToAncestor(designerCanvas).Transform(new Point(0, 0));
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({oppositePoint}) - ({point}) (w, h) = ({point.X - oppositePoint.X}, {point.Y - oppositePoint.Y})";
                }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            BeginDragPoint = null;

            snapAction.OnMouseUp(null);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";
        }
    }
}
