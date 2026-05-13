using Prism.Mvvm;
using System;

namespace boilersGraphics.Models.Anchors;

/// <summary>
/// Phase 3-a §3.3.2 / Q-6 案 A: ユーザが「アンカー追加」ツールで明示的に登録する Anchor。
/// 暗黙 9 点 (<see cref="AnchorPosition"/>) とは別系統で、シリアライズ対象になる。
/// </summary>
[Serializable]
public class Anchor : BindableBase
{
    private Guid _Id = Guid.NewGuid();
    private Guid _OwnerId;
    private double _RelativeX;
    private double _RelativeY;
    private string _Name;

    /// <summary>アンカー一意 ID。</summary>
    public Guid Id
    {
        get => _Id;
        set => SetProperty(ref _Id, value);
    }

    /// <summary>紐づく DesignerItem の ID。</summary>
    public Guid OwnerId
    {
        get => _OwnerId;
        set => SetProperty(ref _OwnerId, value);
    }

    /// <summary>図形の Bounds に対する相対 X (0.0〜1.0)。範囲外も技術的には許容。</summary>
    public double RelativeX
    {
        get => _RelativeX;
        set => SetProperty(ref _RelativeX, value);
    }

    /// <summary>図形の Bounds に対する相対 Y (0.0〜1.0)。</summary>
    public double RelativeY
    {
        get => _RelativeY;
        set => SetProperty(ref _RelativeY, value);
    }

    /// <summary>UI 表示用の任意ラベル (省略可)。</summary>
    public string Name
    {
        get => _Name;
        set => SetProperty(ref _Name, value);
    }
}
