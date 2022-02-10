using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class PolyBezierViewModel : ConnectorBaseViewModel
    {
        public override bool SupportsPropertyDialog => true;

        public PolyBezierViewModel()
            : base()
        { }

        public PolyBezierViewModel(int id, IDiagramViewModel parent)
            : base(id, parent)
        {
            Init(parent);
        }

        public PolyBezierViewModel(IDiagramViewModel parent, Point beginPoint)
            : base(0, parent)
        {
            Init(parent);
            Points.Add(beginPoint);
        }

        public void InitializeSnapPoints(Point begin, Point end)
        {
            SnapPoint0VM = Observable.Return(begin)
                                     .Select(x => new SnapPointViewModel(this, 0, Owner, x.X, x.Y, 3, 3))
                                     .ToReadOnlyReactivePropertySlim();
            SnapPoint1VM = Observable.Return(end)
                                     .Select(x => new SnapPointViewModel(this, 1, Owner, x.X, x.Y, 3, 3))
                                     .ToReadOnlyReactivePropertySlim();
        }

        private void Init(IDiagramViewModel diagramViewModel)
        {
            Points.CollectionChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SnapPoint0VM != null)
                SnapPoint0VM.Dispose();
            SnapPoint0VM = Observable.Return(Points.First())
                                     .Select(x => new SnapPointViewModel(this, 0, Owner, x.X, x.Y, 3, 3))
                                     .ToReadOnlyReactivePropertySlim();
            if (SnapPoint1VM != null)
                SnapPoint1VM.Dispose();
            SnapPoint1VM = Observable.Return(Points.Last())
                                     .Select(x => new SnapPointViewModel(this, 1, Owner, x.X, x.Y, 3, 3))
                                     .ToReadOnlyReactivePropertySlim();
        }

        public override object Clone()
        {
            var clone = new PolyBezierViewModel(0, Owner);
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Points = Points;
            clone.PathGeometry.Value = GeometryCreator.CreatePolyBezier(clone);
            clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
            clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
            clone.PenLineJoin.Value = PenLineJoin.Value;
            clone.StrokeDashArray.Value = StrokeDashArray.Value;
            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailPolyBezier), new DialogParameters() { { "ViewModel", (PolyBezierViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<PolyBezierViewModel>("ViewModel");
                this.Points = viewModel.Points;
                this.SnapPoint0VM.Value.Left.Value = viewModel.Points.First().X;
                this.SnapPoint0VM.Value.Top.Value = viewModel.Points.First().Y;
                this.SnapPoint1VM.Value.Left.Value = viewModel.Points.Last().X;
                this.SnapPoint1VM.Value.Top.Value = viewModel.Points.Last().Y;
                this.StrokeStartLineCap.Value = viewModel.StrokeStartLineCap.Value;
                this.StrokeEndLineCap.Value = viewModel.StrokeEndLineCap.Value;
                this.PenLineJoin.Value = viewModel.PenLineJoin.Value;
                this.StrokeDashArray.Value = viewModel.StrokeDashArray.Value;
            }
        }
    }
}
