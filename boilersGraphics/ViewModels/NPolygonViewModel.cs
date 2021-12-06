using boilersGraphics.Controls;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class NPolygonViewModel : DesignerItemViewModelBase
    {
        public ReactiveCollection<SnapPoint> SnapPoints { get; } = new ReactiveCollection<SnapPoint>();

        public NPolygonViewModel()
            : base()
        {
            Init();
        }

        public NPolygonViewModel(double left, double top, double width, double height)
            : base()
        {
            Init();
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            Height.Value = height;
        }

        public NPolygonViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
        }

        public NPolygonViewModel(int id, IDiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
            Data.Subscribe(_ =>
            {
                UpdatePathGeometryIfEnable();
            })
            .AddTo(_CompositeDisposable);
        }

        public ReactivePropertySlim<string> Data { get; set; } = new ReactivePropertySlim<string>();

        public override bool SupportsPropertyDialog => true;

        public override PathGeometry CreateGeometry()
        {
            return System.Windows.Media.PathGeometry.CreateFromGeometry(Geometry.Parse(Data.Value));
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            var geometry = Geometry.Parse(Data.Value);
            var pathGeometry = System.Windows.Media.PathGeometry.CreateFromGeometry(geometry);
            pathGeometry.Transform = new RotateTransform(angle);
            return pathGeometry;
        }
        public override Type GetViewType()
        {
            return typeof(Path);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new NPolygonViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.FillColor.Value = FillColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.Data.Value = Data.Value;
            return clone;
        }

        #endregion //IClonable

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailPolygon), new DialogParameters() { { "ViewModel", (NPolygonViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<NPolygonViewModel>("ViewModel");
                this.Left.Value = viewModel.Left.Value;
                this.Top.Value = viewModel.Top.Value;
                this.Width.Value = viewModel.Width.Value;
                this.Height.Value = viewModel.Height.Value;
                this.CenterX.Value = viewModel.CenterX.Value;
                this.CenterY.Value = viewModel.CenterY.Value;
                this.RotationAngle.Value = viewModel.RotationAngle.Value;
            }
        }
    }
}
