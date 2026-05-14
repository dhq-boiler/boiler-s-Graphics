using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Threading;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Test.Helpers;

/// <summary>
/// Phase 4-f: 図形側 Glow プロパティ (GlowRadius / GlowIntensity / GlowColor) のラウンドトリップ確認。
/// 1) 全値 RoundTrip / 2) デフォルト値時は省略 / 3) GlowColor null のときは要素省略 / 4) 後方互換 (要素なし)。
/// </summary>
[TestFixture]
public class GlowRoundTripTest
{
    private static DiagramViewModel _diagram;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Mock<IDialogService>();
        _diagram = new MainWindowViewModel(dlg.Object).DiagramViewModel;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Rectangle_Glow全値_完全ラウンドトリップ()
    {
        var src = new NRectangleViewModel { Owner = _diagram };
        src.Left.Value = 10;
        src.Top.Value = 20;
        src.Width.Value = 100;
        src.Height.Value = 50;
        src.EdgeThickness.Value = 1.0;
        src.GlowRadius.Value = 6.5;
        src.GlowIntensity.Value = 0.42;
        src.GlowColor.Value = Color.FromArgb(255, 255, 87, 51);

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml) as NRectangleViewModel;

        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.GlowRadius.Value, Is.EqualTo(6.5).Within(1e-6));
        Assert.That(dst.GlowIntensity.Value, Is.EqualTo(0.42).Within(1e-6));
        Assert.That(dst.GlowColor.Value, Is.EqualTo(Color.FromArgb(255, 255, 87, 51)));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GlowRadius0_3要素全省略_復元時もデフォルト()
    {
        var src = new NRectangleViewModel { Owner = _diagram };
        src.Left.Value = 0;
        src.Top.Value = 0;
        src.Width.Value = 100;
        src.Height.Value = 50;
        src.EdgeThickness.Value = 1.0;
        // GlowRadius は デフォルト 0 のまま

        var xml = ObjectSerializer.ExtractItem(src);

        Assert.That(xml.Element("GlowRadius"), Is.Null, "GlowRadius=0 なら省略");
        Assert.That(xml.Element("GlowIntensity"), Is.Null, "GlowRadius=0 なら省略");
        Assert.That(xml.Element("GlowColor"), Is.Null, "GlowRadius=0 なら省略");

        var dst = ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml) as NRectangleViewModel;
        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.GlowRadius.Value, Is.EqualTo(0));
        Assert.That(dst.GlowIntensity.Value, Is.EqualTo(0.5));
        Assert.That(dst.GlowColor.Value, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GlowColorのみnull_Radius正値_要素2つだけ出力()
    {
        var src = new NRectangleViewModel { Owner = _diagram };
        src.Left.Value = 0;
        src.Top.Value = 0;
        src.Width.Value = 100;
        src.Height.Value = 50;
        src.EdgeThickness.Value = 1.0;
        src.GlowRadius.Value = 3.0;
        src.GlowIntensity.Value = 0.7;
        src.GlowColor.Value = null; // EdgeBrush 同色で合成 (描画側で解決)

        var xml = ObjectSerializer.ExtractItem(src);
        Assert.That(xml.Element("GlowRadius"), Is.Not.Null);
        Assert.That(xml.Element("GlowIntensity"), Is.Not.Null);
        Assert.That(xml.Element("GlowColor"), Is.Null, "null は要素ごと省略");

        var dst = ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml) as NRectangleViewModel;
        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.GlowRadius.Value, Is.EqualTo(3.0).Within(1e-6));
        Assert.That(dst.GlowIntensity.Value, Is.EqualTo(0.7).Within(1e-6));
        Assert.That(dst.GlowColor.Value, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void 後方互換_Glow要素を取り除いてもクラッシュしない()
    {
        // Glow を設定して XML 化 → 3 要素を削除 → 復元 (旧ファイル相当)
        var src = new NRectangleViewModel { Owner = _diagram };
        src.Left.Value = 0;
        src.Top.Value = 0;
        src.Width.Value = 100;
        src.Height.Value = 50;
        src.EdgeThickness.Value = 1.0;
        src.GlowRadius.Value = 5.0;
        src.GlowIntensity.Value = 0.6;
        src.GlowColor.Value = Colors.Red;

        var xml = ObjectSerializer.ExtractItem(src);
        // Phase 4-f 前の古いファイル相当
        xml.Element("GlowRadius")?.Remove();
        xml.Element("GlowIntensity")?.Remove();
        xml.Element("GlowColor")?.Remove();

        var dst = ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml) as NRectangleViewModel;
        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.GlowRadius.Value, Is.EqualTo(0), "デフォルト 0 で初期化される");
        Assert.That(dst.GlowIntensity.Value, Is.EqualTo(0.5));
        Assert.That(dst.GlowColor.Value, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Color_AARRGGBBフォーマット_パース成功()
    {
        var src = new NRectangleViewModel { Owner = _diagram };
        src.Left.Value = 0;
        src.Top.Value = 0;
        src.Width.Value = 100;
        src.Height.Value = 50;
        src.EdgeThickness.Value = 1.0;
        src.GlowRadius.Value = 4.0;
        src.GlowColor.Value = Color.FromArgb(0xCC, 0x12, 0x34, 0x56); // 半透明色

        var xml = ObjectSerializer.ExtractItem(src);
        var glowColorText = xml.Element("GlowColor")?.Value;
        Assert.That(glowColorText, Is.EqualTo("#CC123456"));

        var dst = ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml) as NRectangleViewModel;
        Assert.That(dst, Is.Not.Null);
        Assert.That(dst.GlowColor.Value, Is.EqualTo(Color.FromArgb(0xCC, 0x12, 0x34, 0x56)));
    }
}
