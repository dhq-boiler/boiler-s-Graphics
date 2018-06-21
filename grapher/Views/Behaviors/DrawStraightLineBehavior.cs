using grapher.Models;
using grapher.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace grapher.Views.Behaviors
{
    internal class DrawStraightLineBehavior : DrawAbstractBehavior<StraightLineAdorner>
    {
        protected override StraightLineAdorner CreateAdornerObject(Visual visual, UIElement adornedElement, Point beginPoint)
        {
            var adorner = new StraightLineAdorner(visual, adornedElement);
            adorner.BeginPoint = beginPoint;
            adorner.EndPoint = beginPoint;
            return adorner;
        }

        public override void Draw()
        {
            var renderItem = new StraightLine
            {
                X = Adorner.BeginPoint.X,
                Y = Adorner.BeginPoint.Y,
                X2 = Adorner.EndPoint.X,
                Y2 = Adorner.EndPoint.Y,
                Brush = new SolidColorBrush(Colors.Black),
            };
            var viewModel = new StraightLineViewModel(renderItem);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).RenderItems.Add(viewModel);
        }
    }
}
