using boilersGraphics.Helpers;
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
        public override bool SupportsPropertyDialog => false;

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
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.PathGeometry.Value = GeometryCreator.CreatePolyBezier(clone);

            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }

        public override void OpenPropertyDialog()
        {
            throw new NotImplementedException();
        }
    }
}
