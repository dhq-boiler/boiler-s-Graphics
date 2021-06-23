using boilersGraphics.Models;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels
{
    public class RenderItemViewModel : BindableBase
    {
        public RenderItem Model { get; set; }

        public ReactiveProperty<double> X { get; set; }

        public ReactiveProperty<double> Y { get; set; }

        public ReactiveProperty<double> Width { get; set; }

        public ReactiveProperty<double> Height { get; set; }

        public ReactiveProperty<bool> IsSelected { get; set; }

        public RenderItemViewModel(RenderItem model)
        {
            Model = model;
            X = Model
                .ObserveProperty(x => x.X)
                .ToReactiveProperty();
            Y = Model
                .ObserveProperty(x => x.Y)
                .ToReactiveProperty();
            Width = Model
                .ObserveProperty(x => x.Width)
                .ToReactiveProperty();
            Height = Model
                .ObserveProperty(x => x.Height)
                .ToReactiveProperty();
            IsSelected = Model
                .ObserveProperty(x => x.IsSelected)
                .ToReactiveProperty();
        }
    }
}
