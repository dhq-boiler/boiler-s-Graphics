using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace boilersGraphics.Models
{
    class Setting : BindableBase
    {
        public ReactiveProperty<int> Width { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> Height { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<Color> CanvasBackground { get; set; } = new ReactiveProperty<Color>();
        public ReactiveProperty<bool> EnablePointSnap { get; set; } = new ReactiveProperty<bool>();
        public ReactiveProperty<double> SnapPower { get; set; } = new ReactiveProperty<double>();
    }
}
