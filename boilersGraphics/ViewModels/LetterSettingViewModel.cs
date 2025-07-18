using boilersGraphics.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.ViewModels;

public class LetterSettingViewModel : BindableBase, IDialogAware
{
    private ObservableCollection<FontFamilyEx> _FontFamilies;
    private LetterDesignerItemViewModel _ViewModel;

    public LetterSettingViewModel()
    {
        var fontFamilies = Fonts.GetFontFamilies("C:\\Windows\\Fonts");
        FontFamilies = new ObservableCollection<FontFamilyEx>(fontFamilies.AsValueEnumerable().Select(x => new FontFamilyEx(x)).ToArray());
    }

    public LetterDesignerItemViewModel ViewModel
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
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        ViewModel = parameters.GetValue<LetterDesignerItemViewModel>("ViewModel");
        ViewModel.LetterSettingDialogClose += ViewModel_LetterSettingDialogClose;
    }

    private void ViewModel_LetterSettingDialogClose(object sender, EventArgs e)
    {
        IDialogResult result = new DialogResult(ButtonResult.OK);
        RequestClose.Invoke(result);
    }
}