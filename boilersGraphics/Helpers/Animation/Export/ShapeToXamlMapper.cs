using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using boilersGraphics.ViewModels.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// Phase 5.5-b: <see cref="SelectableDesignerItemViewModelBase"/> 派生図形を WPF Shape XAML 文字列に変換する pure helper。
/// Phase 5.5-b-3 で Rectangle / Ellipse / Line / Letter の 4 種に対応。Phase 5.5-b-4 で
/// Polygon + Path 系 (Path / OrthogonalConnector / AnchorBezierConnector / PolyBezier) を追加予定。
///
/// 仕様書 §6, §7, Q-7, Q-10, Q-11 に従い、属性順序は
///   <c>x:Name → Canvas.Left/Top → Width/Height → Stroke/StrokeThickness/Fill → Effect</c> + RenderTransform。
/// 色は <c>Color.ToString()</c> の <c>#AARRGGBB</c> 形式。<c>SolidColorBrush</c> 以外は <c>Brush.ToString()</c> 直書き。
/// </summary>
public static class ShapeToXamlMapper
{
    /// <summary>
    /// 図形 ViewModel から WPF Shape XAML 文字列を返す。未対応図形は null。
    /// 戻り値は外側で適切にインデント整形されることを期待しない (各行の先頭に
    /// <paramref name="indent"/> 分のスペースを付けた状態で返す)。最後の改行は付けない。
    /// </summary>
    public static string TryMapWpfShape(SelectableDesignerItemViewModelBase item, XamlExportSettings settings, int indentLevel = 0)
    {
        if (item is null || settings is null) return null;
        var indent = new string(' ', settings.IndentWidth * Math.Max(0, indentLevel));
        var nl = settings.NewLine;

        return item switch
        {
            NRectangleViewModel r => BuildRectangle(r, indent, nl),
            NEllipseViewModel e => BuildEllipse(e, indent, nl),
            StraightConnectorViewModel l => BuildLine(l, indent, nl),
            AbstractLetterDesignerItemViewModel t => BuildLetter(t, indent, nl),
            // Phase 6: TextOnPath は Placements 個別配置のため Canvas + 個別 TextBlock 群
            // で出力する。TextElementBaseViewModel ケースより先に判定する必要がある。
            TextOnPathBlockViewModel top => BuildTextOnPath(top, settings, indentLevel),
            TextElementBaseViewModel te => BuildText(te, indent, nl),
            _ => null,
        };
    }

    /// <summary>
    /// Phase 6: <see cref="TextElementBaseViewModel"/> 派生 (MonoText / DataGenerator / NumberSequence / TextMatrix)
    /// を WPF <c>TextBlock</c> XAML 文字列に変換する。Generator 系は前に <c>&lt;!-- Generator: ... --&gt;</c>
    /// コメントを出力して、出力後に同じ条件で値を再生成したいユーザーが情報を失わないようにする (仕様書 Q-3)。
    /// </summary>
    private static string BuildText(TextElementBaseViewModel t, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        var generatorComment = BuildGeneratorComment(t);
        if (generatorComment is not null)
        {
            sb.Append(indent).Append(generatorComment).Append(nl);
        }
        var text = EncodeTextAttribute(t.Text.Value ?? string.Empty);
        sb.Append(indent).Append("<TextBlock x:Name=\"").Append(MakeXName(t.ID)).Append('"').Append(nl);
        AppendCanvasLeftTop(sb, t, inner, nl);
        if (t.Width.Value > 0 || t.Height.Value > 0)
        {
            AppendWidthHeightAlways(sb, t, inner, nl);
        }
        AppendTextAppearance(sb, t, inner, nl);
        sb.Append(inner).Append("Text=\"").Append(text).Append('"').Append(nl);
        CloseOrOpenAndChildren(sb, t, "TextBlock", indent, inner, nl);
        return sb.ToString();
    }

