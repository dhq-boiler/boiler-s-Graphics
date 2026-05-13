namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-b §3.4: TextOnPath で各文字をどの向きに回転させるか。
/// </summary>
public enum TextOnPathRotation
{
    /// <summary>パスの接線方向に各文字を回転 (FUI で円周ラベル等に使う)。</summary>
    Tangent,

    /// <summary>常に直立 (回転なし)。</summary>
    Upright,
}
