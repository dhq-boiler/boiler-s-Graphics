using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels.Animation;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// 「現時刻に対するキーフレームの追加 / 削除 / 状態問合せ」を担う純粋ヘルパ。
/// Phase 5-d-3 の ◇ アイコンクリック動作のロジック本体。UI / Command からは
/// この 3 メソッドを呼ぶだけで済むようにする。
/// </summary>
public static class KeyframeToggleHelper
{
    /// <summary>
    /// 2 つの時刻が「同一キーフレーム」と見なせる近接性 (秒)。
    /// Now が float 演算で多少ぶれても同じキーフレームを掴めるように 1ms 未満を許容。
    /// </summary>
    public const double TimeEpsilon = 1e-3;

    public enum KeyframeStatus
    {
        /// <summary>該当プロパティの Track 自体が存在しない</summary>
        None,
        /// <summary>Track は存在するが現時刻 (Now) にはキーフレームが無い</summary>
        TrackOnly,
        /// <summary>Track があり、現時刻にもキーフレームが存在する</summary>
        HasKeyframeAtNow,
    }

    public static KeyframeStatus GetStatus(Guid itemId, string propertyPath, TimelineViewModel timeline)
    {
        if (timeline is null || string.IsNullOrEmpty(propertyPath)) return KeyframeStatus.None;
        var track = FindTrack(itemId, propertyPath, timeline);
        if (track is null) return KeyframeStatus.None;
        return HasKeyframeAt(track, timeline.Now.Value) ? KeyframeStatus.HasKeyframeAtNow : KeyframeStatus.TrackOnly;
    }

    /// <summary>
    /// 現時刻 (Now) に対してキーフレームをトグル: 既にあれば削除、無ければ追加 (Track が無ければ Track ごと新規)。
    /// </summary>
    /// <param name="itemId">対象 DesignerItem の Guid</param>
    /// <param name="propertyPath">プロパティパス (例: "Left.Value")</param>
    /// <param name="currentValue">現在値 (Track 新規作成時の AnimatedValueType 推論 + Keyframe 初期値に使う)</param>
    /// <param name="timeline">対象 Timeline</param>
    /// <returns>操作後の状態 (None になることは仕様上ない。Add 後は HasKeyframeAtNow、Remove 後で Track が空にならない場合は TrackOnly、空になった場合は None)</returns>
    public static KeyframeStatus ToggleKeyframeAtNow(Guid itemId, string propertyPath, object currentValue, TimelineViewModel timeline)
    {
        if (timeline is null) throw new ArgumentNullException(nameof(timeline));
        if (string.IsNullOrEmpty(propertyPath)) throw new ArgumentException("propertyPath is required", nameof(propertyPath));

        var now = timeline.Now.Value;
        var track = FindTrack(itemId, propertyPath, timeline);

        if (track is not null)
        {
            var existing = track.Keyframes.FirstOrDefault(k => Math.Abs(k.Time.Value - now) < TimeEpsilon);
            if (existing is not null)
            {
                track.Keyframes.Remove(existing);
                existing.Dispose();
                if (track.Keyframes.Count == 0)
                {
                    timeline.Tracks.Remove(track);
                    track.Dispose();
                    return KeyframeStatus.None;
                }
                return KeyframeStatus.TrackOnly;
            }
            track.Keyframes.Add(new Keyframe(now, currentValue, EasingKind.LinearEase, EasingMode.EaseIn));
            return KeyframeStatus.HasKeyframeAtNow;
        }

        var valueType = InferValueType(currentValue);
        var newTrack = new AnimationTrack(new PropertyRef(itemId, propertyPath, valueType));
        newTrack.Keyframes.Add(new Keyframe(now, currentValue, EasingKind.LinearEase, EasingMode.EaseIn));
        timeline.Tracks.Add(newTrack);
        return KeyframeStatus.HasKeyframeAtNow;
    }

    /// <summary>
    /// 現在値の CLR 型から <see cref="AnimatedValueType"/> を推論する。
    /// 推論できない型 (null 等) は <see cref="AnimatedValueType.String"/> にフォールバック。
    /// </summary>
    public static AnimatedValueType InferValueType(object value)
    {
        if (value is null) return AnimatedValueType.String;
        return value switch
        {
            double => AnimatedValueType.Double,
            float => AnimatedValueType.Double,
            int => AnimatedValueType.Int,
            long => AnimatedValueType.Int,
            short => AnimatedValueType.Int,
            byte => AnimatedValueType.Int,
            bool => AnimatedValueType.Boolean,
            Point => AnimatedValueType.Point,
            Color => AnimatedValueType.Color,
            Brush => AnimatedValueType.Brush,
            string => AnimatedValueType.String,
            Enum => AnimatedValueType.Enum,
            _ => AnimatedValueType.String,
        };
    }

    private static AnimationTrack FindTrack(Guid itemId, string propertyPath, TimelineViewModel timeline)
    {
        foreach (var t in timeline.Tracks)
        {
            if (t.Target.ItemId == itemId && t.Target.PropertyPath == propertyPath) return t;
        }
        return null;
    }

    private static bool HasKeyframeAt(AnimationTrack track, double time)
    {
        foreach (var k in track.Keyframes)
        {
            if (Math.Abs(k.Time.Value - time) < TimeEpsilon) return true;
        }
        return false;
    }
}
