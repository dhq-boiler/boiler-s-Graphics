using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using System;
using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// Phase 5.5-d-3: <see cref="SelectableDesignerItemViewModelBase"/> 派生図形を MAUI
/// <c>Microsoft.Maui.Controls.Shapes</c> 系の XAML 文字列に変換する pure helper。
///
/// 仕様書 §6.2 / Q-7 / Q-11 (属性順序: x:Name → AbsoluteLayout.LayoutBounds →
/// WidthRequest/HeightRequest → Stroke/Fill → Shadow → Rotation)。
///
/// <see cref="ShapeToXamlMapper"/> (WPF) と同じく <c>TryMapMauiShape</c> は形状必須のものを、
/// <c>TryMapMauiPath</c> は呼び出し側が用意した PathGeometry を必要とする Path 系を担当する。
/// </summary>
public static class MauiShapeToXamlMapper
{
    public static string MakeXName(Guid id) => ShapeToXamlMapper.MakeXName(id);

    public static string TryMapMauiShape(SelectableDesignerItemViewModelBase item, XamlExportSettings settings, int indentLevel = 0)
    {
        if (item is null || settings is null) return null;
        var indent = new string(' ', settings.IndentWidth * Math.Max(0, indentLevel));
        var nl = settings.NewLine;
        return item switch
        {
            NRectangleViewModel r => BuildRectangle(r, indent, nl),
            NEllipseViewModel e => BuildEllipse(e, indent, nl),
            StraightConnectorViewModel l => BuildLine(l, indent, nl),
            AbstractLetterDesignerItemViewModel t => BuildLabel(t, indent, nl),
            _ => null,
        };
    }

