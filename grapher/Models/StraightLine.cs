using System.Windows.Media;

namespace grapher.Models
{
    class StraightLine : RenderItem
    {
        private double _X2;
        private double _Y2;
        private Brush _Brush;

        public double X2
        {
            get { return _X2; }
            set { SetProperty(ref _X2, value); }
        }

        public double Y2
        {
            get { return _Y2; }
            set { SetProperty(ref _Y2, value); }
        }

        public Brush Brush
        {
            get { return _Brush; }
            set { SetProperty(ref _Brush, value); }
        }
    }
}
