using boilersGraphics.Models.Animation;

namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// Phase 5.5-b: <see cref="PropertyRef.PropertyPath"/> + <see cref="AnimatedValueType"/> を
/// WPF Storyboard で使う <c>Storyboard.TargetProperty</c> 文字列 + アニメ要素種別に変換する pure helper。
///
/// 仕様書: <c>docs/fui/phase5-5-xaml-export.md</c> §7 マッピング表。Phase 5 <see cref="PropertyApplier"/> が
/// 扱う全 11 パスに対応 (ExposedProperties[guid] は Q-9 案 B で展開後の RenderedItems パスに解決済み前提)。
/// </summary>
public static class PropertyToXamlMapper
{
    /// <summary>
    /// 1 つの PropertyPath に対する WPF Storyboard XAML マッピング情報。
    /// </summary>
    /// <param name="TargetProperty"><c>Storyboard.TargetProperty</c> 属性の文字列 (例: "(Canvas.Left)")。</param>
    /// <param name="AnimationElementName">アニメ要素名 (例: "DoubleAnimationUsingKeyFrames")。</param>
    /// <param name="EasingKeyFrameElementName">Easing 系キーフレーム要素名 (例: "EasingDoubleKeyFrame")。</param>
    /// <param name="LinearKeyFrameElementName">線形キーフレーム要素名 (例: "LinearDoubleKeyFrame")。</param>
    public sealed record class WpfMapping(
        string TargetProperty,
        string AnimationElementName,
        string EasingKeyFrameElementName,
        string LinearKeyFrameElementName);

    /// <summary>
    /// 対応する <see cref="WpfMapping"/> を返す。未対応 PropertyPath は null。
    /// </summary>
    public static WpfMapping TryMapWpf(string propertyPath) => propertyPath switch
    {
        "Left.Value" => MakeDouble("(Canvas.Left)"),
        "Top.Value" => MakeDouble("(Canvas.Top)"),
        "Width.Value" => MakeDouble("Width"),
        "Height.Value" => MakeDouble("Height"),
        "RotationAngle.Value" => MakeDouble("(UIElement.RenderTransform).(RotateTransform.Angle)"),
        "EdgeBrush.Value" => MakeColor("(Shape.Stroke).(SolidColorBrush.Color)"),
        "FillBrush.Value" => MakeColor("(Shape.Fill).(SolidColorBrush.Color)"),
        "EdgeThickness.Value" => MakeDouble("StrokeThickness"),
        "GlowRadius.Value" => MakeDouble("(UIElement.Effect).(DropShadowEffect.BlurRadius)"),
        "GlowIntensity.Value" => MakeDouble("(UIElement.Effect).(DropShadowEffect.Opacity)"),
        "GlowColor.Value" => MakeColor("(UIElement.Effect).(DropShadowEffect.Color)"),
        _ => null,
    };

    /// <summary>
    /// <see cref="TryMapWpf"/> の戻り値が null になるパスかどうかを判定する shortcut。
    /// </summary>
    public static bool IsSupportedWpf(string propertyPath) => TryMapWpf(propertyPath) is not null;

    private static WpfMapping MakeDouble(string targetProperty) =>
        new(targetProperty,
            AnimationElementName: "DoubleAnimationUsingKeyFrames",
            EasingKeyFrameElementName: "EasingDoubleKeyFrame",
            LinearKeyFrameElementName: "LinearDoubleKeyFrame");

    private static WpfMapping MakeColor(string targetProperty) =>
        new(targetProperty,
            AnimationElementName: "ColorAnimationUsingKeyFrames",
            EasingKeyFrameElementName: "EasingColorKeyFrame",
            LinearKeyFrameElementName: "LinearColorKeyFrame");
}
