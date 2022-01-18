using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using System;
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

        public ReactivePropertySlim<Geometry> Clip { get; set; } = new ReactivePropertySlim<Geometry>();

        public ReactivePropertySlim<SelectableDesignerItemViewModelBase> ClipObject { get; set; } = new ReactivePropertySlim<SelectableDesignerItemViewModelBase>();

        public override bool SupportsPropertyDialog => true;

        public ReactivePropertySlim<BitmapImage> EmbeddedImage { get; } = new ReactivePropertySlim<BitmapImage>();

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
            EnablePathGeometryUpdate.Value = false;
        }

        public override PathGeometry CreateGeometry()
        {
            throw new NotSupportedException("picture is not supported.");
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            throw new NotSupportedException("picture is not supported.");
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
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.FillColor.Value = FillColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.Clip.Value = Clip.Value;
            clone.FileName = FileName;
            clone.FileWidth = FileWidth;
            clone.FileHeight = FileHeight;
            return clone;
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailPicture), new DialogParameters() { { "ViewModel", (PictureDesignerItemViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<PictureDesignerItemViewModel>("ViewModel");
                this.Left.Value = viewModel.Left.Value;
                this.Top.Value = viewModel.Top.Value;
                this.Width.Value = viewModel.Width.Value;
                this.Height.Value = viewModel.Height.Value;
                this.CenterX.Value = viewModel.CenterX.Value;
                this.CenterY.Value = viewModel.CenterY.Value;
                this.RotationAngle.Value = viewModel.RotationAngle.Value;
            }
        }

        #endregion //IClonable
    }
}
