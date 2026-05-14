using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels;

/// <summary>
/// プロパティダイアログ拡充: TextMatrixBlockViewModel 用の Detail ダイアログ ViewModel。
/// TextElementBase 共通プロパティ (Text/FontFamily/FontSize/Foreground/Background/LetterSpacing/
/// TextOpacity/IsWordWrap) + TextMatrix 固有 (Rows/Columns/CellMode/Separator/SequenceStart/
/// SequenceFormat/DataGenType/DataGenSeed/CustomItems) を編集可能。
/// LineHeight (Nullable&lt;double&gt;) は PropertyOption の型サポート外なので省略。
/// </summary>
public class DetailTextMatrixViewModel : DetailViewModelBase<TextMatrixBlockViewModel>
{
    public DetailTextMatrixViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        // TextMatrix 固有
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, int>(
            ViewModel.Value, "Rows", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, int>(
            ViewModel.Value, "Columns", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, TextMatrixCellMode>(
            ViewModel.Value, "CellMode", new[]
            {
                TextMatrixCellMode.Sequential,
                TextMatrixCellMode.DataGenerator,
                TextMatrixCellMode.CustomList,
            }));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextMatrixBlockViewModel, string>(
            ViewModel.Value, "Separator", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, int>(
            ViewModel.Value, "SequenceStart", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextMatrixBlockViewModel, string>(
            ViewModel.Value, "SequenceFormat", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, DataGeneratorType>(
            ViewModel.Value, "DataGenType", new[]
            {
                DataGeneratorType.Hex,
                DataGeneratorType.Binary,
                DataGeneratorType.Ipv4Address,
                DataGeneratorType.Ipv6Address,
                DataGeneratorType.Uuid,
                DataGeneratorType.Timestamp,
                DataGeneratorType.RandomCode,
                DataGeneratorType.LogLine,
            }));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, int>(
            ViewModel.Value, "DataGenSeed", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextMatrixBlockViewModel, string>(
            ViewModel.Value, "CustomItems", HorizontalAlignment.Left));

        // TextElementBase 共通
        Properties.Add(new PropertyOptionsValueCombinationClass<TextMatrixBlockViewModel, string>(
            ViewModel.Value, "Text", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextMatrixBlockViewModel, string>(
            ViewModel.Value, "FontFamily", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, int>(
            ViewModel.Value, "FontSize", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextMatrixBlockViewModel, Brush>(
            ViewModel.Value, "Foreground", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationClass<TextMatrixBlockViewModel, Brush>(
            ViewModel.Value, "Background", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, double>(
            ViewModel.Value, "LetterSpacing", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, double>(
            ViewModel.Value, "TextOpacity", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, bool>(
            ViewModel.Value, "IsWordWrap", HorizontalAlignment.Left));

        // 配置・装飾
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, double>(
            ViewModel.Value, "Left", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, double>(
            ViewModel.Value, "Top", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, double>(
            ViewModel.Value, "Width", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, double>(
            ViewModel.Value, "Height", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, int>(
            ViewModel.Value, "ZIndex", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<TextMatrixBlockViewModel, double>(
            ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
    }
}
