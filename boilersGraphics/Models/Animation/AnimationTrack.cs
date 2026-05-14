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
    /// Phase 5-c で Helpers/Animation/Interpolator + EasingFunctions に置換予定。
    /// 5-b の段階では Double 型のみ線形補間、それ以外は離散ジャンプ (前の値を返す) のスタブ実装。
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
                if (Target.ValueType == AnimatedValueType.Double
                    && k0.Value.Value is double v0
                    && k1.Value.Value is double v1)
                {
                    var span = k1.Time.Value - k0.Time.Value;
                    if (span <= 0) return v1;
                    var localT = (t - k0.Time.Value) / span;
                    return v0 + (v1 - v0) * localT;
                }
                return k0.Value.Value;
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
