using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.ViewModels.Animation;

[TestFixture]
public class TimelineViewModelTest
{
    [Test]
    public void DefaultConstructor_initializes_to_empty()
    {
        var tl = new TimelineViewModel();
        Assert.That(tl.Duration.Value, Is.EqualTo(0.0));
        Assert.That(tl.Fps.Value, Is.EqualTo(30));
        Assert.That(tl.PlayRangeStart.Value, Is.EqualTo(0.0));
        Assert.That(tl.PlayRangeEnd.Value, Is.EqualTo(0.0));
        Assert.That(tl.Loop.Value, Is.False);
        Assert.That(tl.Now.Value, Is.EqualTo(0.0));
        Assert.That(tl.IsPlaying.Value, Is.False);
        Assert.That(tl.Tracks, Is.Empty);
        Assert.That(tl.IsEmpty, Is.True);
    }

    [Test]
    public void Parameterized_constructor_sets_duration_fps_and_PlayRangeEnd()
    {
        var tl = new TimelineViewModel(duration: 10.0, fps: 60);
        Assert.That(tl.Duration.Value, Is.EqualTo(10.0));
        Assert.That(tl.Fps.Value, Is.EqualTo(60));
        Assert.That(tl.PlayRangeEnd.Value, Is.EqualTo(10.0));
        Assert.That(tl.IsEmpty, Is.False);
    }

    [Test]
    public void IsEmpty_returns_false_when_Duration_is_set()
    {
        var tl = new TimelineViewModel();
        Assert.That(tl.IsEmpty, Is.True);

        tl.Duration.Value = 5.0;
        Assert.That(tl.IsEmpty, Is.False);
    }

    [Test]
    public void IsEmpty_returns_false_when_tracks_exist_even_if_Duration_is_zero()
    {
        var tl = new TimelineViewModel();
        tl.Tracks.Add(new AnimationTrack(new PropertyRef(Guid.NewGuid(), "Left.Value", AnimatedValueType.Double)));
        Assert.That(tl.IsEmpty, Is.False);
    }

    [Test]
    public void Dispose_clears_tracks_and_does_not_throw()
    {
        var tl = new TimelineViewModel(duration: 5.0, fps: 30);
        tl.Tracks.Add(new AnimationTrack(new PropertyRef(Guid.NewGuid(), "Left.Value", AnimatedValueType.Double)));
        Assert.DoesNotThrow(() => tl.Dispose());
        Assert.That(tl.Tracks, Is.Empty);
    }

    [Test]
    public void Now_can_be_set_within_PlayRange()
    {
        var tl = new TimelineViewModel(duration: 10.0, fps: 30);
        tl.Now.Value = 3.5;
        Assert.That(tl.Now.Value, Is.EqualTo(3.5));
    }
}
