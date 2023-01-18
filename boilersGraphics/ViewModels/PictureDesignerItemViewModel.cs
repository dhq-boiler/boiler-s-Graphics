using boilersGraphics.Helpers;
using boilersGraphics.Views;
using NLog;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class PictureDesignerItemViewModel : DesignerItemViewModelBase
    {
        private string _FileName;
        private double _FileWidth;
        private double _FileHeight;

        public string FileName
        {
            get { return _FileName; }
            set { SetProperty(ref _FileName, value); }
        }

        public double FileWidth
        {
            get { return _FileWidth; }
            set { SetProperty(ref _FileWidth, value); }
        }

        public double FileHeight
        {
            get { return _FileHeight; }
            set { SetProperty(ref _FileHeight, value); }
        }

        public ReactivePropertySlim<Rect> ClippingOriginRect { get; set; } = new ReactivePropertySlim<Rect>();

        public ReactivePropertySlim<System.Windows.Thickness> Margin { get; set; } = new ReactivePropertySlim<System.Windows.Thickness>();

        public ReactivePropertySlim<DesignerItemViewModelBase> ClipObject { get; set; } = new ReactivePropertySlim<DesignerItemViewModelBase>();

        public override bool SupportsPropertyDialog => true;

        public ReactivePropertySlim<BitmapImage> EmbeddedImage { get; set; } = new ReactivePropertySlim<BitmapImage>();

        public PictureDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public PictureDesignerItemViewModel()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
        }

        public override void UpdatePathGeometryIfEnable(string propertyName, object oldValue, object newValue, bool flag = false)
        {
            if (EnablePathGeometryUpdate.Value)
            {
                PathGeometryNoRotate.Value = CreateGeometry();

                if (RotationAngle.Value != 0)
                {
                    PathGeometryRotate.Value = CreateGeometry(RotationAngle.Value);
                }
            }
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreatePicture(this, flag);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            return GeometryCreator.CreatePictureWithAngle(this, angle);
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
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(DetailPicture), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
        }

        #endregion //IClonable
    }
}
