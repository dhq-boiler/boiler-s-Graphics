using boilersGraphics.Models;
using R3;

namespace boilersGraphics.ViewModels;

internal interface ILetterDesignerItemViewModel
{
    BindableReactiveProperty<bool> LetterSettingDialogIsOpen { get; }
    BindableReactiveProperty<string> LetterString { get; }
    BindableReactiveProperty<FontFamilyEx> SelectedFontFamily { get; }
    BindableReactiveProperty<bool> IsBold { get; }
    BindableReactiveProperty<bool> IsItalic { get; }
    BindableReactiveProperty<int> FontSize { get; }
    BindableReactiveProperty<bool> IsAutoLineBreak { get; }

    void CloseLetterSettingDialog();
}