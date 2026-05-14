using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Animation;

/// <summary>
/// Phase 5-d-1: DiagramViewModel への Timeline 統合と TimelineSerializer 経由のシリアライズ往復を検証する。
/// BuildSaveXElement は Renderer / DesignerCanvas 等の UI 環境を要求するため直接呼べない (Phase 4-f-2 と同じ事情)。
/// 代わりに TimelineSerializer を直接呼んで構造とラウンドトリップを確認する。
/// </summary>
[TestFixture]
public class TimelineIntegrationTest
{
    private static DiagramViewModel CreateDiagram()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Mock<IDialogService>();
        return new MainWindowViewModel(dlg.Object).DiagramViewModel;
    }

    [Test, Apartment(ApartmentState.STA)]
    public void DiagramViewModel_Timeline_default_is_empty()
    {
        var diagram = CreateDiagram();
        Assert.That(diagram.Timeline, Is.Not.Null);
        Assert.That(diagram.Timeline.IsEmpty, Is.True);
        Assert.That(diagram.Timeline.Tracks, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void DiagramViewModel_Timeline_default_Fps_is_30_and_Duration_is_0()
    {
        var diagram = CreateDiagram();
        Assert.That(diagram.Timeline.Fps.Value, Is.EqualTo(30));
        Assert.That(diagram.Timeline.Duration.Value, Is.EqualTo(0.0));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void SerializeTimeline_from_empty_DiagramTimeline_yields_default_Duration_zero()
    {
        var diagram = CreateDiagram();
        var xml = TimelineSerializer.SerializeTimeline(diagram.Timeline);
        Assert.That(xml.Name.LocalName, Is.EqualTo("Timeline"));
        Assert.That(xml.Element("Duration")?.Value, Is.EqualTo("0"));
        Assert.That(xml.Element("Fps")?.Value, Is.EqualTo("30"));
        Assert.That(xml.Element("Tracks")?.Elements("Track").Count(), Is.EqualTo(0));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void IsEmpty_becomes_false_when_track_added_and_true_again_when_cleared()
    {
        var diagram = CreateDiagram();
        Assert.That(diagram.Timeline.IsEmpty, Is.True);

        diagram.Timeline.Tracks.Add(new AnimationTrack(new PropertyRef(Guid.NewGuid(), "Left.Value", AnimatedValueType.Double)));
        Assert.That(diagram.Timeline.IsEmpty, Is.False);

        diagram.Timeline.Tracks.RemoveAt(0);
        diagram.Timeline.Duration.Value = 0.0;
        Assert.That(diagram.Timeline.IsEmpty, Is.True);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void SerializeTimeline_DiagramTimeline_with_tracks_RoundTrip()
    {
        var diagram = CreateDiagram();
        diagram.Timeline.Duration.Value = 5.0;
        diagram.Timeline.PlayRangeEnd.Value = 5.0;

        var itemId = Guid.NewGuid();
        var pref = new PropertyRef(itemId, "Top.Value", AnimatedValueType.Double);
        var track = new AnimationTrack(pref);
        track.Keyframes.Add(new Keyframe(0.0, 10.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(2.5, 200.0, EasingKind.CubicEase, EasingMode.EaseInOut));
        diagram.Timeline.Tracks.Add(track);

        var xml = TimelineSerializer.SerializeTimeline(diagram.Timeline);
        Assert.That(xml.Element("Duration")?.Value, Is.EqualTo("5"));
        Assert.That(xml.Element("Tracks")?.Elements("Track").Count(), Is.EqualTo(1));

        var restored = TimelineSerializer.DeserializeTimeline(xml);
        Assert.That(restored.Duration.Value, Is.EqualTo(5.0));
        Assert.That(restored.Tracks.Count, Is.EqualTo(1));
        var rtrack = restored.Tracks[0];
        Assert.That(rtrack.Target.ItemId, Is.EqualTo(itemId));
        Assert.That(rtrack.Target.PropertyPath, Is.EqualTo("Top.Value"));
        Assert.That(rtrack.Keyframes.Count, Is.EqualTo(2));
        Assert.That(rtrack.Keyframes[1].Easing.Value, Is.EqualTo(EasingKind.CubicEase));
        Assert.That(rtrack.Keyframes[1].Mode.Value, Is.EqualTo(EasingMode.EaseInOut));
    }
}
