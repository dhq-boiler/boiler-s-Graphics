using grapher.Extensions;
using grapher.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace grapher.Controls
{
    public class ResizeHandle : Connector
    {
        public static readonly RoutedEvent DragDeltaEvent = EventManager.RegisterRoutedEvent(
            "DragDelta", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ResizeHandle));

        public static readonly DependencyProperty OppositeHandleProperty = DependencyProperty.Register("OppositeHandle", typeof(ResizeHandle), typeof(ResizeHandle));

        public event RoutedEventHandler DragDelta
        {
            add { AddHandler(DragDeltaEvent, value); }
            remove { RemoveHandler(DragDeltaEvent, value); }
        }

        public ResizeHandle OppositeHandle
        {
            get { return (ResizeHandle)GetValue(OppositeHandleProperty); }
            set { SetValue(OppositeHandleProperty, value); }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            if (canvas != null)
            {
                canvas.MoveConnector = this;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "変形";
        }
    }
}
