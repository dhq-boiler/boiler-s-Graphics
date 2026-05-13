using boilersGraphics.Models.Themes;
using NUnit.Framework;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test.Models.Themes;

[TestFixture]
public class ColorPaletteTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void GetSemanticColor_未登録スロットはnull()
    {
        var palette = new ColorPalette();
        palette.Colors.Add(Colors.Red);
        Assert.That(palette.GetSemanticColor(SemanticSlotKeys.Primary), Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void GetSemanticColor_登録スロットの色が返る()
    {
        var palette = new ColorPalette();
        palette.Colors.Add(Colors.Red);
        palette.Colors.Add(Colors.Green);
        palette.SemanticSlots[SemanticSlotKeys.Primary] = 0;
        palette.SemanticSlots[SemanticSlotKeys.Accent] = 1;
        Assert.That(palette.GetSemanticColor(SemanticSlotKeys.Primary), Is.EqualTo(Colors.Red));
        Assert.That(palette.GetSemanticColor(SemanticSlotKeys.Accent), Is.EqualTo(Colors.Green));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void GetSemanticColor_範囲外インデックスはnull()
    {
        var palette = new ColorPalette();
        palette.Colors.Add(Colors.Red);
        palette.SemanticSlots[SemanticSlotKeys.Primary] = 5;
        Assert.That(palette.GetSemanticColor(SemanticSlotKeys.Primary), Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void SemanticSlotKeysAll_5種類が宣言順に並ぶ()
    {
        Assert.That(SemanticSlotKeys.All, Is.EqualTo(new[]
        {
            SemanticSlotKeys.Primary,
            SemanticSlotKeys.Accent,
            SemanticSlotKeys.Warning,
            SemanticSlotKeys.Info,
            SemanticSlotKeys.Background,
        }));
    }
}
