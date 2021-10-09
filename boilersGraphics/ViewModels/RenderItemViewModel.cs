using boilersGraphics.Models;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;

namespace boilersGraphics.ViewModels
{
    [Obsolete]
    public class RenderItemViewModel : BindableBase
    {
        public RenderItem Model { get; set; }

        public ReadOnlyReactivePropertySlim<double> X { get; set; }

        public ReadOnlyReactivePropertySlim<double> Y { get; set; }

        public ReadOnlyReactivePropertySlim<double> Width { get; set; }

        public ReadOnlyReactivePropertySlim<double> Height { get; set; }

        public ReadOnlyReactivePropertySlim<bool> IsSelected { get; set; }

        public RenderItemViewModel(RenderItem model)
        {
            Model = model;
            X = Model
                .ObserveProperty(x => x.X)
                .ToReadOnlyReactivePropertySlim();
            Y = Model
                .ObserveProperty(x => x.Y)
                .ToReadOnlyReactivePropertySlim();
            Width = Model
                .ObserveProperty(x => x.Width)
                .ToReadOnlyReactivePropertySlim();
            Height = Model
                .ObserveProperty(x => x.Height)
                .ToReadOnlyReactivePropertySlim();
            IsSelected = Model
                .ObserveProperty(x => x.IsSelected)
                .ToReadOnlyReactivePropertySlim();
        }
    }
}
