using Prism.Mvvm;
using System;
using System.Xml.Serialization;

namespace boilersGraphics.Models
{
    [Serializable]
    [XmlInclude(typeof(Rectangle))]
    [XmlInclude(typeof(StraightLine))]
    [XmlInclude(typeof(Ellipse))]
    public class RenderItem : BindableBase
    {
        private double _X;
        private double _Y;
        private double _Width;
        private double _Height;
        private bool _IsSelected;

        public double X
        {
            get { return _X; }
            set { SetProperty(ref _X, value); }
        }

        public double Y
        {
            get { return _Y; }
            set { SetProperty(ref _Y, value); }
        }

        public double Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, value); }
        }

        public double Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, value); }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }
    }
}
