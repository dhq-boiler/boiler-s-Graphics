using boilersGraphics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.ViewModels
{
    interface ILetterDesignerItemViewModel
    {
        bool LetterSettingDialogIsOpen { get; set; }
        string LetterString { get; set; }
        FontFamilyEx SelectedFontFamily { get; set; }
        bool IsBold { get; set; }
        bool IsItalic { get; set; }
        int FontSize { get; set; }
        bool AutoLineBreak { get; set; }

        void CloseLetterSettingDialog();
    }
}
