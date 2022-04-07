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

        public override bool SupportsPropertyDialog => true;

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
            dialogService.ShowDialog(nameof(DetailPie), new DialogParameters() { { "ViewModel", (NPieViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<NPieViewModel>("ViewModel");
                this.Left.Value = viewModel.Left.Value;
                this.Top.Value = viewModel.Top.Value;
                this.Width.Value = viewModel.Width.Value;
                this.Height.Value = viewModel.Height.Value;
                this.CenterX.Value = viewModel.CenterX.Value;
                this.CenterY.Value = viewModel.CenterY.Value;
                this.RotationAngle.Value = viewModel.RotationAngle.Value;
                this.PieCenterPoint.Value = viewModel.PieCenterPoint.Value;
                this.DonutWidth.Value = viewModel.DonutWidth.Value;
                this.Distance.Value = viewModel.Distance.Value;
                this.StartDegree.Value = viewModel.StartDegree.Value;
                this.EndDegree.Value = viewModel.EndDegree.Value;
                this.SweepDirection.Value = viewModel.SweepDirection.Value;
                this.PenLineJoin.Value = viewModel.PenLineJoin.Value;
                this.StrokeDashArray.Value = viewModel.StrokeDashArray.Value;
            }
        }

        public ReactivePropertySlim<Point> PieCenterPoint { get; } = new ReactivePropertySlim<Point>();

        public ReactivePropertySlim<double> DonutWidth { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Distance { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> StartDegree { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> EndDegree { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<SweepDirection> SweepDirection { get; } = new ReactivePropertySlim<SweepDirection>();



        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreateDonut(PieCenterPoint.Value, DonutWidth.Value, Distance.Value, StartDegree.Value, EndDegree.Value, SweepDirection.Value);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            var geometry = GeometryCreator.CreateDonut(PieCenterPoint.Value, DonutWidth.Value, Distance.Value, StartDegree.Value, EndDegree.Value, SweepDirection.Value);
            geometry.Transform = new RotateTransform(angle, PieCenterPoint.Value.X, PieCenterPoint.Value.Y);
            return geometry;
        }

        public NEllipseViewModel CreateNEllipseViewModelLong()
        {
            var viewModel = new NEllipseViewModel();
            viewModel.Left.Value = PieCenterPoint.Value.X - Distance.Value;
            viewModel.Top.Value = PieCenterPoint.Value.Y - Distance.Value;
            viewModel.Width.Value = Distance.Value * 2;
            viewModel.Height.Value = Distance.Value * 2;
            viewModel.PathGeometryNoRotate.Value = GeometryCreator.CreateEllipse(PieCenterPoint.Value.X, PieCenterPoint.Value.Y, new System.Windows.Thickness(PieCenterPoint.Value.X - Distance.Value, PieCenterPoint.Value.Y - Distance.Value, PieCenterPoint.Value.X + Distance.Value, PieCenterPoint.Value.Y + Distance.Value));
            return viewModel;
        }

        public NEllipseViewModel CreateNEllipseViewModelShort()
        {
            var viewModel = new NEllipseViewModel();
            viewModel.Left.Value = PieCenterPoint.Value.X - (Distance.Value - DonutWidth.Value);
            viewModel.Top.Value = PieCenterPoint.Value.Y - (Distance.Value - DonutWidth.Value);
            viewModel.Width.Value = (Distance.Value - DonutWidth.Value) * 2;
            viewModel.Height.Value = (Distance.Value - DonutWidth.Value) * 2;
            viewModel.PathGeometryNoRotate.Value = GeometryCreator.CreateEllipse(PieCenterPoint.Value.X, PieCenterPoint.Value.Y, new System.Windows.Thickness(PieCenterPoint.Value.X - Distance.Value, PieCenterPoint.Value.Y - Distance.Value, PieCenterPoint.Value.X + Distance.Value, PieCenterPoint.Value.Y + Distance.Value));
            return viewModel;
        }

        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
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
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.PathGeometryNoRotate.Value = CreateGeometry();
            clone.PieCenterPoint.Value = PieCenterPoint.Value;
            clone.DonutWidth.Value = DonutWidth.Value;
            clone.Distance.Value = Distance.Value;
            clone.StartDegree.Value = StartDegree.Value;
            clone.EndDegree.Value = EndDegree.Value;
            clone.SweepDirection.Value = SweepDirection.Value;
            clone.PenLineJoin.Value = PenLineJoin.Value;
            clone.StrokeDashArray.Value = StrokeDashArray.Value;
            return clone;
        }


        #endregion //IClonable
    }
}
