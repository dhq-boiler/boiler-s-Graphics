using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Animation;

[TestFixture]
public class TimelinePlaybackTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static (TimelineViewModel tl, NRectangleViewModel rect) NewWithTrack(double duration = 10.0)
    {
        var tl = new TimelineViewModel(duration, fps: 30);
        tl.PlayRangeEnd.Value = duration;
        var rect = new NRectangleViewModel();
        rect.Left.Value = 100.0;

        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(10.0, 200.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);

        tl.ItemResolver = g => g == rect.ID ? rect : null;
        return (tl, rect);
    }

    // ----- State machine -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void Play_は_IsPlaying_を_true_にする()
    {
        var (tl, _) = NewWithTrack();
        tl.Play();
        Assert.That(tl.IsPlaying.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Play_中の_Play_は_noop_状態維持()
    {
        var (tl, _) = NewWithTrack();
        tl.Play();
        tl.AdvanceBy(2.0); // Now=2 まで進める
        var nowBeforeSecondPlay = tl.Now.Value;

        tl.Play(); // 2 回目 - no-op
        Assert.That(tl.IsPlaying.Value, Is.True);
        Assert.That(tl.Now.Value, Is.EqualTo(nowBeforeSecondPlay));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Pause_は_IsPlaying_を_false_Now_は_維持()
    {
        var (tl, _) = NewWithTrack();
        tl.Play();
        tl.AdvanceBy(3.0);
        var nowAtPause = tl.Now.Value;

        tl.Pause();
        Assert.That(tl.IsPlaying.Value, Is.False);
        Assert.That(tl.Now.Value, Is.EqualTo(nowAtPause));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Pause_未再生_は_noop()
    {
        var (tl, _) = NewWithTrack();
        tl.Pause();
        Assert.That(tl.IsPlaying.Value, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Stop_は_IsPlaying_false_Now_PlayRangeStart_にリセット()
    {
        var (tl, _) = NewWithTrack();
        tl.PlayRangeStart.Value = 0.0;
        tl.Play();
        tl.AdvanceBy(4.0);

        tl.Stop();
        Assert.That(tl.IsPlaying.Value, Is.False);
        Assert.That(tl.Now.Value, Is.EqualTo(0.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Stop_は_Snapshotを_Restore()
    {
        var (tl, rect) = NewWithTrack();
        // Play 開始時の Left = 100、再生中に値を変えても Stop で 100 に戻る
        tl.Play();
        tl.AdvanceBy(5.0); // EvaluateAt 5.0 で Left = 150 (補間)
        Assert.That(rect.Left.Value, Is.EqualTo(150.0));

        tl.Stop();
        Assert.That(rect.Left.Value, Is.EqualTo(100.0), "Stop は Play 直前の値に Restore する");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Stop_は_PlayRangeStart_非0_でも_その値に巻き戻る()
    {
        var (tl, _) = NewWithTrack();
        tl.PlayRangeStart.Value = 2.5;
        tl.Now.Value = 4.0;

        tl.Stop();
        Assert.That(tl.Now.Value, Is.EqualTo(2.5));
    }

    // ----- AdvanceBy ロジック -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_dt_0以下_は_noop()
    {
        var (tl, _) = NewWithTrack();
        tl.Now.Value = 1.0;
        tl.AdvanceBy(0.0);
        Assert.That(tl.Now.Value, Is.EqualTo(1.0));
        tl.AdvanceBy(-1.0);
        Assert.That(tl.Now.Value, Is.EqualTo(1.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_は_Now_を加算_ApplyAt_で_VM値を更新()
    {
        var (tl, rect) = NewWithTrack();
        tl.Play(); // Snapshot
        tl.AdvanceBy(5.0);
        Assert.That(tl.Now.Value, Is.EqualTo(5.0));
        Assert.That(rect.Left.Value, Is.EqualTo(150.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_Loop_false_end到達_で_自動停止()
    {
        var (tl, rect) = NewWithTrack();
        tl.Play();
        tl.AdvanceBy(11.0); // end=10 を超える
        Assert.That(tl.Now.Value, Is.EqualTo(10.0));
        Assert.That(tl.IsPlaying.Value, Is.False);
        // end 位置の値が適用される (Left = 200)
        Assert.That(rect.Left.Value, Is.EqualTo(200.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_Loop_true_end超え_は_折返し()
    {
        var (tl, _) = NewWithTrack();
        tl.Loop.Value = true;
        tl.Play();
        tl.AdvanceBy(12.0); // 0→12 で 0..10 loop → Now = 2
        Assert.That(tl.Now.Value, Is.EqualTo(2.0).Within(1e-9));
        Assert.That(tl.IsPlaying.Value, Is.True, "Loop=true なら end 超えても継続");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_PlayRangeEnd_未設定_は_Duration_を使う()
    {
        var tl = new TimelineViewModel(duration: 5.0, fps: 30);
        tl.PlayRangeEnd.Value = 0.0; // 明示的に 0 にして「未設定」扱い
        var rect = new NRectangleViewModel();
        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(5.0, 50.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);
        tl.ItemResolver = g => g == rect.ID ? rect : null;
        tl.Play();

        tl.AdvanceBy(2.5);
        Assert.That(tl.Now.Value, Is.EqualTo(2.5));
        Assert.That(rect.Left.Value, Is.EqualTo(25.0));
    }

    // ----- ItemResolver guards -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void Play_ItemResolver_null_でも_例外なし_Snapshot未取得()
    {
        var tl = new TimelineViewModel(duration: 5.0, fps: 30);
        tl.PlayRangeEnd.Value = 5.0;
        Assert.DoesNotThrow(() => tl.Play());
        Assert.That(tl.IsPlaying.Value, Is.True);
        Assert.DoesNotThrow(() => tl.Stop());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_ItemResolver_null_でも_Now_だけ_進む()
    {
        var tl = new TimelineViewModel(duration: 10.0, fps: 30);
        tl.PlayRangeEnd.Value = 10.0;
        tl.AdvanceBy(2.0);
        Assert.That(tl.Now.Value, Is.EqualTo(2.0));
    }

    // ----- Commands -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void PlayCommand_発火_は_Play_と同等()
    {
        var (tl, _) = NewWithTrack();
        tl.PlayCommand.Execute(default);
        Assert.That(tl.IsPlaying.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PauseCommand_発火_は_Pause_と同等()
    {
        var (tl, _) = NewWithTrack();
        tl.Play();
        tl.PauseCommand.Execute(default);
        Assert.That(tl.IsPlaying.Value, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void StopCommand_発火_は_Stop_と同等()
    {
        var (tl, rect) = NewWithTrack();
        tl.Play();
        tl.AdvanceBy(5.0);
        tl.StopCommand.Execute(default);

        Assert.That(tl.IsPlaying.Value, Is.False);
        Assert.That(tl.Now.Value, Is.EqualTo(0.0));
        Assert.That(rect.Left.Value, Is.EqualTo(100.0));
    }
}
