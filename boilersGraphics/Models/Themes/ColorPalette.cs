using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace boilersGraphics.Models.Themes;

/// <summary>
/// Phase 4-a §3.1 / Q-2 案 A: 順序色 + 固定 5 セマンティックスロットを持つカラーパレット。
/// </summary>
[Serializable]
public class ColorPalette : BindableBase
{
    private string _Name;
    private Guid _Id = Guid.NewGuid();
    private bool _IsBuiltIn;

    /// <summary>パレット表示名 ("Bladerunner" / "Matrix" 等)。</summary>
    public string Name
    {
        get => _Name;
        set => SetProperty(ref _Name, value);
    }

    /// <summary>パレット一意 ID。</summary>
    public Guid Id
    {
        get => _Id;
        set => SetProperty(ref _Id, value);
    }

    /// <summary>組み込みパレットなら true。ユーザー追加なら false。</summary>
    public bool IsBuiltIn
    {
        get => _IsBuiltIn;
        set => SetProperty(ref _IsBuiltIn, value);
    }

    /// <summary>順序色 (≥5 推奨)。SemanticSlots はこのインデックスを参照する。</summary>
    public ObservableCollection<Color> Colors { get; } = new();

    /// <summary>
    /// セマンティックスロット → 順序色インデックスのマップ。
    /// キーは <see cref="SemanticSlotKeys"/> の定数のいずれか。
    /// </summary>
    public Dictionary<string, int> SemanticSlots { get; } = new();

    /// <summary>セマンティックキーで色を取得。未登録 / インデックス範囲外なら null。</summary>
    public Color? GetSemanticColor(string slotKey)
    {
        if (!SemanticSlots.TryGetValue(slotKey, out var index)) return null;
        if (index < 0 || index >= Colors.Count) return null;
        return Colors[index];
    }
}
