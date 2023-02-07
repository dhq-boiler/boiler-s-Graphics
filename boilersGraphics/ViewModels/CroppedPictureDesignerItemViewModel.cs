using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reactive.Bindings;

namespace boilersGraphics.ViewModels;

public class CroppedPictureDesignerItemViewModel : DesignerItemViewModelBase, IEmbeddedImage
{
    private double _FileHeight;
    private string _FileName;
    private double _FileWidth;

    public CroppedPictureDesignerItemViewModel()
    {
    }

    public CroppedPictureDesignerItemViewModel(BitmapImage bitmapImage)
    {
        EmbeddedImage.Value = bitmapImage;
    }

    public CroppedPictureDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top,
        BitmapImage bitmapImage)
        : base(id, parent, left, top)
    {
        EmbeddedImage.Value = bitmapImage;
    }

    public double FileWidth
    {
        get => _FileWidth;
        set => SetProperty(ref _FileWidth, value);
    }

    public double FileHeight
    {
        get => _FileHeight;
        set => SetProperty(ref _FileHeight, value);
    }

    public override bool SupportsPropertyDialog => false;

    public string FileName
    {
        get => _FileName;
        set => SetProperty(ref _FileName, value);
    }

    public ReactivePropertySlim<BitmapImage> EmbeddedImage { get; set; } = new();

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        var pg = PathGeometryNoRotate.Value;
        var group = new TransformGroup();
        group.Children.Add(new ScaleTransform(Width.Value / pg.Bounds.Width, Height.Value / pg.Bounds.Height));
        group.Children.Add(new TranslateTransform(EdgeThickness.Value / 2, EdgeThickness.Value / 2));
        pg.Transform = group;
        return pg;
    }


    public override Type GetViewType()
    {
        return typeof(Image);
    }

    public override void OpenPropertyDialog()
    {
        throw new NotImplementedException();
    }

    public override object Clone()
    {
        var clone = new CroppedPictureDesignerItemViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.PathGeometryNoRotate.Value = PathGeometryNoRotate.Value;
        clone.PathGeometryRotate.Value = PathGeometryRotate.Value;
        clone.FileName = FileName;
        clone.FileWidth = FileWidth;
        clone.FileHeight = FileHeight;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        return clone;
    }
}