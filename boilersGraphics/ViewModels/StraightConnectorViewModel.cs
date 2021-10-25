using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Windows;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class StraightConnectorViewModel : ConnectorBaseViewModel
    {
        public override bool SupportsPropertyDialog => true;

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

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailStraightLine), new DialogParameters() { { "ViewModel", (StraightConnectorViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<StraightConnectorViewModel>("ViewModel");
                this.Points[0] = viewModel.Points[0];
                this.Points[1] = viewModel.Points[1];
            }
        }

        #endregion //IClonable
    }
}
