using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public class LetterSettingViewModel : BindableBase, IDialogAware
    {
        private LetterDesignerItemViewModel _ViewModel;
        private ObservableCollection<FontFamily> _FontFamilies;

        public LetterDesignerItemViewModel ViewModel
        {
            get { return _ViewModel; }
            set { SetProperty(ref _ViewModel, value); }
        }

        public ObservableCollection<FontFamily> FontFamilies
        {
            get { return _FontFamilies; }
            set { SetProperty(ref _FontFamilies, value); }
        }

        public LetterSettingViewModel()
        {
            System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();
            FontFamilies = new ObservableCollection<FontFamily>();
            FontFamilies.AddRange(ifc.Families);
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
            ViewModel = parameters.GetValue<LetterDesignerItemViewModel>("ViewModel");
            ViewModel.LetterSettingDialogClose += ViewModel_LetterSettingDialogClose;
        }

        private void ViewModel_LetterSettingDialogClose(object sender, EventArgs e)
        {
            IDialogResult result = new DialogResult(ButtonResult.OK);
            RequestClose.Invoke(result);
        }
    }
}
