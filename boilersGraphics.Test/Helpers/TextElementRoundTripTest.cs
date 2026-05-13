using boilersGraphics.Helpers;
using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Text;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers;

/// <summary>
/// Phase 2-e: ObjectSerializer.ExtractItem → ObjectDeserializer.ExtractDesignerItemViewModelBase
/// のラウンドトリップで、MonoTextBlock / DataGeneratorTextBlock / NumberSequenceBlock の
/// 状態が完全に復元できることを確認する。
/// </summary>
[TestFixture]
public class TextElementRoundTripTest
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
    public void MonoTextBlock_共通テキスト属性が完全復元()
    {
        var src = new MonoTextBlockViewModel();
        src.Left.Value = 10;
        src.Top.Value = 20;
        src.Width.Value = 200;
        src.Height.Value = 40;
        src.Text.Value = "0xCAFE / 192.168.0.1";
        src.FontFamily.Value = "Cascadia Code";
        src.FontSize.Value = 18;
        src.Foreground.Value = new SolidColorBrush(Colors.LimeGreen);
        src.Background.Value = new SolidColorBrush(Colors.Black);
        src.LineHeight.Value = 22.0;
        src.LetterSpacing.Value = 1.5;
        src.TextOpacity.Value = 0.85;
        src.IsWordWrap.Value = true;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (MonoTextBlockViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml);

        Assert.That(dst.Text.Value, Is.EqualTo("0xCAFE / 192.168.0.1"));
        Assert.That(dst.FontFamily.Value, Is.EqualTo("Cascadia Code"));
        Assert.That(dst.FontSize.Value, Is.EqualTo(18));
        Assert.That(((SolidColorBrush)dst.Foreground.Value).Color, Is.EqualTo(Colors.LimeGreen));
        Assert.That(((SolidColorBrush)dst.Background.Value).Color, Is.EqualTo(Colors.Black));
        Assert.That(dst.LineHeight.Value, Is.EqualTo(22.0));
        Assert.That(dst.LetterSpacing.Value, Is.EqualTo(1.5));
        Assert.That(dst.TextOpacity.Value, Is.EqualTo(0.85));
        Assert.That(dst.IsWordWrap.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void MonoTextBlock_Background_LineHeight_null時もラウンドトリップ()
    {
        var src = new MonoTextBlockViewModel();
        src.Background.Value = null;
        src.LineHeight.Value = null;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (MonoTextBlockViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml);

        Assert.That(dst.Background.Value, Is.Null);
        Assert.That(dst.LineHeight.Value, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DataGeneratorTextBlock_完全ラウンドトリップ_Textも一致()
    {
        var src = new DataGeneratorTextBlockViewModel();
        src.Type.Value = DataGeneratorType.Ipv4Address;
        src.Seed.Value = 4242;
        src.IsSeedLocked.Value = true;
        src.Count.Value = 5;
        src.Separator.Value = " / ";
        src.Layout.Value = DataGeneratorLayout.OneLine;

        var beforeText = src.Text.Value;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (DataGeneratorTextBlockViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml);

        Assert.That(dst.Type.Value, Is.EqualTo(DataGeneratorType.Ipv4Address));
        Assert.That(dst.Seed.Value, Is.EqualTo(4242));
        Assert.That(dst.IsSeedLocked.Value, Is.True);
        Assert.That(dst.Count.Value, Is.EqualTo(5));
        Assert.That(dst.Separator.Value, Is.EqualTo(" / "));
        Assert.That(dst.Layout.Value, Is.EqualTo(DataGeneratorLayout.OneLine));
        Assert.That(dst.Text.Value, Is.EqualTo(beforeText), "Seed が同じなら Generator が同じ Text を再生成する");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void NumberSequenceBlock_完全ラウンドトリップ_Textも一致()
    {
        var src = new NumberSequenceBlockViewModel();
        src.Start.Value = 0;
        src.End.Value = 9;
        src.Step.Value = 1;
        src.Format.Value = "X2";
        src.Separator.Value = "-";
        src.Direction.Value = NumberSequenceDirection.Grid;
        src.GridRows.Value = 2;
        src.GridColumns.Value = 5;

        var beforeText = src.Text.Value;

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (NumberSequenceBlockViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml);

        Assert.That(dst.Start.Value, Is.EqualTo(0));
        Assert.That(dst.End.Value, Is.EqualTo(9));
        Assert.That(dst.Step.Value, Is.EqualTo(1));
        Assert.That(dst.Format.Value, Is.EqualTo("X2"));
        Assert.That(dst.Separator.Value, Is.EqualTo("-"));
        Assert.That(dst.Direction.Value, Is.EqualTo(NumberSequenceDirection.Grid));
        Assert.That(dst.GridRows.Value, Is.EqualTo(2));
        Assert.That(dst.GridColumns.Value, Is.EqualTo(5));
        Assert.That(dst.Text.Value, Is.EqualTo(beforeText), "Start/End/Step が同じなら同じ Text に再生成される");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void NumberSequenceBlock_小数Step_InvariantCultureで往復()
    {
        var src = new NumberSequenceBlockViewModel();
        src.Start.Value = 0.25;
        src.End.Value = 1.75;
        src.Step.Value = 0.125;
        src.Format.Value = "F3";

        var xml = ObjectSerializer.ExtractItem(src);
        var dst = (NumberSequenceBlockViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, xml);

        Assert.That(dst.Start.Value, Is.EqualTo(0.25));
        Assert.That(dst.End.Value, Is.EqualTo(1.75));
        Assert.That(dst.Step.Value, Is.EqualTo(0.125));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TextElement_FontFamilyとIsWordWrap_型ごとに独立復元()
    {
        var mono = new MonoTextBlockViewModel();
        mono.FontFamily.Value = "Cascadia Code";
        mono.IsWordWrap.Value = true;

        var datagen = new DataGeneratorTextBlockViewModel();
        datagen.FontFamily.Value = "Consolas";
        datagen.IsWordWrap.Value = false;

        var monoXml = ObjectSerializer.ExtractItem(mono);
        var datagenXml = ObjectSerializer.ExtractItem(datagen);

        var monoBack = (MonoTextBlockViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, monoXml);
        var datagenBack = (DataGeneratorTextBlockViewModel)ObjectDeserializer.ExtractDesignerItemViewModelBase(_diagram, datagenXml);

        Assert.That(monoBack.FontFamily.Value, Is.EqualTo("Cascadia Code"));
        Assert.That(monoBack.IsWordWrap.Value, Is.True);
        Assert.That(datagenBack.FontFamily.Value, Is.EqualTo("Consolas"));
        Assert.That(datagenBack.IsWordWrap.Value, Is.False);
    }
}
