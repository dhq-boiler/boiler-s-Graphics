using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace boiler_sGraphics.Views.Behaviors
{
    public abstract class AbstractDragAdorner : Adorner
    {
        #region Fields
        protected UIElement child;
        protected double XCenter;
        protected double YCenter;
        private double _leftOffset;
        private double _topOffset;
        #endregion Fields
        #region Properties
        public double LeftOffset
        {
            get { return _leftOffset; }
            set
            {
                _leftOffset = value - this.XCenter;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get { return _topOffset; }
            set
            {
                _topOffset = value - this.YCenter;
                UpdatePosition();
            }
        }
        #endregion Properties
        #region Construct
        public AbstractDragAdorner(UIElement owner) : base(owner) { }

        public AbstractDragAdorner(UIElement owner, UIElement adornElement, double opacity, Point dragPos)
            : base(owner)
        {
            this.child = CreateVisualChild(adornElement, opacity, dragPos);
        }
        #endregion Construct
        #region Methods

        protected abstract UIElement CreateVisualChild(UIElement adornElement, double opacity, Point dragPos);

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.child;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size finalSize)
        {
            this.child.Measure(finalSize);
            return this.child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.child.Arrange(new Rect(this.child.DesiredSize));
            return finalSize;
        }

        private void UpdatePosition()
        {
            var adorner = this.Parent as AdornerLayer;
            if (adorner != null)
            {
                adorner.Update(this.AdornedElement);
            }
        }
        #endregion Method
    }
}
