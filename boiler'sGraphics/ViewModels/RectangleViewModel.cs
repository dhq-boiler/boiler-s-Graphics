using boiler_sGraphics.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Media;

namespace boiler_sGraphics.ViewModels
{
    internal class RectangleViewModel : RenderItemViewModel
    {
        public ReactiveProperty<Brush> Stroke { get; set; }

        public ReactiveProperty<Brush> Fill { get; set; }

        public RectangleViewModel(Rectangle model)
            : base(model)
        {
            Stroke = model
                .ObserveProperty(x => x.Stroke)
                .ToReactiveProperty();
            Fill = model
                .ObserveProperty(x => x.Fill)
                .ToReactiveProperty();
        }
    }
}
