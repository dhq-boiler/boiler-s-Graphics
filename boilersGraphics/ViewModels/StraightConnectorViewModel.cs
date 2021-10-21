
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class StraightConnectorViewModel : ConnectorBaseViewModel
    {
        public StraightConnectorViewModel(int id, IDiagramViewModel parent)
            : base(id, parent)
        { }

        public StraightConnectorViewModel()
            : base()
        { }

        public StraightConnectorViewModel(IDiagramViewModel diagramViewModel, Point p1, Point p2)
            : base()
        {
            AddPoints(diagramViewModel, p1, p2);
            InitIsSelectedOnSnapPoints();
        }

        public override Type GetViewType()
        {
            return typeof(Line);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new StraightConnectorViewModel(Owner, Points[0], Points[1]);
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
