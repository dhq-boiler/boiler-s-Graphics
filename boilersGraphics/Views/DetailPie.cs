﻿using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
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
        public DetailPie()
        {
            Name = "DetailPie";
            Loaded += DetailPie_Loaded;
        }

        private void DetailPie_Loaded(object sender, RoutedEventArgs e)
        {
            var dockPanel = new DockPanel();
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
            dockPanel.Children.Add(childDockPanel);
            _detailPathGeometry = new DetailPathGeometry();
            _detailPathGeometry.Name = "DetailPathGeometry";
            _detailPathGeometry.SetBinding(FrameworkElement.DataContextProperty, "ViewModel.Value");
            _detailPathGeometry.CenterVisibility = Visibility.Collapsed;
            _detailPathGeometry.Stretch = System.Windows.Media.Stretch.Fill;
            _detailPathGeometry.SetBinding(WidthPlacementProperty, new Binding("WidthPlacement") { Source = this });
            var canvas = new Canvas();
            var centerPoint = new Ellipse();
            centerPoint.Width = 5;
            centerPoint.Height = 5;
            centerPoint.Fill = Brushes.Red;
            var viewModel = ((DataContext as DetailPieViewModel).ViewModel.Value as NPieViewModel);
            centerPoint.SetValue(Canvas.LeftProperty, viewModel.PieCenterPoint.Value.X - viewModel.Left.Value + 100);
            centerPoint.SetValue(Canvas.TopProperty, viewModel.PieCenterPoint.Value.Y - viewModel.Top.Value + 100);
            canvas.Children.Add(centerPoint);
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
            dockPanel.Children.Add(_detailPathGeometry);
            Content = dockPanel;
        }


        public Placement WidthPlacement
        {
            get
            {
                var detailPathGeometry = _detailPathGeometry;
                detailPathGeometry.ApplyTemplate();
                var contentPresenter = FindVisualChild<ContentPresenter>(detailPathGeometry);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                var target = dataTemplate.FindName("WidthCell", contentPresenter);

                // コードが続きます...

                return Placement.Top;
            }
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
