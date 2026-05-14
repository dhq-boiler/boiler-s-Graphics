using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace boilersGraphics.ViewModels.Animation;

public class TimelineViewModel : BindableBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    private DispatcherTimer _timer;
    private DateTime _lastTickUtc;
    private Dictionary<PropertyRef, object> _snapshot;

    public BindableReactiveProperty<double> Duration { get; }
    public BindableReactiveProperty<int> Fps { get; }
    public BindableReactiveProperty<double> PlayRangeStart { get; }
    public BindableReactiveProperty<double> PlayRangeEnd { get; }
    public BindableReactiveProperty<bool> Loop { get; }
    public BindableReactiveProperty<double> Now { get; }
    public BindableReactiveProperty<bool> IsPlaying { get; }
    public ObservableCollection<AnimationTrack> Tracks { get; }

    /// <summary>
    /// Phase 5-e-2: Guid -> DesignerItem 解決関数。<c>DiagramViewModel</c> から AllItems を見て
    /// 設定する。null のままだと再生時に <see cref="PlaybackEngine.ApplyAt"/> 等が item を解決できず
    /// 何も書き戻さない (= 効果なし)。テスト時に明示的に差し替えるためにも setter は public。
    /// </summary>
    public Func<Guid, SelectableDesignerItemViewModelBase> ItemResolver { get; set; }

    /// <summary>
    /// 再生開始 / 停止コマンド。Phase 5-d-2 までは UI ボタン側で IsEnabled=False としていたが
    /// Phase 5-e-2 でバインドする。
    /// </summary>
    public ReactiveCommand PlayCommand { get; }
    public ReactiveCommand PauseCommand { get; }
    public ReactiveCommand StopCommand { get; }

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

        PlayCommand = new ReactiveCommand();
        PlayCommand.Subscribe(_ => Play()).AddTo(_disposables);
        PauseCommand = new ReactiveCommand();
        PauseCommand.Subscribe(_ => Pause()).AddTo(_disposables);
        StopCommand = new ReactiveCommand();
        StopCommand.Subscribe(_ => Stop()).AddTo(_disposables);
    }

    /// <summary>
    /// Empty timeline: 何のアニメーション情報も持たない状態。
    /// シリアライズ時、IsEmpty なら &lt;Timeline&gt; セクションを出力しない (= Phase 4 以前互換)。
    /// </summary>
    public bool IsEmpty => Duration.Value == 0.0 && Tracks.Count == 0;

    /// <summary>
    /// 再生開始。すでに再生中なら no-op。
    /// Snapshot は IsPlaying が false の状態で 1 回だけ取得 (= Stop でその値に戻る)。
    /// </summary>
    public void Play()
    {
        if (IsPlaying.Value) return;
        if (ItemResolver is not null)
        {
            _snapshot = PlaybackEngine.Snapshot(this, ItemResolver);
        }
        IsPlaying.Value = true;
        _lastTickUtc = DateTime.UtcNow;
        EnsureTimer();
        _timer?.Start();
    }

    /// <summary>
    /// 一時停止。現状の Now / プロパティ値は維持。再 Play で同じ Snapshot から続行。
    /// </summary>
    public void Pause()
    {
        if (!IsPlaying.Value) return;
        IsPlaying.Value = false;
        _timer?.Stop();
    }

    /// <summary>
    /// 停止。Now を PlayRangeStart に巻き戻し、Snapshot を Restore。Snapshot は破棄。
    /// </summary>
    public void Stop()
    {
        IsPlaying.Value = false;
        _timer?.Stop();
        Now.Value = PlayRangeStart.Value;
        if (_snapshot is not null && ItemResolver is not null)
        {
            PlaybackEngine.Restore(_snapshot, ItemResolver);
        }
        _snapshot = null;
    }

    private void EnsureTimer()
    {
        if (_timer is not null) return;
        var interval = TimeSpan.FromSeconds(1.0 / Math.Max(1, Fps.Value));
        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = interval };
        _timer.Tick += (_, _) =>
        {
            var nowUtc = DateTime.UtcNow;
            var dt = (nowUtc - _lastTickUtc).TotalSeconds;
            _lastTickUtc = nowUtc;
            AdvanceBy(dt);
        };
    }

    /// <summary>
    /// 経過秒 <paramref name="dt"/> を加算し Now を進めて補間を適用する。
    /// Loop=true なら PlayRange を折返し、Loop=false で end に達したら自動停止 + Snapshot Restore。
    /// Tick ロジックの本体だが、DispatcherTimer に依らずテストから直接呼べるよう internal で公開する。
    /// </summary>
    internal void AdvanceBy(double dt)
    {
        if (dt <= 0) return;
        var start = PlayRangeStart.Value;
        var end = ResolvePlayRangeEnd();
        var next = Now.Value + dt;
        if (!Loop.Value && next >= end)
        {
            Now.Value = end;
            if (ItemResolver is not null) PlaybackEngine.ApplyAt(this, end, ItemResolver);
            // 自動停止 (Snapshot Restore はせず、Now=end の状態を保つ)
            IsPlaying.Value = false;
            _timer?.Stop();
            return;
        }
        var normalized = PlaybackEngine.NormalizeTime(next, start, end, Loop.Value);
        Now.Value = normalized;
        if (ItemResolver is not null) PlaybackEngine.ApplyAt(this, normalized, ItemResolver);
    }

    /// <summary>
    /// PlayRangeEnd が 0 以下 (= 未設定) の場合は Duration を使う。
    /// </summary>
    private double ResolvePlayRangeEnd()
    {
        var end = PlayRangeEnd.Value;
        if (end <= 0) end = Duration.Value;
        return end;
    }

    public void Dispose()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer = null;
        }
        foreach (var t in Tracks) t.Dispose();
        Tracks.Clear();
        _disposables.Dispose();
    }
}
