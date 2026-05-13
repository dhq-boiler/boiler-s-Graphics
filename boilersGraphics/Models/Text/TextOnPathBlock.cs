using System;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-b §3.4 / §4.1 / Q-7 案 B: 任意のパス (既存 PolyBezierViewModel) に沿って文字を並べる要素。
/// PathReferenceId に PolyBezier の ID (Guid) を保持し、VM 層で実体を解決して PathGeometry を取得する。
/// </summary>
[Serializable]
public class TextOnPathBlock : TextElementBase
{
    private Guid? _PathReferenceId;
    private double _StartOffset;
    private double _Spacing;
    private TextOnPathSide _Side = TextOnPathSide.On;
    private TextOnPathRotation _Rotation = TextOnPathRotation.Tangent;

    /// <summary>参照する PolyBezier の ID (Guid)。未設定なら描画されない。</summary>
    public Guid? PathReferenceId
    {
        get => _PathReferenceId;
        set => SetProperty(ref _PathReferenceId, value);
    }

    /// <summary>パス先頭からのオフセット (0.0〜1.0 のパス全長に対する割合)。</summary>
    public double StartOffset
    {
        get => _StartOffset;
        set => SetProperty(ref _StartOffset, value);
    }

    /// <summary>文字間に追加する隙間 (px)。負値で詰める。0 で「文字幅 (≒ FontSize × 0.6) ぶんだけ進む」。</summary>
    public double Spacing
    {
        get => _Spacing;
        set => SetProperty(ref _Spacing, value);
    }

    public TextOnPathSide Side
    {
        get => _Side;
        set => SetProperty(ref _Side, value);
    }

    public TextOnPathRotation Rotation
    {
        get => _Rotation;
        set => SetProperty(ref _Rotation, value);
    }
}