    /// <summary>
    /// Phase 6 (Q-5 案 A): TextOnPathBlock を <c>Canvas</c> + Placements 個数分の子 <c>TextBlock</c> 群に展開する。
    /// 親 Canvas のみ x:Name を持ち、Storyboard.TargetName で参照可能 (子 TextBlock 単位のアニメは未対応)。
    /// </summary>
    private static string BuildTextOnPath(TextOnPathBlockViewModel top, XamlExportSettings settings, int indentLevel)
    {
        var indent = new string(' ', settings.IndentWidth * Math.Max(0, indentLevel));
        var nl = settings.NewLine;
        var inner = indent + new string(' ', 4);
        var charIndent = inner;
        var charInnerIndent = inner + new string(' ', 4);

        var sb = new StringBuilder();
        var generatorComment = BuildGeneratorComment(top);
        if (generatorComment is not null)
        {
            sb.Append(indent).Append(generatorComment).Append(nl);
        }
        sb.Append(indent).Append("<Canvas x:Name=\"").Append(MakeXName(top.ID)).Append('"').Append(nl);
        AppendCanvasLeftTop(sb, top, inner, nl);
        if (top.Width.Value > 0 || top.Height.Value > 0)
        {
            AppendWidthHeightAlways(sb, top, inner, nl);
        }
        var hasTransform = !IsZeroAngle(top.RotationAngle.Value);
        if (!hasTransform && top.Placements.Count == 0)
        {
            sb.Append(indent).Append("/>");
            return sb.ToString();
        }
        sb.Append(indent).Append(">").Append(nl);
        if (hasTransform)
        {
            AppendRenderTransform(sb, top.RotationAngle.Value, "Canvas", inner, nl);
        }

        // 各文字を Canvas.Left/Top + RenderTransform で配置。
        // フォント設定は親と同じ。x:Name は付与しない (TargetName 解決は親 Canvas のみ)。
        var fontFamily = ShortenFontFamily(top.FontFamily.Value);
        foreach (var p in top.Placements)
        {
            sb.Append(charIndent).Append("<TextBlock").Append(nl);
            sb.Append(charInnerIndent).Append("Canvas.Left=\"").Append(FormatDouble(p.X)).Append("\" Canvas.Top=\"").Append(FormatDouble(p.Y)).Append('"').Append(nl);
            if (t_HasFontSize(top))
            {
                sb.Append(charInnerIndent).Append("FontSize=\"").Append(FormatDouble(top.FontSize.Value)).Append('"').Append(nl);
            }
            if (!string.IsNullOrEmpty(fontFamily))
            {
                sb.Append(charInnerIndent).Append("FontFamily=\"").Append(EscapeXmlAttribute(fontFamily)).Append('"').Append(nl);
            }
            if (top.Foreground.Value is { } fg)
            {
                sb.Append(charInnerIndent).Append("Foreground=\"").Append(FormatBrush(fg)).Append('"').Append(nl);
            }
            sb.Append(charInnerIndent).Append("Text=\"").Append(EncodeTextAttribute(p.Char ?? string.Empty)).Append('"').Append(nl);
            if (IsZeroAngle(p.Angle))
            {
                sb.Append(charIndent).Append("/>").Append(nl);
            }
            else
            {
                sb.Append(charInnerIndent).Append("RenderTransformOrigin=\"0,0\">").Append(nl);
                sb.Append(charInnerIndent).Append("<TextBlock.RenderTransform>").Append(nl);
                sb.Append(charInnerIndent).Append("    <RotateTransform Angle=\"").Append(FormatDouble(p.Angle)).Append("\" />").Append(nl);
                sb.Append(charInnerIndent).Append("</TextBlock.RenderTransform>").Append(nl);
                sb.Append(charIndent).Append("</TextBlock>").Append(nl);
            }
        }
        sb.Append(indent).Append("</Canvas>");
        return sb.ToString();
    }

