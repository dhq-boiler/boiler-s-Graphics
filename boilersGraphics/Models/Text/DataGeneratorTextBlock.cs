using System;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2-a §3.2 / §4.1: ハイブリッド Seed (Q-3 案 C) を持つダミーデータ生成テキストブロック。
/// Type / Seed / Count / Separator / Layout の変更で内容を即時再生成する想定 (再生成は VM 層で実施)。
/// </summary>
[Serializable]
public class DataGeneratorTextBlock : TextElementBase
{
    private DataGeneratorType _Type = DataGeneratorType.Hex;
    private int _Seed = Random.Shared.Next();
    private bool _IsSeedLocked;
    private int _Count = 8;
    private string _Separator = " ";
    private DataGeneratorLayout _Layout = DataGeneratorLayout.OneLine;

    public DataGeneratorType Type
    {
        get => _Type;
        set => SetProperty(ref _Type, value);
    }

    public int Seed
    {
        get => _Seed;
        set => SetProperty(ref _Seed, value);
    }

    /// <summary>
    /// Q-3 案 C のハイブリッド Seed モード切替。false = 自動 (再生成時に新規 Seed)、true = 明示指定 (Seed をそのまま使う)。
    /// 「再生成」コマンドの挙動切替に使う想定 (UI は Phase 2 後半)。
    /// </summary>
    public bool IsSeedLocked
    {
        get => _IsSeedLocked;
        set => SetProperty(ref _IsSeedLocked, value);
    }

    public int Count
    {
        get => _Count;
        set => SetProperty(ref _Count, value);
    }

    public string Separator
    {
        get => _Separator;
        set => SetProperty(ref _Separator, value);
    }

    public DataGeneratorLayout Layout
    {
        get => _Layout;
        set => SetProperty(ref _Layout, value);
    }
}
