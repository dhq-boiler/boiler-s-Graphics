using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace grapher.Views.Behaviors
{
    abstract class AbstractAdorner : Adorner
    {
        private AdornerLayer _layer;
        private bool _isAttached;

        public AbstractAdorner(Visual visual, UIElement adornedElement)
            : base(adornedElement)
        {
            this._layer = AdornerLayer.GetAdornerLayer(visual);

            Attach();
        }

        public static readonly DependencyProperty BeginPointProperty = DependencyProperty.Register("BeginPoint", typeof(Point), typeof(AbstractAdorner), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(Point), typeof(AbstractAdorner), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

        public Point BeginPoint
        {
            get { return (Point)GetValue(BeginPointProperty); }
            set { SetValue(BeginPointProperty, value); }
        }

        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set { SetValue(EndPointProperty, value); }
        }

        public void Attach()
        {
            if (this._layer != null)
            {
                if (!this._isAttached)
                {
                    this._layer.Add(this);
                    this._isAttached = true;
                }
            }
        }

        public void Detach()
        {
            if (this._layer != null)
            {
                if (this._isAttached)
                {
                    this._layer.Remove(this);
                    this._isAttached = false;
                }
            }
        }
    }
}
