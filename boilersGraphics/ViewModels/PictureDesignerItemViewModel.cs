using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Thickness = System.Windows.Thickness;

namespace boilersGraphics.ViewModels;

public class PictureDesignerItemViewModel : DesignerItemViewModelBase, IEmbeddedImage
{
    private double _FileHeight;
    private string _FileName;
    private double _FileWidth;

    public PictureDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        Init();
    }

    public PictureDesignerItemViewModel()
    {
        Init();
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

    public ReactivePropertySlim<Rect> ClippingOriginRect { get; set; } = new();

    public ReactivePropertySlim<Thickness> Margin { get; set; } = new();

    public ReactivePropertySlim<DesignerItemViewModelBase> ClipObject { get; set; } = new();

    public override bool SupportsPropertyDialog => true;

    public string FileName
    {
        get => _FileName;
        set => SetProperty(ref _FileName, value);
    }

    public ReactivePropertySlim<BitmapImage> EmbeddedImage { get; set; } = new();

    protected virtual void Init()
    {
        ShowConnectors = false;
        UpdatingStrategy.Value = PathGeometryUpdatingStrategy.Initial;
    }

    public override void UpdatePathGeometryIfEnable(string propertyName, object oldValue, object newValue,
        bool flag = false)
    {
        if (UpdatingStrategy.Value == PathGeometryUpdatingStrategy.Initial)
        {
            PathGeometryNoRotate.Value = CreateGeometry();

            if (RotationAngle.Value != 0d) PathGeometryRotate.Value = GeometryCreator.Rotate(PathGeometryNoRotate.Value, RotationAngle.Value, CenterPoint.Value);
        }
    }

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        return GeometryCreator.CreatePicture(this, flag);
    }


    public override Type GetViewType()
    {
        return typeof(Image);
    }

    #region IClonable

    public override object Clone()
    {
        var clone = new PictureDesignerItemViewModel();
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

    public override void OpenPropertyDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailPicture), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }

    #endregion //IClonable
}