using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;
using R3;

namespace boilersGraphics.Helpers;

public class ToolItemData : BindableBase
{
    private bool _IsChecked;

    public ToolItemData(string name, string imageUrl, ICommand command)
    {
        Name.Value = name;
        ImageUrl = imageUrl;
        Command = command;
    }

    public ToolItemData(string name, string imageUrl, string tooltip, ICommand command)
    {
        Name.Value = name;
        ImageUrl = imageUrl;
        Command = command;
        Tooltip.Value = tooltip;
    }

    //public string Name { get; private set; }
    public BindableReactiveProperty<string> Name { get; } = new();
    public string ImageUrl { get; }
    public ICommand Command { get; }
    public BindableReactiveProperty<string> Tooltip { get; } = new();

    // 排他グループ。null の場合は独立トグル（ToolItems2 用）。
    // 設定すると、IsChecked=true セット時に同グループ内の他項目を自動で false にする。
    // これにより、UIA Toggle Pattern など Command を経由しない経路でも排他制御が効く。
    internal IEnumerable<ToolItemData> ExclusiveGroup { get; set; }

    public bool IsChecked
    {
        get => _IsChecked;
        set
        {
            if (!SetProperty(ref _IsChecked, value)) return;
            if (value && ExclusiveGroup != null)
            {
                foreach (var other in ExclusiveGroup)
                {
                    if (!ReferenceEquals(other, this) && other.IsChecked)
                        other.IsChecked = false;
                }
            }
        }
    }
}