using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NLog;
using System.Windows;
using System.Windows.Input;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Controls
{
    public class LineResizeHandle : SnapPoint
    {
        private SnapAction snapAction;
        public OperationRecorder Recorder { get; } = new OperationRecorder((App.Current.MainWindow.DataContext as MainWindowViewModel).Controller);

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

            BeginDragPoint = e.GetPosition(App.Current.MainWindow.GetChildOfType<DesignerCanvas>());

            var vm = DataContext as ConnectorBaseViewModel ?? (DataContext as SnapPointViewModel).Parent.Value as ConnectorBaseViewModel;
            Point point = vm.Points[TargetPointIndex];

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "変形";

            Recorder.BeginRecode();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (BeginDragPoint.HasValue)
            {
                var snapPointVM = DataContext as SnapPointViewModel;
                var connectorVM = DataContext as ConnectorBaseViewModel ?? (DataContext as SnapPointViewModel).Parent.Value as ConnectorBaseViewModel;

                snapPointVM.IsSelected.Value = true;

                Point currentPosition = Mouse.GetPosition(App.Current.MainWindow.GetChildOfType<DesignerCanvas>());

                Point point = currentPosition;
                Recorder.Current.ExecuteSetProperty(snapPointVM, "Left.Value", currentPosition.X);
                Recorder.Current.ExecuteSetProperty(snapPointVM, "Top.Value", currentPosition.Y);

                snapAction.OnMouseMove(ref point);

                Recorder.Current.ExecuteSetProperty(connectorVM, $"Points[{TargetPointIndex}]", point);

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

            Recorder.EndRecode();

            var snapPointVM = DataContext as SnapPointViewModel;
            var connectorVM = DataContext as ConnectorBaseViewModel ?? (DataContext as SnapPointViewModel).Parent.Value as ConnectorBaseViewModel;
            LogManager.GetCurrentClassLogger().Info($"Deform item {connectorVM.ShowPropertiesAndFields()}");
        }
    }
}