    public static string TryMapMauiPath(SelectableDesignerItemViewModelBase item, PathGeometry geometry, XamlExportSettings settings, int indentLevel = 0)
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
            var points = ShapeToXamlMapper.ExtractPolygonPoints(geometry);
            if (points is not null) return BuildPolygon(poly, points, indent, nl);
        }
        return BuildPath(item, geometry, indent, nl);
    }

    private static string BuildRectangle(NRectangleViewModel r, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Rectangle x:Name=\"").Append(MakeXName(r.ID)).Append('"').Append(nl);
        AppendAbsoluteLayoutBounds(sb, r, inner, nl);
        AppendRadius(sb, r.RadiusX.Value, r.RadiusY.Value, inner, nl);
        AppendStrokeFill(sb, r, inner, nl);
        AppendRotationAndShadow(sb, r, indent, inner, nl, hasClosingTag: true, shapeTag: "Rectangle");
        return sb.ToString();
    }

    private static string BuildEllipse(NEllipseViewModel e, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Ellipse x:Name=\"").Append(MakeXName(e.ID)).Append('"').Append(nl);
        AppendAbsoluteLayoutBounds(sb, e, inner, nl);
        AppendStrokeFill(sb, e, inner, nl);
        AppendRotationAndShadow(sb, e, indent, inner, nl, hasClosingTag: true, shapeTag: "Ellipse");
        return sb.ToString();
    }

    private static string BuildLine(StraightConnectorViewModel l, string indent, string nl)
    {
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
        AppendRotationAndShadow(sb, l, indent, inner, nl, hasClosingTag: true, shapeTag: "Line", emitRotation: false);
        return sb.ToString();
    }

    private static string BuildLabel(AbstractLetterDesignerItemViewModel t, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        var text = EscapeXmlAttribute(t.LetterString.Value ?? string.Empty);
        sb.Append(indent).Append("<Label x:Name=\"").Append(MakeXName(t.ID)).Append('"').Append(nl);
        AppendAbsoluteLayoutBounds(sb, t, inner, nl);
        if (t.FontSize.Value > 0)
        {
            sb.Append(inner).Append("FontSize=\"").Append(FormatDouble(t.FontSize.Value)).Append('"').Append(nl);
        }
        if (t.SelectedFontFamily.Value is { FamilyName: { } family } && !string.IsNullOrEmpty(family))
        {
            sb.Append(inner).Append("FontFamily=\"").Append(EscapeXmlAttribute(family)).Append('"').Append(nl);
        }
        if (t.IsBold.Value || t.IsItalic.Value)
        {
            sb.Append(inner).Append("FontAttributes=\"");
            if (t.IsBold.Value) sb.Append("Bold");
            if (t.IsBold.Value && t.IsItalic.Value) sb.Append(',');
            if (t.IsItalic.Value) sb.Append("Italic");
            sb.Append('"').Append(nl);
        }
        if (t.FillBrush.Value is { } fg)
        {
            sb.Append(inner).Append("TextColor=\"").Append(FormatBrush(fg)).Append('"').Append(nl);
        }
        sb.Append(inner).Append("Text=\"").Append(text).Append('"').Append(nl);
        AppendRotationAndShadow(sb, t, indent, inner, nl, hasClosingTag: true, shapeTag: "Label");
        return sb.ToString();
    }

    private static string BuildPolygon(NPolygonViewModel p, string points, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Polygon x:Name=\"").Append(MakeXName(p.ID)).Append('"').Append(nl);
        AppendAbsoluteLayoutBounds(sb, p, inner, nl);
        sb.Append(inner).Append("Points=\"").Append(points).Append('"').Append(nl);
        AppendStrokeFill(sb, p, inner, nl);
        AppendRotationAndShadow(sb, p, indent, inner, nl, hasClosingTag: true, shapeTag: "Polygon");
        return sb.ToString();
    }

    private static string BuildPath(SelectableDesignerItemViewModelBase item, PathGeometry geometry, string indent, string nl)
    {
        var sb = new StringBuilder();
        var inner = indent + new string(' ', 4);
        sb.Append(indent).Append("<Path x:Name=\"").Append(MakeXName(item.ID)).Append('"').Append(nl);
        if (item is DesignerItemViewModelBase d)
        {
            AppendAbsoluteLayoutBounds(sb, d, inner, nl);
        }
        AppendStrokeFill(sb, item, inner, nl);
        sb.Append(inner).Append("Data=\"").Append(EscapeXmlAttribute(geometry.ToString(CultureInfo.InvariantCulture))).Append('"').Append(nl);
        AppendRotationAndShadow(sb, item, indent, inner, nl, hasClosingTag: true, shapeTag: "Path");
        return sb.ToString();
    }

    // --------- 共通ヘルパ ---------

    private static void AppendAbsoluteLayoutBounds(StringBuilder sb, DesignerItemViewModelBase d, string inner, string nl)
    {
        var x = FormatDouble(d.Left.Value);
        var y = FormatDouble(d.Top.Value);
        var w = FormatDouble(d.Width.Value);
        var h = FormatDouble(d.Height.Value);
        sb.Append(inner).Append("AbsoluteLayout.LayoutBounds=\"").Append(x).Append(',').Append(y).Append(',').Append(w).Append(',').Append(h).Append('"').Append(nl);
        sb.Append(inner).Append("AbsoluteLayout.LayoutFlags=\"None\"").Append(nl);
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
        if (item.StrokeLineJoin.Value != System.Windows.Media.PenLineJoin.Miter)
        {
            sb.Append(inner).Append("StrokeLineJoin=\"").Append(item.StrokeLineJoin.Value).Append('"').Append(nl);
        }
    }

    /// <summary>
    /// Rotation 属性 (MAUI は WPF と違い直接プロパティ) と Shadow ブロックを必要なら追加し、
    /// self-closing or open/close を判定して閉じる。
    /// </summary>
    private static void AppendRotationAndShadow(StringBuilder sb, SelectableDesignerItemViewModelBase item, string indent, string inner, string nl, bool hasClosingTag, string shapeTag, bool emitRotation = true)
    {
        var hasRotation = emitRotation && !IsZeroAngle(item.RotationAngle.Value);
        if (hasRotation)
        {
            sb.Append(inner).Append("Rotation=\"").Append(FormatDouble(item.RotationAngle.Value)).Append('"').Append(nl);
        }
        var hasShadow = HasGlow(item);
        if (!hasShadow)
        {
            sb.Append(indent).Append("/>");
            return;
        }
        sb.Append(indent).Append(">").Append(nl);
        AppendShadow(sb, item, shapeTag, inner, nl);
        sb.Append(indent).Append("</").Append(shapeTag).Append('>');
    }

    private static void AppendShadow(StringBuilder sb, SelectableDesignerItemViewModelBase item, string shapeTag, string inner, string nl)
    {
        sb.Append(inner).Append('<').Append(shapeTag).Append(".Shadow>").Append(nl);
        sb.Append(inner).Append("    <Shadow Radius=\"").Append(FormatDouble(item.GlowRadius.Value)).Append('"');
        sb.Append(" Opacity=\"").Append(FormatDouble(item.GlowIntensity.Value)).Append('"');
        if (item.GlowColor.Value is { } gc)
        {
            sb.Append(" Brush=\"").Append(FormatColor(gc)).Append('"');
        }
        sb.Append(" />").Append(nl);
        sb.Append(inner).Append("</").Append(shapeTag).Append(".Shadow>").Append(nl);
    }

    private static bool HasGlow(SelectableDesignerItemViewModelBase item) =>
        item.GlowRadius.Value > 0 || item.GlowColor.Value is not null;

    private static bool IsZeroAngle(double angle) => Math.Abs(angle) < 1e-9;

    private static string FormatDouble(double v) => v.ToString("0.######", CultureInfo.InvariantCulture);
    private static string FormatColor(Color c) => c.ToString(CultureInfo.InvariantCulture);

    private static string FormatBrush(Brush brush)
    {
        if (brush is SolidColorBrush scb) return FormatColor(scb.Color);
        return brush.ToString(CultureInfo.InvariantCulture);
    }

    private static string EscapeXmlAttribute(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
