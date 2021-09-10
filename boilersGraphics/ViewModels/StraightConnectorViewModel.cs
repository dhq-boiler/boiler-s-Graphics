
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class StraightConnectorViewModel : ConnectorBaseViewModel
    {
        public ReadOnlyReactivePropertySlim<SnapPointViewModel> SnapPoint0VM { get; }
        public ReadOnlyReactivePropertySlim<SnapPointViewModel> SnapPoint1VM { get; }

        public StraightConnectorViewModel(int id, IDiagramViewModel parent)
            : base(id, parent)
        { }

        public StraightConnectorViewModel()
            : base()
        { }

        public StraightConnectorViewModel(Point p1, Point p2)
            : base()
        {
            Points.Add(p1);
            Points.Add(p2);
            SnapPoint0VM = Observable.Return(Points[0])
                                     .Select(x => new SnapPointViewModel(this, 0, (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel, x.X, x.Y, 3, 3))
                                     .ToReadOnlyReactivePropertySlim();
            SnapPoint1VM = Observable.Return(Points[1])
                                     .Select(x => new SnapPointViewModel(this, 1, (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel, x.X, x.Y, 3, 3))
                                     .ToReadOnlyReactivePropertySlim();
        }

        public override Type GetViewType()
        {
            return typeof(Line);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new StraightConnectorViewModel(Points[0], Points[1]);
            clone.Owner = Owner;
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Points[0] = Points[0];
            clone.Points[1] = Points[1];

            return clone;
        }

        #endregion //IClonable
    }
}
