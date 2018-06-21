using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace grapher.Views.Behaviors
{
    internal abstract class AbstractAdorner : Adorner
    {
        private AdornerLayer _layer;
        private bool _isAttached;

        public AbstractAdorner(Visual visual, UIElement adornedElement)
            : base(adornedElement)
        {
            _layer = AdornerLayer.GetAdornerLayer(visual);

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
            if (_layer != null)
            {
                if (!_isAttached)
                {
                    _layer.Add(this);
                    _isAttached = true;
                }
            }
        }

        public void Detach()
        {
            if (_layer != null)
            {
                if (_isAttached)
                {
                    _layer.Remove(this);
                    _isAttached = false;
                }
            }
        }
    }
}
