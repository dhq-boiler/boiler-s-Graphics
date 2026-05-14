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

    [Test, Apartment(ApartmentState.STA)]
    public void CopyDashArray_新しいインスタンスを返す()
    {
        var ls = new LineStyle { StrokeDashArray = new System.Windows.Media.DoubleCollection { 4, 2 } };
        var copy = ThemeApplier.CopyDashArray(ls);
        Assert.That(copy, Is.Not.SameAs(ls.StrokeDashArray));
        Assert.That(copy, Is.EquivalentTo(new[] { 4.0, 2.0 }));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CopyDashArray_nullには空コレクション()
    {
        var copy = ThemeApplier.CopyDashArray(null);
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CopyDashArray_StrokeDashArrayがnullなら空コレクション()
    {
        var ls = new LineStyle { StrokeDashArray = null };
        var copy = ThemeApplier.CopyDashArray(ls);
        Assert.That(copy, Is.Not.Null);
        Assert.That(copy, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CopyDashArray_組込Steppedをコピーすると元と独立()
    {
        var stepped = ThemeRepository.CreateBuiltInLineStyles().First(ls => ls.Name == "Stepped");
        var copy = ThemeApplier.CopyDashArray(stepped);
        copy.Add(99);
        Assert.That(stepped.StrokeDashArray.Count, Is.EqualTo(4), "原本は触らない");
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveGlow_テーマnullは0_0_null()
    {
        var (r, i, c) = ThemeApplier.ResolveGlow(null);
        Assert.That(r, Is.EqualTo(0));
        Assert.That(i, Is.EqualTo(0));
        Assert.That(c, Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveGlow_Bladerunnerの値が返る()
    {
        var theme = ThemeRepository.CreateBuiltIn().First(t => t.Name == "Bladerunner");
        var (r, i, c) = ThemeApplier.ResolveGlow(theme);
        Assert.That(r, Is.EqualTo(6));
        Assert.That(i, Is.EqualTo(0.6).Within(1e-6));
        Assert.That(c, Is.EqualTo((Color)ColorConverter.ConvertFromString("#FF5733")));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveKernelSize_radius0は1()
    {
        Assert.That(ThemeApplier.ResolveKernelSize(0), Is.EqualTo(1));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveKernelSize_radius3は7()
    {
        // 3 * 2 + 1 = 7 (奇数)
        Assert.That(ThemeApplier.ResolveKernelSize(3), Is.EqualTo(7));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveKernelSize_負値は1にクランプ()
    {
        Assert.That(ThemeApplier.ResolveKernelSize(-5), Is.EqualTo(1));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ResolveKernelSize_radius2_5は四捨五入で2_2_1_5()
    {
        // Math.Round(2.5) = 2 (バンカーズラウンディング)、2 * 2 + 1 = 5
        Assert.That(ThemeApplier.ResolveKernelSize(2.5), Is.EqualTo(5));
    }
}
