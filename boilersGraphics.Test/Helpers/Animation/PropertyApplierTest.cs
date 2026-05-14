using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation;

[TestFixture]
public class PropertyApplierTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_Left_Value_writes_to_DesignerItem_Left()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, "Left.Value", 123.5);
        Assert.That(ok, Is.True);
        Assert.That(rect.Left.Value, Is.EqualTo(123.5));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_Top_Value_writes_to_DesignerItem_Top()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, "Top.Value", 99.0);
        Assert.That(ok, Is.True);
        Assert.That(rect.Top.Value, Is.EqualTo(99.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_Width_and_Height_Value_write_to_DesignerItem()
    {
        var rect = new NRectangleViewModel();
        PropertyApplier.Apply(rect, "Width.Value", 200.0);
        PropertyApplier.Apply(rect, "Height.Value", 100.0);
        Assert.That(rect.Width.Value, Is.EqualTo(200.0));
        Assert.That(rect.Height.Value, Is.EqualTo(100.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_RotationAngle_Value_writes_to_Selectable()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, "RotationAngle.Value", 45.0);
        Assert.That(ok, Is.True);
        Assert.That(rect.RotationAngle.Value, Is.EqualTo(45.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_EdgeBrush_Value_writes_brush()
    {
        var rect = new NRectangleViewModel();
        var brush = new SolidColorBrush(Colors.Red);
        var ok = PropertyApplier.Apply(rect, "EdgeBrush.Value", brush);
        Assert.That(ok, Is.True);
        Assert.That(rect.EdgeBrush.Value, Is.SameAs(brush));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_FillBrush_Value_writes_brush()
    {
        var rect = new NRectangleViewModel();
        var brush = new SolidColorBrush(Colors.Blue);
        var ok = PropertyApplier.Apply(rect, "FillBrush.Value", brush);
        Assert.That(ok, Is.True);
        Assert.That(rect.FillBrush.Value, Is.SameAs(brush));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_EdgeThickness_Value_writes_double()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, "EdgeThickness.Value", 3.5);
        Assert.That(ok, Is.True);
        Assert.That(rect.EdgeThickness.Value, Is.EqualTo(3.5));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_GlowRadius_GlowIntensity_GlowColor_all_write()
    {
        var rect = new NRectangleViewModel();
        PropertyApplier.Apply(rect, "GlowRadius.Value", 10.0);
        PropertyApplier.Apply(rect, "GlowIntensity.Value", 0.7);
        PropertyApplier.Apply(rect, "GlowColor.Value", Colors.Magenta);

        Assert.That(rect.GlowRadius.Value, Is.EqualTo(10.0));
        Assert.That(rect.GlowIntensity.Value, Is.EqualTo(0.7));
        Assert.That(rect.GlowColor.Value, Is.EqualTo(Colors.Magenta));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_unknown_PropertyPath_returns_false_without_throwing()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, "NonExistent.Value", 1.0);
        Assert.That(ok, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_null_item_returns_false()
    {
        var ok = PropertyApplier.Apply(null, "Left.Value", 1.0);
        Assert.That(ok, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_null_value_returns_false()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, "Left.Value", null);
        Assert.That(ok, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_empty_path_returns_false()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, string.Empty, 1.0);
        Assert.That(ok, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_ExposedProperties_with_invalid_guid_returns_false()
    {
        var part = new PartInstanceViewModel();
        var ok = PropertyApplier.Apply(part, "ExposedProperties[not-a-guid]", 42.0);
        Assert.That(ok, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_ExposedProperties_on_non_PartInstance_returns_false()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.Apply(rect, $"ExposedProperties[{Guid.NewGuid()}]", 42.0);
        Assert.That(ok, Is.False);
    }

    // ----- TryGet (Phase 5-e-1) -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_Left_Value_returns_current_Left()
    {
        var rect = new NRectangleViewModel();
        rect.Left.Value = 77.5;
        var ok = PropertyApplier.TryGet(rect, "Left.Value", out var v);
        Assert.That(ok, Is.True);
        Assert.That(v, Is.EqualTo(77.5));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_Top_Width_Height_returns_current_values()
    {
        var rect = new NRectangleViewModel();
        rect.Top.Value = 11.0;
        rect.Width.Value = 22.0;
        rect.Height.Value = 33.0;

        Assert.That(PropertyApplier.TryGet(rect, "Top.Value", out var top), Is.True);
        Assert.That(top, Is.EqualTo(11.0));
        Assert.That(PropertyApplier.TryGet(rect, "Width.Value", out var w), Is.True);
        Assert.That(w, Is.EqualTo(22.0));
        Assert.That(PropertyApplier.TryGet(rect, "Height.Value", out var h), Is.True);
        Assert.That(h, Is.EqualTo(33.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_RotationAngle_EdgeThickness_GlowRadius_GlowIntensity()
    {
        var rect = new NRectangleViewModel();
        rect.RotationAngle.Value = 30.0;
        rect.EdgeThickness.Value = 3.0;
        rect.GlowRadius.Value = 5.0;
        rect.GlowIntensity.Value = 0.7;

        Assert.That(PropertyApplier.TryGet(rect, "RotationAngle.Value", out var r), Is.True);
        Assert.That(r, Is.EqualTo(30.0));
        Assert.That(PropertyApplier.TryGet(rect, "EdgeThickness.Value", out var t), Is.True);
        Assert.That(t, Is.EqualTo(3.0));
        Assert.That(PropertyApplier.TryGet(rect, "GlowRadius.Value", out var gr), Is.True);
        Assert.That(gr, Is.EqualTo(5.0));
        Assert.That(PropertyApplier.TryGet(rect, "GlowIntensity.Value", out var gi), Is.True);
        Assert.That(gi, Is.EqualTo(0.7));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_EdgeBrush_FillBrush_returns_brush()
    {
        var rect = new NRectangleViewModel();
        var edge = new SolidColorBrush(Colors.Red);
        var fill = new SolidColorBrush(Colors.Blue);
        rect.EdgeBrush.Value = edge;
        rect.FillBrush.Value = fill;

        Assert.That(PropertyApplier.TryGet(rect, "EdgeBrush.Value", out var eb), Is.True);
        Assert.That(eb, Is.SameAs(edge));
        Assert.That(PropertyApplier.TryGet(rect, "FillBrush.Value", out var fb), Is.True);
        Assert.That(fb, Is.SameAs(fill));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_未対応プロパティパス_returns_false()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.TryGet(rect, "NoSuchProperty", out var v);
        Assert.That(ok, Is.False);
        Assert.That(v, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_item_null_returns_false()
    {
        var ok = PropertyApplier.TryGet(null, "Left.Value", out var v);
        Assert.That(ok, Is.False);
        Assert.That(v, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_path_null_or_empty_returns_false()
    {
        var rect = new NRectangleViewModel();
        Assert.That(PropertyApplier.TryGet(rect, null, out _), Is.False);
        Assert.That(PropertyApplier.TryGet(rect, "", out _), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TryGet_ExposedProperties_on_non_PartInstance_returns_false()
    {
        var rect = new NRectangleViewModel();
        var ok = PropertyApplier.TryGet(rect, $"ExposedProperties[{Guid.NewGuid()}]", out var v);
        Assert.That(ok, Is.False);
    }
}
