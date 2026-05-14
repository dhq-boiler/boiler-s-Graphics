using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-e-1: 再生エンジン純粋ロジック。
///
/// 「Timeline + Item 解決関数 (Guid → SelectableDesignerItemViewModelBase)」を入力に、
/// <list type="bullet">
/// <item>Snapshot — Tracks の各 PropertyRef における現在値を <see cref="PropertyApplier.TryGet"/> で取得</item>
/// <item>Restore — Snapshot に保存された値を <see cref="PropertyApplier.Apply"/> で書き戻す</item>
/// <item>ApplyAt — Tracks の各 EvaluateAt(time) を <see cref="PropertyApplier.Apply"/> で書き込む</item>
/// </list>
/// を提供する。DispatcherTimer 等の時刻駆動は Phase 5-e-2 で TimelineViewModel に乗せる。
///
/// すべて副作用は item の ReactiveProperty への書き込みのみ。Timeline.Tracks 自体は変更しない。
/// </summary>
public static class PlaybackEngine
{
    /// <summary>
    /// Tracks に登場する各 PropertyRef について現在値をスナップショットして返す。
    /// 解決できない (resolver が null を返す / PropertyApplier.TryGet が false) 項目はスキップ。
    /// </summary>
    public static Dictionary<PropertyRef, object> Snapshot(
        TimelineViewModel timeline,
        Func<Guid, SelectableDesignerItemViewModelBase> resolver)
    {
        if (timeline is null) throw new ArgumentNullException(nameof(timeline));
        if (resolver is null) throw new ArgumentNullException(nameof(resolver));

        var dict = new Dictionary<PropertyRef, object>();
        foreach (var track in timeline.Tracks)
        {
            var key = track.Target;
            if (dict.ContainsKey(key)) continue;

            var item = resolver(key.ItemId);
            if (item is null) continue;

            if (PropertyApplier.TryGet(item, key.PropertyPath, out var current))
            {
                dict[key] = current;
            }
        }
        return dict;
    }

    /// <summary>
    /// Snapshot に保存された各 (PropertyRef, value) を resolver 経由で取得した item に書き戻す。
    /// 戻り値は実際に書き戻せた件数 (resolver miss / PropertyApplier.Apply false はカウントされない)。
    /// </summary>
    public static int Restore(
        Dictionary<PropertyRef, object> snapshot,
        Func<Guid, SelectableDesignerItemViewModelBase> resolver)
    {
        if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));
        if (resolver is null) throw new ArgumentNullException(nameof(resolver));

        var applied = 0;
        foreach (var kv in snapshot)
        {
            var item = resolver(kv.Key.ItemId);
            if (item is null) continue;
            if (PropertyApplier.Apply(item, kv.Key.PropertyPath, kv.Value)) applied++;
        }
        return applied;
    }

    /// <summary>
    /// Tracks の各 EvaluateAt(time) を resolver 経由の item に書き込む。
    /// Track.Keyframes.Count == 0 のものは EvaluateAt が null を返すので適用スキップ。
    /// 戻り値は実際に適用できた件数。
    /// </summary>
    public static int ApplyAt(
        TimelineViewModel timeline,
        double time,
        Func<Guid, SelectableDesignerItemViewModelBase> resolver)
    {
        if (timeline is null) throw new ArgumentNullException(nameof(timeline));
        if (resolver is null) throw new ArgumentNullException(nameof(resolver));

        var applied = 0;
        foreach (var track in timeline.Tracks)
        {
            var item = resolver(track.Target.ItemId);
            if (item is null) continue;

            var v = track.EvaluateAt(time);
            if (v is null) continue;

            if (PropertyApplier.Apply(item, track.Target.PropertyPath, v)) applied++;
        }
        return applied;
    }

    /// <summary>
    /// Loop 設定を考慮して time を [start, end] の範囲に正規化する。
    /// loop=true の場合、time が end を超えたら start 付近に折り返す (time - start を span でモジュロ)。
    /// loop=false の場合は [start, end] にクランプ。
    /// span (= end - start) が 0 以下の場合は start を返す。
    /// </summary>
    public static double NormalizeTime(double time, double start, double end, bool loop)
    {
        var span = end - start;
        if (span <= 0) return start;
        if (!loop)
        {
            if (time < start) return start;
            if (time > end) return end;
            return time;
        }
        var rel = time - start;
        var mod = rel - Math.Floor(rel / span) * span;
        // mod が span ちょうどになると end と数値的に同値になり次フレーム巻き戻りの境界が
        // 不安定になりかねないので、span の場合は 0 (= start) として返す。
        if (mod >= span) mod -= span;
        if (mod < 0) mod += span;
        return start + mod;
    }
}
