using boilersGraphics.ViewModels.Connectors;
using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels;

/// <summary>
/// プロパティダイアログ拡充: AnchorBezierConnectorViewModel 用の Detail ダイアログ ViewModel。
/// 編集可能: BeginPoint / EndPoint / BeginControlPoint / EndControlPoint,
///         BeginAnchorRef / EndAnchorRef, EdgeThickness, StrokeLineJoin, StrokeDashArray,
///         ZIndex, RotationAngle
/// </summary>
public class DetailAnchorBezierConnectorViewModel : DetailViewModelBase<AnchorBezierConnectorViewModel>
{
    public DetailAnchorBezierConnectorViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, Point>(
            ViewModel.Value, "BeginPoint", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, Point>(
            ViewModel.Value, "EndPoint", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, Point>(
            ViewModel.Value, "BeginControlPoint", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, Point>(
            ViewModel.Value, "EndControlPoint", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<AnchorBezierConnectorViewModel, string>(
            ViewModel.Value, "BeginAnchorRef", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationClass<AnchorBezierConnectorViewModel, string>(
            ViewModel.Value, "EndAnchorRef", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, double>(
            ViewModel.Value, "EdgeThickness", HorizontalAlignment.Stretch,
            new[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 45.0, 50.0, 100.0 }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, PenLineJoin>(
            ViewModel.Value, "StrokeLineJoin", new[]
            {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round,
            }));
        Properties.Add(new PropertyOptionsValueCombinationClass<AnchorBezierConnectorViewModel, DoubleCollection>(
            ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, int>(
            ViewModel.Value, "ZIndex", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorBezierConnectorViewModel, double>(
            ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
    }
}
