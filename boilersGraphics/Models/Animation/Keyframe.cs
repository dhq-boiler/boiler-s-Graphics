using Prism.Mvvm;
using R3;
using System;

namespace boilersGraphics.Models.Animation;

public class Keyframe : BindableBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public BindableReactiveProperty<double> Time { get; }
    public BindableReactiveProperty<object> Value { get; }
    public BindableReactiveProperty<EasingKind> Easing { get; }
    public BindableReactiveProperty<EasingMode> Mode { get; }

    public Keyframe()
        : this(time: 0.0, value: null, easing: EasingKind.LinearEase, mode: EasingMode.EaseIn)
    {
    }

    public Keyframe(double time, object value, EasingKind easing, EasingMode mode)
    {
        Time = new BindableReactiveProperty<double>(time);
        _disposables.Add(Time);
        Value = new BindableReactiveProperty<object>(value);
        _disposables.Add(Value);
        Easing = new BindableReactiveProperty<EasingKind>(easing);
        _disposables.Add(Easing);
        Mode = new BindableReactiveProperty<EasingMode>(mode);
        _disposables.Add(Mode);
    }

    public void Dispose() => _disposables.Dispose();
}
