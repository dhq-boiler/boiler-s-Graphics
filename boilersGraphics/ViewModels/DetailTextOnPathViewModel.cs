using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels;

/// <summary>
/// プロパティダイアログ拡充: TextOnPathBlockViewModel 用の Detail ダイアログ ViewModel。
/// TextElementBase 共通プロパティ + TextOnPath 固有 (StartOffset/Spacing/Side/Rotation)。
/// PathReferenceId (Nullable&lt;Guid&gt;) は PropertyOption の型サポート外なので省略 (canvas
/// 上で PolyBezier を選んで紐付けする経路は維持)。
/// </summary>
public class DetailTextOnPathViewModel : DetailViewModelBase<TextOnPathBlockViewModel>
{
    public DetailTextOnPathViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        // TextOnPath 固有
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "StartOffset", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "Spacing", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, TextOnPathSide>(
            ViewModel.Value, "Side", new[]
            {
                TextOnPathSide.Above,
                TextOnPathSide.On,
                TextOnPathSide.Below,
            }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, TextOnPathRotation>(
            ViewModel.Value, "Rotation", new[]
            {
                TextOnPathRotation.Tangent,
                TextOnPathRotation.Upright,
            }));

        // TextElementBase 共通
        Properties.Add(new PropertyOptionsValueCombinationClass<TextOnPathBlockViewModel, string>(
            ViewModel.Value, "Text", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextOnPathBlockViewModel, string>(
            ViewModel.Value, "FontFamily", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, int>(
            ViewModel.Value, "FontSize", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextOnPathBlockViewModel, Brush>(
            ViewModel.Value, "Foreground", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextOnPathBlockViewModel, Brush>(
            ViewModel.Value, "Background", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "LetterSpacing", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "TextOpacity", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, bool>(
            ViewModel.Value, "IsWordWrap", HorizontalAlignment.Left));

        // 配置・装飾
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "Left", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "Top", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "Width", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "Height", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, int>(
            ViewModel.Value, "ZIndex", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextOnPathBlockViewModel, double>(
            ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
    }
}
