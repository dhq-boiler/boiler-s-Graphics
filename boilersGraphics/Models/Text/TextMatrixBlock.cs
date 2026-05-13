using System;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-a §3.3 / §4.1: 行 × 列の格子状にテキストを並べるレイアウト。
/// Q-6 案 D で 3 モード (Sequential / DataGenerator / CustomList) すべて対応。
/// セル寸法 / Gap / Alignment は最小スコープでは未対応。生成結果は改行 + Separator で連結した単一 Text。
/// </summary>
[Serializable]
public class TextMatrixBlock : TextElementBase
{
    private int _Rows = 4;
    private int _Columns = 4;
    private TextMatrixCellMode _CellMode = TextMatrixCellMode.Sequential;
    private string _Separator = " ";

    private int _SequenceStart;
    private string _SequenceFormat = string.Empty;

    private DataGeneratorType _DataGenType = DataGeneratorType.Hex;
    private int _DataGenSeed = Random.Shared.Next();

    private string _CustomItems = string.Empty;

    public int Rows
    {
        get => _Rows;
        set => SetProperty(ref _Rows, value);
    }

    public int Columns
    {
        get => _Columns;
        set => SetProperty(ref _Columns, value);
    }

    public TextMatrixCellMode CellMode
    {
        get => _CellMode;
        set => SetProperty(ref _CellMode, value);
    }

    public string Separator
    {
        get => _Separator;
        set => SetProperty(ref _Separator, value);
    }

    public int SequenceStart
    {
        get => _SequenceStart;
        set => SetProperty(ref _SequenceStart, value);
    }

    /// <summary>Sequential モードで通し番号を書式化する .NET 書式指定子 ("D3" / "X2" / 空文字 = そのまま)。</summary>
    public string SequenceFormat
    {
        get => _SequenceFormat;
        set => SetProperty(ref _SequenceFormat, value);
    }

    public DataGeneratorType DataGenType
    {
        get => _DataGenType;
        set => SetProperty(ref _DataGenType, value);
    }

    /// <summary>DataGenerator モードのルート Seed。各セルの実 Seed は (rootSeed, r, c) のハッシュで決定。</summary>
    public int DataGenSeed
    {
        get => _DataGenSeed;
        set => SetProperty(ref _DataGenSeed, value);
    }

    /// <summary>CustomList モードで使う改行区切りの文字列リスト。足りない分は空文字で埋める。</summary>
    public string CustomItems
    {
        get => _CustomItems;
        set => SetProperty(ref _CustomItems, value);
    }
}
