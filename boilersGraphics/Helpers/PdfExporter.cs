using boilersGraphics.ViewModels;
using NLog;
using OpenPdf.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Helpers;

public static class PdfExporter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void Export(string filePath, DiagramViewModel diagramViewModel, Rect? sliceRect)
    {
        var background = diagramViewModel.BackgroundItem.Value;
        double canvasWidth, canvasHeight, offsetX, offsetY;

        if (sliceRect.HasValue)
        {
            canvasWidth = sliceRect.Value.Width;
            canvasHeight = sliceRect.Value.Height;
            offsetX = sliceRect.Value.X;
            offsetY = sliceRect.Value.Y;
        }
        else
        {
            canvasWidth = background.Width.Value;
            canvasHeight = background.Height.Value;
            offsetX = background.Left.Value;
            offsetY = background.Top.Value;
        }

        using var doc = PdfDocument.Create(filePath);
        var page = doc.AddPage(canvasWidth, canvasHeight);

        DrawBackground(page, background, canvasWidth, canvasHeight, offsetX, offsetY);

        var items = diagramViewModel.AllItems.Value
            .Where(x => x.IsVisible.Value
                && !(x is BackgroundViewModel)
                && !(x is SnapPointViewModel))
            .OrderBy(x => x.ZIndex.Value);

        foreach (var item in items)
        {
            try
            {
                DrawItem(page, item, canvasHeight, offsetX, offsetY);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to export item {item.GetType().Name} to PDF");
            }
        }

        doc.Save();
    }

    private static void DrawBackground(PdfPageBuilder page, BackgroundViewModel background,
        double pageWidth, double pageHeight, double offsetX, double offsetY)
    {
        var (r, g, b, opacity) = ExtractColor(background.FillBrush.Value);
        if (opacity > 0)
        {
            page.SaveGraphicsState();
            if (opacity < 1.0)
                page.SetTransparency(opacity, opacity, null);
            page.SetFillColor(r, g, b);
            page.Rectangle(0, 0, pageWidth, pageHeight);
            page.Fill();
            page.RestoreGraphicsState();
        }
    }

    private static void DrawItem(PdfPageBuilder page, SelectableDesignerItemViewModelBase item,
        double pageHeight, double offsetX, double offsetY)
    {
        switch (item)
        {
            case EffectViewModel effect:
                DrawEffect(page, effect, pageHeight, offsetX, offsetY);
                break;
            case PictureDesignerItemViewModel picture:
                DrawPicture(page, picture, pageHeight, offsetX, offsetY);
                break;
            case CroppedPictureDesignerItemViewModel croppedPicture:
                DrawCroppedPicture(page, croppedPicture, pageHeight, offsetX, offsetY);
                break;
            case StraightConnectorViewModel line:
                DrawStraightConnector(page, line, pageHeight, offsetX, offsetY);
                break;
            case BezierCurveViewModel bezier:
                DrawBezierCurve(page, bezier, pageHeight, offsetX, offsetY);
                break;
            case PolyBezierViewModel polyBezier:
                DrawPolyBezier(page, polyBezier, pageHeight, offsetX, offsetY);
                break;
            case DesignerItemViewModelBase designerItem:
                DrawDesignerItem(page, designerItem, pageHeight, offsetX, offsetY);
                break;
        }
    }

    private static void DrawDesignerItem(PdfPageBuilder page, DesignerItemViewModelBase item,
        double pageHeight, double offsetX, double offsetY)
    {
        var geometry = item.PathGeometryNoRotate.Value;
        if (geometry == null) return;

        double itemLeft = item.Left.Value;
        double itemTop = item.Top.Value;
        double centerLocalX = item.Width.Value / 2;
        double centerLocalY = item.Height.Value / 2;
        double angle = item.RotationAngle.Value;

        bool hasFill = HasVisibleBrush(item.FillBrush.Value);
        bool hasStroke = HasVisibleBrush(item.EdgeBrush.Value) && item.EdgeThickness.Value > 0;

        if (!hasFill && !hasStroke) return;

        page.SaveGraphicsState();

        SetStrokeStyle(page, item);
        SetFillStyle(page, item);

        DrawPathGeometry(page, geometry, itemLeft, itemTop, centerLocalX, centerLocalY, angle,
            pageHeight, offsetX, offsetY, hasFill, hasStroke, item);

        page.RestoreGraphicsState();
    }

    private static void DrawStraightConnector(PdfPageBuilder page, StraightConnectorViewModel line,
        double pageHeight, double offsetX, double offsetY)
    {
        if (line.P1X == null || line.P2X == null) return;

        bool hasStroke = HasVisibleBrush(line.EdgeBrush.Value) && line.EdgeThickness.Value > 0;
        if (!hasStroke) return;

        page.SaveGraphicsState();
        SetStrokeStyle(page, line);

        var p1 = ConvertCanvasPoint(line.P1X.Value, line.P1Y.Value, pageHeight, offsetX, offsetY);
        var p2 = ConvertCanvasPoint(line.P2X.Value, line.P2Y.Value, pageHeight, offsetX, offsetY);

        page.MoveTo(p1.X, p1.Y);
        page.LineTo(p2.X, p2.Y);
        page.Stroke();

        page.RestoreGraphicsState();
    }

    private static void DrawBezierCurve(PdfPageBuilder page, BezierCurveViewModel bezier,
        double pageHeight, double offsetX, double offsetY)
    {
        if (bezier.P1X == null || bezier.P2X == null) return;

        bool hasStroke = HasVisibleBrush(bezier.EdgeBrush.Value) && bezier.EdgeThickness.Value > 0;
        if (!hasStroke) return;

        page.SaveGraphicsState();
        SetStrokeStyle(page, bezier);

        var p1 = ConvertCanvasPoint(bezier.P1X.Value, bezier.P1Y.Value, pageHeight, offsetX, offsetY);
        var p2 = ConvertCanvasPoint(bezier.P2X.Value, bezier.P2Y.Value, pageHeight, offsetX, offsetY);
        var c1 = ConvertCanvasPoint(bezier.C1X.Value, bezier.C1Y.Value, pageHeight, offsetX, offsetY);
        var c2 = ConvertCanvasPoint(bezier.C2X.Value, bezier.C2Y.Value, pageHeight, offsetX, offsetY);

        page.MoveTo(p1.X, p1.Y);
        page.CurveTo(c1.X, c1.Y, c2.X, c2.Y, p2.X, p2.Y);
        page.Stroke();

        page.RestoreGraphicsState();
    }

    private static void DrawPolyBezier(PdfPageBuilder page, PolyBezierViewModel polyBezier,
        double pageHeight, double offsetX, double offsetY)
    {
        if (polyBezier.Points == null || polyBezier.Points.Count < 2) return;

        bool hasStroke = HasVisibleBrush(polyBezier.EdgeBrush.Value) && polyBezier.EdgeThickness.Value > 0;
        if (!hasStroke) return;

        page.SaveGraphicsState();
        SetStrokeStyle(page, polyBezier);

        var firstPt = ConvertCanvasPoint(polyBezier.Points[0].X, polyBezier.Points[0].Y,
            pageHeight, offsetX, offsetY);
        page.MoveTo(firstPt.X, firstPt.Y);

        int i = 1;
        while (i + 2 < polyBezier.Points.Count)
        {
            var cp1 = ConvertCanvasPoint(polyBezier.Points[i].X, polyBezier.Points[i].Y,
                pageHeight, offsetX, offsetY);
            var cp2 = ConvertCanvasPoint(polyBezier.Points[i + 1].X, polyBezier.Points[i + 1].Y,
                pageHeight, offsetX, offsetY);
            var end = ConvertCanvasPoint(polyBezier.Points[i + 2].X, polyBezier.Points[i + 2].Y,
                pageHeight, offsetX, offsetY);
            page.CurveTo(cp1.X, cp1.Y, cp2.X, cp2.Y, end.X, end.Y);
            i += 3;
        }

        // Remaining points as line segments
        while (i < polyBezier.Points.Count)
        {
            var pt = ConvertCanvasPoint(polyBezier.Points[i].X, polyBezier.Points[i].Y,
                pageHeight, offsetX, offsetY);
            page.LineTo(pt.X, pt.Y);
            i++;
        }

        page.Stroke();
        page.RestoreGraphicsState();
    }

    private static void DrawPicture(PdfPageBuilder page, PictureDesignerItemViewModel picture,
        double pageHeight, double offsetX, double offsetY)
    {
        var bitmapImage = picture.EmbeddedImage.Value;
        if (bitmapImage == null) return;

        byte[] pngData = BitmapImageToPng(bitmapImage);
        if (pngData == null) return;

        page.SaveGraphicsState();

        double itemLeft = picture.Left.Value;
        double itemTop = picture.Top.Value;
        double width = picture.Width.Value;
        double height = picture.Height.Value;
        double angle = picture.RotationAngle.Value;

        if (angle != 0)
        {
            ApplyRotationTransform(page, itemLeft, itemTop, width, height, angle,
                pageHeight, offsetX, offsetY);
        }

        double pdfX = itemLeft - offsetX;
        double pdfY = pageHeight - (itemTop - offsetY) - height;

        var imageName = page.AddPngImage(pngData);
        page.DrawImage(imageName, pdfX, pdfY, width, height);

        page.RestoreGraphicsState();
    }

    private static void DrawCroppedPicture(PdfPageBuilder page, CroppedPictureDesignerItemViewModel picture,
        double pageHeight, double offsetX, double offsetY)
    {
        var bitmapImage = picture.EmbeddedImage.Value;
        if (bitmapImage == null) return;

        byte[] pngData = BitmapImageToPng(bitmapImage);
        if (pngData == null) return;

        page.SaveGraphicsState();

        double itemLeft = picture.Left.Value;
        double itemTop = picture.Top.Value;
        double width = picture.Width.Value;
        double height = picture.Height.Value;
        double angle = picture.RotationAngle.Value;

        if (angle != 0)
        {
            ApplyRotationTransform(page, itemLeft, itemTop, width, height, angle,
                pageHeight, offsetX, offsetY);
        }

        double pdfX = itemLeft - offsetX;
        double pdfY = pageHeight - (itemTop - offsetY) - height;

        var imageName = page.AddPngImage(pngData);
        page.DrawImage(imageName, pdfX, pdfY, width, height);

        page.RestoreGraphicsState();
    }

    private static void DrawEffect(PdfPageBuilder page, EffectViewModel effect,
        double pageHeight, double offsetX, double offsetY)
    {
        var bitmap = effect.Bitmap.Value;
        if (bitmap == null) return;

        byte[] pngData = WriteableBitmapToPng(bitmap);
        if (pngData == null) return;

        page.SaveGraphicsState();

        double itemLeft = effect.Left.Value;
        double itemTop = effect.Top.Value;
        double width = effect.Width.Value;
        double height = effect.Height.Value;
        double angle = effect.RotationAngle.Value;

        if (angle != 0)
        {
            ApplyRotationTransform(page, itemLeft, itemTop, width, height, angle,
                pageHeight, offsetX, offsetY);
        }

        double pdfX = itemLeft - offsetX;
        double pdfY = pageHeight - (itemTop - offsetY) - height;

        var imageName = page.AddPngImage(pngData);
        page.DrawImage(imageName, pdfX, pdfY, width, height);

        page.RestoreGraphicsState();
    }

    #region PathGeometry Conversion

    private static void DrawPathGeometry(PdfPageBuilder page, PathGeometry geometry,
        double itemLeft, double itemTop, double centerLocalX, double centerLocalY, double angle,
        double pageHeight, double offsetX, double offsetY,
        bool hasFill, bool hasStroke, DesignerItemViewModelBase item)
    {
        if (geometry.Figures == null || geometry.Figures.Count == 0) return;

        // Set transparency if fill brush has opacity
        if (hasFill)
        {
            var (_, _, _, fillOpacity) = ExtractColor(item.FillBrush.Value);
            var (_, _, _, strokeOpacity) = hasStroke ? ExtractColor(item.EdgeBrush.Value) : (0, 0, 0, 1.0);
            if (fillOpacity < 1.0 || strokeOpacity < 1.0)
                page.SetTransparency(fillOpacity, strokeOpacity, null);
        }

        // Check if geometry contains arc segments that need flattening
        bool hasArcs = ContainsArcSegments(geometry);

        if (hasArcs)
        {
            // Flatten the entire geometry to polylines for arc accuracy
            // Use tight tolerance for smooth curves
            var flattened = geometry.GetFlattenedPathGeometry(0.5, ToleranceType.Absolute);
            DrawFlattenedGeometry(page, flattened, itemLeft, itemTop, centerLocalX, centerLocalY,
                angle, pageHeight, offsetX, offsetY);
        }
        else
        {
            // Draw directly preserving bezier curves
            DrawGeometryFigures(page, geometry, itemLeft, itemTop, centerLocalX, centerLocalY,
                angle, pageHeight, offsetX, offsetY);
        }

        if (hasFill && hasStroke)
            page.FillAndStroke();
        else if (hasFill)
            page.Fill();
        else if (hasStroke)
            page.Stroke();
    }

    private static bool ContainsArcSegments(PathGeometry geometry)
    {
        foreach (var figure in geometry.Figures)
        {
            foreach (var segment in figure.Segments)
            {
                if (segment is ArcSegment)
                    return true;
            }
        }
        return false;
    }

    private static void DrawGeometryFigures(PdfPageBuilder page, PathGeometry geometry,
        double itemLeft, double itemTop, double centerLocalX, double centerLocalY, double angle,
        double pageHeight, double offsetX, double offsetY)
    {
        foreach (var figure in geometry.Figures)
        {
            var startPt = ConvertLocalPoint(figure.StartPoint, itemLeft, itemTop,
                centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
            page.MoveTo(startPt.X, startPt.Y);

            foreach (var segment in figure.Segments)
            {
                DrawSegment(page, segment, itemLeft, itemTop, centerLocalX, centerLocalY,
                    angle, pageHeight, offsetX, offsetY);
            }

            if (figure.IsClosed)
                page.ClosePath();
        }
    }

    private static void DrawFlattenedGeometry(PdfPageBuilder page, PathGeometry flattened,
        double itemLeft, double itemTop, double centerLocalX, double centerLocalY, double angle,
        double pageHeight, double offsetX, double offsetY)
    {
        foreach (var figure in flattened.Figures)
        {
            var startPt = ConvertLocalPoint(figure.StartPoint, itemLeft, itemTop,
                centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
            page.MoveTo(startPt.X, startPt.Y);

            foreach (var segment in figure.Segments)
            {
                if (segment is PolyLineSegment polyLine)
                {
                    foreach (var pt in polyLine.Points)
                    {
                        var converted = ConvertLocalPoint(pt, itemLeft, itemTop,
                            centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                        page.LineTo(converted.X, converted.Y);
                    }
                }
                else if (segment is LineSegment lineSeg)
                {
                    var converted = ConvertLocalPoint(lineSeg.Point, itemLeft, itemTop,
                        centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                    page.LineTo(converted.X, converted.Y);
                }
            }

            if (figure.IsClosed)
                page.ClosePath();
        }
    }

    private static void DrawSegment(PdfPageBuilder page, PathSegment segment,
        double itemLeft, double itemTop, double centerLocalX, double centerLocalY, double angle,
        double pageHeight, double offsetX, double offsetY)
    {
        switch (segment)
        {
            case LineSegment line:
            {
                var pt = ConvertLocalPoint(line.Point, itemLeft, itemTop,
                    centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                page.LineTo(pt.X, pt.Y);
                break;
            }
            case BezierSegment bezier:
            {
                var p1 = ConvertLocalPoint(bezier.Point1, itemLeft, itemTop,
                    centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                var p2 = ConvertLocalPoint(bezier.Point2, itemLeft, itemTop,
                    centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                var p3 = ConvertLocalPoint(bezier.Point3, itemLeft, itemTop,
                    centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                page.CurveTo(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
                break;
            }
            case PolyLineSegment polyLine:
            {
                foreach (var pt in polyLine.Points)
                {
                    var converted = ConvertLocalPoint(pt, itemLeft, itemTop,
                        centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                    page.LineTo(converted.X, converted.Y);
                }
                break;
            }
            case PolyBezierSegment polyBezier:
            {
                for (int i = 0; i + 2 < polyBezier.Points.Count; i += 3)
                {
                    var cp1 = ConvertLocalPoint(polyBezier.Points[i], itemLeft, itemTop,
                        centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                    var cp2 = ConvertLocalPoint(polyBezier.Points[i + 1], itemLeft, itemTop,
                        centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                    var end = ConvertLocalPoint(polyBezier.Points[i + 2], itemLeft, itemTop,
                        centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                    page.CurveTo(cp1.X, cp1.Y, cp2.X, cp2.Y, end.X, end.Y);
                }
                break;
            }
            case QuadraticBezierSegment quad:
            {
                var end = ConvertLocalPoint(quad.Point2, itemLeft, itemTop,
                    centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                var ctrl = ConvertLocalPoint(quad.Point1, itemLeft, itemTop,
                    centerLocalX, centerLocalY, angle, pageHeight, offsetX, offsetY);
                page.CurveTo(ctrl.X, ctrl.Y, ctrl.X, ctrl.Y, end.X, end.Y);
                break;
            }
        }
    }

    #endregion

    #region Coordinate Conversion

    private static Point ConvertLocalPoint(Point localPt,
        double itemLeft, double itemTop, double centerLocalX, double centerLocalY, double angle,
        double pageHeight, double offsetX, double offsetY)
    {
        double x = localPt.X;
        double y = localPt.Y;

        // Apply rotation around local center
        if (Math.Abs(angle) > 0.001)
        {
            double dx = x - centerLocalX;
            double dy = y - centerLocalY;
            double rad = angle * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            x = dx * cos - dy * sin + centerLocalX;
            y = dx * sin + dy * cos + centerLocalY;
        }

        // Convert to canvas coordinates, then to PDF coordinates
        double canvasX = itemLeft + x;
        double canvasY = itemTop + y;
        double pdfX = canvasX - offsetX;
        double pdfY = pageHeight - (canvasY - offsetY);

        return new Point(pdfX, pdfY);
    }

    private static Point ConvertCanvasPoint(double canvasX, double canvasY,
        double pageHeight, double offsetX, double offsetY)
    {
        double pdfX = canvasX - offsetX;
        double pdfY = pageHeight - (canvasY - offsetY);
        return new Point(pdfX, pdfY);
    }

    #endregion

    #region Style Helpers

    private static void SetStrokeStyle(PdfPageBuilder page, SelectableDesignerItemViewModelBase item)
    {
        if (HasVisibleBrush(item.EdgeBrush.Value))
        {
            var (r, g, b, opacity) = ExtractColor(item.EdgeBrush.Value);
            page.SetStrokeColor(r, g, b);
            page.SetLineWidth(item.EdgeThickness.Value);

            if (opacity < 1.0)
                page.SetTransparency(1.0, opacity, null);

            // Dash array
            if (item.StrokeDashArray.Value != null && item.StrokeDashArray.Value.Count > 0)
            {
                var dashArray = item.StrokeDashArray.Value;
                var dashString = string.Join(" ", dashArray.Select(d => FormatDouble(d * item.EdgeThickness.Value)));
                page.AppendRawContent($"[{dashString}] 0 d\n");
            }

            // Line join
            int lineJoin = item.StrokeLineJoin.Value switch
            {
                PenLineJoin.Miter => 0,
                PenLineJoin.Round => 1,
                PenLineJoin.Bevel => 2,
                _ => 0
            };
            page.AppendRawContent($"{lineJoin} j\n");

            // Miter limit
            if (item.StrokeMiterLimit.Value > 0)
                page.AppendRawContent($"{FormatDouble(item.StrokeMiterLimit.Value)} M\n");
        }
    }

    private static void SetFillStyle(PdfPageBuilder page, DesignerItemViewModelBase item)
    {
        if (HasVisibleBrush(item.FillBrush.Value))
        {
            var (r, g, b, _) = ExtractColor(item.FillBrush.Value);
            page.SetFillColor(r, g, b);
        }
    }

    private static (double r, double g, double b, double opacity) ExtractColor(Brush brush)
    {
        if (brush is SolidColorBrush scb)
        {
            double opacity = (scb.Color.A / 255.0) * scb.Opacity;
            return (scb.Color.R / 255.0, scb.Color.G / 255.0, scb.Color.B / 255.0, opacity);
        }

        if (brush is LinearGradientBrush lgb && lgb.GradientStops.Count > 0)
        {
            // Use the first gradient stop color as approximation
            var color = lgb.GradientStops[0].Color;
            double opacity = (color.A / 255.0) * lgb.Opacity;
            return (color.R / 255.0, color.G / 255.0, color.B / 255.0, opacity);
        }

        if (brush is RadialGradientBrush rgb && rgb.GradientStops.Count > 0)
        {
            var color = rgb.GradientStops[0].Color;
            double opacity = (color.A / 255.0) * rgb.Opacity;
            return (color.R / 255.0, color.G / 255.0, color.B / 255.0, opacity);
        }

        return (0, 0, 0, 1.0);
    }

    private static bool HasVisibleBrush(Brush brush)
    {
        if (brush == null) return false;
        if (brush.Opacity <= 0) return false;
        if (brush is SolidColorBrush scb && scb.Color.A == 0) return false;
        return true;
    }

    #endregion

    #region Image Helpers

    private static byte[] BitmapImageToPng(BitmapImage bitmapImage)
    {
        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to convert BitmapImage to PNG");
            return null;
        }
    }

    private static byte[] WriteableBitmapToPng(WriteableBitmap bitmap)
    {
        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Failed to convert WriteableBitmap to PNG");
            return null;
        }
    }

    private static void ApplyRotationTransform(PdfPageBuilder page,
        double itemLeft, double itemTop, double width, double height, double angle,
        double pageHeight, double offsetX, double offsetY)
    {
        // Rotation center in PDF coordinates
        double cx = itemLeft + width / 2 - offsetX;
        double cy = pageHeight - (itemTop + height / 2 - offsetY);

        double rad = -angle * Math.PI / 180.0; // Negate for PDF coordinate system
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);

        // Translate to origin, rotate, translate back
        // Combined matrix: [cos sin -sin cos cx-cx*cos+cy*sin cy-cx*sin-cy*cos]
        double e = cx - cx * cos + cy * sin;
        double f = cy - cx * sin - cy * cos;

        page.AppendRawContent($"{FormatDouble(cos)} {FormatDouble(sin)} {FormatDouble(-sin)} {FormatDouble(cos)} {FormatDouble(e)} {FormatDouble(f)} cm\n");
    }

    #endregion

    private static string FormatDouble(double value)
    {
        return value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
    }
}
