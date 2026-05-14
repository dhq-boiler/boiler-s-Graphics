using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation;

[TestFixture]
public class KeyframeToggleHelperTest
{
    private static TimelineViewModel NewTimeline(double now = 1.0)
    {
        var tl = new TimelineViewModel(duration: 5.0, fps: 30);
        tl.Now.Value = now;
        return tl;
    }

    private static readonly Guid ItemA = Guid.NewGuid();
    private static readonly Guid ItemB = Guid.NewGuid();

    // ----------- InferValueType -----------

    [Test]
    public void InferValueType_double_は_Double()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(3.14), Is.EqualTo(AnimatedValueType.Double));
    }

    [Test]
    public void InferValueType_int_は_Int()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(42), Is.EqualTo(AnimatedValueType.Int));
    }

    [Test]
    public void InferValueType_bool_は_Boolean()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(true), Is.EqualTo(AnimatedValueType.Boolean));
    }

    [Test]
    public void InferValueType_Point_は_Point()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(new Point(1, 2)), Is.EqualTo(AnimatedValueType.Point));
    }

    [Test]
    public void InferValueType_Color_は_Color()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(Colors.Red), Is.EqualTo(AnimatedValueType.Color));
    }

    [Test]
    public void InferValueType_Brush_は_Brush()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(new SolidColorBrush(Colors.Blue)), Is.EqualTo(AnimatedValueType.Brush));
    }

    [Test]
    public void InferValueType_string_は_String()
    {
        Assert.That(KeyframeToggleHelper.InferValueType("foo"), Is.EqualTo(AnimatedValueType.String));
    }

    [Test]
    public void InferValueType_Enum_は_Enum()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(EasingKind.SineEase), Is.EqualTo(AnimatedValueType.Enum));
    }

    [Test]
    public void InferValueType_null_は_String_フォールバック()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(null), Is.EqualTo(AnimatedValueType.String));
    }

    [Test]
    public void InferValueType_未知型_は_String_フォールバック()
    {
        Assert.That(KeyframeToggleHelper.InferValueType(new object()), Is.EqualTo(AnimatedValueType.String));
    }

    // ----------- GetStatus -----------

    [Test]
    public void GetStatus_Track無し_は_None()
    {
        var tl = NewTimeline();
        Assert.That(KeyframeToggleHelper.GetStatus(ItemA, "Left.Value", tl), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
    }

    [Test]
    public void GetStatus_別時刻のKeyframeのみ_は_TrackOnly()
    {
        var tl = NewTimeline(now: 1.0);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        tl.Now.Value = 2.0;
        Assert.That(KeyframeToggleHelper.GetStatus(ItemA, "Left.Value", tl), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.TrackOnly));
    }

    [Test]
    public void GetStatus_現時刻にKeyframe_は_HasKeyframeAtNow()
    {
        var tl = NewTimeline(now: 1.0);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        Assert.That(KeyframeToggleHelper.GetStatus(ItemA, "Left.Value", tl), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.HasKeyframeAtNow));
    }

    [Test]
    public void GetStatus_別ItemのTrack_は_None()
    {
        var tl = NewTimeline();
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        Assert.That(KeyframeToggleHelper.GetStatus(ItemB, "Left.Value", tl), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
    }

    [Test]
    public void GetStatus_別プロパティパス_は_None()
    {
        var tl = NewTimeline();
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        Assert.That(KeyframeToggleHelper.GetStatus(ItemA, "Top.Value", tl), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
    }

    [Test]
    public void GetStatus_timeline_null_は_None()
    {
        Assert.That(KeyframeToggleHelper.GetStatus(ItemA, "Left.Value", null), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
    }

    [Test]
    public void GetStatus_propertyPath_null_or_empty_は_None()
    {
        var tl = NewTimeline();
        Assert.That(KeyframeToggleHelper.GetStatus(ItemA, null, tl), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
        Assert.That(KeyframeToggleHelper.GetStatus(ItemA, "", tl), Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
    }

    // ----------- ToggleKeyframeAtNow: add -----------

    [Test]
    public void Toggle_Track無し_は_Track_と_Keyframe_を新規作成()
    {
        var tl = NewTimeline(now: 1.5);
        var status = KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 42.0, tl);

        Assert.That(status, Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.HasKeyframeAtNow));
        Assert.That(tl.Tracks.Count, Is.EqualTo(1));
        var track = tl.Tracks[0];
        Assert.That(track.Target.ItemId, Is.EqualTo(ItemA));
        Assert.That(track.Target.PropertyPath, Is.EqualTo("Left.Value"));
        Assert.That(track.Target.ValueType, Is.EqualTo(AnimatedValueType.Double));
        Assert.That(track.Keyframes.Count, Is.EqualTo(1));
        Assert.That(track.Keyframes[0].Time.Value, Is.EqualTo(1.5));
        Assert.That(track.Keyframes[0].Value.Value, Is.EqualTo(42.0));
    }

    [Test]
    public void Toggle_既存Track_別時刻_は_Keyframe追加()
    {
        var tl = NewTimeline(now: 1.0);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        tl.Now.Value = 3.0;
        var status = KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 30.0, tl);

        Assert.That(status, Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.HasKeyframeAtNow));
        Assert.That(tl.Tracks.Count, Is.EqualTo(1));
        Assert.That(tl.Tracks[0].Keyframes.Count, Is.EqualTo(2));
    }

    [Test]
    public void Toggle_別Item_は_別Track作成()
    {
        var tl = NewTimeline();
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemB, "Left.Value", 20.0, tl);

        Assert.That(tl.Tracks.Count, Is.EqualTo(2));
    }

    [Test]
    public void Toggle_別プロパティ_は_別Track作成()
    {
        var tl = NewTimeline();
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Top.Value", 20.0, tl);

        Assert.That(tl.Tracks.Count, Is.EqualTo(2));
    }

    // ----------- ToggleKeyframeAtNow: remove -----------

    [Test]
    public void Toggle_現時刻に既存Keyframe_は_削除して_TrackOnly()
    {
        var tl = NewTimeline(now: 1.0);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        tl.Now.Value = 3.0;
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 30.0, tl);

        // 1.0 に戻して toggle = 1.0 のキーフレームのみ削除、3.0 のは残る
        tl.Now.Value = 1.0;
        var status = KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);

        Assert.That(status, Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.TrackOnly));
        Assert.That(tl.Tracks.Count, Is.EqualTo(1));
        Assert.That(tl.Tracks[0].Keyframes.Count, Is.EqualTo(1));
        Assert.That(tl.Tracks[0].Keyframes[0].Time.Value, Is.EqualTo(3.0));
    }

    [Test]
    public void Toggle_最後のKeyframeを削除_は_Trackごと削除して_None()
    {
        var tl = NewTimeline(now: 1.0);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        var status = KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);

        Assert.That(status, Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
        Assert.That(tl.Tracks.Count, Is.EqualTo(0));
    }

    [Test]
    public void Toggle_TimeEpsilon内のKeyframe_も削除対象()
    {
        var tl = NewTimeline(now: 1.0);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);

        // 1ms 未満のずれ → 同一キーフレーム扱い → 削除
        tl.Now.Value = 1.0 + 5e-4;
        var status = KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);
        Assert.That(status, Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.None));
    }

    [Test]
    public void Toggle_TimeEpsilonを超えるずれ_は_別Keyframe扱いで_追加()
    {
        var tl = NewTimeline(now: 1.0);
        KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 10.0, tl);

        // 2ms のずれ → 別キーフレーム扱い → 追加
        tl.Now.Value = 1.0 + 2e-3;
        var status = KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 11.0, tl);
        Assert.That(status, Is.EqualTo(KeyframeToggleHelper.KeyframeStatus.HasKeyframeAtNow));
        Assert.That(tl.Tracks[0].Keyframes.Count, Is.EqualTo(2));
    }

    // ----------- guards -----------

    [Test]
    public void Toggle_timeline_null_で_ArgumentNullException()
    {
        Assert.That(() => KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "Left.Value", 1.0, null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Toggle_propertyPath_null_or_empty_で_ArgumentException()
    {
        var tl = NewTimeline();
        Assert.That(() => KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, null, 1.0, tl),
            Throws.TypeOf<ArgumentException>());
        Assert.That(() => KeyframeToggleHelper.ToggleKeyframeAtNow(ItemA, "", 1.0, tl),
            Throws.TypeOf<ArgumentException>());
    }
}
