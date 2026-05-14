using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class ShapeToXamlMapperTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static readonly XamlExportSettings DefaultSettings = new();

    [Test]
    public void MakeXName_は_Guid_N_形式()
    {
        var g = Guid.Parse("11111111-2222-3333-4444-555555555555");
        Assert.That(ShapeToXamlMapper.MakeXName(g), Is.EqualTo("Item_11111111222233334444555555555555"));
    }

    [Test]
    public void null_item_は_null()
    {
        Assert.That(ShapeToXamlMapper.TryMapWpfShape(null, DefaultSettings), Is.Null);
    }

    [Test]
    public void Rectangle_最小構成_self_closing()
    {
        var r = new NRectangleViewModel { Left = { Value = 10 }, Top = { Value = 20 }, Width = { Value = 100 }, Height = { Value = 50 } };
        r.ID = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        r.EdgeBrush.Value = null;
        r.FillBrush.Value = null;
        r.EdgeThickness.Value = 0;

        var xaml = ShapeToXamlMapper.TryMapWpfShape(r, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Rectangle x:Name=\"Item_aaaaaaaabbbbccccddddeeeeeeeeeeee\""));
        Assert.That(xaml, Does.Contain("Canvas.Left=\"10\""));
        Assert.That(xaml, Does.Contain("Canvas.Top=\"20\""));
        Assert.That(xaml, Does.Contain("Width=\"100\" Height=\"50\""));
        Assert.That(xaml, Does.EndWith("/>"));
        // 角丸 / 回転 / 効果が無い → RotateTransform / Effect ブロックは出ない
        Assert.That(xaml, Does.Not.Contain("RotateTransform"));
        Assert.That(xaml, Does.Not.Contain("DropShadowEffect"));
        Assert.That(xaml, Does.Not.Contain("RadiusX"));
    }

    [Test]
    public void Rectangle_Stroke_Fill_RadiusXY_が_出力される()
    {
        var r = new NRectangleViewModel(0, 0, 80, 40);
        r.EdgeBrush.Value = new SolidColorBrush(Color.FromArgb(0xFF, 0x12, 0x34, 0x56));
        r.FillBrush.Value = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0x00, 0x00));
        r.EdgeThickness.Value = 2.5;
        r.RadiusX.Value = 4;
        r.RadiusY.Value = 6;

        var xaml = ShapeToXamlMapper.TryMapWpfShape(r, DefaultSettings);

        Assert.That(xaml, Does.Contain("Stroke=\"#FF123456\""));
        Assert.That(xaml, Does.Contain("StrokeThickness=\"2.5\""));
        Assert.That(xaml, Does.Contain("Fill=\"#80FF0000\""));
        Assert.That(xaml, Does.Contain("RadiusX=\"4\" RadiusY=\"6\""));
    }

    [Test]
    public void Rectangle_RotationAngle_あり_は_RenderTransform_展開()
    {
        var r = new NRectangleViewModel(0, 0, 50, 50, 45.0);
        var xaml = ShapeToXamlMapper.TryMapWpfShape(r, DefaultSettings);
        Assert.That(xaml, Does.Contain("<Rectangle.RenderTransform>"));
        Assert.That(xaml, Does.Contain("<RotateTransform Angle=\"45\""));
        Assert.That(xaml, Does.Contain("</Rectangle>"));
        Assert.That(xaml, Does.Not.EndWith("/>"));
    }

    [Test]
    public void Rectangle_Glow_あり_は_DropShadowEffect_展開()
    {
        var r = new NRectangleViewModel(0, 0, 50, 50);
        r.GlowRadius.Value = 10;
        r.GlowIntensity.Value = 0.75;
        r.GlowColor.Value = Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF);

        var xaml = ShapeToXamlMapper.TryMapWpfShape(r, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Rectangle.Effect>"));
        Assert.That(xaml, Does.Contain("<DropShadowEffect BlurRadius=\"10\""));
        Assert.That(xaml, Does.Contain("ShadowDepth=\"0\""));
        Assert.That(xaml, Does.Contain("Opacity=\"0.75\""));
        Assert.That(xaml, Does.Contain("Color=\"#FFFF00FF\""));
    }

    [Test]
    public void Ellipse_最小構成()
    {
        var e = new NEllipseViewModel();
        e.Left.Value = 5;
        e.Top.Value = 6;
        e.Width.Value = 40;
        e.Height.Value = 20;
        e.EdgeBrush.Value = null;
        e.FillBrush.Value = null;

        var xaml = ShapeToXamlMapper.TryMapWpfShape(e, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Ellipse x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Canvas.Left=\"5\""));
        Assert.That(xaml, Does.Contain("Width=\"40\" Height=\"20\""));
        Assert.That(xaml, Does.EndWith("/>"));
    }

    [Test]
    public void Line_座標_と_Stroke_が_出力_Fill_は_出力されない()
    {
        var l = new StraightConnectorViewModel
        {
            P1X = new R3.BindableReactiveProperty<double>(10),
            P1Y = new R3.BindableReactiveProperty<double>(20),
            P2X = new R3.BindableReactiveProperty<double>(110),
            P2Y = new R3.BindableReactiveProperty<double>(220),
        };
        l.EdgeBrush.Value = new SolidColorBrush(Colors.Black);
        l.EdgeThickness.Value = 1.5;
        l.FillBrush.Value = new SolidColorBrush(Colors.Red);  // Line では Fill は出さない

        var xaml = ShapeToXamlMapper.TryMapWpfShape(l, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Line x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("X1=\"10\" Y1=\"20\""));
        Assert.That(xaml, Does.Contain("X2=\"110\" Y2=\"220\""));
        Assert.That(xaml, Does.Contain("Stroke=\"#FF000000\""));
        Assert.That(xaml, Does.Contain("StrokeThickness=\"1.5\""));
        Assert.That(xaml, Does.Not.Contain("Fill="));
    }

    [Test]
    public void Line_P1_P2_未初期化_でも_0_でフォールバック()
    {
        var l = new StraightConnectorViewModel();  // P1X 等は null
        var xaml = ShapeToXamlMapper.TryMapWpfShape(l, DefaultSettings);
        Assert.That(xaml, Does.Contain("X1=\"0\" Y1=\"0\""));
        Assert.That(xaml, Does.Contain("X2=\"0\" Y2=\"0\""));
    }

    [Test]
    public void Letter_テキスト_FontSize_FontFamily_FontWeight_FontStyle()
    {
        var t = new LetterDesignerItemViewModel();
        t.Left.Value = 0;
        t.Top.Value = 0;
        t.LetterString.Value = "Hello";
        t.FontSize.Value = 24;
        t.IsBold.Value = true;
        t.IsItalic.Value = true;
        t.FillBrush.Value = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));

        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);

        Assert.That(xaml, Does.Contain("<TextBlock x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Text=\"Hello\""));
        Assert.That(xaml, Does.Contain("FontSize=\"24\""));
        Assert.That(xaml, Does.Contain("FontWeight=\"Bold\""));
        Assert.That(xaml, Does.Contain("FontStyle=\"Italic\""));
        Assert.That(xaml, Does.Contain("Foreground=\"#FF00FF00\""));
    }

    [Test]
    public void Letter_XML_特殊文字は_エスケープ()
    {
        var t = new LetterDesignerItemViewModel();
        t.LetterString.Value = "a<b&c\"d>e";
        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);
        Assert.That(xaml, Does.Contain("Text=\"a&lt;b&amp;c&quot;d&gt;e\""));
    }

    [Test]
    public void インデント設定が_出力に反映される()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        r.EdgeBrush.Value = null;
        r.FillBrush.Value = null;
        var s2 = new XamlExportSettings { IndentWidth = 2 };
        var xaml2 = ShapeToXamlMapper.TryMapWpfShape(r, s2, indentLevel: 1);
        // インデント幅 2 × indentLevel 1 = 半角スペース 2 個で開始
        Assert.That(xaml2, Does.StartWith("  <Rectangle"));
    }

    [Test]
    public void 未対応図形_例えば_SnapPointViewModel_は_null()
    {
        // 通常 SnapPoint は SelectableDesignerItemViewModelBase 直系ではないが、
        // 「対応外図形は null」を確認するため、適当な無関係 ViewModel を用意できないので Polygon でテスト。
        // Phase 5.5-b-4 で Polygon は対応するので、その時点でこのテストは更新される。
        var p = new NPolygonViewModel();
        var xaml = ShapeToXamlMapper.TryMapWpfShape(p, DefaultSettings);
        Assert.That(xaml, Is.Null);
    }
}
