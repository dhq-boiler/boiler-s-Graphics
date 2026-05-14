using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace boilersGraphics.Models.Themes;

/// <summary>
/// Phase 4-a §4: テーマ単位。パレット + 線種プリセット + デフォルトグロー設定を束ねる。
/// 組み込み (Bladerunner / Matrix / 医療系青白 / アンバー CRT) はコードハードコード、
/// ユーザー追加はプロジェクトファイルに保存される (Q-1 案 C)。
/// </summary>
[Serializable]
public class Theme : BindableBase
{
    private string _Name;
    private Guid _Id = Guid.NewGuid();
    private bool _IsBuiltIn;
    private ColorPalette _Palette = new();
    private GlowSettings _DefaultGlow = new();

    /// <summary>テーマ表示名 ("Bladerunner" 等)。</summary>
    public string Name
    {
        get => _Name;
        set => SetProperty(ref _Name, value);
    }

    /// <summary>テーマ一意 ID。</summary>
    public Guid Id
    {
        get => _Id;
        set => SetProperty(ref _Id, value);
    }

    /// <summary>組み込みテーマなら true。ユーザー追加なら false。</summary>
    public bool IsBuiltIn
    {
        get => _IsBuiltIn;
        set => SetProperty(ref _IsBuiltIn, value);
    }

    /// <summary>テーマのカラーパレット。</summary>
    public ColorPalette Palette
    {
        get => _Palette;
        set => SetProperty(ref _Palette, value);
    }

    /// <summary>テーマに含まれる線種プリセット (組み込み 6 種 + ユーザー追加)。</summary>
    public ObservableCollection<LineStyle> LineStyles { get; } = new();

    /// <summary>テーマの既定グロー設定。</summary>
    public GlowSettings DefaultGlow
    {
        get => _DefaultGlow;
        set => SetProperty(ref _DefaultGlow, value);
    }
}
