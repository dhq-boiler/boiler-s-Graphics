using grapher.Models;
using grapher.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;

namespace grapher.Views.Behaviors
{
    internal class DrawEllipseBehavior : DrawAbstractBehavior<EllipseAdorner>
    {
        protected override EllipseAdorner CreateAdornerObject(Visual visual, UIElement adornedElement, Point beginPoint)
        {
            var adorner = new EllipseAdorner(visual, adornedElement);
            adorner.BeginPoint = beginPoint;
            adorner.EndPoint = beginPoint;
            return adorner;
        }

        public override void Draw()
        {
            var renderItem = new Ellipse
            {
                X = Math.Min(Adorner.BeginPoint.X, Adorner.EndPoint.X),
                Y = Math.Min(Adorner.BeginPoint.Y, Adorner.EndPoint.Y),
                Width = Math.Max(Adorner.EndPoint.X - Adorner.BeginPoint.X, Adorner.BeginPoint.X - Adorner.EndPoint.X),
                Height = Math.Max(Adorner.EndPoint.Y - Adorner.BeginPoint.Y, Adorner.BeginPoint.Y - Adorner.EndPoint.Y),
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Transparent)
            };
            var viewModel = new EllipseViewModel(renderItem);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).RenderItems.Add(viewModel);
        }
    }
}
