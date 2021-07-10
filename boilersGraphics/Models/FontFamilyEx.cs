using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace boilersGraphics.Models
{
    public class FontFamilyEx : FontFamily
    {
        public FontFamilyEx(FontFamily fontFamily)
            : base(new Uri(fontFamily.Source), GetFamilyName(fontFamily.FamilyNames))
        {
        }

        public FontFamilyEx(string familyName)
            : base(familyName)
        { }

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
