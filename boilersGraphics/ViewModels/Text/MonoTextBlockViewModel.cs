using boilersGraphics.Models.Text;
using System;

namespace boilersGraphics.ViewModels.Text;

/// <summary>
/// Phase 2-a §3.1 / §4.1: モノスペーステキストブロックの VM。
/// Phase 2-b 最小実装では追加プロパティなしで、TextElementBase の共通プロパティのみを扱う。
/// </summary>
public class MonoTextBlockViewModel : TextElementBaseViewModel
{
    public MonoTextBlockViewModel() : this(new MonoTextBlock())
    {
    }

    public MonoTextBlockViewModel(MonoTextBlock model) : base(model)
    {
    }

    public override bool IsResizable => true;

    public override bool SupportsPropertyDialog => false;

    public override object Clone()
    {
        var clone = new MonoTextBlockViewModel();
        CopyCommonPropertiesTo(clone);
        return clone;
    }
}