    /// <summary>
    /// 共通テキスト属性 (FontSize / FontFamily / Foreground / Background / Opacity / LineHeight / TextWrapping)。
    /// </summary>
    private static void AppendTextAppearance(StringBuilder sb, TextElementBaseViewModel t, string inner, string nl)
    {
        if (t_HasFontSize(t))
        {
            sb.Append(inner).Append("FontSize=\"").Append(FormatDouble(t.FontSize.Value)).Append('"').Append(nl);
        }
        var fontFamily = ShortenFontFamily(t.FontFamily.Value);
        if (!string.IsNullOrEmpty(fontFamily))
        {
            sb.Append(inner).Append("FontFamily=\"").Append(EscapeXmlAttribute(fontFamily)).Append('"').Append(nl);
        }
        if (t.Foreground.Value is { } fg)
        {
            sb.Append(inner).Append("Foreground=\"").Append(FormatBrush(fg)).Append('"').Append(nl);
        }
        if (t.Background.Value is { } bg)
        {
            sb.Append(inner).Append("Background=\"").Append(FormatBrush(bg)).Append('"').Append(nl);
        }
        if (Math.Abs(t.TextOpacity.Value - 1.0) > 1e-9)
        {
            sb.Append(inner).Append("Opacity=\"").Append(FormatDouble(t.TextOpacity.Value)).Append('"').Append(nl);
        }
        if (t.LineHeight.Value is { } lh && lh > 0)
        {
            sb.Append(inner).Append("LineHeight=\"").Append(FormatDouble(lh)).Append('"').Append(nl);
        }
        sb.Append(inner).Append("TextWrapping=\"").Append(t.IsWordWrap.Value ? "Wrap" : "NoWrap").Append('"').Append(nl);
    }

    private static bool t_HasFontSize(TextElementBaseViewModel t) => t.FontSize.Value > 0;

    /// <summary>
    /// pack:// URI 形式の FontFamily を短縮名のみに変換する (仕様書 Q-2)。
    /// 例: <c>pack://application:,,,/...;component/Fonts/#JetBrains Mono</c> → <c>JetBrains Mono</c>
    /// '#' がなければそのまま返す。
    /// </summary>
    public static string ShortenFontFamily(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        var idx = raw.LastIndexOf('#');
        if (idx >= 0 && idx + 1 < raw.Length) return raw.Substring(idx + 1);
        return raw;
    }

    /// <summary>
    /// XAML 属性内のテキストエンコード。改行は <c>&amp;#x0A;</c> に変換 (TextMatrix の行区切り対応)。
    /// </summary>
    public static string EncodeTextAttribute(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("&", "&amp;")
                .Replace("\"", "&quot;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\r\n", "&#x0A;")
                .Replace("\n", "&#x0A;")
                .Replace("\r", "&#x0A;");
    }

    /// <summary>
    /// DataGenerator / NumberSequence / TextMatrix / TextOnPath の場合に、生成パラメータを
    /// XAML コメントとして残す (仕様書 Q-3)。MonoText 等パラメータ無しの場合は null。
    /// MAUI 側 <see cref="MauiShapeToXamlMapper"/> でも同じ書式を再利用する。
    /// </summary>
    internal static string BuildGeneratorComment(TextElementBaseViewModel t)
    {
        return t switch
        {
            DataGeneratorTextBlockViewModel dg =>
                $"<!-- Generator: DataGenerator (Type={dg.Type.Value}, Seed={dg.Seed.Value}, Count={dg.Count.Value}, Separator=\"{EscapeForComment(dg.Separator.Value)}\", Layout={dg.Layout.Value}) -->",
            NumberSequenceBlockViewModel ns =>
                $"<!-- Generator: NumberSequence (Start={FormatDouble(ns.Start.Value)}, End={FormatDouble(ns.End.Value)}, Step={FormatDouble(ns.Step.Value)}, Format=\"{EscapeForComment(ns.Format.Value)}\", Separator=\"{EscapeForComment(ns.Separator.Value)}\", Direction={ns.Direction.Value}, GridRows={ns.GridRows.Value}, GridColumns={ns.GridColumns.Value}) -->",
            TextMatrixBlockViewModel tm =>
                $"<!-- Generator: TextMatrix (Rows={tm.Rows.Value}, Columns={tm.Columns.Value}, CellMode={tm.CellMode.Value}, Separator=\"{EscapeForComment(tm.Separator.Value)}\", SequenceStart={tm.SequenceStart.Value}, SequenceFormat=\"{EscapeForComment(tm.SequenceFormat.Value)}\", DataGenType={tm.DataGenType.Value}, DataGenSeed={tm.DataGenSeed.Value}) -->",
            TextOnPathBlockViewModel top =>
                $"<!-- Generator: TextOnPath (PathRefId={top.PathReferenceId.Value?.ToString("N") ?? "null"}, StartOffset={FormatDouble(top.StartOffset.Value)}, Spacing={FormatDouble(top.Spacing.Value)}, Side={top.Side.Value}, Rotation={top.Rotation.Value}, Placements={top.Placements.Count}) -->",
            _ => null,
        };
    }

