using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using System;
using System.Windows.Media;

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
        }

        public ReactivePropertySlim<string> Data { get; set; } = new ReactivePropertySlim<string>();

        public override bool SupportsPropertyDialog => true;

        public override void UpdatePathGeometryIfEnable(string propertyName, object oldValue, object newValue, bool flag = false)
        {
            if (EnablePathGeometryUpdate.Value)
            {
                if (!flag)
                {
                    if (Left.Value != 0 && Top.Value != 0 && Width.Value != 0 && Height.Value != 0)
                    {
                        PathGeometryNoRotate.Value = CreateGeometry(flag);
                        if (Width.Value != PathGeometryNoRotate.Value.Bounds.Width || Height.Value != PathGeometryNoRotate.Value.Bounds.Height)
                        {
                            var lhs = PathGeometryNoRotate.Value.Clone();
                            var coefficientWidth = Width.Value / lhs.Bounds.Width;
                            var coefficientHeight = Height.Value / lhs.Bounds.Height;
                            if (coefficientWidth == 0)
                                coefficientWidth = 1;
                            if (coefficientHeight == 0)
                                coefficientHeight = 1;
                            var newlhs = GeometryCreator.Scale(lhs, coefficientWidth, coefficientHeight);
                            newlhs = GeometryCreator.Translate(newlhs, -newlhs.Bounds.Left, -newlhs.Bounds.Top);
                            PathGeometryNoRotate.Value = newlhs;
                        }
                    }
                    if (!(PathGeometryNoRotate.Value is null))
                    {
                        Data.Value = PathGeometryNoRotate.Value.ToString();
                    }
                }

                if (RotationAngle.Value != 0)
                {
                    PathGeometryRotate.Value = CreateGeometry(RotationAngle.Value);
                }
            }
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreatePolygon(this, Data.Value, flag);
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
            return typeof(System.Windows.Shapes.Path);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new NPolygonViewModel();
            clone.Owner = Owner;
            clone.Data.Value = Data.Value;
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

        #endregion //IClonable

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(DetailPolygon), new DialogParameters() { { "ViewModel", (NPolygonViewModel)this.Clone() } }, ret => result = ret);
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
                this.StrokeLineJoin.Value = viewModel.StrokeLineJoin.Value;
                this.StrokeDashArray.Value = viewModel.StrokeDashArray.Value;
                this.StrokeMiterLimit.Value = viewModel.StrokeMiterLimit.Value;
            }
        }
    }
}
