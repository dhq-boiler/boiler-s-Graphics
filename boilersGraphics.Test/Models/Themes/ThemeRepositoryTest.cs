using boilersGraphics.Models.Themes;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test.Models.Themes;

[TestFixture]
public class ThemeRepositoryTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_4種類が返る()
    {
        var themes = ThemeRepository.CreateBuiltIn();
        Assert.That(themes, Has.Count.EqualTo(4));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_4種類の名前が定義どおり()
    {
        var names = ThemeRepository.CreateBuiltIn().Select(t => t.Name).ToArray();
        Assert.That(names, Is.EquivalentTo(new[] { "Bladerunner", "Matrix", "MedicalBlueWhite", "AmberCrt" }));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_全テーマがIsBuiltIn_True()
    {
        foreach (var theme in ThemeRepository.CreateBuiltIn())
        {
            Assert.That(theme.IsBuiltIn, Is.True, theme.Name);
            Assert.That(theme.Palette.IsBuiltIn, Is.True, theme.Name);
        }
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_全テーマがセマンティック5スロット全て割当()
    {
        foreach (var theme in ThemeRepository.CreateBuiltIn())
        {
            foreach (var slot in SemanticSlotKeys.All)
            {
                Assert.That(theme.Palette.SemanticSlots.ContainsKey(slot), Is.True,
                    $"{theme.Name} missing slot {slot}");
            }
        }
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_全テーマが順序色5色以上()
    {
        foreach (var theme in ThemeRepository.CreateBuiltIn())
        {
            Assert.That(theme.Palette.Colors.Count, Is.GreaterThanOrEqualTo(5), theme.Name);
        }
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_BladerunnerのprimaryはFF5733()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Bladerunner");
        var primary = theme.Palette.GetSemanticColor(SemanticSlotKeys.Primary);
        Assert.That(primary, Is.EqualTo((Color)ColorConverter.ConvertFromString("#FF5733")));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_Matrixのprimaryは00FF41()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Matrix");
        var primary = theme.Palette.GetSemanticColor(SemanticSlotKeys.Primary);
        Assert.That(primary, Is.EqualTo((Color)ColorConverter.ConvertFromString("#00FF41")));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_全テーマが組込線種6種を持つ()
    {
        foreach (var theme in ThemeRepository.CreateBuiltIn())
        {
            Assert.That(theme.LineStyles.Count, Is.EqualTo(6), theme.Name);
        }
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltInLineStyles_名前6種が定義どおり()
    {
        var names = ThemeRepository.CreateBuiltInLineStyles().Select(ls => ls.Name).ToArray();
        Assert.That(names, Is.EquivalentTo(new[] { "Solid", "Dash", "Dot", "DashDot", "LongDash", "Stepped" }));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltInLineStyles_SteppedはDash8_4_2_4()
    {
        var stepped = ThemeRepository.CreateBuiltInLineStyles().First(ls => ls.Name == "Stepped");
        Assert.That(stepped.StrokeDashArray, Is.EquivalentTo(new[] { 8.0, 4.0, 2.0, 4.0 }));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltInLineStyles_SolidはDashArray空()
    {
        var solid = ThemeRepository.CreateBuiltInLineStyles().First(ls => ls.Name == "Solid");
        Assert.That(solid.StrokeDashArray, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateBuiltIn_DefaultGlowが全テーマで設定されている()
    {
        foreach (var theme in ThemeRepository.CreateBuiltIn())
        {
            Assert.That(theme.DefaultGlow.Radius, Is.GreaterThan(0), theme.Name);
            Assert.That(theme.DefaultGlow.Intensity, Is.GreaterThan(0).And.LessThanOrEqualTo(1), theme.Name);
            Assert.That(theme.DefaultGlow.Color, Is.Not.Null, theme.Name);
        }
    }
}
