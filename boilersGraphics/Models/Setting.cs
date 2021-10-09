using Prism.Mvvm;
using Reactive.Bindings;
using System.Windows.Media;

namespace boilersGraphics.Models
{
    class Setting : BindableBase
    {
        public ReactivePropertySlim<int> Width { get; set; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> Height { get; set; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<Color> CanvasBackground { get; set; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<bool> EnablePointSnap { get; set; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<double> SnapPower { get; set; } = new ReactivePropertySlim<double>();
    }
}
