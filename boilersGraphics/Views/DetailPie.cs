using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using Reactive.Bindings;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
            LayoutUpdated += DetailPie_LayoutUpdated;
        }

        private void DetailPie_Loaded(object sender, RoutedEventArgs e)
        {
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
            _detailPathGeometry.SetBinding(WidthPlacementProperty, new Binding("WidthPlacement.Value") { Source = this });
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

        private bool loaded = false;

        private void DetailPie_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (loaded) return;
            var detailPathGeometry = _detailPathGeometry;
            detailPathGeometry.ApplyTemplate();
            var widthCell = detailPathGeometry.FindVisualChildren<DockPanel>().First(x => x.Name == "WidthCell");
            var dpgRoot = widthCell.FindAncestor<Grid>().FindAncestor<Grid>().FindAncestor<Grid>();
            var x = _centerPoint.FindAncestor<Canvas>();
            dpgRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            dpgRoot.Arrange(new Rect(0, 0, dpgRoot.DesiredSize.Width, dpgRoot.DesiredSize.Height));
            widthCell.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            widthCell.Arrange(new Rect(0, 0, widthCell.DesiredSize.Width, widthCell.DesiredSize.Height));
            x.UpdateLayout();
            var widthCellBounds = widthCell.BoundsRelativeTo(dpgRoot);
            LogManager.GetCurrentClassLogger().Info(widthCellBounds);
            var centerPointBounds = _centerPoint.BoundsRelativeTo(x);
            var isIntersect = widthCellBounds.IntersectsWith(centerPointBounds);
            if (isIntersect)
            {
                WidthPlacement.Value = Placement.Top;
            }
            else
            {
                WidthPlacement.Value = Placement.Bottom;
            }
            loaded = true;
        }

        private void DetailPie_Loaded1(object sender, RoutedEventArgs e)
        {
            
        }

        

        public ReactivePropertySlim<Placement> WidthPlacement { get; } = new ReactivePropertySlim<Placement>(Placement.Bottom);

        //public Placement WidthPlacement
        //{
        //    get
        //    {
        //        var detailPathGeometry = _detailPathGeometry;
        //        detailPathGeometry.ApplyTemplate();
        //        var widthCell = detailPathGeometry.FindVisualChildren<DockPanel>().First(x => x.Name == "WidthCell");

        //        var dataContext = DataContext as DetailPieViewModel;
        //        var inputElement = widthCell.InputHitTest(dataContext.ViewModel.Value.CenterPoint.Value);

        //        return Placement.Bottom;
        //    }
        //}
    }
}
