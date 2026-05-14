using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using WinForms = System.Windows.Forms;

namespace boilersGraphics.ViewModels.Animation;

/// <summary>
/// Phase 5-f-2: PNG 連番書出ダイアログ ViewModel。
/// 入力: Start / End / Fps / OutputDirectory / FilenamePrefix + 表示用 Timeline.Duration。
/// 出力: OK 押下時に <see cref="DialogResult"/> に "Settings" = <see cref="PngSequenceExportSettings"/> を載せる。
/// 実 Renderer 呼び出しは呼び出し側 (DiagramViewModel) の責務。
/// </summary>
public class PngSequenceExportDialogViewModel : BindableBase, IDialogAware, IDisposable
{
    private CompositeDisposable _disposables = new();
    private bool _disposedValue;

    public PngSequenceExportDialogViewModel()
    {
        // FrameCount は Start/End/Fps から派生。Validate が NG なら 0。
        FrameCount = Observable
            .CombineLatest(Start, End, Fps, OutputDirectory, FilenamePrefix,
                (s, e, f, _, _) => new PngSequenceExportSettings
                {
                    Start = s,
                    End = e,
                    Fps = f,
                    OutputDirectory = "ok", // FrameCount 計算には影響しないダミー
                    FilenamePrefix = "ok",
                })
            .Select(PngSequenceExporter.ComputeFrameCount)
            .ToBindableReactiveProperty();
        _disposables.Add(FrameCount);

        ValidationMessage = Observable
            .CombineLatest(Start, End, Fps, OutputDirectory, FilenamePrefix,
                (s, e, f, dir, prefix) =>
                {
                    var settings = new PngSequenceExportSettings
                    {
                        Start = s,
                        End = e,
                        Fps = f,
                        OutputDirectory = dir,
                        FilenamePrefix = prefix,
                    };
                    var r = PngSequenceExporter.Validate(settings, TimelineDuration.Value);
                    return r.IsValid ? string.Empty : r.ErrorMessage;
                })
            .ToBindableReactiveProperty();
        _disposables.Add(ValidationMessage);

        var canExport = ValidationMessage.Select(m => string.IsNullOrEmpty(m));

        ExecuteCommand = canExport.ToReactiveCommand();
        ExecuteCommand.Subscribe(_ =>
        {
            var settings = new PngSequenceExportSettings
            {
                Start = Start.Value,
                End = End.Value,
                Fps = Fps.Value,
                OutputDirectory = OutputDirectory.Value,
                FilenamePrefix = FilenamePrefix.Value,
            };
            var ret = new DialogResult(ButtonResult.OK, new DialogParameters { { "Settings", settings } });
            RequestClose?.Invoke(ret);
        }).AddTo(_disposables);

        CancelCommand = new ReactiveCommand();
        CancelCommand.Subscribe(_ =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel, null));
        }).AddTo(_disposables);

        BrowseCommand = new ReactiveCommand();
        BrowseCommand.Subscribe(_ => BrowseOutputDirectory()).AddTo(_disposables);
    }

    public BindableReactiveProperty<double> Start { get; } = new(0.0);
    public BindableReactiveProperty<double> End { get; } = new(1.0);
    public BindableReactiveProperty<int> Fps { get; } = new(30);
    public BindableReactiveProperty<string> OutputDirectory { get; } = new(string.Empty);
    public BindableReactiveProperty<string> FilenamePrefix { get; } = new("frame_");

    /// <summary>Timeline.Duration の値。ダイアログ表示時に渡される。0 のとき End 上限チェックはスキップ。</summary>
    public BindableReactiveProperty<double> TimelineDuration { get; } = new(0.0);

    /// <summary>派生: 現在の設定で書き出されるフレーム数。表示用。</summary>
    public BindableReactiveProperty<int> FrameCount { get; }

    /// <summary>派生: バリデーション NG 時のエラーメッセージ。OK のときは空文字。</summary>
    public BindableReactiveProperty<string> ValidationMessage { get; }

    public ReactiveCommand ExecuteCommand { get; }
    public ReactiveCommand CancelCommand { get; }
    public ReactiveCommand BrowseCommand { get; }

    public string Title => "PNG 連番書出";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog() => true;
    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters is null) return;
        var start = parameters.TryGetValue<double>("Start", out var s) ? s : 0.0;
        var end = parameters.TryGetValue<double>("End", out var e) ? e : 1.0;
        var fps = parameters.TryGetValue<int>("Fps", out var f) ? f : 30;
        var duration = parameters.TryGetValue<double>("Duration", out var d) ? d : 0.0;
        Start.Value = start;
        End.Value = end > start ? end : start + 1.0;
        Fps.Value = fps > 0 ? fps : 30;
        TimelineDuration.Value = duration;
    }

    private void BrowseOutputDirectory()
    {
        using var dlg = new WinForms.FolderBrowserDialog
        {
            Description = "PNG 連番の出力先フォルダを選択してください",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true,
            InitialDirectory = string.IsNullOrEmpty(OutputDirectory.Value) ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) : OutputDirectory.Value,
        };
        if (dlg.ShowDialog() == WinForms.DialogResult.OK)
        {
            OutputDirectory.Value = dlg.SelectedPath;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            _disposables.Dispose();
        }
        _disposedValue = true;
    }
}
