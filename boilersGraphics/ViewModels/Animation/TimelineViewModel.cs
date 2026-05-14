using boilersGraphics.Models.Animation;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.ObjectModel;

namespace boilersGraphics.ViewModels.Animation;

public class TimelineViewModel : BindableBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public BindableReactiveProperty<double> Duration { get; }
    public BindableReactiveProperty<int> Fps { get; }
    public BindableReactiveProperty<double> PlayRangeStart { get; }
    public BindableReactiveProperty<double> PlayRangeEnd { get; }
    public BindableReactiveProperty<bool> Loop { get; }
    public BindableReactiveProperty<double> Now { get; }
    public BindableReactiveProperty<bool> IsPlaying { get; }
    public ObservableCollection<AnimationTrack> Tracks { get; }

    public TimelineViewModel()
        : this(duration: 0.0, fps: 30)
    {
    }

    public TimelineViewModel(double duration, int fps)
    {
        Duration = new BindableReactiveProperty<double>(duration);
        _disposables.Add(Duration);
        Fps = new BindableReactiveProperty<int>(fps);
        _disposables.Add(Fps);
        PlayRangeStart = new BindableReactiveProperty<double>(0.0);
        _disposables.Add(PlayRangeStart);
        PlayRangeEnd = new BindableReactiveProperty<double>(duration);
        _disposables.Add(PlayRangeEnd);
        Loop = new BindableReactiveProperty<bool>(false);
        _disposables.Add(Loop);
        Now = new BindableReactiveProperty<double>(0.0);
        _disposables.Add(Now);
        IsPlaying = new BindableReactiveProperty<bool>(false);
        _disposables.Add(IsPlaying);
        Tracks = new ObservableCollection<AnimationTrack>();
    }

    /// <summary>
    /// Empty timeline: 何のアニメーション情報も持たない状態。
    /// シリアライズ時、IsEmpty なら &lt;Timeline&gt; セクションを出力しない (= Phase 4 以前互換)。
    /// </summary>
    public bool IsEmpty => Duration.Value == 0.0 && Tracks.Count == 0;

    public void Dispose()
    {
        foreach (var t in Tracks) t.Dispose();
        Tracks.Clear();
        _disposables.Dispose();
    }
}
