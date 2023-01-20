using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    interface IEmbeddedImage
    {
        ReactivePropertySlim<BitmapImage> EmbeddedImage { get; }

        string FileName { get; }
    }
}
