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
public class PartDefinitionsRoundTripTest
{
    private static DiagramViewModel CreateDiagram()
    {
        boilersGraphics.App.IsTest = true;
        var dlgService = new Mock<IDialogService>();
        var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
        return new DiagramViewModel(mainWindowViewModel);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DiagramViewModel_PartDefinitionsプロパティの初期状態は空()
    {
        var vm = CreateDiagram();
        Assert.That(vm.PartDefinitions, Is.Not.Null);
        Assert.That(vm.PartDefinitions, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ReadObjectsFromXML_PartDefinitionsを復元する()
    {
        var def1 = new PartDefinition { Name = "A" };
        var def2 = new PartDefinition { Name = "B" };
        var root = new XElement("boilersGraphics",
            new XElement("Version", "1.0"),
            new XElement("Layers"),
            PartSerializer.SerializeAll(new[] { def1, def2 }));

        var loadVm = CreateDiagram();
        ObjectDeserializer.ReadObjectsFromXML(loadVm, null, root);

        Assert.That(loadVm.PartDefinitions.Count, Is.EqualTo(2));
        var names = loadVm.PartDefinitions.Select(p => p.Name.Value).ToArray();
        Assert.That(names, Is.EqualTo(new[] { "A", "B" }));
        var ids = loadVm.PartDefinitions.Select(p => p.Id.Value).ToArray();
        Assert.That(ids, Is.EqualTo(new[] { def1.Id, def2.Id }));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ReadObjectsFromXML_PartDefinitionsセクションが無い旧形式でも例外なし()
    {
        var vm = CreateDiagram();
        var oldFormat = new XElement("boilersGraphics",
            new XElement("Version", "1.0"),
            new XElement("Layers"));

        Assert.DoesNotThrow(() =>
            ObjectDeserializer.ReadObjectsFromXML(vm, null, oldFormat));
        Assert.That(vm.PartDefinitions, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DiagramViewModel_PartDefinitionsに追加できる()
    {
        var vm = CreateDiagram();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });

        vm.PartDefinitions.Add(def);

        Assert.That(vm.PartDefinitions.Count, Is.EqualTo(1));
        Assert.That(vm.PartDefinitions[0].Name.Value, Is.EqualTo("リング"));
    }
}
