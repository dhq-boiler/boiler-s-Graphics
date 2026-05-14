using boilersGraphics.Models.Animation;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.Models.Animation;

[TestFixture]
public class AnimationTrackTest
{
    private static PropertyRef MakeDoubleRef() =>
        new(Guid.NewGuid(), "Left.Value", AnimatedValueType.Double);

    [Test]
    public void EvaluateAt_no_keyframes_returns_null()
    {
        var track = new AnimationTrack(MakeDoubleRef());
        Assert.That(track.EvaluateAt(0.0), Is.Null);
    }

    [Test]
    public void EvaluateAt_one_keyframe_returns_that_value_regardless_of_time()
    {
        var track = new AnimationTrack(MakeDoubleRef());
        track.Keyframes.Add(new Keyframe(1.0, 42.0, EasingKind.LinearEase, EasingMode.EaseIn));
        Assert.That(track.EvaluateAt(0.0), Is.EqualTo(42.0));
        Assert.That(track.EvaluateAt(1.0), Is.EqualTo(42.0));
        Assert.That(track.EvaluateAt(99.0), Is.EqualTo(42.0));
    }

    [Test]
    public void EvaluateAt_two_keyframes_double_interpolates_linearly()
    {
        var track = new AnimationTrack(MakeDoubleRef());
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(2.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));

        Assert.That(track.EvaluateAt(0.0), Is.EqualTo(0.0));
        Assert.That(track.EvaluateAt(1.0), Is.EqualTo(50.0));
        Assert.That(track.EvaluateAt(2.0), Is.EqualTo(100.0));
    }

    [Test]
    public void EvaluateAt_out_of_range_clamps_to_endpoints()
    {
        var track = new AnimationTrack(MakeDoubleRef());
        track.Keyframes.Add(new Keyframe(1.0, 10.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(2.0, 20.0, EasingKind.LinearEase, EasingMode.EaseIn));

        Assert.That(track.EvaluateAt(-10.0), Is.EqualTo(10.0));
        Assert.That(track.EvaluateAt(100.0), Is.EqualTo(20.0));
    }

    [Test]
    public void EvaluateAt_three_keyframes_finds_correct_segment()
    {
        var track = new AnimationTrack(MakeDoubleRef());
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(1.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(2.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));

        Assert.That(track.EvaluateAt(0.5), Is.EqualTo(50.0));
        Assert.That(track.EvaluateAt(1.5), Is.EqualTo(50.0));
    }

    [Test]
    public void EvaluateAt_non_double_returns_discrete_left_value()
    {
        var pref = new PropertyRef(Guid.NewGuid(), "Label.Value", AnimatedValueType.String);
        var track = new AnimationTrack(pref);
        track.Keyframes.Add(new Keyframe(0.0, "before", EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(1.0, "after", EasingKind.LinearEase, EasingMode.EaseIn));

        Assert.That(track.EvaluateAt(0.5), Is.EqualTo("before"));
        Assert.That(track.EvaluateAt(1.0), Is.EqualTo("after"));
    }

    [Test]
    public void EvaluateAt_unordered_keyframes_still_returns_correct_value()
    {
        var track = new AnimationTrack(MakeDoubleRef());
        track.Keyframes.Add(new Keyframe(2.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));

        Assert.That(track.EvaluateAt(1.0), Is.EqualTo(50.0));
    }

    [Test]
    public void Dispose_clears_keyframes()
    {
        var track = new AnimationTrack(MakeDoubleRef());
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(1.0, 1.0, EasingKind.LinearEase, EasingMode.EaseIn));

        track.Dispose();
        Assert.That(track.Keyframes, Is.Empty);
    }
}
