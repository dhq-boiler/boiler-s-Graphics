using Prism.Mvvm;
using Reactive.Bindings;
using System.Windows;

namespace boilersGraphics.Models
{
    internal class Corner : BindableBase
    {
        public ReactiveProperty<int> Number { get; set; } = new ReactiveProperty<int>();

        public ReactiveProperty<double> Radius { get; set; } = new ReactiveProperty<double>();

        public ReactiveProperty<double> Angle { get; set; } = new ReactiveProperty<double>();

        public ReactiveProperty<Point> Point { get; set; } = new ReactiveProperty<Point>();
    }
}