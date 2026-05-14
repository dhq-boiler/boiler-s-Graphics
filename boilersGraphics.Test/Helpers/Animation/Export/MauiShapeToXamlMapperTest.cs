using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using NUnit.Framework;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class MauiShapeToXamlMapperTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static readonly XamlExportSettings DefaultSettings = new();

    [Test]
    public void null_item_は_null()
    {
        Assert.That(MauiShapeToXamlMapper.TryMapMauiShape(null, DefaultSettings), Is.Null);
        Assert.That(MauiShapeToXamlMapper.TryMapMauiPath(null, new PathGeometry(), DefaultSettings), Is.Null);
    }

    [Test]
    public void Rectangle_AbsoluteLayoutLayoutBoundsで配置()
    {
        var r = new NRectangleViewModel(10, 20, 30, 40);
        r.EdgeBrush.Value = null;
        r.FillBrush.Value = null;
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(r, DefaultSettings);

        Assert.That(xaml, Does.StartWith("<Rectangle x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutBounds=\"10,20,30,40\""));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutFlags=\"None\""));
        Assert.That(xaml, Does.EndWith("/>"));
        Assert.That(xaml, Does.Not.Contain("Shadow"));
        Assert.That(xaml, Does.Not.Contain("Rotation"));
    }

    [Test]
    public void Rectangle_Stroke_Fill_Radius_Rotation()
    {
        var r = new NRectangleViewModel(0, 0, 50, 50, 30.0);
        r.EdgeBrush.Value = new SolidColorBrush(Colors.Black);
        r.FillBrush.Value = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));
        r.EdgeThickness.Value = 1.5;
        r.RadiusX.Value = 4;
        r.RadiusY.Value = 6;

        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(r, DefaultSettings);

        Assert.That(xaml, Does.Contain("Stroke=\"#FF000000\""));
        Assert.That(xaml, Does.Contain("Fill=\"#FF00FF00\""));
        Assert.That(xaml, Does.Contain("StrokeThickness=\"1.5\""));
        Assert.That(xaml, Does.Contain("RadiusX=\"4\" RadiusY=\"6\""));
        Assert.That(xaml, Does.Contain("Rotation=\"30\""));
    }

    [Test]
    public void Rectangle_GlowはShadow_ブロック展開()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        r.GlowRadius.Value = 5;
        r.GlowIntensity.Value = 0.5;
        r.GlowColor.Value = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);

        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(r, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Rectangle.Shadow>"));
        Assert.That(xaml, Does.Contain("<Shadow Radius=\"5\""));
        Assert.That(xaml, Does.Contain("Opacity=\"0.5\""));
        Assert.That(xaml, Does.Contain("Brush=\"#FFFF0000\""));
        Assert.That(xaml, Does.Contain("</Rectangle>"));
    }

    [Test]
    public void Ellipse_AbsoluteLayout()
    {
        var e = new NEllipseViewModel();
        e.Left.Value = 5; e.Top.Value = 5; e.Width.Value = 20; e.Height.Value = 20;
        e.EdgeBrush.Value = null;
        e.FillBrush.Value = null;
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(e, DefaultSettings);
        Assert.That(xaml, Does.StartWith("<Ellipse x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutBounds=\"5,5,20,20\""));
        Assert.That(xaml, Does.EndWith("/>"));
    }

    [Test]
    public void Line_X1Y1X2Y2と_Stroke()
    {
        var l = new StraightConnectorViewModel
        {
            P1X = new R3.BindableReactiveProperty<double>(10),
            P1Y = new R3.BindableReactiveProperty<double>(20),
            P2X = new R3.BindableReactiveProperty<double>(110),
            P2Y = new R3.BindableReactiveProperty<double>(220),
        };
        l.EdgeBrush.Value = new SolidColorBrush(Colors.Black);
        l.EdgeThickness.Value = 1;

        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(l, DefaultSettings);
        Assert.That(xaml, Does.StartWith("<Line x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("X1=\"10\" Y1=\"20\""));
        Assert.That(xaml, Does.Contain("X2=\"110\" Y2=\"220\""));
        Assert.That(xaml, Does.Contain("Stroke=\"#FF000000\""));
        // Line に Rotation は出さない
        Assert.That(xaml, Does.Not.Contain("Rotation="));
    }

    [Test]
    public void Letter_は_Labelに変換_FontAttributes_TextColor()
    {
        var t = new LetterDesignerItemViewModel();
        t.LetterString.Value = "Hi";
        t.FontSize.Value = 18;
        t.IsBold.Value = true;
        t.IsItalic.Value = true;
        t.FillBrush.Value = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0xCC, 0xFF));

        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(t, DefaultSettings);

        Assert.That(xaml, Does.StartWith("<Label x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("FontSize=\"18\""));
        Assert.That(xaml, Does.Contain("FontAttributes=\"Bold,Italic\""));
        Assert.That(xaml, Does.Contain("TextColor=\"#FF99CCFF\""));
        Assert.That(xaml, Does.Contain("Text=\"Hi\""));
    }

    [Test]
    public void Polygon_直線GeometryはPoints属性で出力()
    {
        var p = new NPolygonViewModel(0, 0, 100, 50);
        p.EdgeBrush.Value = new SolidColorBrush(Colors.Black);
        p.FillBrush.Value = null;
        var fig = new PathFigure(new Point(0, 0), new PathSegment[]
        {
            new LineSegment(new Point(100, 0), true),
            new LineSegment(new Point(50, 50), true),
        }, closed: true);
        var g = new PathGeometry(new[] { fig });

        var xaml = MauiShapeToXamlMapper.TryMapMauiPath(p, g, DefaultSettings);

        Assert.That(xaml, Does.StartWith("<Polygon x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Points=\"0,0 100,0 50,50\""));
    }

    [Test]
    public void Path系図形は_Path要素として出力()
    {
        var c = new OrthogonalConnectorViewModel();
        var fig = new PathFigure(new Point(0, 0), new PathSegment[]
        {
            new LineSegment(new Point(50, 50), true),
        }, closed: false);
        var g = new PathGeometry(new[] { fig });

        var xaml = MauiShapeToXamlMapper.TryMapMauiPath(c, g, DefaultSettings);

        Assert.That(xaml, Does.StartWith("<Path x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Data=\""));
    }

    [Test]
    public void TryMapMauiShape_Path系図形には常にnull()
    {
        var p = new NPolygonViewModel();
        Assert.That(MauiShapeToXamlMapper.TryMapMauiShape(p, DefaultSettings), Is.Null);
        var pdi = new PathDesignerItemViewModel();
        Assert.That(MauiShapeToXamlMapper.TryMapMauiShape(pdi, DefaultSettings), Is.Null);
    }

    [Test]
    public void インデント設定が_出力に反映される()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        r.EdgeBrush.Value = null;
        r.FillBrush.Value = null;
        var s = DefaultSettings with { IndentWidth = 2 };
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(r, s, indentLevel: 2);
        Assert.That(xaml, Does.StartWith("    <Rectangle"));  // 2 * 2 = 4 spaces
    }
}
