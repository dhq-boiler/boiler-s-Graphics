using boilersGraphics.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.ViewModels;

public class LetterVerticalSettingViewModel : BindableBase, IDialogAware
{
    private ObservableCollection<FontFamilyEx> _FontFamilies;
    private LetterVerticalDesignerItemViewModel _ViewModel;

    public LetterVerticalSettingViewModel()
    {
        var fontFamilies = Fonts.GetFontFamilies("C:\\Windows\\Fonts");
        FontFamilies = new ObservableCollection<FontFamilyEx>(fontFamilies.AsValueEnumerable().Select(x => new FontFamilyEx(x)).ToArray());
    }

    public LetterVerticalDesignerItemViewModel ViewModel
    {
        get => _ViewModel;
        set => SetProperty(ref _ViewModel, value);
    }

    public ObservableCollection<FontFamilyEx> FontFamilies
    {
        get => _FontFamilies;
        set => SetProperty(ref _FontFamilies, value);
    }

    public string Title => "レタリング";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
        ViewModel.LetterSettingDialogClose -= ViewModel_LetterSettingDialogClose;
        ViewModel.LetterSettingDialogIsOpen.Value = false;
        ViewModel.IsSelected.Value = false;
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        ViewModel = parameters.GetValue<LetterVerticalDesignerItemViewModel>("ViewModel");
        ViewModel.LetterSettingDialogClose += ViewModel_LetterSettingDialogClose;
    }

    private void ViewModel_LetterSettingDialogClose(object sender, EventArgs e)
    {
        IDialogResult result = new DialogResult(ButtonResult.OK);
        RequestClose.Invoke(result);
    }
}