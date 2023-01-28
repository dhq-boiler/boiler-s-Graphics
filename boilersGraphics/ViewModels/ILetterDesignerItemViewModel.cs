using boilersGraphics.Models;
using Reactive.Bindings;

namespace boilersGraphics.ViewModels;

internal interface ILetterDesignerItemViewModel
{
    ReactivePropertySlim<bool> LetterSettingDialogIsOpen { get; }
    ReactivePropertySlim<string> LetterString { get; }
    ReactivePropertySlim<FontFamilyEx> SelectedFontFamily { get; }
    ReactivePropertySlim<bool> IsBold { get; }
    ReactivePropertySlim<bool> IsItalic { get; }
    ReactivePropertySlim<int> FontSize { get; }
    ReactivePropertySlim<bool> IsAutoLineBreak { get; }

    void CloseLetterSettingDialog();
}