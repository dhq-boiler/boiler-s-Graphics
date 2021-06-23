using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace boilersGraphics.Models
{
    [Serializable]
    public class Rectangle : RenderItem
    {
        [NonSerialized]
        private readonly BrushConverter _brushConverter = new BrushConverter();

        [XmlElement("StrokeString")]
        private string _strokeString;
        public string StrokeString
        {
            get { return _strokeString; }
            set
            {
                _strokeString = value;
                Stroke = (Brush)_brushConverter.ConvertFrom(value);
            }
        }

        [NonSerialized]
        private Brush _Stroke;
        public Brush Stroke
        {
            get { return _Stroke; }
            set
            {
                SetProperty(ref _Stroke, value);
            }
        }

        [XmlElement("FillString")]
        private string _fillString;
        public string FillString
        {
            get { return _fillString; }
            set
            {
                _fillString = value;
                Fill = (Brush)_brushConverter.ConvertFrom(value);
            }
        }

        [NonSerialized]
        private Brush _Fill;
        public Brush Fill
        {
            get { return _Fill; }
            set { SetProperty(ref _Fill, value); }
        }
    }
}
