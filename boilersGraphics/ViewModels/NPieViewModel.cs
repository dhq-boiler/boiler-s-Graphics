using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class NPieViewModel : DesignerItemViewModelBase
    {

        public NPieViewModel()
            : base()
        {
            Init();
        }

        public NPieViewModel(double left, double top, double width, double height)
            : base()
        {
            Init();
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            Height.Value = height;
        }

        public NPieViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
        }

        public NPieViewModel(int id, IDiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public ReactiveCommand MouseDoubleClickCommand { get; } = new ReactiveCommand();

        public override bool SupportsPropertyDialog => false;

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
            MouseDoubleClickCommand.Subscribe(x =>
            {
                OpenPropertyDialog();
            })
            .AddTo(_CompositeDisposable);
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailRectangle), new DialogParameters() { { "ViewModel", (NRectangleViewModel)this.Clone() } }, ret => result = ret);
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
            }
        }

        public ReactivePropertySlim<Point> PieCenterPoint { get; } = new ReactivePropertySlim<Point>();

        public ReactivePropertySlim<double> DonutWidth { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Distance { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> StartDegree { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> EndDegree { get; } = new ReactivePropertySlim<double>();



        public override PathGeometry CreateGeometry()
        {
            return GeometryCreator.CreateDonut(PieCenterPoint.Value, DonutWidth.Value, Distance.Value, StartDegree.Value, EndDegree.Value, SweepDirection.Clockwise);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            var geometry = GeometryCreator.CreateDonut(PieCenterPoint.Value, DonutWidth.Value, Distance.Value, StartDegree.Value, EndDegree.Value, SweepDirection.Clockwise);
            geometry.Transform = new RotateTransform(angle, PieCenterPoint.Value.X, PieCenterPoint.Value.Y);
            return geometry;
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new NPieViewModel();
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
            clone.PathGeometry.Value = CreateGeometry(clone.RotationAngle.Value);
            return clone;
        }


        #endregion //IClonable
    }
}
