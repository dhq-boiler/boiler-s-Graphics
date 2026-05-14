using boilersGraphics.ViewModels;
using NUnit.Framework;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class DiagramViewModelXamlExportResolverTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static PathGeometry MakeTriangle()
    {
        var fig = new PathFigure(new Point(0, 0), new PathSegment[]
        {
            new LineSegment(new Point(10, 0), true),
            new LineSegment(new Point(5, 10), true),
        }, closed: true);
        return new PathGeometry(new[] { fig });
    }

    [Test]
    public void null_item_は_null()
    {
        var resolver = DiagramViewModel.BuildPathGeometryResolverForXamlExport();
        Assert.That(resolver(null), Is.Null);
    }

    [Test]
    public void PathGeometryNoRotateに値があれば_そのまま返す()
    {
        var resolver = DiagramViewModel.BuildPathGeometryResolverForXamlExport();
        var rect = new NRectangleViewModel();
        var g = MakeTriangle();
        rect.PathGeometryNoRotate.Value = g;
        Assert.That(resolver(rect), Is.SameAs(g));
    }

    [Test]
    public void PathGeometryNoRotateがnullなら_CreateGeometryをフォールバック()
    {
        var resolver = DiagramViewModel.BuildPathGeometryResolverForXamlExport();
        var rect = new NRectangleViewModel(0, 0, 50, 50);
        rect.PathGeometryNoRotate.Value = null;
        // NRectangleViewModel.CreateGeometry は GeometryCreator.CreateRectangle を呼ぶ。
        // App.IsTest = true でも純粋な PathGeometry 生成は通る。
        var got = resolver(rect);
        Assert.That(got, Is.Not.Null);
        Assert.That(got.Figures.Count, Is.GreaterThan(0));
    }

    [Test]
    public void PathDesignerItemViewModel_CreateGeometryがNotSupportedでも_例外を呑んでnull()
    {
        var resolver = DiagramViewModel.BuildPathGeometryResolverForXamlExport();
        var path = new PathDesignerItemViewModel();
        path.PathGeometryNoRotate.Value = null;
        // CreateGeometry は NotSupportedException を投げる。resolver は try/catch で null フォールバック。
        Assert.That(resolver(path), Is.Null);
    }
}
