using boilersGraphics.Helpers;
using System.Windows.Media;
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

        protected override void Init()
        {
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            var pg = PathGeometryNoRotate.Value;
            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform((Width.Value - EdgeThickness.Value) / pg.Bounds.Width, (Height.Value - EdgeThickness.Value) / pg.Bounds.Height));
            group.Children.Add(new TranslateTransform(EdgeThickness.Value / 2, EdgeThickness.Value / 2));
            pg.Transform = group;
            return pg;
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            var pg = PathGeometryNoRotate.Value;
            pg.Transform = new RotateTransform(RotationAngle.Value);
            return pg;
        }
    }
}
