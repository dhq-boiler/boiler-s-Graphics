using System.Windows.Media.Imaging;
using Reactive.Bindings;

namespace boilersGraphics.ViewModels;

internal interface IEmbeddedImage
{
    ReactivePropertySlim<BitmapImage> EmbeddedImage { get; }

    string FileName { get; }
}