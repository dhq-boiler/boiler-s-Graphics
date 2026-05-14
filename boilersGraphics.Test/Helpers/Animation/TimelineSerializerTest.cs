using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation;

[TestFixture]
public class TimelineSerializerTest
{
    [Test]
    public void SerializeTimeline_empty_has_basic_structure()
    {
        var tl = new TimelineViewModel();
        var xml = TimelineSerializer.SerializeTimeline(tl);
        Assert.That(xml.Name.LocalName, Is.EqualTo("Timeline"));
        Assert.That(xml.Element("Duration")?.Value, Is.EqualTo("0"));
        Assert.That(xml.Element("Fps")?.Value, Is.EqualTo("30"));
        Assert.That(xml.Element("Tracks")?.Elements("Track").Count(), Is.EqualTo(0));
    }

    [Test]
    public void SerializeTimeline_null_returns_empty_Timeline_element()
    {
        var xml = TimelineSerializer.SerializeTimeline(null);
        Assert.That(xml, Is.Not.Null);
        Assert.That(xml.Name.LocalName, Is.EqualTo("Timeline"));
    }

    [Test]
    public void Roundtrip_empty_timeline_preserves_basic_fields()
    {
        var tl = new TimelineViewModel(duration: 5.0, fps: 30);
        tl.PlayRangeStart.Value = 1.0;
        tl.PlayRangeEnd.Value = 4.0;
        tl.Loop.Value = true;

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);

