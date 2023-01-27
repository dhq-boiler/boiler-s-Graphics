using System.Windows;
using System.Windows.Media;
using Prism.Regions;

namespace boilersGraphics.ViewModels;

internal class DetailPolyBezierViewModel : DetailViewModelBase<PolyBezierViewModel>
{
    public DetailPolyBezierViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        Properties.Add(new PropertyOptionsValueCombinationStruct<PolyBezierViewModel, PenLineJoin>(ViewModel.Value,
            "StrokeLineJoin", new[]
            {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round
            }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<PolyBezierViewModel, PenLineCap>(ViewModel.Value,
            "StrokeStartLineCap", new[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle
            }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<PolyBezierViewModel, PenLineCap>(ViewModel.Value,
            "StrokeEndLineCap", new[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle
            }));
        Properties.Add(new PropertyOptionsValueCombinationClass<PolyBezierViewModel, DoubleCollection>(ViewModel.Value,
            "StrokeDashArray", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<PolyBezierViewModel, double>(ViewModel.Value,
            "StrokeMiterLimit", HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<PolyBezierViewModel, int>(ViewModel.Value, "ZIndex",
                HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<PolyBezierViewModel, double>(ViewModel.Value,
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