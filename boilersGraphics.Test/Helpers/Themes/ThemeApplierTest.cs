using boilersGraphics.Helpers.Themes;
using boilersGraphics.Models.Themes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Themes;

[TestFixture]
public class ThemeApplierTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void ToSolidColorBrush_BladerunnerのprimaryはFF5733()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Bladerunner");
        var brush = ThemeApplier.ToSolidColorBrush(theme, SemanticSlotKeys.Primary);
        Assert.That(brush, Is.Not.Null);
        Assert.That(brush.Color, Is.EqualTo((Color)ColorConverter.ConvertFromString("#FF5733")));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToSolidColorBrush_Frozen()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Matrix");
        var brush = ThemeApplier.ToSolidColorBrush(theme, SemanticSlotKeys.Primary);
        Assert.That(brush.IsFrozen, Is.True);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToSolidColorBrush_未登録スロットはnull()
    {
        var theme = new Theme();
        theme.Palette.Colors.Add(Colors.Red);
        // SemanticSlots に何も登録していない
        var brush = ThemeApplier.ToSolidColorBrush(theme, SemanticSlotKeys.Primary);
        Assert.That(brush, Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToSolidColorBrush_テーマnullはnull()
    {
        var brush = ThemeApplier.ToSolidColorBrush(null, SemanticSlotKeys.Primary);
        Assert.That(brush, Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveBrushes_EdgeOnlyはfillがnull()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Bladerunner");
        var (edge, fill) = ThemeApplier.ResolveBrushes(theme, ThemeApplyTarget.EdgeOnly);
        Assert.That(edge, Is.Not.Null);
        Assert.That(fill, Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveBrushes_FillOnlyはedgeがnull()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Bladerunner");
        var (edge, fill) = ThemeApplier.ResolveBrushes(theme, ThemeApplyTarget.FillOnly);
        Assert.That(edge, Is.Null);
        Assert.That(fill, Is.Not.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveBrushes_Bothで両方非null_Edgeはprimary_Fillはbackground()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Bladerunner");
        var (edge, fill) = ThemeApplier.ResolveBrushes(theme, ThemeApplyTarget.Both);
        Assert.That(edge.Color, Is.EqualTo((Color)ColorConverter.ConvertFromString("#FF5733")));   // primary
        Assert.That(fill.Color, Is.EqualTo((Color)ColorConverter.ConvertFromString("#0A0303")));   // background
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveScope_SelectedItemsはselectedを返す()
    {
        var selected = new List<int> { 1, 2 };
        var activeLayer = new List<int> { 1, 2, 3 };
        var all = new List<int> { 1, 2, 3, 4, 5 };
        var result = ThemeApplier.ResolveScope(ThemeApplyScope.SelectedItems, selected, activeLayer, all);
        Assert.That(result, Is.EqualTo(selected));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveScope_ActiveLayerはactiveLayerを返す()
    {
        var selected = new List<int> { 1, 2 };
        var activeLayer = new List<int> { 1, 2, 3 };
        var all = new List<int> { 1, 2, 3, 4, 5 };
        var result = ThemeApplier.ResolveScope(ThemeApplyScope.ActiveLayer, selected, activeLayer, all);
        Assert.That(result, Is.EqualTo(activeLayer));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveScope_EntireProjectはallを返す()
    {
        var selected = new List<int> { 1, 2 };
        var activeLayer = new List<int> { 1, 2, 3 };
        var all = new List<int> { 1, 2, 3, 4, 5 };
        var result = ThemeApplier.ResolveScope(ThemeApplyScope.EntireProject, selected, activeLayer, all);
        Assert.That(result, Is.EqualTo(all));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveScope_nullリストは空リストを返す()
    {
        var result = ThemeApplier.ResolveScope<int>(ThemeApplyScope.SelectedItems, null, null, null);
        Assert.That(result, Is.Empty);
    }
}
