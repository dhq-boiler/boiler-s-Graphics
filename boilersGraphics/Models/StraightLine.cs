using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace boilersGraphics.Models
{
    [Serializable]
    public class StraightLine : RenderItem
    {
        [NonSerialized]
        private readonly BrushConverter _brushConverter = new BrushConverter();


        private double _X2;
        public double X2
        {
            get { return _X2; }
            set { SetProperty(ref _X2, value); }
        }

        private double _Y2;
        public double Y2
        {
            get { return _Y2; }
            set { SetProperty(ref _Y2, value); }
        }

        [XmlElement("BrushString")]
        private string _brushString;
        public string BrushString
        {
            get { return _brushString; }
            set
            {
                _brushString = value;
                Brush = (Brush)_brushConverter.ConvertFrom(value);
            }
        }

        [NonSerialized]
        private Brush _Brush;
        public Brush Brush
        {
            get { return _Brush; }
            set { SetProperty(ref _Brush, value); }
        }
    }
}