    /// <summary>
    /// XAML コメント内 (<c>&lt;!-- ... --&gt;</c>) で安全な文字列に整形する。連続ハイフン (-) はコメント終端と
    /// 区別できないため空白を入れる。改行は <c>\n</c> リテラル表記に置換。
    /// </summary>
    private static string EscapeForComment(string s)
    {
        if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
        return s.Replace("--", "- -")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n")
                .Replace("\r", "\\n");
    }

    /// <summary>
    /// 仕様書 §6 の <c>x:Name="Item_{Guid:N}"</c>。Storyboard.TargetName と一致する形で出力する。
    /// </summary>
    public static string MakeXName(Guid id) => "Item_" + id.ToString("N");

    /// <summary>
    /// Path 系図形 (PathDesignerItemViewModel / NPolygonViewModel /
    /// PolyBezierViewModel / OrthogonalConnectorViewModel / AnchorBezierConnectorViewModel)
    /// を WPF Shape XAML 文字列に変換する。<paramref name="geometry"/> は呼び出し側で計算済みの
    /// PathGeometry (実描画上のそれ) を渡す。<see cref="TryMapWpfShape"/> の Pure 性を保つため、
    /// PathGeometry 生成自体は呼び出し側 (Phase 5.5-c の Exporter) の責務とする。
    ///
    /// <see cref="NPolygonViewModel"/> のみは <see cref="ExtractPolygonPoints"/> で
    /// "x1,y1 x2,y2 ..." 形式が取れれば <c>&lt;Polygon Points="..."/&gt;</c>、取れなければ
    /// <c>&lt;Path Data="..."/&gt;</c> にフォールバック。それ以外は常に <c>&lt;Path&gt;</c>。
    /// </summary>
    public static string TryMapWpfPath(SelectableDesignerItemViewModelBase item, PathGeometry geometry, XamlExportSettings settings, int indentLevel = 0)
    {
        if (item is null || settings is null || geometry is null) return null;

        var isPathFamily = item is PathDesignerItemViewModel
                        or NPolygonViewModel
                        or PolyBezierViewModel
                        or OrthogonalConnectorViewModel
                        or AnchorBezierConnectorViewModel;
        if (!isPathFamily) return null;

        var indent = new string(' ', settings.IndentWidth * Math.Max(0, indentLevel));
        var nl = settings.NewLine;

        if (item is NPolygonViewModel poly)
        {
            var points = ExtractPolygonPoints(geometry);
            if (points is not null) return BuildPolygon(poly, points, indent, nl);
        }

        return BuildPath(item, geometry, indent, nl);
    }

    /// <summary>
    /// 直線セグメントのみで構成された <see cref="PathGeometry"/> から
    /// <c>Polygon.Points</c> 形式の文字列を返す。非直線セグメント (Arc / Bezier) を含む場合は null。
    /// 最初の Figure のみ扱う (Phase 5.5-b では複合 Figure は将来枠)。
    /// </summary>
    public static string ExtractPolygonPoints(PathGeometry geometry)
    {
        if (geometry is null) return null;
        if (geometry.Figures is null || geometry.Figures.Count == 0) return null;
        var fig = geometry.Figures[0];
        var pts = new List<Point> { fig.StartPoint };
        foreach (var seg in fig.Segments)
        {
            switch (seg)
            {
                case LineSegment ls: pts.Add(ls.Point); break;
                case PolyLineSegment pls: pts.AddRange(pls.Points); break;
                default: return null;
            }
        }
        return string.Join(" ", pts.Select(p => FormatDouble(p.X) + "," + FormatDouble(p.Y)));
    }

