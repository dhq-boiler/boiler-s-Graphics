using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace boilersGraphics.Models;

public class FontFamilyEx : FontFamily
{
    public FontFamilyEx(FontFamily fontFamily)
        : base(new Uri(fontFamily.Source), GetFamilyName(fontFamily.FamilyNames))
    {
    }

    public FontFamilyEx(string familyName)
        : base(familyName)
    {
    }

    public string FamilyName => GetFamilyName(FamilyNames);

    private static string GetFamilyName(LanguageSpecificStringDictionary familyNames)
    {
        if (familyNames.ContainsKey(XmlLanguage.GetLanguage("ja-jp")))
            return familyNames[XmlLanguage.GetLanguage("ja-jp")];
        return familyNames[XmlLanguage.GetLanguage("en-US")];
    }
}