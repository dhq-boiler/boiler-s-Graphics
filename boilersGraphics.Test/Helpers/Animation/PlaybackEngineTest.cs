using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Animation;

[TestFixture]
public class PlaybackEngineTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static TimelineViewModel NewTimeline(double duration = 10.0, double now = 0.0)
    {
        var tl = new TimelineViewModel(duration, fps: 30);
        tl.Now.Value = now;
        return tl;
    }

    private static Func<Guid, SelectableDesignerItemViewModelBase> SingleItemResolver(Guid id, SelectableDesignerItemViewModelBase item)
        => g => g == id ? item : null;

    // ----- Snapshot -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void Snapshot_Track無し_は_空辞書()
    {
        var tl = NewTimeline();
        var snap = PlaybackEngine.Snapshot(tl, _ => null);
        Assert.That(snap, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Snapshot_Track有り_は_PropertyApplier_TryGet_の現在値を保存()
    {
        var tl = NewTimeline();
        var rect = new NRectangleViewModel();
        rect.Left.Value = 100.0;

        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(5.0, 200.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);

        var snap = PlaybackEngine.Snapshot(tl, SingleItemResolver(rect.ID, rect));
        Assert.That(snap.Count, Is.EqualTo(1));
        Assert.That(snap[track.Target], Is.EqualTo(100.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Snapshot_resolver_null返却_は_スキップ()
    {
        var tl = NewTimeline();
        var someGuid = Guid.NewGuid();
        var track = new AnimationTrack(new PropertyRef(someGuid, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 10.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);

        var snap = PlaybackEngine.Snapshot(tl, _ => null);
        Assert.That(snap, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Snapshot_未対応プロパティパス_は_スキップ()
    {
        var tl = NewTimeline();
        var rect = new NRectangleViewModel();
        var track = new AnimationTrack(new PropertyRef(rect.ID, "DoesNotExist", AnimatedValueType.String));
        track.Keyframes.Add(new Keyframe(0.0, "x", EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);

        var snap = PlaybackEngine.Snapshot(tl, SingleItemResolver(rect.ID, rect));
        Assert.That(snap, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Snapshot_timeline_null_で_ArgumentNullException()
    {
        Assert.That(() => PlaybackEngine.Snapshot(null, _ => null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Snapshot_resolver_null_で_ArgumentNullException()
    {
        var tl = NewTimeline();
        Assert.That(() => PlaybackEngine.Snapshot(tl, null), Throws.TypeOf<ArgumentNullException>());
    }

    // ----- Restore -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void Restore_は_PropertyApplier_Apply_で_書き戻す()
    {
        var rect = new NRectangleViewModel();
        rect.Left.Value = 999.0;

        var snap = new Dictionary<PropertyRef, object>
        {
            { new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double), 42.0 }
        };

        var applied = PlaybackEngine.Restore(snap, SingleItemResolver(rect.ID, rect));
        Assert.That(applied, Is.EqualTo(1));
        Assert.That(rect.Left.Value, Is.EqualTo(42.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Restore_空_は_0件()
    {
        var applied = PlaybackEngine.Restore(new Dictionary<PropertyRef, object>(), _ => null);
        Assert.That(applied, Is.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Restore_resolver_miss_は_スキップ_カウント0()
    {
        var snap = new Dictionary<PropertyRef, object>
        {
            { new PropertyRef(Guid.NewGuid(), "Left.Value", AnimatedValueType.Double), 1.0 }
        };
        var applied = PlaybackEngine.Restore(snap, _ => null);
        Assert.That(applied, Is.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Restore_snapshot_null_で_ArgumentNullException()
    {
        Assert.That(() => PlaybackEngine.Restore(null, _ => null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Snapshot_then_Restore_でラウンドトリップ()
    {
        var tl = NewTimeline();
        var rect = new NRectangleViewModel();
        rect.Left.Value = 100.0;
        rect.Top.Value = 50.0;

        var t1 = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        t1.Keyframes.Add(new Keyframe(0.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        var t2 = new AnimationTrack(new PropertyRef(rect.ID, "Top.Value", AnimatedValueType.Double));
        t2.Keyframes.Add(new Keyframe(0.0, 50.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(t1);
        tl.Tracks.Add(t2);

        var resolver = SingleItemResolver(rect.ID, rect);
        var snap = PlaybackEngine.Snapshot(tl, resolver);

        // 値をいじってから Restore で元に戻ることを確認
        rect.Left.Value = 0.0;
        rect.Top.Value = 0.0;
        PlaybackEngine.Restore(snap, resolver);

        Assert.That(rect.Left.Value, Is.EqualTo(100.0));
        Assert.That(rect.Top.Value, Is.EqualTo(50.0));
    }

    // ----- ApplyAt -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void ApplyAt_は_EvaluateAt_の補間値を書き込む()
    {
        var tl = NewTimeline();
        var rect = new NRectangleViewModel();

        // 0.0s で 100, 10.0s で 200 にリニア補間
        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(10.0, 200.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);

        var applied = PlaybackEngine.ApplyAt(tl, 5.0, SingleItemResolver(rect.ID, rect));
        Assert.That(applied, Is.EqualTo(1));
        Assert.That(rect.Left.Value, Is.EqualTo(150.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ApplyAt_キーフレーム0_は_スキップ()
    {
        var tl = NewTimeline();
        var rect = new NRectangleViewModel();
        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        tl.Tracks.Add(track);

        var applied = PlaybackEngine.ApplyAt(tl, 1.0, SingleItemResolver(rect.ID, rect));
        Assert.That(applied, Is.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ApplyAt_resolver_miss_は_スキップ()
    {
        var tl = NewTimeline();
        var track = new AnimationTrack(new PropertyRef(Guid.NewGuid(), "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 1.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);

        var applied = PlaybackEngine.ApplyAt(tl, 0.0, _ => null);
        Assert.That(applied, Is.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ApplyAt_複数Track_は_全部適用()
    {
        var tl = NewTimeline();
        var rect = new NRectangleViewModel();

        var t1 = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        t1.Keyframes.Add(new Keyframe(0.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        t1.Keyframes.Add(new Keyframe(10.0, 200.0, EasingKind.LinearEase, EasingMode.EaseIn));
        var t2 = new AnimationTrack(new PropertyRef(rect.ID, "Top.Value", AnimatedValueType.Double));
        t2.Keyframes.Add(new Keyframe(0.0, 50.0, EasingKind.LinearEase, EasingMode.EaseIn));
        t2.Keyframes.Add(new Keyframe(10.0, 80.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(t1);
        tl.Tracks.Add(t2);

        var applied = PlaybackEngine.ApplyAt(tl, 5.0, SingleItemResolver(rect.ID, rect));
        Assert.That(applied, Is.EqualTo(2));
        Assert.That(rect.Left.Value, Is.EqualTo(150.0));
        Assert.That(rect.Top.Value, Is.EqualTo(65.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ApplyAt_timeline_null_で_ArgumentNullException()
    {
        Assert.That(() => PlaybackEngine.ApplyAt(null, 0.0, _ => null), Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ApplyAt_resolver_null_で_ArgumentNullException()
    {
        var tl = NewTimeline();
        Assert.That(() => PlaybackEngine.ApplyAt(tl, 0.0, null), Throws.TypeOf<ArgumentNullException>());
    }

    // ----- NormalizeTime -----

    [Test]
    public void NormalizeTime_loop_false_範囲内_はそのまま()
    {
        Assert.That(PlaybackEngine.NormalizeTime(3.0, 0.0, 10.0, loop: false), Is.EqualTo(3.0));
    }

    [Test]
    public void NormalizeTime_loop_false_範囲下未満_は_start()
    {
        Assert.That(PlaybackEngine.NormalizeTime(-5.0, 0.0, 10.0, loop: false), Is.EqualTo(0.0));
    }

    [Test]
    public void NormalizeTime_loop_false_範囲超え_は_end()
    {
        Assert.That(PlaybackEngine.NormalizeTime(15.0, 0.0, 10.0, loop: false), Is.EqualTo(10.0));
    }

    [Test]
    public void NormalizeTime_loop_true_範囲内_はそのまま()
    {
        Assert.That(PlaybackEngine.NormalizeTime(3.0, 0.0, 10.0, loop: true), Is.EqualTo(3.0).Within(1e-9));
    }

    [Test]
    public void NormalizeTime_loop_true_範囲超え_は_折返し()
    {
        // 0..10 のループで 12 → 2
        Assert.That(PlaybackEngine.NormalizeTime(12.0, 0.0, 10.0, loop: true), Is.EqualTo(2.0).Within(1e-9));
    }

    [Test]
    public void NormalizeTime_loop_true_2周分超え_は_2周ぶん戻して位置決め()
    {
        // 0..10 で 23 → 3
        Assert.That(PlaybackEngine.NormalizeTime(23.0, 0.0, 10.0, loop: true), Is.EqualTo(3.0).Within(1e-9));
    }

    [Test]
    public void NormalizeTime_loop_true_負の超え_は_前方折返し()
    {
        // 0..10 で -3 → 7
        Assert.That(PlaybackEngine.NormalizeTime(-3.0, 0.0, 10.0, loop: true), Is.EqualTo(7.0).Within(1e-9));
    }

    [Test]
    public void NormalizeTime_loop_true_endちょうど_は_start()
    {
        // span 10、end ちょうどはモジュロで 0 に落ちる → start
        Assert.That(PlaybackEngine.NormalizeTime(10.0, 0.0, 10.0, loop: true), Is.EqualTo(0.0).Within(1e-9));
    }

    [Test]
    public void NormalizeTime_span_zero_は_start()
    {
        Assert.That(PlaybackEngine.NormalizeTime(5.0, 5.0, 5.0, loop: true), Is.EqualTo(5.0));
        Assert.That(PlaybackEngine.NormalizeTime(5.0, 5.0, 5.0, loop: false), Is.EqualTo(5.0));
    }

    [Test]
    public void NormalizeTime_span_負_は_start()
    {
        Assert.That(PlaybackEngine.NormalizeTime(0.0, 10.0, 5.0, loop: true), Is.EqualTo(10.0));
    }

    [Test]
    public void NormalizeTime_PlayRange_start_非0_でも_折返し位置正しい()
    {
        // 2..7 (span 5) のループで 8 → 3 (2 + (8-2)%5 = 2 + 1 = 3)
        Assert.That(PlaybackEngine.NormalizeTime(8.0, 2.0, 7.0, loop: true), Is.EqualTo(3.0).Within(1e-9));
    }
}
