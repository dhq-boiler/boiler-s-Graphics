using System;
using System.Collections.Generic;
using System.Globalization;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-a §3.3 / Q-5 案 A: TextMatrixBlock の文字列化を担う純関数群 (常に同期)。
/// Rows × Columns の格子に対し、CellMode に応じて各セルを生成し、行内は Separator、行間は改行で結合する。
/// Culture 非依存にするため CultureInfo.InvariantCulture を使う。
/// </summary>
public static class TextMatrixGenerator
{
    public static string Generate(
        int rows,
        int columns,
        TextMatrixCellMode mode,
        string separator,
        int sequenceStart,
        string sequenceFormat,
        DataGeneratorType dataGenType,
        int dataGenSeed,
        string customItems)
    {
        if (rows <= 0 || columns <= 0) return string.Empty;
        var sep = separator ?? string.Empty;

        var customList = SplitCustomItems(customItems);

        var lines = new string[rows];
        for (var r = 0; r < rows; r++)
        {
            var row = new string[columns];
            for (var c = 0; c < columns; c++)
            {
                row[c] = mode switch
                {
                    TextMatrixCellMode.Sequential => FormatSequential(sequenceStart + r * columns + c, sequenceFormat),
                    TextMatrixCellMode.DataGenerator => GenerateDataCell(dataGenType, dataGenSeed, r, c),
                    TextMatrixCellMode.CustomList => PickCustom(customList, r * columns + c),
                    _ => string.Empty,
                };
            }
            lines[r] = string.Join(sep, row);
        }
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatSequential(int value, string format)
    {
        if (string.IsNullOrEmpty(format)) return value.ToString(CultureInfo.InvariantCulture);
        // NumberSequence と同じ判定ロジック
        var first = char.ToUpperInvariant(format[0]);
        if (first == 'D' || first == 'X' || first == 'B')
            return value.ToString(format, CultureInfo.InvariantCulture);
        return ((double)value).ToString(format, CultureInfo.InvariantCulture);
    }

    private static string GenerateDataCell(DataGeneratorType type, int rootSeed, int r, int c)
    {
        // セル単位 Seed: rootSeed と (r, c) を混ぜたハッシュ。HashCode.Combine で衝突確率を抑える。
        var cellSeed = HashCode.Combine(rootSeed, r, c);
        return DataGenerator.Generate(type, cellSeed, 1, string.Empty, DataGeneratorLayout.OneLine);
    }

    private static string PickCustom(IReadOnlyList<string> items, int index)
    {
        if (items.Count == 0) return string.Empty;
        // 足りないセルは空文字で埋める (循環はしない: Q-6 仕様 "任意文字列リスト")
        return index < items.Count ? items[index] : string.Empty;
    }

    private static IReadOnlyList<string> SplitCustomItems(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
        // CRLF / LF 両対応
        return raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }
}
