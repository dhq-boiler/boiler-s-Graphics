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
}
