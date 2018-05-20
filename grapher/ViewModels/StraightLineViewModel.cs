using grapher.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Media;

namespace grapher.ViewModels
{
    class StraightLineViewModel : RenderItemViewModel
    {
        public ReactiveProperty<double> X2 { get; set; }

        public ReactiveProperty<double> Y2 { get; set; }

        public ReactiveProperty<Brush> Brush { get; set; }

        public StraightLineViewModel(StraightLine model)
            : base(model)
        {
            X2 = model
                .ObserveProperty(x => x.X2)
                .ToReactiveProperty();
            Y2 = model
                .ObserveProperty(x => x.Y2)
                .ToReactiveProperty();
            Brush = model
                .ObserveProperty(x => x.Brush)
                .ToReactiveProperty();
        }
    }
}
