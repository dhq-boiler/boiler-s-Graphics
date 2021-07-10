using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.Views.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.Views
{
    /*
     * How to arrange Thumbs with a Line in a WPF custom Adorner https://stackoverflow.com/questions/10821556/how-to-arrange-thumbs-with-a-line-in-a-wpf-custom-adorner
     * quizzer canha https://stackoverflow.com/users/614815/canha
     * answerer Trevor Elliott https://stackoverflow.com/users/852555/trevor-elliott
     */

    internal class StraightLineResizeHandle : Adorner
    {
        private Point _start;
        private Point _end;
        private Thumb _startThumb;
        private Thumb _endThumb;
        private Line _selectedLine;
        private VisualCollection _visualChildren;
        private CursorBehavior _startThumbBehavior;
        private CursorBehavior _endThumbBehavior;

        public StraightLineResizeHandle(UIElement adornedElement)
            : base(adornedElement)
        {
            _visualChildren = new VisualCollection(this);

            var template = (ControlTemplate)App.Current.MainWindow.FindResource("ResizeHandleTemplate");
            _startThumb = new Thumb
            {
                Width = 7,
                Height = 7,
                Template = template,
                UseLayoutRounding = true,
            };
            _endThumb = new Thumb
            {
                Width = 7,
                Height = 7,
                Template = template,
                UseLayoutRounding = true,
            };

            Cursor cursor = GetCursor(adornedElement as Line);
            _startThumbBehavior = new CursorBehavior() { DefaultCursor = Cursors.Arrow, SpecificCursor = cursor };
            _endThumbBehavior = new CursorBehavior() { DefaultCursor = Cursors.Arrow, SpecificCursor = cursor };

            var behaviors = Interaction.GetBehaviors(_startThumb);
            behaviors.Add(_startThumbBehavior);
            behaviors = Interaction.GetBehaviors(_endThumb);
            behaviors.Add(_endThumbBehavior);

            _startThumb.DragDelta += StartThumb_DragDelta;
            _endThumb.DragDelta += EndThumb_DragDelta;

            _visualChildren.Add(_startThumb);
            _visualChildren.Add(_endThumb);

            _selectedLine = AdornedElement as Line;
        }

        private static Cursor GetCursor(Line line)
        {
            Cursor cursor = null;
            var radian = Math.Atan((line.Y2 - line.Y1) / (line.X2 - line.X1));
            if (radian >= -1d / 2d * Math.PI && radian < -3d / 8d * Math.PI)
            {
                cursor = Cursors.SizeNS;
            }
            if (radian >= -3d / 8d * Math.PI && radian < -1d / 8d * Math.PI)
            {
                cursor = Cursors.SizeNESW;
            }
            else if (radian >= -1d / 8d * Math.PI && radian < 1d / 8d * Math.PI)
            {
                cursor = Cursors.SizeWE;
            }
            else if (radian >= 1d / 8d * Math.PI && radian < 3d / 8d * Math.PI)
            {
                cursor = Cursors.SizeNWSE;
            }
            else if (radian >= 3d / 8d * Math.PI && radian <= 1d / 2d * Math.PI)
            {
                cursor = Cursors.SizeNS;
            }
            return cursor;
        }

        private void StartThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            Point position = Mouse.GetPosition(canvas);
            var viewModel = _selectedLine.DataContext as ConnectorBaseViewModel;
            viewModel.Points[0] = position;
        }

        private void EndThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            Point position = Mouse.GetPosition(canvas);
            var viewModel = _selectedLine.DataContext as ConnectorBaseViewModel;
            viewModel.Points[1] = position;
        }

        protected override int VisualChildrenCount => _visualChildren.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (AdornedElement is Line)
            {
                _startThumbBehavior.SpecificCursor = GetCursor(AdornedElement as Line);
                _endThumbBehavior.SpecificCursor = GetCursor(AdornedElement as Line);

                _selectedLine = AdornedElement as Line;
                _start = new Point(_selectedLine.X1, _selectedLine.Y1);
                _end = new Point(_selectedLine.X2, _selectedLine.Y2);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _selectedLine = AdornedElement as Line;

            double left = Math.Min(_selectedLine.X1, _selectedLine.X2);
            double top = Math.Min(_selectedLine.Y1, _selectedLine.Y2);

            var startRect = new Rect(_selectedLine.X1 - (_startThumb.Width / 2), _selectedLine.Y1 - (_startThumb.Height / 2), _startThumb.Width, _startThumb.Height);
            _startThumb.Arrange(startRect);

            var endRect = new Rect(_selectedLine.X2 - (_endThumb.Width / 2), _selectedLine.Y2 - (_endThumb.Height / 2), _endThumb.Width, _endThumb.Height);
            _endThumb.Arrange(endRect);

            return finalSize;
        }
    }
}
