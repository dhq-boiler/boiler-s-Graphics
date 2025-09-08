using Prism.Mvvm;
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

    public bool IsChecked
    {
        get => _IsChecked;
        set => SetProperty(ref _IsChecked, value);
    }
}