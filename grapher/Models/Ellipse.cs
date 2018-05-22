using System.Windows.Media;

namespace grapher.Models
{
    public class Ellipse : RenderItem
    {
        private Brush _Stroke;
        private Brush _Fill;

        public Brush Stroke
        {
            get { return _Stroke; }
            set { SetProperty(ref _Stroke, value); }
        }

        public Brush Fill
        {
            get { return _Fill; }
            set { SetProperty(ref _Fill, value); }
        }
    }
}
