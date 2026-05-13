using boilersGraphics.Models.Text;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Models.Text;

[TestFixture]
public class TextOnPathGeneratorTest
{
    /// <summary>(0,0)→(100,0) の直線を 1 セグメントの PathGeometry として作る。</summary>
    private static PathGeometry CreateHorizontalLine()
    {
        var figure = new PathFigure
        {
            StartPoint = new Point(0, 0),
            Segments = { new LineSegment(new Point(100, 0), true) },
        };
        var geom = new PathGeometry();
        geom.Figures.Add(figure);
        return geom;
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ComputePathLength_直線100px_長さ100()
    {
        var path = CreateHorizontalLine();
        var len = TextOnPathGenerator.ComputePathLength(path);
        Assert.That(len, Is.EqualTo(100).Within(0.01));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_空テキスト_空リスト()
    {
        var path = CreateHorizontalLine();
        var result = TextOnPathGenerator.Generate(string.Empty, path,
            startOffset: 0, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 12);
        Assert.That(result, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_pathがnull_空リスト()
    {
        var result = TextOnPathGenerator.Generate("ABC", null,
            startOffset: 0, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 12);
        Assert.That(result, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_直線上_各文字が指定step刻みで配置される()
    {
        var path = CreateHorizontalLine();
        // FontSize 10 × CharWidthRatio 0.6 = 6 px / 文字
        var result = TextOnPathGenerator.Generate("ABCDE", path,
            startOffset: 0, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 10);
        Assert.That(result.Count, Is.EqualTo(5));
        for (var i = 0; i < result.Count; i++)
        {
            Assert.That(result[i].X, Is.EqualTo(i * 6).Within(0.5), $"index {i} X");
            Assert.That(result[i].Y, Is.EqualTo(0).Within(0.5), $"index {i} Y");
            Assert.That(result[i].Char, Is.EqualTo("ABCDE"[i].ToString()));
        }
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_直線水平_接線角度はおよそ0度()
    {
        var path = CreateHorizontalLine();
        var result = TextOnPathGenerator.Generate("X", path,
            startOffset: 0.5, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 12);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Angle, Is.EqualTo(0).Within(0.5));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_Upright_接線無視で常に0度()
    {
        var path = CreateHorizontalLine();
        var result = TextOnPathGenerator.Generate("X", path,
            startOffset: 0.5, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Upright, fontSize: 12);
        Assert.That(result[0].Angle, Is.EqualTo(0));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_Above_法線分Yがマイナス側にシフト()
    {
        var path = CreateHorizontalLine();
        var result = TextOnPathGenerator.Generate("X", path,
            startOffset: 0.5, spacing: 0, TextOnPathSide.Above, TextOnPathRotation.Tangent, fontSize: 12);
        // 水平線で接線 (1,0) → 単位法線 (0,1) → sideOffset = -fontSize/2 = -6
        // y = 0 + 1 * (-6) = -6
        Assert.That(result[0].Y, Is.EqualTo(-6).Within(0.5));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_Below_法線分Yがプラス側にシフト()
    {
        var path = CreateHorizontalLine();
        var result = TextOnPathGenerator.Generate("X", path,
            startOffset: 0.5, spacing: 0, TextOnPathSide.Below, TextOnPathRotation.Tangent, fontSize: 12);
        Assert.That(result[0].Y, Is.EqualTo(6).Within(0.5));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_StartOffset_最初の文字がパスのStartOffset位置に配置()
    {
        var path = CreateHorizontalLine();
        var result = TextOnPathGenerator.Generate("X", path,
            startOffset: 0.25, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 10);
        // 100px の 25% = X=25
        Assert.That(result[0].X, Is.EqualTo(25).Within(0.5));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_パス範囲外の文字はスキップされる()
    {
        var path = CreateHorizontalLine();
        // FontSize 10 × 0.6 = 6 px / 文字。パス全長 100。20 文字 × 6 = 120 で超過
        var result = TextOnPathGenerator.Generate("ABCDEFGHIJKLMNOPQRST", path,
            startOffset: 0, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 10);
        // fraction > 1 になる文字は除外される
        Assert.That(result.Count, Is.LessThan(20));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void Generate_Spacing_文字間隔が広がる()
    {
        var path = CreateHorizontalLine();
        var r1 = TextOnPathGenerator.Generate("AB", path,
            startOffset: 0, spacing: 0, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 10);
        var r2 = TextOnPathGenerator.Generate("AB", path,
            startOffset: 0, spacing: 4, TextOnPathSide.On, TextOnPathRotation.Tangent, fontSize: 10);
        // Spacing 0: step=6 → X=6
        // Spacing 4: step=10 → X=10
        Assert.That(r1[1].X, Is.EqualTo(6).Within(0.5));
        Assert.That(r2[1].X, Is.EqualTo(10).Within(0.5));
    }
}
