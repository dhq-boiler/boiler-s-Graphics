using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class NEllipseViewModel : DesignerItemViewModelBase
    {
        public override bool SupportsPropertyDialog => true;

        public NEllipseViewModel()
            : base()
        {
            Init();
        }

        public NEllipseViewModel(double left, double top, double width, double height)
            : base()
        {
            Init();
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            Height.Value = height;
        }

        public NEllipseViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
        }

        public NEllipseViewModel(int id, IDiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreateEllipse(this, flag);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            return GeometryCreator.CreateEllipse(this, angle);
        }
        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new NEllipseViewModel();
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
            clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
            return clone;
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(DetailEllipse), new DialogParameters() { { "ViewModel", (NEllipseViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<NEllipseViewModel>("ViewModel");
                this.Left.Value = viewModel.Left.Value;
                this.Top.Value = viewModel.Top.Value;
                this.Width.Value = viewModel.Width.Value;
                this.Height.Value = viewModel.Height.Value;
                this.CenterX.Value = viewModel.CenterX.Value;
                this.CenterY.Value = viewModel.CenterY.Value;
                this.RotationAngle.Value = viewModel.RotationAngle.Value;
                this.StrokeLineJoin.Value = viewModel.StrokeLineJoin.Value;
                this.StrokeDashArray.Value = viewModel.StrokeDashArray.Value;
                this.StrokeMiterLimit.Value = viewModel.StrokeMiterLimit.Value;
            }
        }

        #endregion //IClonable
    }
}