        Assert.That(restored.Duration.Value, Is.EqualTo(5.0));
        Assert.That(restored.Fps.Value, Is.EqualTo(30));
        Assert.That(restored.PlayRangeStart.Value, Is.EqualTo(1.0));
        Assert.That(restored.PlayRangeEnd.Value, Is.EqualTo(4.0));
        Assert.That(restored.Loop.Value, Is.True);
    }

    [Test]
    public void Roundtrip_double_track_preserves_keyframes_and_easing()
    {
        var itemId = Guid.NewGuid();
        var pref = new PropertyRef(itemId, "Left.Value", AnimatedValueType.Double);
        var track = new AnimationTrack(pref);
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(2.0, 100.0, EasingKind.CubicEase, EasingMode.EaseInOut));

        var tl = new TimelineViewModel(duration: 3.0, fps: 30);
        tl.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);

        Assert.That(restored.Tracks.Count, Is.EqualTo(1));
        var rtrack = restored.Tracks[0];
        Assert.That(rtrack.Target.ItemId, Is.EqualTo(itemId));
        Assert.That(rtrack.Target.PropertyPath, Is.EqualTo("Left.Value"));
        Assert.That(rtrack.Target.ValueType, Is.EqualTo(AnimatedValueType.Double));
        Assert.That(rtrack.Keyframes.Count, Is.EqualTo(2));

        Assert.That(rtrack.Keyframes[0].Time.Value, Is.EqualTo(0.0));
        Assert.That(rtrack.Keyframes[0].Value.Value, Is.EqualTo(0.0));
        Assert.That(rtrack.Keyframes[0].Easing.Value, Is.EqualTo(EasingKind.LinearEase));
        Assert.That(rtrack.Keyframes[0].Mode.Value, Is.EqualTo(EasingMode.EaseIn));

        Assert.That(rtrack.Keyframes[1].Time.Value, Is.EqualTo(2.0));
        Assert.That(rtrack.Keyframes[1].Value.Value, Is.EqualTo(100.0));
        Assert.That(rtrack.Keyframes[1].Easing.Value, Is.EqualTo(EasingKind.CubicEase));
        Assert.That(rtrack.Keyframes[1].Mode.Value, Is.EqualTo(EasingMode.EaseInOut));
    }

    [Test]
    public void Roundtrip_int_value_preserves_42()
    {
        var pref = new PropertyRef(Guid.NewGuid(), "Count.Value", AnimatedValueType.Int);
        var track = new AnimationTrack(pref);
        track.Keyframes.Add(new Keyframe(0.0, 42, EasingKind.LinearEase, EasingMode.EaseIn));

        var tl = new TimelineViewModel(1.0, 30);
        tl.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);
        Assert.That(restored.Tracks[0].Keyframes[0].Value.Value, Is.EqualTo(42));
    }

    [Test]
    public void Roundtrip_boolean_value_preserves_true()
    {
        var pref = new PropertyRef(Guid.NewGuid(), "IsVisible.Value", AnimatedValueType.Boolean);
        var track = new AnimationTrack(pref);
        track.Keyframes.Add(new Keyframe(0.0, true, EasingKind.LinearEase, EasingMode.EaseIn));

        var tl = new TimelineViewModel(1.0, 30);
        tl.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);
        Assert.That(restored.Tracks[0].Keyframes[0].Value.Value, Is.EqualTo(true));
    }

    [Test]
    public void Roundtrip_string_value_preserves_text()
    {
        var pref = new PropertyRef(Guid.NewGuid(), "Label.Value", AnimatedValueType.String);
        var track = new AnimationTrack(pref);
        track.Keyframes.Add(new Keyframe(0.0, "hello", EasingKind.LinearEase, EasingMode.EaseIn));

        var tl = new TimelineViewModel(1.0, 30);
        tl.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);
        Assert.That(restored.Tracks[0].Keyframes[0].Value.Value, Is.EqualTo("hello"));
    }

    [Test]
    public void Roundtrip_color_value_preserves_argb()
    {
        var pref = new PropertyRef(Guid.NewGuid(), "EdgeBrush.Value", AnimatedValueType.Color);
        var track = new AnimationTrack(pref);
        var color = Color.FromArgb(255, 100, 200, 50);
        track.Keyframes.Add(new Keyframe(0.0, color, EasingKind.LinearEase, EasingMode.EaseIn));

        var tl = new TimelineViewModel(1.0, 30);
        tl.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);
        var restoredColor = (Color)restored.Tracks[0].Keyframes[0].Value.Value;
        Assert.That(restoredColor, Is.EqualTo(color));
    }

    [Test]
    public void Roundtrip_point_value_preserves_xy()
    {
        var pref = new PropertyRef(Guid.NewGuid(), "Foo.Value", AnimatedValueType.Point);
        var track = new AnimationTrack(pref);
        var pt = new Point(123.45, 678.90);
        track.Keyframes.Add(new Keyframe(0.0, pt, EasingKind.LinearEase, EasingMode.EaseIn));

        var tl = new TimelineViewModel(1.0, 30);
        tl.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);
        var restoredPt = (Point)restored.Tracks[0].Keyframes[0].Value.Value;
        Assert.That(restoredPt.X, Is.EqualTo(123.45).Within(0.001));
        Assert.That(restoredPt.Y, Is.EqualTo(678.90).Within(0.001));
    }

    [Test]
    public void Roundtrip_brush_value_preserves_solid_color()
    {
        var pref = new PropertyRef(Guid.NewGuid(), "FillBrush.Value", AnimatedValueType.Brush);
        var track = new AnimationTrack(pref);
        var brush = new SolidColorBrush(Color.FromRgb(50, 150, 250));
        track.Keyframes.Add(new Keyframe(0.0, brush, EasingKind.LinearEase, EasingMode.EaseIn));

        var tl = new TimelineViewModel(1.0, 30);
        tl.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(tl);
        var restored = TimelineSerializer.DeserializeTimeline(xml);
        var restoredBrush = restored.Tracks[0].Keyframes[0].Value.Value as SolidColorBrush;
        Assert.That(restoredBrush, Is.Not.Null);
        Assert.That(restoredBrush!.Color.R, Is.EqualTo(50));
        Assert.That(restoredBrush.Color.G, Is.EqualTo(150));
        Assert.That(restoredBrush.Color.B, Is.EqualTo(250));
    }

    [Test]
    public void DeserializeTimeline_null_or_wrong_element_returns_empty()
    {
        var restored = TimelineSerializer.DeserializeTimeline(null);
        Assert.That(restored.IsEmpty, Is.True);

        var wrong = new System.Xml.Linq.XElement("WrongName");
        var restored2 = TimelineSerializer.DeserializeTimeline(wrong);
        Assert.That(restored2.IsEmpty, Is.True);
    }
}
