
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class NRectangleViewModel : DesignerItemViewModelBase, IRadius
    {
        public NRectangleViewModel()
            : base()
        {
            Init();
        }

        public NRectangleViewModel(double left, double top, double width, double height)
            : base()
        {
            Init();
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            PathGeometryNoRotate.Value = null;
            Height.Value = height;
        }

        public NRectangleViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
        }

        public NRectangleViewModel(int id, IDiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public ReactiveCommand MouseDoubleClickCommand { get; } = new ReactiveCommand();

        public override bool SupportsPropertyDialog => true;

        public ReactivePropertySlim<double> RadiusX { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> RadiusY { get; } =  new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
            MouseDoubleClickCommand.Subscribe(x =>
            {
                OpenPropertyDialog();
            })
            .AddTo(_CompositeDisposable);
            RadiusX
                .Zip(RadiusX.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(RadiusX), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            RadiusY
                .Zip(RadiusY.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(RadiusY), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(DetailRectangle), new DialogParameters() { { "ViewModel", (NRectangleViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<NRectangleViewModel>("ViewModel");
                this.Left.Value = viewModel.Left.Value;
                this.Top.Value = viewModel.Top.Value;
                this.Width.Value = viewModel.Width.Value;
                this.Height.Value = viewModel.Height.Value;
                this.CenterX.Value = viewModel.CenterX.Value;
                this.CenterY.Value = viewModel.CenterY.Value;
                this.RotationAngle.Value = viewModel.RotationAngle.Value;
                this.StrokeLineJoin.Value = viewModel.StrokeLineJoin.Value;
                this.StrokeDashArray.Value = viewModel.StrokeDashArray.Value;
                this.RadiusX.Value = viewModel.RadiusX.Value;
                this.RadiusY.Value = viewModel.RadiusY.Value;
                this.StrokeMiterLimit.Value = viewModel.StrokeMiterLimit.Value;
            }
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreateRectangle(this, RadiusX.Value, RadiusY.Value, flag);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            return GeometryCreator.CreateRectangleWithAngle(this, RadiusX.Value, RadiusY.Value, angle);
        }

        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new NRectangleViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
            clone.StrokeDashArray.Value = StrokeDashArray.Value;
            clone.RadiusX.Value = RadiusX.Value;
            clone.RadiusY.Value = RadiusY.Value;
            clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
            return clone;
        }


        #endregion //IClonable
    }
}
