using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using Reactive.Bindings;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using static boilersGraphics.Views.DetailPathGeometry;

namespace boilersGraphics.Views
{
    public class DetailPie : UserControl
    {
        private DetailPathGeometry _detailPathGeometry;
        private Ellipse _centerPoint;
        private DockPanel _dockPanel;
        public DetailPie()
        {
            Name = "DetailPie";
            Loaded += DetailPie_Loaded;
        }

        private void DetailPie_Loaded(object sender, RoutedEventArgs e)
        {
            var pie = (DataContext as DetailPieViewModel).ViewModel.Value;
            var midline = (pie.EndDegree.Value + pie.StartDegree.Value) / 2;
            Debug.WriteLine(midline);
            if (midline > 0 && midline <= 180)
            {
                WidthPlacement.Value = Placement.Bottom;
            }
            else if ((midline >= -180 && midline < 0) || (midline > 180 && midline < 360))
            {
                WidthPlacement.Value = Placement.Top;
            }
            _dockPanel = new DockPanel();
            var childDockPanel = new DockPanel();
            childDockPanel.SetValue(DockPanel.DockProperty, Dock.Bottom);
            var grandchildDockPanel = new DockPanel();
            grandchildDockPanel.SetValue(DockPanel.DockProperty, Dock.Right);
            var button = new Button();
            button.Width = 100;
            button.Margin = new System.Windows.Thickness(5);
            button.HorizontalAlignment = HorizontalAlignment.Right;
            button.SetBinding(ButtonBase.CommandProperty, "OKCommand");
            button.Content = "OK";
            grandchildDockPanel.Children.Add(button);
            childDockPanel.Children.Add(grandchildDockPanel);
            _dockPanel.Children.Add(childDockPanel);
            _detailPathGeometry = new DetailPathGeometry();
            _detailPathGeometry.Name = "DetailPathGeometry";
            _detailPathGeometry.SetBinding(FrameworkElement.DataContextProperty, "ViewModel.Value");
            _detailPathGeometry.CenterVisibility = Visibility.Collapsed;
            _detailPathGeometry.Stretch = System.Windows.Media.Stretch.Fill;
            _detailPathGeometry.SetValue(WidthPlacementProperty, WidthPlacement.Value);
            var canvas = new Canvas();
            _centerPoint = new Ellipse();
            _centerPoint.Width = 5;
            _centerPoint.Height = 5;
            _centerPoint.Fill = Brushes.Red;
            var viewModel = ((DataContext as DetailPieViewModel).ViewModel.Value as NPieViewModel);
            _centerPoint.SetValue(Canvas.LeftProperty, viewModel.PieCenterPoint.Value.X - viewModel.Left.Value + 100);
            _centerPoint.SetValue(Canvas.TopProperty, viewModel.PieCenterPoint.Value.Y - viewModel.Top.Value + 100);
            canvas.Children.Add(_centerPoint);
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.SetValue(Canvas.LeftProperty, viewModel.PieCenterPoint.Value.X - viewModel.Left.Value + 115);
            stackPanel.SetValue(Canvas.TopProperty, viewModel.PieCenterPoint.Value.Y - viewModel.Top.Value + 115);
            var style = new Style();
            style.TargetType = typeof(DoubleTextBox);
            var setter = new Setter();
            setter.Property = WidthProperty;
            setter.Value = 40;
            style.Setters.Add(setter);
            stackPanel.Resources.Add(string.Empty, style);
            var label = new Label();
            label.Foreground = Brushes.Red;
            label.Content = "Center Point";
            stackPanel.Children.Add(label);
            var doubleTextBox = new DoubleTextBox();
            doubleTextBox.Foreground = Brushes.Red;
            doubleTextBox.SetBinding(DoubleTextBox.DoubleTextProperty, "PieCenterPoint.Value.X");
            stackPanel.Children.Add(doubleTextBox);
            var doubleTextBox2 = new DoubleTextBox();
            doubleTextBox2.Foreground = Brushes.Red;
            doubleTextBox2.SetBinding(DoubleTextBox.DoubleTextProperty, "PieCenterPoint.Value.Y");
            stackPanel.Children.Add(doubleTextBox2);
            canvas.Children.Add(stackPanel);
            _detailPathGeometry.Content = canvas;
            _dockPanel.Children.Add(_detailPathGeometry);
            Content = _dockPanel;
        }

        public ReactivePropertySlim<Placement> WidthPlacement { get; } = new ReactivePropertySlim<Placement>(Placement.Bottom);
    }
}
