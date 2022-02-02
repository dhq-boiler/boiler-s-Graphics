using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace boilersGraphics.Models
{
    public class GradientStop : BindableBase
    {
        public ReactivePropertySlim<Color> Color { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<double> Offset { get; } = new ReactivePropertySlim<double>();

        public GradientStop(Color color, double offset)
        {
            Color.Value = color;
            Offset.Value = offset;
        }

        public System.Windows.Media.GradientStop ConvertToGradientStop()
        {
            return new System.Windows.Media.GradientStop(Color.Value, Offset.Value);
        }

        internal IObservable<Unit> GradientStopChangedAsObservable()
        {
            return this.ObserveProperty(x => x.Color).ToUnit()
                       .Merge(this.ObserveProperty(x => x.Offset).ToUnit());
        }
    }
}
