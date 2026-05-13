using System;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2-a §3.5 / §4.1: 開始値 / 終了値 / ステップから自動生成される連番列。
/// Format は .NET 書式指定子 ("D2", "X4", "F2" など)。
/// Direction で 1 行 / 1 列 / 格子の 3 形態を切替。
/// </summary>
[Serializable]
public class NumberSequenceBlock : TextElementBase
{
    private double _Start;
    private double _End = 10;
    private double _Step = 1;
    private string _Format = string.Empty;
    private string _Separator = " ";
    private NumberSequenceDirection _Direction = NumberSequenceDirection.Horizontal;
    private int _GridRows = 1;
    private int _GridColumns = 1;

    public double Start
    {
        get => _Start;
        set => SetProperty(ref _Start, value);
    }

    public double End
    {
        get => _End;
        set => SetProperty(ref _End, value);
    }

    public double Step
    {
        get => _Step;
        set => SetProperty(ref _Step, value);
    }

    public string Format
    {
        get => _Format;
        set => SetProperty(ref _Format, value);
    }

    public string Separator
    {
        get => _Separator;
        set => SetProperty(ref _Separator, value);
    }

    public NumberSequenceDirection Direction
    {
        get => _Direction;
        set => SetProperty(ref _Direction, value);
    }

    public int GridRows
    {
        get => _GridRows;
        set => SetProperty(ref _GridRows, value);
    }

    public int GridColumns
    {
        get => _GridColumns;
        set => SetProperty(ref _GridColumns, value);
    }
}
