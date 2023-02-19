using System.Windows;
using System.Windows.Media;
using Prism.Regions;

namespace boilersGraphics.ViewModels;

public class DetailColorCorrectViewModel : DetailViewModelBase<ColorCorrectViewModel>
{
    public DetailColorCorrectViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        Properties.Add(new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, PenLineJoin>(ViewModel.Value,
            "StrokeLineJoin", new[]
            {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round
            }));
        Properties.Add(new PropertyOptionsValueCombinationClass<ColorCorrectViewModel, DoubleCollection>(ViewModel.Value,
            "StrokeDashArray", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value,
            "StrokeMiterLimit", HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value, "Left",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value, "Top",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value, "Width",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value, "Height",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value, "CenterX",
                HorizontalAlignment.Right));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value, "CenterY",
                HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value,
            "RotationAngle", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<ColorCorrectViewModel, PathGeometry>(ViewModel.Value,
            "PathGeometryNoRotate", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationReadOnlyClass<ColorCorrectViewModel, PathGeometry>(ViewModel.Value,
            "PathGeometry", HorizontalAlignment.Left));
        Properties.Add(
            new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, int>(ViewModel.Value, "ZIndex",
                HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<ColorCorrectViewModel, double>(ViewModel.Value,
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