using boilersGraphics.Helpers.Animation.Export;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using WinForms = System.Windows.Forms;

namespace boilersGraphics.ViewModels.Animation;

/// <summary>
/// Phase 5.5-d-6: MAUI Animation XAML 書出ダイアログ ViewModel。
/// 構造は <see cref="WpfXamlExportDialogViewModel"/> とほぼ同じだが、
/// MAUI では <c>GenerateCodeBehind</c> は常に true (Animation API がコード側依存)。
/// </summary>
public class MauiXamlExportDialogViewModel : BindableBase, IDialogAware, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private bool _disposedValue;

    public MauiXamlExportDialogViewModel()
    {
        ValidationMessage = Observable.CombineLatest(
                TargetNamespace, ClassName, AccessModifier, IndentWidth, OutputPath,
                (ns, cn, am, iw, op) => WpfXamlExportDialogViewModel.Validate(ns, cn, am, iw, op))
            .ToBindableReactiveProperty();
        _disposables.Add(ValidationMessage);

        var canExport = ValidationMessage.Select(string.IsNullOrEmpty);

        ExecuteCommand = canExport.ToReactiveCommand();
        ExecuteCommand.Subscribe(_ =>
        {
            var settings = new XamlExportSettings
            {
                TargetNamespace = TargetNamespace.Value,
                ClassName = ClassName.Value,
                AccessModifier = AccessModifier.Value,
                GenerateCodeBehind = true,  // MAUI は固定 true
                IndentWidth = IndentWidth.Value,
                IncludeHeaderComment = IncludeHeaderComment.Value,
            };
            var ret = new DialogResult(ButtonResult.OK, new DialogParameters
            {
                { "Settings", settings },
                { "OutputPath", OutputPath.Value },
            });
            RequestClose?.Invoke(ret);
        }).AddTo(_disposables);

        CancelCommand = new ReactiveCommand();
        CancelCommand.Subscribe(_ => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel, null))).AddTo(_disposables);

        BrowseCommand = new ReactiveCommand();
        BrowseCommand.Subscribe(_ => BrowseOutputFile()).AddTo(_disposables);
    }

    public BindableReactiveProperty<string> TargetNamespace { get; } = new("MyApp.Animations");
    public BindableReactiveProperty<string> ClassName { get; } = new("FuiAnimation");
    public BindableReactiveProperty<string> AccessModifier { get; } = new("public");
    public BindableReactiveProperty<int> IndentWidth { get; } = new(4);
    public BindableReactiveProperty<bool> IncludeHeaderComment { get; } = new(true);
    public BindableReactiveProperty<string> OutputPath { get; } = new(string.Empty);

    public string[] AccessModifierChoices { get; } = { "public", "internal" };

    public BindableReactiveProperty<string> ValidationMessage { get; }

    public ReactiveCommand ExecuteCommand { get; }
    public ReactiveCommand CancelCommand { get; }
    public ReactiveCommand BrowseCommand { get; }

    public string Title => "MAUI Animation XAML 書出";

    public event Action<IDialogResult> RequestClose;
    public bool CanCloseDialog() => true;
    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters is null) return;
        if (parameters.TryGetValue<string>("InitialClassName", out var cn) && !string.IsNullOrEmpty(cn))
            ClassName.Value = cn;
        if (parameters.TryGetValue<string>("InitialOutputPath", out var op) && !string.IsNullOrEmpty(op))
            OutputPath.Value = op;
    }

    private void BrowseOutputFile()
    {
        using var dlg = new WinForms.SaveFileDialog
        {
            Title = "MAUI Animation XAML の出力先を選択してください",
            Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*",
            DefaultExt = "xaml",
            FileName = string.IsNullOrEmpty(OutputPath.Value) ? ClassName.Value + ".xaml" : System.IO.Path.GetFileName(OutputPath.Value),
            InitialDirectory = string.IsNullOrEmpty(OutputPath.Value)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : System.IO.Path.GetDirectoryName(OutputPath.Value),
            OverwritePrompt = true,
        };
        if (dlg.ShowDialog() == WinForms.DialogResult.OK) OutputPath.Value = dlg.FileName;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing) _disposables.Dispose();
        _disposedValue = true;
    }
}
