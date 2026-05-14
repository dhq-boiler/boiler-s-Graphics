using Prism.Mvvm;
using System;
using System.Windows.Media;

namespace boilersGraphics.Models.Themes;

/// <summary>
/// Phase 4-a §3.2 / Q-5 案 B: 完全新規 LineStyle 型。
/// StrokeDashArray + StrokeLineJoin + 簡易グロー設定を 1 つにまとめる。
/// </summary>
[Serializable]
public class LineStyle : BindableBase
{
    private string _Name;
    private Guid _Id = Guid.NewGuid();
    private bool _IsBuiltIn;
    private DoubleCollection _StrokeDashArray = new();
    private PenLineJoin _StrokeLineJoin = PenLineJoin.Miter;
    private double? _GlowRadius;
    private double? _GlowIntensity;

    /// <summary>線種表示名 ("Solid" / "Dash" / "Stepped" 等)。</summary>
    public string Name
    {
        get => _Name;
        set => SetProperty(ref _Name, value);
    }

    /// <summary>線種一意 ID。</summary>
    public Guid Id
    {
        get => _Id;
        set => SetProperty(ref _Id, value);
    }

    /// <summary>組み込み線種なら true。ユーザー追加なら false。</summary>
    public bool IsBuiltIn
    {
        get => _IsBuiltIn;
        set => SetProperty(ref _IsBuiltIn, value);
    }

    /// <summary>StrokeDashArray。空コレクションでベタ線。</summary>
    public DoubleCollection StrokeDashArray
    {
        get => _StrokeDashArray;
        set => SetProperty(ref _StrokeDashArray, value);
    }

    /// <summary>StrokeLineJoin。デフォルト <see cref="PenLineJoin.Miter"/>。</summary>
    public PenLineJoin StrokeLineJoin
    {
        get => _StrokeLineJoin;
        set => SetProperty(ref _StrokeLineJoin, value);
    }

    /// <summary>グロー半径 (px)。null ならグロー未付与。</summary>
    public double? GlowRadius
    {
        get => _GlowRadius;
        set => SetProperty(ref _GlowRadius, value);
    }

    /// <summary>グロー強度 (0..1)。null ならグロー未付与。</summary>
    public double? GlowIntensity
    {
        get => _GlowIntensity;
        set => SetProperty(ref _GlowIntensity, value);
    }
}
