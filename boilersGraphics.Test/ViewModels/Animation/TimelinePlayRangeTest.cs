using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Animation;

/// <summary>
/// Phase 6.6: Duration / PlayRangeStart / PlayRangeEnd の編集が AdvanceBy / Stop / Loop に
/// 正しく反映されることを確認するホワイトボックステスト。モデル層 (TimelineViewModel) は Phase 5-e
/// で実装済みで、Phase 6.6 では UI からの編集経路を有効化する。
/// </summary>
[TestFixture]
public class TimelinePlayRangeTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static TimelineViewModel NewTimeline(double duration = 10.0)
    {
        var tl = new TimelineViewModel(duration, fps: 30);
        tl.PlayRangeEnd.Value = duration;
        return tl;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Duration_の初期値は_ctor_引数()
    {
        var tl = new TimelineViewModel(5.5, fps: 30);
        Assert.That(tl.Duration.Value, Is.EqualTo(5.5));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Duration_は_TwoWay_で書き換え可能()
    {
        var tl = NewTimeline();
        tl.Duration.Value = 20.0;
        Assert.That(tl.Duration.Value, Is.EqualTo(20.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PlayRangeStart_の初期値は_0()
    {
        var tl = NewTimeline();
        Assert.That(tl.PlayRangeStart.Value, Is.EqualTo(0.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PlayRangeEnd_は_TwoWay_で書き換え可能()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeEnd.Value = 7.5;
        Assert.That(tl.PlayRangeEnd.Value, Is.EqualTo(7.5));
    }

    // ----- Stop で Now が PlayRangeStart に巻き戻る -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void Stop_は_Now_を_PlayRangeStart_に巻き戻す()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeStart.Value = 2.0;
        tl.Now.Value = 5.0;
        tl.Stop();
        Assert.That(tl.Now.Value, Is.EqualTo(2.0));
    }

    // ----- AdvanceBy は PlayRangeStart / PlayRangeEnd の区間内 -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_は_PlayRangeEnd_を超えない_Loop_false()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeStart.Value = 1.0;
        tl.PlayRangeEnd.Value = 4.0;
        tl.Loop.Value = false;
        tl.Now.Value = 3.0;
        tl.AdvanceBy(2.0); // 5.0 になるところを End=4.0 でクランプ
        Assert.That(tl.Now.Value, Is.EqualTo(4.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AdvanceBy_は_Loop_true_なら_PlayRangeStart_に巻き戻る()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeStart.Value = 1.0;
        tl.PlayRangeEnd.Value = 4.0;
        tl.Loop.Value = true;
        tl.Now.Value = 3.5;
        tl.AdvanceBy(1.0); // 4.5 → wrap to Start + (4.5 - End) = 1 + 0.5 = 1.5
        Assert.That(tl.Now.Value, Is.GreaterThanOrEqualTo(1.0));
        Assert.That(tl.Now.Value, Is.LessThan(4.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PlayRangeEnd_が_0_以下なら_Duration_を使う_AdvanceBy()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeEnd.Value = 0.0; // 未設定扱い
        tl.Loop.Value = false;
        tl.Now.Value = 9.0;
        tl.AdvanceBy(2.0); // 11.0 になるところを Duration=10.0 でクランプ
        Assert.That(tl.Now.Value, Is.EqualTo(10.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PlayRangeStart_を_中盤に置いて_Stop_すれば_Now_は_そこに戻る()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeStart.Value = 3.0;
        tl.PlayRangeEnd.Value = 7.0;
        // 再生中の状態を経由せずに直接 Now をいじって Stop
        tl.Now.Value = 6.5;
        tl.Stop();
        Assert.That(tl.Now.Value, Is.EqualTo(3.0));
    }

    // ----- Phase 6.6-c 双方向整合性連動 -----

    [Test, RequiresThread(ApartmentState.STA)]
    public void End_を_Duration_より大きく設定すると_Duration_が追随して延びる()
    {
        var tl = NewTimeline(5.0);
        tl.PlayRangeEnd.Value = 10.0;
        Assert.That(tl.Duration.Value, Is.EqualTo(10.0));
        Assert.That(tl.PlayRangeEnd.Value, Is.EqualTo(10.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Duration_を_End_より小さく設定すると_End_が_クランプされる()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeEnd.Value = 8.0;
        tl.Duration.Value = 5.0;
        Assert.That(tl.PlayRangeEnd.Value, Is.EqualTo(5.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Duration_を_Start_より小さく設定すると_Start_も_クランプされる()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeStart.Value = 6.0;
        tl.PlayRangeEnd.Value = 8.0;
        tl.Duration.Value = 3.0;
        Assert.That(tl.PlayRangeStart.Value, Is.EqualTo(3.0));
        Assert.That(tl.PlayRangeEnd.Value, Is.EqualTo(3.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Start_を_負の値に設定すると_0_に_クランプ()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeStart.Value = -2.5;
        Assert.That(tl.PlayRangeStart.Value, Is.EqualTo(0.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Start_を_End_超に設定すると_End_に_クランプ()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeEnd.Value = 4.0;
        tl.PlayRangeStart.Value = 7.0;
        Assert.That(tl.PlayRangeStart.Value, Is.EqualTo(4.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void End_を_Duration_未満に設定すれば_Duration_は据え置き()
    {
        var tl = NewTimeline(10.0);
        tl.PlayRangeEnd.Value = 6.0;
        // End <= Duration なら Duration を引っ張らない
        Assert.That(tl.Duration.Value, Is.EqualTo(10.0));
        Assert.That(tl.PlayRangeEnd.Value, Is.EqualTo(6.0));
    }
}
