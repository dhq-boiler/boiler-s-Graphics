﻿using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using System;
using System.Reactive.Linq;
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
            P1X = Observable.Return(p1.X).ToReactiveProperty();
            P1Y = Observable.Return(p1.Y).ToReactiveProperty();
            P2X = Observable.Return(p2.X).ToReactiveProperty();
            P2Y = Observable.Return(p2.Y).ToReactiveProperty();
            InitIsSelectedOnSnapPoints();
        }

        public ReactiveProperty<double> P1X { get; set; }
        public ReactiveProperty<double> P1Y { get; set; }
        public ReactiveProperty<double> P2X { get; set; }
        public ReactiveProperty<double> P2Y { get; set; }

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
                this.Points[0] = new Point(viewModel.P1X.Value, viewModel.P1Y.Value);
                this.Points[1] = new Point(viewModel.P2X.Value, viewModel.P2Y.Value);
                this.SnapPoint0VM.Value.Left.Value = viewModel.P1X.Value;
                this.SnapPoint0VM.Value.Top.Value = viewModel.P1Y.Value;
                this.SnapPoint1VM.Value.Left.Value = viewModel.P2X.Value;
                this.SnapPoint1VM.Value.Top.Value = viewModel.P2Y.Value;
            }
        }

        #endregion //IClonable
    }
}