    private static string BuildPolygon(NPolygonViewModel p, string points, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Polygon x:Name=\"").Append(MakeXName(p.ID)).Append('"').Append(nl);
        AppendCanvasLeftTop(sb, p, inner, nl);
        AppendWidthHeightAlways(sb, p, inner, nl);
        sb.Append(inner).Append("Points=\"").Append(points).Append('"').Append(nl);
        AppendStrokeFill(sb, p, inner, nl);
        CloseOrOpenAndChildren(sb, p, "Polygon", indent, inner, nl);
        return sb.ToString();
    }

    private static string BuildPath(SelectableDesignerItemViewModelBase item, PathGeometry geometry, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Path x:Name=\"").Append(MakeXName(item.ID)).Append('"').Append(nl);
        if (item is DesignerItemViewModelBase d)
        {
            AppendCanvasLeftTop(sb, d, inner, nl);
            if (d.Width.Value > 0 || d.Height.Value > 0)
            {
                AppendWidthHeightAlways(sb, d, inner, nl);
            }
        }
        // Path.Fill / Path.Stroke はそれぞれ Stroke / Fill 属性で出す (Shape 共通)
        AppendStrokeFill(sb, item, inner, nl);
        sb.Append(inner).Append("Data=\"").Append(EscapeXmlAttribute(geometry.ToString(CultureInfo.InvariantCulture))).Append('"').Append(nl);
        CloseOrOpenAndChildren(sb, item, "Path", indent, inner, nl);
        return sb.ToString();
    }

    private static string BuildRectangle(NRectangleViewModel r, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Rectangle x:Name=\"").Append(MakeXName(r.ID)).Append('"').Append(nl);
        AppendCanvasLeftTop(sb, r, inner, nl);
        AppendWidthHeightAlways(sb, r, inner, nl);
        AppendRadius(sb, r.RadiusX.Value, r.RadiusY.Value, inner, nl);
        AppendStrokeFill(sb, r, inner, nl);
        CloseOrOpenAndChildren(sb, r, "Rectangle", indent, inner, nl);
        return sb.ToString();
    }

    private static string BuildEllipse(NEllipseViewModel e, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Ellipse x:Name=\"").Append(MakeXName(e.ID)).Append('"').Append(nl);
        AppendCanvasLeftTop(sb, e, inner, nl);
        AppendWidthHeightAlways(sb, e, inner, nl);
        AppendStrokeFill(sb, e, inner, nl);
        CloseOrOpenAndChildren(sb, e, "Ellipse", indent, inner, nl);
        return sb.ToString();
    }

    private static string BuildLine(StraightConnectorViewModel l, string indent, string nl)
    {
        // P1/P2 が null (= 未初期化) の場合は 0 にフォールバック。Phase 5 IR 的にはあり得ないが、安全策。
        var p1x = l.P1X?.Value ?? 0.0;
        var p1y = l.P1Y?.Value ?? 0.0;
        var p2x = l.P2X?.Value ?? 0.0;
        var p2y = l.P2Y?.Value ?? 0.0;

        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Line x:Name=\"").Append(MakeXName(l.ID)).Append('"').Append(nl);
        sb.Append(inner).Append("X1=\"").Append(FormatDouble(p1x)).Append("\" Y1=\"").Append(FormatDouble(p1y)).Append('"').Append(nl);
        sb.Append(inner).Append("X2=\"").Append(FormatDouble(p2x)).Append("\" Y2=\"").Append(FormatDouble(p2y)).Append('"').Append(nl);
        AppendStrokeFill(sb, l, inner, nl, includeFill: false);
        // Line は RotateTransform を出さない (P1/P2 で十分)
        var hasEffect = HasGlow(l);
        if (!hasEffect)
        {
            sb.Append(indent).Append("/>");
            return sb.ToString();
        }
        sb.Append(indent).Append(">").Append(nl);
        AppendEffect(sb, l, "Line", inner, nl);
        sb.Append(indent).Append("</Line>");
        return sb.ToString();
    }

    private static string BuildLetter(AbstractLetterDesignerItemViewModel t, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        var text = EscapeXmlAttribute(t.LetterString.Value ?? string.Empty);
        sb.Append(indent).Append("<TextBlock x:Name=\"").Append(MakeXName(t.ID)).Append('"').Append(nl);
        AppendCanvasLeftTop(sb, t, inner, nl);
        if (t.Width.Value > 0 || t.Height.Value > 0)
        {
            AppendWidthHeightAlways(sb, t, inner, nl);
        }
        var fontSize = t.FontSize.Value;
        if (fontSize > 0)
        {
            sb.Append(inner).Append("FontSize=\"").Append(FormatDouble(fontSize)).Append('"').Append(nl);
        }
        if (t.SelectedFontFamily.Value is { FamilyName: { } familyName } && !string.IsNullOrEmpty(familyName))
        {
            sb.Append(inner).Append("FontFamily=\"").Append(EscapeXmlAttribute(familyName)).Append('"').Append(nl);
        }
        if (t.IsBold.Value) sb.Append(inner).Append("FontWeight=\"Bold\"").Append(nl);
        if (t.IsItalic.Value) sb.Append(inner).Append("FontStyle=\"Italic\"").Append(nl);
        // FillBrush は Foreground にマップ (色アニメーション対応のため Stroke 経由ではなく Foreground)
        if (t.FillBrush.Value is { } fg) sb.Append(inner).Append("Foreground=\"").Append(FormatBrush(fg)).Append('"').Append(nl);
        sb.Append(inner).Append("Text=\"").Append(text).Append('"').Append(nl);
        CloseOrOpenAndChildren(sb, t, "TextBlock", indent, inner, nl);
        return sb.ToString();
    }

    // --------- 共通ヘルパ ---------

    /// <summary>
    /// 全角 / 任意の余白あり前提の属性出力。RotationAngle / Effect の有無で self-close か open-close を切替える。
    /// </summary>
    private static void CloseOrOpenAndChildren(StringBuilder sb, SelectableDesignerItemViewModelBase item, string shapeTag, string indent, string inner, string nl)
    {
        var hasTransform = !IsZeroAngle(item.RotationAngle.Value);
        var hasEffect = HasGlow(item);
        if (!hasTransform && !hasEffect)
        {
            sb.Append(indent).Append("/>");
            return;
        }
        sb.Append(indent).Append(">").Append(nl);
        AppendRenderTransform(sb, item.RotationAngle.Value, shapeTag, inner, nl);
        AppendEffect(sb, item, shapeTag, inner, nl);
        sb.Append(indent).Append("</").Append(shapeTag).Append('>');
    }

    private static void AppendCanvasLeftTop(StringBuilder sb, DesignerItemViewModelBase d, string inner, string nl)
    {
        sb.Append(inner).Append("Canvas.Left=\"").Append(FormatDouble(d.Left.Value)).Append("\" Canvas.Top=\"").Append(FormatDouble(d.Top.Value)).Append('"').Append(nl);
    }

    private static void AppendWidthHeightAlways(StringBuilder sb, DesignerItemViewModelBase d, string inner, string nl)
    {
        sb.Append(inner).Append("Width=\"").Append(FormatDouble(d.Width.Value)).Append("\" Height=\"").Append(FormatDouble(d.Height.Value)).Append('"').Append(nl);
    }

    private static void AppendRadius(StringBuilder sb, double rx, double ry, string inner, string nl)
    {
        if (rx <= 0 && ry <= 0) return;
        sb.Append(inner).Append("RadiusX=\"").Append(FormatDouble(rx)).Append("\" RadiusY=\"").Append(FormatDouble(ry)).Append('"').Append(nl);
    }

    private static void AppendStrokeFill(StringBuilder sb, SelectableDesignerItemViewModelBase item, string inner, string nl, bool includeFill = true)
    {
        if (item.EdgeBrush.Value is { } eb)
        {
            sb.Append(inner).Append("Stroke=\"").Append(FormatBrush(eb)).Append('"').Append(nl);
        }
        if (item.EdgeThickness.Value > 0)
        {
            sb.Append(inner).Append("StrokeThickness=\"").Append(FormatDouble(item.EdgeThickness.Value)).Append('"').Append(nl);
        }
        if (includeFill && item.FillBrush.Value is { } fb)
        {
            sb.Append(inner).Append("Fill=\"").Append(FormatBrush(fb)).Append('"').Append(nl);
        }
        if (item.StrokeDashArray.Value is { Count: > 0 } da)
        {
            sb.Append(inner).Append("StrokeDashArray=\"").Append(da.ToString(CultureInfo.InvariantCulture)).Append('"').Append(nl);
        }
        if (item.StrokeLineJoin.Value != PenLineJoin.Miter)
        {
            sb.Append(inner).Append("StrokeLineJoin=\"").Append(item.StrokeLineJoin.Value).Append('"').Append(nl);
        }
    }

    private static void AppendRenderTransform(StringBuilder sb, double angle, string shapeTag, string inner, string nl)
    {
        if (IsZeroAngle(angle)) return;
        sb.Append(inner).Append('<').Append(shapeTag).Append(".RenderTransform>").Append(nl);
        sb.Append(inner).Append("    <RotateTransform Angle=\"").Append(FormatDouble(angle)).Append("\" />").Append(nl);
        sb.Append(inner).Append("</").Append(shapeTag).Append(".RenderTransform>").Append(nl);
    }

    private static void AppendEffect(StringBuilder sb, SelectableDesignerItemViewModelBase item, string shapeTag, string inner, string nl)
    {
        if (!HasGlow(item)) return;
        sb.Append(inner).Append('<').Append(shapeTag).Append(".Effect>").Append(nl);
        sb.Append(inner).Append("    <DropShadowEffect BlurRadius=\"").Append(FormatDouble(item.GlowRadius.Value)).Append('"');
        sb.Append(" ShadowDepth=\"0\"");
        sb.Append(" Opacity=\"").Append(FormatDouble(item.GlowIntensity.Value)).Append('"');
        if (item.GlowColor.Value is { } gc)
        {
            sb.Append(" Color=\"").Append(FormatColor(gc)).Append('"');
        }
        sb.Append(" />").Append(nl);
        sb.Append(inner).Append("</").Append(shapeTag).Append(".Effect>").Append(nl);
    }

    private static bool HasGlow(SelectableDesignerItemViewModelBase item) =>
        item.GlowRadius.Value > 0 || item.GlowColor.Value is not null;

    private static bool IsZeroAngle(double angle) => Math.Abs(angle) < 1e-9;

    /// <summary>
    /// double 値の文字列化。
    /// XAML / WPF は invariant culture (= "." 小数点) を期待するので、必ず CultureInfo.InvariantCulture を渡す。
    /// </summary>
    private static string FormatDouble(double v) => v.ToString("0.######", CultureInfo.InvariantCulture);

    private static string FormatColor(Color c) => c.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Brush の XAML 表現。SolidColorBrush は Color の AARRGGBB を直接、それ以外は ToString() 結果をフォールバック。
    /// 仕様書 Q-10 案 A 「テーマ自体は出力時点での値を埋め込み」に従う。
    /// </summary>
    private static string FormatBrush(Brush brush)
    {
        if (brush is SolidColorBrush scb) return FormatColor(scb.Color);
        return brush.ToString(CultureInfo.InvariantCulture);
    }

    private static string EscapeXmlAttribute(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("&", "&amp;")
                .Replace("\"", "&quot;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
    }
}
