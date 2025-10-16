using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels;

internal class DetailStraightLineViewModel : DetailViewModelBase<StraightConnectorViewModel>
{
    public DetailStraightLineViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        Properties.Add(new PropertyOptionsValueCombinationStruct<StraightConnectorViewModel, PenLineCap>(
            ViewModel.Value, "StrokeStartLineCap", new[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle
            }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<StraightConnectorViewModel, PenLineCap>(
            ViewModel.Value, "StrokeEndLineCap", new[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle
            }));
        Properties.Add(
            new PropertyOptionsValueCombinationClass<StraightConnectorViewModel, DoubleCollection>(ViewModel.Value,
                "StrokeDashArray", HorizontalAlignment.Left));
        Properties.Add(
            new PropertyOptionsValueCombinationStructRP<StraightConnectorViewModel, double>(ViewModel.Value, "P1X",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStructRP<StraightConnectorViewModel, double>(ViewModel.Value, "P1Y",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStructRP<StraightConnectorViewModel, double>(ViewModel.Value, "P2X",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStructRP<StraightConnectorViewModel, double>(ViewModel.Value, "P2Y",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<StraightConnectorViewModel, int>(ViewModel.Value, "ZIndex",
                HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<StraightConnectorViewModel, double>(ViewModel.Value,
            "EdgeThickness", HorizontalAlignment.Stretch,
            new[]
            {
                0.0,
                1.0,
                2.0,
                3.0,
                4.0,
                5.0,
                10.0,
                15.0,
                20.0,
                25.0,
                30.0,
                35.0,
                40.0,
                45.0,
                50.0,
                100.0
            }));
    }
}