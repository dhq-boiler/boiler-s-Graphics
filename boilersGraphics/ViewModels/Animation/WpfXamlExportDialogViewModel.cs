using boilersGraphics.Helpers.Animation.Export;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using System.Text.RegularExpressions;
using WinForms = System.Windows.Forms;

namespace boilersGraphics.ViewModels.Animation;

/// <summary>
/// Phase 5.5-c-2: WPF Storyboard XAML 書出ダイアログ ViewModel。
/// 入力: TargetNamespace / ClassName / AccessModifier / GenerateCodeBehind /
///       IndentWidth / IncludeHeaderComment / OutputPath。
/// 出力: OK 押下時に <see cref="DialogResult"/> に "Settings" = <see cref="XamlExportSettings"/>、
///       "OutputPath" = ファイル絶対パス を載せる。
/// </summary>
public class WpfXamlExportDialogViewModel : BindableBase, IDialogAware, IDisposable
{
    private static readonly Regex CsIdentifierRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);
    private static readonly Regex CsQualifiedNameRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.Compiled);

    private readonly CompositeDisposable _disposables = new();
    private bool _disposedValue;

    public WpfXamlExportDialogViewModel()
    {
        ValidationMessage = Observable.CombineLatest(
                TargetNamespace, ClassName, AccessModifier, IndentWidth, OutputPath,
                (ns, cn, am, iw, op) => Validate(ns, cn, am, iw, op))
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
                GenerateCodeBehind = GenerateCodeBehind.Value,
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
        CancelCommand.Subscribe(_ =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel, null));
        }).AddTo(_disposables);

        BrowseCommand = new ReactiveCommand();
        BrowseCommand.Subscribe(_ => BrowseOutputFile()).AddTo(_disposables);
    }

    public BindableReactiveProperty<string> TargetNamespace { get; } = new("MyApp.Animations");
    public BindableReactiveProperty<string> ClassName { get; } = new("FuiAnimation");
    public BindableReactiveProperty<string> AccessModifier { get; } = new("public");
    public BindableReactiveProperty<bool> GenerateCodeBehind { get; } = new(true);
    public BindableReactiveProperty<int> IndentWidth { get; } = new(4);
    public BindableReactiveProperty<bool> IncludeHeaderComment { get; } = new(true);
    public BindableReactiveProperty<string> OutputPath { get; } = new(string.Empty);

    public string[] AccessModifierChoices { get; } = { "public", "internal" };

    public BindableReactiveProperty<string> ValidationMessage { get; }

    public ReactiveCommand ExecuteCommand { get; }
    public ReactiveCommand CancelCommand { get; }
    public ReactiveCommand BrowseCommand { get; }

    public string Title => "WPF Storyboard XAML 書出";

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

    /// <summary>
    /// 入力値の形式 / 必須をチェックして、NG なら表示用エラーメッセージ、OK なら空文字を返す pure 関数。
    /// テストから直接呼ぶためにも public static として公開する。
    /// </summary>
    public static string Validate(string targetNamespace, string className, string accessModifier, int indentWidth, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(targetNamespace))
            return "ターゲット名前空間を指定してください。";
        if (!CsQualifiedNameRegex.IsMatch(targetNamespace))
            return "ターゲット名前空間は C# 識別子 (例: MyApp.Animations) で記述してください。";
        if (string.IsNullOrWhiteSpace(className))
            return "クラス名を指定してください。";
        if (!CsIdentifierRegex.IsMatch(className))
            return "クラス名は C# 識別子 (英字 / アンダースコア始まり) で記述してください。";
        if (accessModifier is not ("public" or "internal"))
            return "アクセス修飾子は public / internal のいずれかにしてください。";
        if (indentWidth <= 0 || indentWidth > 8)
            return "インデント幅は 1〜8 の範囲で指定してください。";
        if (string.IsNullOrWhiteSpace(outputPath))
            return "出力ファイルパスを指定してください。";
        return string.Empty;
    }

    private void BrowseOutputFile()
    {
        using var dlg = new WinForms.SaveFileDialog
        {
            Title = "WPF Storyboard XAML の出力先を選択してください",
            Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*",
            DefaultExt = "xaml",
            FileName = string.IsNullOrEmpty(OutputPath.Value)
                ? ClassName.Value + ".xaml"
                : System.IO.Path.GetFileName(OutputPath.Value),
            InitialDirectory = string.IsNullOrEmpty(OutputPath.Value)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : System.IO.Path.GetDirectoryName(OutputPath.Value),
            OverwritePrompt = true,
        };
        if (dlg.ShowDialog() == WinForms.DialogResult.OK)
        {
            OutputPath.Value = dlg.FileName;
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
        if (disposing) _disposables.Dispose();
        _disposedValue = true;
    }
}
