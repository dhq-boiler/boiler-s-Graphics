using boilersGraphics.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    [Obsolete]
    internal class StraightLineViewModel : RenderItemViewModel
    {
        public ReadOnlyReactivePropertySlim<double> X2 { get; set; }

        public ReadOnlyReactivePropertySlim<double> Y2 { get; set; }

        public ReadOnlyReactivePropertySlim<Brush> Brush { get; set; }

        public StraightLineViewModel(StraightLine model)
            : base(model)
        {
            X2 = model
                .ObserveProperty(x => x.X2)
                .ToReadOnlyReactivePropertySlim();
            Y2 = model
                .ObserveProperty(x => x.Y2)
                .ToReadOnlyReactivePropertySlim();
            Brush = model
                .ObserveProperty(x => x.Brush)
                .ToReadOnlyReactivePropertySlim();
        }
    }
}
