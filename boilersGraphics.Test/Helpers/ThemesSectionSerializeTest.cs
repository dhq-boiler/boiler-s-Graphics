using boilersGraphics.Helpers;
using boilersGraphics.Models.Themes;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace boilersGraphics.Test.Helpers;

/// <summary>
/// Phase 4-f-2: ObjectSerializer.SerializeThemes による `<Themes>` セクションの出力を検証する。
/// 復元側 (ObjectDeserializer.RestoreThemesSection) は private のため、SerializeThemes の出力構造のみテスト。
/// </summary>
[TestFixture]
public class ThemesSectionSerializeTest
{
    private static DiagramViewModel CreateDiagram()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Mock<IDialogService>();
        return new MainWindowViewModel(dlg.Object).DiagramViewModel;
    }

    [Test, Apartment(ApartmentState.STA)]
    public void SerializeThemes_ActiveTheme未設定はThemes要素のみ()
    {
        var diagram = CreateDiagram();
        // ActiveTheme.Value はデフォルト null
        var xml = ObjectSerializer.SerializeThemes(diagram);
        Assert.That(xml.Name.LocalName, Is.EqualTo("Themes"));
        Assert.That(xml.Element("ActiveThemeId"), Is.Null, "未設定なら子要素なし");
    }

    [Test, Apartment(ApartmentState.STA)]
    public void SerializeThemes_ActiveTheme設定済みはActiveThemeId子要素()
    {
        var diagram = CreateDiagram();
        var bladerunner = diagram.AvailableThemes.First(t => t.Name == "Bladerunner");
        diagram.ActiveTheme.Value = bladerunner;

        var xml = ObjectSerializer.SerializeThemes(diagram);
        var activeId = xml.Element("ActiveThemeId");
        Assert.That(activeId, Is.Not.Null);
        Assert.That(activeId.Value, Is.EqualTo(ThemeRepository.BladerunnerId.ToString()));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void SerializeThemes_diagram_nullでもクラッシュしない()
    {
        var xml = ObjectSerializer.SerializeThemes(null);
        Assert.That(xml, Is.Not.Null);
        Assert.That(xml.Name.LocalName, Is.EqualTo("Themes"));
        Assert.That(xml.Element("ActiveThemeId"), Is.Null);
    }
}
