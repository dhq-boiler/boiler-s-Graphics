using Prism.Mvvm;
using System;
using System.Windows.Media;

namespace boilersGraphics.Models.Themes;

/// <summary>
/// Phase 4-a §3.3 / §4: テーマ単位で持つデフォルトグロー設定。
/// 個別図形のグローは <see cref="boilersGraphics.ViewModels.SelectableDesignerItemViewModelBase"/> に
/// 直接 GlowRadius / GlowIntensity / GlowColor として持たせる (Q-9 案 A、非破壊)。
/// このクラスは「テーマ既定値」のみを保持する POCO に近い役割。
/// </summary>
[Serializable]
public class GlowSettings : BindableBase
{
    private double _Radius;
    private double _Intensity = 0.5;
    private Color? _Color;

    /// <summary>ぼかし半径 (px)。0 でグロー無効。</summary>
    public double Radius
    {
        get => _Radius;
        set => SetProperty(ref _Radius, value);
    }

    /// <summary>加算合成の強度 (0..1)。</summary>
    public double Intensity
    {
        get => _Intensity;
        set => SetProperty(ref _Intensity, value);
    }

    /// <summary>グロー色。null の場合は EdgeBrush と同色で合成。</summary>
    public Color? Color
    {
        get => _Color;
        set => SetProperty(ref _Color, value);
    }
}
