using R3;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels;

internal interface IEmbeddedImage
{
    BindableReactiveProperty<BitmapImage> EmbeddedImage { get; }

    string FileName { get; }
}