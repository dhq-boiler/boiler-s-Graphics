using Prism.Mvvm;

namespace grapher.Models
{
    public class DraggingItem : BindableBase
    {
        private object _Item;
        private double _XOffset;
        private double _YOffset;

        public object Item
        {
            get { return _Item; }
            set { SetProperty(ref _Item, value); }
        }

        public double XOffset
        {
            get { return _XOffset; }
            set { SetProperty(ref _XOffset, value); }
        }

        public double YOffset
        {
            get { return _YOffset; }
            set { SetProperty(ref _YOffset, value); }
        }
    }
}
