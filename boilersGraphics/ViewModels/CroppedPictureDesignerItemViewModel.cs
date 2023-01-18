using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class CroppedPictureDesignerItemViewModel : PictureDesignerItemViewModel
    {
        public CroppedPictureDesignerItemViewModel()
        {
        }

        public CroppedPictureDesignerItemViewModel(BitmapImage bitmapImage)
        {
            EmbeddedImage.Value = bitmapImage;
        }
        public CroppedPictureDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top, BitmapImage bitmapImage)
            : base(id, parent, left, top)
        {
            EmbeddedImage.Value = bitmapImage;
        }
    }
}
