namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-b: TextOnPath で 1 文字を配置するための位置・回転情報。
/// DataTemplate 側で ItemsControl + Canvas + RenderTransform でレンダリングする想定。
/// </summary>
public sealed class TextOnPathCharPlacement
{
    public string Char { get; init; } = string.Empty;

    /// <summary>キャンバス上 X 座標 (要素ローカル)。</summary>
    public double X { get; init; }

    /// <summary>キャンバス上 Y 座標 (要素ローカル)。</summary>
    public double Y { get; init; }

    /// <summary>回転角度 (度数法)。Upright モードでは常に 0。</summary>
    public double Angle { get; init; }
}
