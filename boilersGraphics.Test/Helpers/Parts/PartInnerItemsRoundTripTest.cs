using boilersGraphics.Helpers;
using boilersGraphics.Helpers.Parts;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace boilersGraphics.Test.Helpers.Parts;

[TestFixture]
public class PartInnerItemsRoundTripTest
{
    private static DiagramViewModel CreateDiagram()
    {
        boilersGraphics.App.IsTest = true;
        var dlgService = new Mock<IDialogService>();
        var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
        return new DiagramViewModel(mainWindowViewModel);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SerializeDefinitionFromViewModel_内部図形がItemsの子要素として書き出される()
    {
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        var r1 = new NRectangleViewModel(10, 20, 30, 40);
        var r2 = new NRectangleViewModel(50, 60, 30, 40);
        def.Items.Add(r1);
        def.Items.Add(r2);

        var elm = PartSerializer.SerializeDefinitionFromViewModel(def);

        var items = elm.Element("Items");
        Assert.That(items, Is.Not.Null);
        Assert.That(items!.Elements("DesignerItem").Count(), Is.EqualTo(2));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ReadObjectsFromXML_PartDefinitions内の内部図形を復元する()
    {
        var saveVm = CreateDiagram();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        def.Items.Add(new NRectangleViewModel(10, 20, 30, 40));
        def.Items.Add(new NRectangleViewModel(50, 60, 70, 80));
        saveVm.PartDefinitions.Add(def);

        var root = new XElement("boilersGraphics",
            new XElement("Version", "1.0"),
            new XElement("Layers"),
            PartSerializer.SerializeAll(saveVm.PartDefinitions));

        var loadVm = CreateDiagram();
        ObjectDeserializer.ReadObjectsFromXML(loadVm, null, root);

        Assert.That(loadVm.PartDefinitions.Count, Is.EqualTo(1));
        Assert.That(loadVm.PartDefinitions[0].Items.Count, Is.EqualTo(2));
        Assert.That(loadVm.PartDefinitions[0].Items[0], Is.TypeOf<NRectangleViewModel>());
        var r1 = (NRectangleViewModel)loadVm.PartDefinitions[0].Items[0];
        Assert.That(r1.Left.Value, Is.EqualTo(10));
        Assert.That(r1.Top.Value, Is.EqualTo(20));
        Assert.That(r1.Width.Value, Is.EqualTo(30));
        Assert.That(r1.Height.Value, Is.EqualTo(40));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ReadObjectsFromXML_空Itemsでもクラッシュしない()
    {
        var def = new PartDefinition { Name = "空パーツ" };
        var root = new XElement("boilersGraphics",
            new XElement("Version", "1.0"),
            new XElement("Layers"),
            PartSerializer.SerializeAll(new[] { def }));

        var loadVm = CreateDiagram();
        Assert.DoesNotThrow(() => ObjectDeserializer.ReadObjectsFromXML(loadVm, null, root));
        Assert.That(loadVm.PartDefinitions.Count, Is.EqualTo(1));
        Assert.That(loadVm.PartDefinitions[0].Items, Is.Empty);
    }
}
