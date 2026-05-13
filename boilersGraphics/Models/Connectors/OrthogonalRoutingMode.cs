namespace boilersGraphics.Models.Connectors;

/// <summary>
/// Phase 3-a §3.1 / Q-2 案 A: L 字 (Orthogonal) コネクタのルーティングモード。
/// Auto は始点・終点の差分から HFirst/VFirst を自動選択し、Manual はユーザが MidPoints を直接編集する。
/// </summary>
public enum OrthogonalRoutingMode
{
    /// <summary>差分の大きい方向を先に進む (横の差分 ≥ 縦なら HFirst、それ以外 VFirst)。</summary>
    Auto,

    /// <summary>水平方向 → 垂直方向の順に直角に曲がる (1 中間点)。</summary>
    HFirst,

    /// <summary>垂直方向 → 水平方向の順に直角に曲がる (1 中間点)。</summary>
    VFirst,

    /// <summary>ユーザが MidPoints を 0..N 個明示する。直角制約は付けない (自由な折れ線)。</summary>
    Manual,
}
