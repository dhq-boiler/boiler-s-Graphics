using boilersGraphics.Models.Connectors;
using boilersGraphics.ViewModels.Connectors;
using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels;

/// <summary>
/// プロパティダイアログ拡充: OrthogonalConnectorViewModel 用の Detail ダイアログ ViewModel。
/// 編集可能: RoutingMode (Auto/HFirst/VFirst/Manual), CornerRadius, BeginAnchorRef / EndAnchorRef,
///         BeginPoint / EndPoint, EdgeThickness, StrokeLineJoin, StrokeDashArray, ZIndex, RotationAngle
/// 読み取り専用: PathGeometry (ReadOnly Class)
/// MidPoints (ObservableCollection&lt;Point&gt;) は型サポート外なので非表示 (Manual モードのときキャンバス上で操作)。
/// </summary>
public class DetailOrthogonalConnectorViewModel : DetailViewModelBase<OrthogonalConnectorViewModel>
{
    public DetailOrthogonalConnectorViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, OrthogonalRoutingMode>(
            ViewModel.Value, "RoutingMode", new[]
            {
                OrthogonalRoutingMode.Auto,
                OrthogonalRoutingMode.HFirst,
                OrthogonalRoutingMode.VFirst,
                OrthogonalRoutingMode.Manual,
            }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, double>(
            ViewModel.Value, "CornerRadius", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<OrthogonalConnectorViewModel, string>(
            ViewModel.Value, "BeginAnchorRef", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationClass<OrthogonalConnectorViewModel, string>(
            ViewModel.Value, "EndAnchorRef", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, Point>(
            ViewModel.Value, "BeginPoint", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, Point>(
            ViewModel.Value, "EndPoint", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, double>(
            ViewModel.Value, "EdgeThickness", HorizontalAlignment.Stretch,
            new[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 45.0, 50.0, 100.0 }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, PenLineJoin>(
            ViewModel.Value, "StrokeLineJoin", new[]
            {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round,
            }));
        Properties.Add(new PropertyOptionsValueCombinationClass<OrthogonalConnectorViewModel, DoubleCollection>(
            ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, int>(
            ViewModel.Value, "ZIndex", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<OrthogonalConnectorViewModel, double>(
            ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
    }
}
