using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using R3;
using System;
using System.Windows;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels;

public class StraightConnectorViewModel : ConnectorBaseViewModel
{
    public StraightConnectorViewModel(int id, IDiagramViewModel parent)
        : base(id, parent)
    {
    }

    public StraightConnectorViewModel()
    {
    }

    public StraightConnectorViewModel(IDiagramViewModel diagramViewModel, Point p1)
    {
        AddPointP1(diagramViewModel, p1);
    }

    [Obsolete]
    public StraightConnectorViewModel(IDiagramViewModel diagramViewModel, Point p1, Point p2)
    {
        AddPoints(diagramViewModel, p1, p2);
        InitIsSelectedOnSnapPoints();
    }

    public override bool SupportsPropertyDialog => true;

    public BindableReactiveProperty<double> P1X { get; set; }
    public BindableReactiveProperty<double> P1Y { get; set; }
    public BindableReactiveProperty<double> P2X { get; set; }
    public BindableReactiveProperty<double> P2Y { get; set; }

    public override void PostProcess_AddPointP1(Point p1)
    {
        P1X = Observable.Return(p1.X).ToBindableReactiveProperty();
        P1Y = Observable.Return(p1.Y).ToBindableReactiveProperty();
    }

    public override void PostProcess_AddPointP2(Point p2)
    {
        P2X = Observable.Return(p2.X).ToBindableReactiveProperty();
        P2Y = Observable.Return(p2.Y).ToBindableReactiveProperty();
    }

    public override Type GetViewType()
    {
        return typeof(Line);
    }

    #region IClonable

    public override object Clone()
    {
        var clone = new StraightConnectorViewModel(Owner, Points[0]);
        clone.Owner = Owner;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.AddPointP2(Owner, Points[1]);
        clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
        clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        return clone;
    }

    public override void OpenPropertyDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        var parameters = new DialogParameters { { "ViewModel", this } };
        dialogService.Show(nameof(DetailStraightLine), parameters, ret => result = ret);
    }

    #endregion //IClonable
}