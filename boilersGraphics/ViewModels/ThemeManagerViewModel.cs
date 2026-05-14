using boilersGraphics.Models.Themes;
using ObservableCollections;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using System.Linq;

namespace boilersGraphics.ViewModels;

/// <summary>
/// Phase 4-c: テーマ選択 + 適用範囲 + 適用対象を選んでパレットを適用するダイアログの ViewModel。
/// 入力: 利用可能テーマリスト + 現在のアクティブテーマ。
/// 出力: 選ばれたテーマ + 適用範囲 + 適用対象 (呼び出し側で ThemeApplier を使って書換)。
/// </summary>
public class ThemeManagerViewModel : BindableBase, IDialogAware, IDisposable
{
    private CompositeDisposable _disposables = new();
    private bool _disposedValue;

    public ThemeManagerViewModel()
    {
        // Phase 4-d: 選択テーマ変更時に利用可能線種を入れ替え。
        SelectedTheme.Subscribe(theme =>
        {
            AvailableLineStyles.Clear();
            if (theme?.LineStyles != null)
            {
                foreach (var ls in theme.LineStyles) AvailableLineStyles.Add(ls);
            }
            SelectedLineStyle.Value = null;
        }).AddTo(_disposables);

        ApplyCommand = SelectedTheme
            .Select(t => t != null)
            .ToReactiveCommand();
        ApplyCommand.Subscribe(_ =>
        {
            var parameters = new DialogParameters
            {
                { "Theme", SelectedTheme.Value },
                { "Scope", SelectedScope.Value },
                { "Target", SelectedTarget.Value },
                { "LineStyle", SelectedLineStyle.Value },
                { "ApplyGlow", ApplyGlow.Value },
            };
            var ret = new DialogResult(ButtonResult.OK, parameters);
            RequestClose?.Invoke(ret);
        }).AddTo(_disposables);

        CancelCommand = new ReactiveCommand();
        CancelCommand.Subscribe(_ =>
        {
            var ret = new DialogResult(ButtonResult.Cancel, null);
            RequestClose?.Invoke(ret);
        }).AddTo(_disposables);
    }

    /// <summary>ダイアログに渡された利用可能テーマリスト (組込 + ユーザー追加)。</summary>
    public ObservableList<Theme> AvailableThemes { get; } = new();

    /// <summary>選択中のテーマ。<see cref="ApplyCommand"/> 実行時に Dialog 結果に乗る。</summary>
    public BindableReactiveProperty<Theme> SelectedTheme { get; } = new();

    /// <summary>適用範囲 (デフォルト: 選択中図形)。</summary>
    public BindableReactiveProperty<ThemeApplyScope> SelectedScope { get; } = new(ThemeApplyScope.SelectedItems);

    /// <summary>適用対象 (デフォルト: 両方)。</summary>
    public BindableReactiveProperty<ThemeApplyTarget> SelectedTarget { get; } = new(ThemeApplyTarget.Both);

    /// <summary>Phase 4-d: 選択テーマに紐づく線種プリセット一覧 (テーマ切替で入れ替わる)。</summary>
    public ObservableList<LineStyle> AvailableLineStyles { get; } = new();

    /// <summary>Phase 4-d: 選択中の線種プリセット。null なら線種は変更しない。</summary>
    public BindableReactiveProperty<LineStyle> SelectedLineStyle { get; } = new();

    /// <summary>Phase 4-e: テーマの DefaultGlow を選択範囲の図形に流し込むかどうか。</summary>
    public BindableReactiveProperty<bool> ApplyGlow { get; } = new(false);

    public ReactiveCommand ApplyCommand { get; }
    public ReactiveCommand CancelCommand { get; }

    public string Title => "テーマ選択 / パレット適用";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        AvailableThemes.Clear();
        var themes = parameters.GetValue<System.Collections.Generic.IReadOnlyList<Theme>>("Themes");
        if (themes != null)
        {
            foreach (var t in themes) AvailableThemes.Add(t);
        }
        var active = parameters.GetValue<Theme>("ActiveTheme");
        SelectedTheme.Value = active ?? AvailableThemes.FirstOrDefault();
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
        _disposables = null;
        _disposedValue = true;
    }
}
