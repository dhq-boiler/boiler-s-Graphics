using grapher.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public class LetterVerticalSettingViewModel : BindableBase, IDialogAware
    {
        private LetterVerticalDesignerItemViewModel _ViewModel;
        private ObservableCollection<FontFamilyEx> _FontFamilies;

        public LetterVerticalDesignerItemViewModel ViewModel
        {
            get { return _ViewModel; }
            set { SetProperty(ref _ViewModel, value); }
        }

        public ObservableCollection<FontFamilyEx> FontFamilies
        {
            get { return _FontFamilies; }
            set { SetProperty(ref _FontFamilies, value); }
        }

        public LetterVerticalSettingViewModel()
        {
            var fontFamilies = Fonts.GetFontFamilies("C:\\Windows\\Fonts");
            FontFamilies = new ObservableCollection<FontFamilyEx>(fontFamilies.Select(x => new FontFamilyEx(x)));
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
            ViewModel.LetterSettingDialogIsOpen = false;
            ViewModel.IsSelected = false;
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
}
