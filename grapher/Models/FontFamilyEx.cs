using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace grapher.Models
{
    public class FontFamilyEx : FontFamily
    {
        private Uri _BaseUri;

        public FontFamilyEx(FontFamily fontFamily)
            : base(new Uri(fontFamily.Source), GetFamilyName(fontFamily.FamilyNames))
        {
            BaseUri = new Uri(fontFamily.Source);
        }

        public new Uri BaseUri
        {
            get { return base.BaseUri != null ? base.BaseUri : _BaseUri; }
            set { _BaseUri = value; }
        }

        public string FamilyName
        {
            get
            {
                return GetFamilyName(FamilyNames);
            }
        }

        private static string GetFamilyName(LanguageSpecificStringDictionary familyNames)
        {
            if (familyNames.ContainsKey(XmlLanguage.GetLanguage("ja-jp")))
            {
                return familyNames[XmlLanguage.GetLanguage("ja-jp")];
            }
            else
            {
                return familyNames[XmlLanguage.GetLanguage("en-US")];
            }
        }
    }
}
