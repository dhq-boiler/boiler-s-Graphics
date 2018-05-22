using grapher.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Media;

namespace grapher.ViewModels
{
    class EllipseViewModel : RenderItemViewModel
    {
        public ReactiveProperty<Brush> Stroke { get; set; }

        public ReactiveProperty<Brush> Fill { get; set; }

        public EllipseViewModel(Ellipse model)
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
