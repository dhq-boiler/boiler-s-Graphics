
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
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class NRectangleViewModel : DesignerItemViewModelBase
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

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
            MouseDoubleClickCommand.Subscribe(x =>
            {
                var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
                IDialogResult result = null;
                dialogService.Show(nameof(DetailRectangle), new DialogParameters() { { "ViewModel", (NRectangleViewModel)this.Clone() } }, ret => result = ret);
                if (result != null)
                {
                    var viewModel = result.Parameters.GetValue<NRectangleViewModel>("ViewModel");
                    this.Left.Value = viewModel.Left.Value;
                    this.Top.Value = viewModel.Top.Value;
                    this.Width.Value = viewModel.Width.Value;
                    this.Height.Value = viewModel.Height.Value;
                }
            })
            .AddTo(_CompositeDisposable);
        }

        public override PathGeometry CreateGeometry()
        {
            return GeometryCreator.CreateRectangle(this);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            return GeometryCreator.CreateRectangle(this, angle);
        }

        public override Type GetViewType()
        {
            return typeof(Path);
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
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.FillColor.Value = FillColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.PathGeometry.Value = GeometryCreator.CreateRectangle(clone);
            return clone;
        }


        #endregion //IClonable
    }
}
