using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace boilersGraphics.Models.Themes;

/// <summary>
/// Phase 4-a §3.1.4 / Q-4 案 A: 組み込みテーマ 4 種を提供するリポジトリ。
/// Bladerunner / Matrix / 医療系青白 / アンバー CRT は固定で、起動時にコードから生成。
/// </summary>
public static class ThemeRepository
{
    /// <summary>組み込みテーマ 4 種を新規インスタンスで返す。</summary>
    public static IReadOnlyList<Theme> CreateBuiltIn() =>
        new[]
        {
            CreateBladerunner(),
            CreateMatrix(),
            CreateMedicalBlueWhite(),
            CreateAmberCrt(),
        };

    /// <summary>組み込み線種 6 種を新規インスタンスで返す (テーマに紐づく既定線種ライブラリ)。</summary>
    public static IReadOnlyList<LineStyle> CreateBuiltInLineStyles() =>
        new[]
        {
            CreateLineStyle("Solid", new DoubleCollection()),
            CreateLineStyle("Dash", new DoubleCollection { 4, 2 }),
            CreateLineStyle("Dot", new DoubleCollection { 1, 2 }),
            CreateLineStyle("DashDot", new DoubleCollection { 4, 2, 1, 2 }),
            CreateLineStyle("LongDash", new DoubleCollection { 8, 4 }),
            CreateLineStyle("Stepped", new DoubleCollection { 8, 4, 2, 4 }),
        };

    private static LineStyle CreateLineStyle(string name, DoubleCollection dash)
    {
        var ls = new LineStyle
        {
            Name = name,
            IsBuiltIn = true,
            StrokeDashArray = dash,
            StrokeLineJoin = PenLineJoin.Round,
        };
        return ls;
    }

    private static Theme CreateBladerunner()
    {
        return CreateThemeWith(
            name: "Bladerunner",
            colors: new[]
            {
                (Color)ColorConverter.ConvertFromString("#FF5733"),  // primary 暖色赤
                (Color)ColorConverter.ConvertFromString("#FFB94B"),  // accent 暖色黄
                (Color)ColorConverter.ConvertFromString("#9E2A1B"),  // warning 暗赤
                (Color)ColorConverter.ConvertFromString("#2C1810"),  // info 暗黒
                (Color)ColorConverter.ConvertFromString("#0A0303"),  // background 黒
            },
            glowColor: (Color)ColorConverter.ConvertFromString("#FF5733"),
            glowRadius: 6,
            glowIntensity: 0.6);
    }

    private static Theme CreateMatrix()
    {
        return CreateThemeWith(
            name: "Matrix",
            colors: new[]
            {
                (Color)ColorConverter.ConvertFromString("#00FF41"),  // primary 蛍光緑
                (Color)ColorConverter.ConvertFromString("#33B85A"),  // accent 中緑
                (Color)ColorConverter.ConvertFromString("#0F5A1F"),  // warning 暗緑
                (Color)ColorConverter.ConvertFromString("#001500"),  // info 黒緑
                (Color)ColorConverter.ConvertFromString("#000000"),  // background 黒
            },
            glowColor: (Color)ColorConverter.ConvertFromString("#00FF41"),
            glowRadius: 5,
            glowIntensity: 0.7);
    }

    private static Theme CreateMedicalBlueWhite()
    {
        return CreateThemeWith(
            name: "MedicalBlueWhite",
            colors: new[]
            {
                (Color)ColorConverter.ConvertFromString("#3FE0FF"),  // primary 蛍光青
                (Color)ColorConverter.ConvertFromString("#FFFFFF"),  // accent 白
                (Color)ColorConverter.ConvertFromString("#B0C8D5"),  // warning 灰青
                (Color)ColorConverter.ConvertFromString("#0A1A30"),  // info 暗青
                (Color)ColorConverter.ConvertFromString("#001020"),  // background 黒青
            },
            glowColor: (Color)ColorConverter.ConvertFromString("#3FE0FF"),
            glowRadius: 4,
            glowIntensity: 0.5);
    }

    private static Theme CreateAmberCrt()
    {
        return CreateThemeWith(
            name: "AmberCrt",
            colors: new[]
            {
                (Color)ColorConverter.ConvertFromString("#FFB000"),  // primary 琥珀
                (Color)ColorConverter.ConvertFromString("#FFD568"),  // accent 淡琥珀
                (Color)ColorConverter.ConvertFromString("#7A5500"),  // warning 暗琥珀
                (Color)ColorConverter.ConvertFromString("#1F1500"),  // info 黒褐
                (Color)ColorConverter.ConvertFromString("#0A0700"),  // background 暗黒
            },
            glowColor: (Color)ColorConverter.ConvertFromString("#FFB000"),
            glowRadius: 8,
            glowIntensity: 0.65);
    }

    private static Theme CreateThemeWith(
        string name,
        IReadOnlyList<Color> colors,
        Color glowColor,
        double glowRadius,
        double glowIntensity)
    {
        var theme = new Theme
        {
            Name = name,
            IsBuiltIn = true,
            DefaultGlow =
            {
                Radius = glowRadius,
                Intensity = glowIntensity,
                Color = glowColor,
            },
        };
        theme.Palette.Name = name;
        theme.Palette.IsBuiltIn = true;
        for (var i = 0; i < colors.Count; i++)
        {
            theme.Palette.Colors.Add(colors[i]);
        }
        // Q-2 案 A: 順序色 0..4 を primary / accent / warning / info / background に割当
        for (var i = 0; i < SemanticSlotKeys.All.Length && i < colors.Count; i++)
        {
            theme.Palette.SemanticSlots[SemanticSlotKeys.All[i]] = i;
        }
        foreach (var ls in CreateBuiltInLineStyles())
        {
            theme.LineStyles.Add(ls);
        }
        return theme;
    }
}
