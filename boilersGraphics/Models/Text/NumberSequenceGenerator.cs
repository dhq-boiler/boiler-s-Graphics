using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2-c: NumberSequenceBlock の文字列化を担う純関数群 (Q-5 案 A: 常に同期)。
/// Start..End を Step 刻みで列挙し、Format で文字列化して Direction に応じて結合する。
/// Culture 依存を避けるため CultureInfo.InvariantCulture を使う (FUI で小数点が , になるとデザインが崩れるため)。
/// </summary>
public static class NumberSequenceGenerator
{
    public static string Generate(
        double start,
        double end,
        double step,
        string format,
        string separator,
        NumberSequenceDirection direction,
        int gridRows,
        int gridColumns)
    {
        var values = Enumerate(start, end, step).Select(v => FormatValue(v, format)).ToList();
        var sep = separator ?? string.Empty;

        return direction switch
        {
            NumberSequenceDirection.Horizontal => string.Join(sep, values),
            NumberSequenceDirection.Vertical => string.Join(Environment.NewLine, values),
            NumberSequenceDirection.Grid => FormatGrid(values, gridRows, gridColumns, sep),
            _ => string.Empty,
        };
    }

    /// <summary>
    /// start から step 刻みで end を超えないところまで列挙する。
    /// 整数インデックス (start + step * i) で組むことで累積誤差を避ける。
    /// step が 0、または方向が end と合わない (step&gt;0 で start&gt;end など) ときは 0 件。
    /// </summary>
    public static IEnumerable<double> Enumerate(double start, double end, double step)
    {
        if (step == 0) yield break;

        // 端点の浮動小数点誤差を許す: count 計算前にわずかに緩めた閾値を使う
        var diff = end - start;
        if ((step > 0 && diff < 0) || (step < 0 && diff > 0)) yield break;

        var count = (int)Math.Floor(diff / step + 1e-9) + 1;
        if (count <= 0) yield break;

        for (var i = 0; i < count; i++)
            yield return start + step * i;
    }

    private static string FormatValue(double value, string format)
    {
        if (string.IsNullOrEmpty(format))
            return value.ToString(CultureInfo.InvariantCulture);

        // 整数系フォーマット (D / X / B) は double では扱えないので long に丸める
        var first = char.ToUpperInvariant(format[0]);
        if (first == 'D' || first == 'X' || first == 'B')
            return ((long)value).ToString(format, CultureInfo.InvariantCulture);

        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    private static string FormatGrid(IReadOnlyList<string> items, int rows, int cols, string separator)
    {
        if (rows <= 0 || cols <= 0) return string.Empty;

        var lines = new string[rows];
        var index = 0;
        for (var r = 0; r < rows; r++)
        {
            var row = new string[cols];
            for (var c = 0; c < cols; c++)
            {
                row[c] = index < items.Count ? items[index++] : string.Empty;
            }
            lines[r] = string.Join(separator, row);
        }
        return string.Join(Environment.NewLine, lines);
    }
}
