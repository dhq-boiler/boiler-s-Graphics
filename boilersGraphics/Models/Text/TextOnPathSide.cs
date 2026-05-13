namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-b §3.4: TextOnPath で文字をパスのどちら側に配置するか。
/// </summary>
public enum TextOnPathSide
{
    /// <summary>パスの上側 (法線正方向) に配置。</summary>
    Above,

    /// <summary>パス上にちょうど配置 (offset 0)。</summary>
    On,

    /// <summary>パスの下側 (法線負方向) に配置。</summary>
    Below,
}
