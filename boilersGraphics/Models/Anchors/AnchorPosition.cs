namespace boilersGraphics.Models.Anchors;

/// <summary>
/// Phase 3-a §3.3.1 / Q-5 案 A: 全 DesignerItem に暗黙で持たせる 9 点アンカーの位置。
/// AnchorRef 文字列の予約語 ("#tl", "#tc", ... "#br", "#c") と 1:1 対応する。
/// </summary>
public enum AnchorPosition
{
    /// <summary>左上 (0.0, 0.0)。予約語: "tl"</summary>
    TopLeft,

    /// <summary>上中央 (0.5, 0.0)。予約語: "tc"</summary>
    TopCenter,

    /// <summary>右上 (1.0, 0.0)。予約語: "tr"</summary>
    TopRight,

    /// <summary>左中央 (0.0, 0.5)。予約語: "lc"</summary>
    LeftCenter,

    /// <summary>中心 (0.5, 0.5)。予約語: "c"</summary>
    Center,

    /// <summary>右中央 (1.0, 0.5)。予約語: "rc"</summary>
    RightCenter,

    /// <summary>左下 (0.0, 1.0)。予約語: "bl"</summary>
    BottomLeft,

    /// <summary>下中央 (0.5, 1.0)。予約語: "bc"</summary>
    BottomCenter,

    /// <summary>右下 (1.0, 1.0)。予約語: "br"</summary>
    BottomRight,
}
