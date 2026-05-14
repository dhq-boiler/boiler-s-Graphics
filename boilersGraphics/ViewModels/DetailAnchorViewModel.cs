using boilersGraphics.ViewModels.Anchors;
using Prism.Regions;
using System;
using System.Windows;

namespace boilersGraphics.ViewModels;

/// <summary>
/// プロパティダイアログ拡充: AnchorViewModel 用の Detail ダイアログ ViewModel。
/// 編集可能: RelativeX / RelativeY / AnchorName / ZIndex
/// 読み取り専用: OwnerId / Left (派生) / Top (派生)
/// </summary>
public class DetailAnchorViewModel : DetailViewModelBase<AnchorViewModel>
{
    public DetailAnchorViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorViewModel, double>(ViewModel.Value,
            "RelativeX", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorViewModel, double>(ViewModel.Value,
            "RelativeY", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationClass<AnchorViewModel, string>(ViewModel.Value,
            "AnchorName", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationReadOnlyStruct<AnchorViewModel, Guid>(ViewModel.Value,
            "OwnerId", HorizontalAlignment.Left));
        Properties.Add(new PropertyOptionsValueCombinationReadOnlyStruct<AnchorViewModel, double>(ViewModel.Value,
            "Left", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationReadOnlyStruct<AnchorViewModel, double>(ViewModel.Value,
            "Top", HorizontalAlignment.Right));
        Properties.Add(new PropertyOptionsValueCombinationStruct<AnchorViewModel, int>(ViewModel.Value,
            "ZIndex", HorizontalAlignment.Right));
    }
}
