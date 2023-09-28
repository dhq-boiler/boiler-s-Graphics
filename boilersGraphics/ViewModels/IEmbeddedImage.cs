using Reactive.Bindings;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels;

internal interface IEmbeddedImage
{
    ReactivePropertySlim<BitmapImage> EmbeddedImage { get; }

    string FileName { get; }
}