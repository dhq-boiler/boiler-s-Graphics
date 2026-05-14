namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// MAUI Animation API は double 1 次元しか直接補間できないので、Color 系は callback 内で
/// fromColor/toColor を補間する。<see cref="Double"/> は単純な double 値、<see cref="Color"/> は
/// fromColor/toColor から callback 内で <c>Color c</c> を計算する想定。
/// </summary>
public enum MauiAnimationKind { Double, Color }

/// <summary>
/// Phase 5.5-d-2: Phase 5 <see cref="boilersGraphics.Models.Animation.PropertyRef.PropertyPath"/> を
/// MAUI コードビハインド向けの callback 本体テンプレ + アニメ種別に変換する pure helper。
///
/// 仕様書 §7 マッピング表 (MAUI 列) に従う。<see cref="MauiAnimationKind.Double"/> 系は
/// <see cref="MauiPropertyMapping.DoubleCallbackTemplate"/> を、<see cref="MauiAnimationKind.Color"/>
/// 系は <see cref="MauiPropertyMapping.ColorCallbackTemplate"/> をそれぞれ
/// 「{0}=ターゲット名、d=double 値、c=lerp 済み Color」のテンプレ文字列として返す。
/// </summary>
public static class MauiPropertyToCSharpMapper
{
    public sealed record class MauiPropertyMapping(
        MauiAnimationKind Kind,
        string DoubleCallbackTemplate,
        string ColorCallbackTemplate);

    public static MauiPropertyMapping TryMap(string propertyPath) => propertyPath switch
    {
        // Left/Top: AbsoluteLayout.LayoutBounds の X/Y 成分のみ更新。
        // 他の Track が同時に動いていても破壊しない (Rect の他成分を都度読み出して再構築)。
        "Left.Value" => Double(
            "var __b = AbsoluteLayout.GetLayoutBounds({0}); AbsoluteLayout.SetLayoutBounds({0}, new Rect(d, __b.Y, __b.Width, __b.Height));"),
        "Top.Value" => Double(
            "var __b = AbsoluteLayout.GetLayoutBounds({0}); AbsoluteLayout.SetLayoutBounds({0}, new Rect(__b.X, d, __b.Width, __b.Height));"),
        "Width.Value" => Double("{0}.WidthRequest = d;"),
        "Height.Value" => Double("{0}.HeightRequest = d;"),
        "RotationAngle.Value" => Double("{0}.Rotation = d;"),
        "EdgeThickness.Value" => Double("{0}.StrokeThickness = d;"),
        "GlowRadius.Value" => Double("if ({0}.Shadow != null) {0}.Shadow.Radius = d;"),
        "GlowIntensity.Value" => Double("if ({0}.Shadow != null) {0}.Shadow.Opacity = (float)d;"),

        "EdgeBrush.Value" => Color("{0}.Stroke = new SolidColorBrush(c);"),
        "FillBrush.Value" => Color("{0}.Fill = new SolidColorBrush(c);"),
        "GlowColor.Value" => Color("if ({0}.Shadow != null) {0}.Shadow.Brush = new SolidColorBrush(c);"),

        _ => null,
    };

    public static bool IsSupported(string propertyPath) => TryMap(propertyPath) is not null;

    private static MauiPropertyMapping Double(string template) =>
        new(MauiAnimationKind.Double, template, null);

    private static MauiPropertyMapping Color(string template) =>
        new(MauiAnimationKind.Color, null, template);
}
