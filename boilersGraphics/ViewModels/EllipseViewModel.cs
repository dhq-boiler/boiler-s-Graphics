using boilersGraphics.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    internal class EllipseViewModel : RenderItemViewModel
    {
        public ReadOnlyReactivePropertySlim<Brush> Stroke { get; set; }

        public ReadOnlyReactivePropertySlim<Brush> Fill { get; set; }

        public EllipseViewModel(Ellipse model)
            : base(model)
        {
            Stroke = model
                .ObserveProperty(x => x.Stroke)
                .ToReadOnlyReactivePropertySlim();
            Fill = model
                .ObserveProperty(x => x.Fill)
                .ToReadOnlyReactivePropertySlim();
        }
    }
}
