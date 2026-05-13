namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2-a §3.5: NumberSequenceBlock の出力レイアウト方向。
/// </summary>
public enum NumberSequenceDirection
{
    /// <summary>1 行に Separator 区切りで並べる。</summary>
    Horizontal,

    /// <summary>1 列に改行区切りで並べる。</summary>
    Vertical,

    /// <summary>GridRows × GridColumns の格子に並べる (行内は Separator、行間は改行)。</summary>
    Grid,
}
