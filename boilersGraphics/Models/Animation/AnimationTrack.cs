using boilersGraphics.Helpers.Animation;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace boilersGraphics.Models.Animation;

public class AnimationTrack : BindableBase, IDisposable
{
    public PropertyRef Target { get; }

    public ObservableCollection<Keyframe> Keyframes { get; }

    public AnimationTrack(PropertyRef target)
    {
        Target = target;
        Keyframes = new ObservableCollection<Keyframe>();
    }

    /// <summary>
    /// 指定時刻における補間値を返す。
    /// Phase 5-c から <see cref="Interpolator"/> + <see cref="EasingFunctions"/> による正式実装。
    /// イージング (kind/mode) は「区間左側のキーフレーム」のものを使う (AE / WPF KeyFrame と同じ流儀)。
    /// </summary>
    public object EvaluateAt(double t)
    {
        if (Keyframes.Count == 0) return null;
        if (Keyframes.Count == 1) return Keyframes[0].Value.Value;

        var sorted = Keyframes.OrderBy(k => k.Time.Value).ToList();

        if (t <= sorted[0].Time.Value) return sorted[0].Value.Value;
        if (t >= sorted[^1].Time.Value) return sorted[^1].Value.Value;

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            var k0 = sorted[i];
            var k1 = sorted[i + 1];
            if (t >= k0.Time.Value && t <= k1.Time.Value)
            {
                var span = k1.Time.Value - k0.Time.Value;
                if (span <= 0) return k1.Value.Value;
                var localT = (t - k0.Time.Value) / span;
                var easedT = EasingFunctions.Apply(k0.Easing.Value, k0.Mode.Value, localT);
                return Interpolator.Interpolate(Target.ValueType, k0.Value.Value, k1.Value.Value, easedT);
            }
        }
        return sorted[^1].Value.Value;
    }

    public void Dispose()
    {
        foreach (var k in Keyframes) k.Dispose();
        Keyframes.Clear();
    }
}
