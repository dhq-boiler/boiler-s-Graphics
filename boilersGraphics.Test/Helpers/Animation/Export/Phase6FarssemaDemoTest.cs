using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation.Export;

/// <summary>
/// Phase 6 ホワイトボックス通し検証: autodebugger で UI 経由のカラー変更が多段操作になる代わりに、
/// コード経由で Farssema 風カラフル FUI を構築し、WpfStoryboardXamlBuilder / MauiAnimationXamlBuilder
/// から実 XAML を出力してファイルに dump する。
///
/// 出力ファイルは <c>.autodebugger-runs/e2e-20260522-phase6-fullrun/</c> 下に置かれるので、生成後に
/// 通常の WPF プロジェクトへコピペで貼れる demo になる。
/// </summary>
[TestFixture]
public class Phase6FarssemaDemoTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    // Farssema 風カラーパレット
    private static readonly Color BgDeepNavy = Color.FromArgb(0xFF, 0x05, 0x08, 0x10);
    private static readonly Color FrameOutline = Color.FromArgb(0xFF, 0x1B, 0x27, 0x33);
    private static readonly Color HudAmber = Color.FromArgb(0xFF, 0xFF, 0xB0, 0x00);
    private static readonly Color HudAmberFill = Color.FromArgb(0x1A, 0xFF, 0xB0, 0x00);
    private static readonly Color TitleCyan = Color.FromArgb(0xFF, 0x00, 0xDD, 0xFF);
    private static readonly Color CounterGreen = Color.FromArgb(0xFF, 0x59, 0xFF, 0x8F);
    private static readonly Color MatrixMagenta = Color.FromArgb(0xFF, 0xFF, 0x66, 0xAA);

    private static SolidColorBrush B(Color c) => new SolidColorBrush(c);

    [Test]
    public void Phase6_Farssema_FUI_WPF_出力をファイルに_dump()
    {
        var items = BuildFarssemaScene();

        var timeline = new TimelineViewModel(2.0, 30);
        var settings = new XamlExportSettings { ClassName = "FarssemaFui" };

        var xaml = WpfStoryboardXamlBuilder.Build(timeline, items, settings);

        // 主要要素のホワイトボックス検証
        Assert.That(xaml, Does.Contain("Text=\"FUI 12.0.6 :: TACTICAL READOUT\""));
        Assert.That(xaml, Does.Contain("Foreground=\"#FF00DDFF\""), "title cyan");
        Assert.That(xaml, Does.Contain("Foreground=\"#FFFFB000\""), "datagen amber");
        Assert.That(xaml, Does.Contain("Foreground=\"#FF59FF8F\""), "counter green");
        Assert.That(xaml, Does.Contain("Foreground=\"#FFFF66AA\""), "matrix magenta");
        Assert.That(xaml, Does.Contain("&#x0A;"), "matrix newlines encoded");
        Assert.That(xaml, Does.Contain("<!-- Generator: DataGenerator"));
        Assert.That(xaml, Does.Contain("<!-- Generator: NumberSequence"));
        Assert.That(xaml, Does.Contain("<!-- Generator: TextMatrix"));

        // demo ファイル出力 (ホワイトボックステストの副産物として残す)
        var outPath = ResolveDumpPath("farssema-fui-whitebox.xaml");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, xaml);
        TestContext.WriteLine($"WPF XAML dumped to: {outPath}");
    }

    [Test]
    public void Phase6_Farssema_FUI_MAUI_出力をファイルに_dump()
    {
        var items = BuildFarssemaScene();

        var timeline = new TimelineViewModel(2.0, 30);
        var settings = new XamlExportSettings { ClassName = "FarssemaFui" };

        var xaml = MauiAnimationXamlBuilder.Build(timeline, items, settings);

        Assert.That(xaml, Does.Contain("<ContentView"));
        Assert.That(xaml, Does.Contain("<AbsoluteLayout>"));
        Assert.That(xaml, Does.Contain("Text=\"FUI 12.0.6 :: TACTICAL READOUT\""));
        Assert.That(xaml, Does.Contain("TextColor=\"#FF00DDFF\""), "title cyan -> MAUI TextColor");
        Assert.That(xaml, Does.Contain("TextColor=\"#FFFFB000\""), "datagen amber");
        Assert.That(xaml, Does.Contain("&#x0A;"));

        var outPath = ResolveDumpPath("farssema-fui-whitebox-maui.xaml");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, xaml);
        TestContext.WriteLine($"MAUI XAML dumped to: {outPath}");
    }

    /// <summary>
    /// 共通シーン構築: 黒背景 + amber HUD frame + cyan タイトル + amber DataGen + green NumSeq + magenta TextMatrix。
    /// </summary>
    private static IReadOnlyList<SelectableDesignerItemViewModelBase> BuildFarssemaScene()
    {
        // 背景プレート: deep navy
        var bg = new NRectangleViewModel(0, 0, 1000, 1000);
        bg.EdgeBrush.Value = B(FrameOutline);
        bg.FillBrush.Value = B(BgDeepNavy);
        bg.EdgeThickness.Value = 1;

        // HUD frame: amber outline + amber 半透明 fill
        var frame = new NRectangleViewModel(40, 40, 920, 80);
        frame.EdgeBrush.Value = B(HudAmber);
        frame.FillBrush.Value = B(HudAmberFill);
        frame.EdgeThickness.Value = 1;

        // タイトル MonoText: cyan 太字
        var title = new MonoTextBlockViewModel();
        title.Left.Value = 60;
        title.Top.Value = 60;
        title.Width.Value = 900;
        title.Height.Value = 30;
        title.Text.Value = "FUI 12.0.6 :: TACTICAL READOUT";
        title.FontSize.Value = 18;
        title.Foreground.Value = B(TitleCyan);

        // DataGenerator: amber UUID 列 (Seed 固定で再現性を持たせる)
        var dataGen = new DataGeneratorTextBlockViewModel();
        dataGen.Left.Value = 60;
        dataGen.Top.Value = 160;
        dataGen.Width.Value = 900;
        dataGen.Height.Value = 24;
        dataGen.Type.Value = DataGeneratorType.Uuid;
        dataGen.Seed.Value = 12345;
        dataGen.Count.Value = 1;
        dataGen.Separator.Value = " ";
        dataGen.Layout.Value = DataGeneratorLayout.OneLine;
        dataGen.Foreground.Value = B(HudAmber);
        dataGen.FontSize.Value = 14;
        dataGen.Regenerate(); // Seed 適用後の値を Text にコミット

        // NumberSequence: green counter
        var numSeq = new NumberSequenceBlockViewModel();
        numSeq.Left.Value = 60;
        numSeq.Top.Value = 200;
        numSeq.Width.Value = 900;
        numSeq.Height.Value = 24;
        numSeq.Start.Value = 0;
        numSeq.End.Value = 15;
        numSeq.Step.Value = 1;
        numSeq.Direction.Value = NumberSequenceDirection.Horizontal;
        numSeq.Foreground.Value = B(CounterGreen);
        numSeq.FontSize.Value = 14;
        numSeq.Regenerate();

        // TextMatrix: magenta 4x4 grid
        var matrix = new TextMatrixBlockViewModel();
        matrix.Left.Value = 60;
        matrix.Top.Value = 250;
        matrix.Width.Value = 900;
        matrix.Height.Value = 120;
        matrix.Rows.Value = 4;
        matrix.Columns.Value = 4;
        matrix.CellMode.Value = TextMatrixCellMode.Sequential;
        matrix.SequenceStart.Value = 0;
        matrix.Separator.Value = " ";
        matrix.Foreground.Value = B(MatrixMagenta);
        matrix.FontSize.Value = 14;
        matrix.Regenerate();

        return new SelectableDesignerItemViewModelBase[]
        {
            bg, frame, title, dataGen, numSeq, matrix,
        };
    }

    private static string ResolveDumpPath(string fileName)
    {
        // Test 実行 dir からプロジェクト root を辿って .autodebugger-runs/e2e-20260522-phase6-fullrun へ
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, ".autodebugger-runs")))
        {
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        if (dir == null) dir = TestContext.CurrentContext.TestDirectory;
        return Path.Combine(dir, ".autodebugger-runs", "e2e-20260522-phase6-fullrun", fileName);
    }
}
