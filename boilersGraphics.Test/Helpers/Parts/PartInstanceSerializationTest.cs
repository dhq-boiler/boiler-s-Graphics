using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Parts;

[TestFixture]
public class PartInstanceSerializationTest
{
    private static MainWindowViewModel _mainVM;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Moq.Mock<Prism.Services.Dialogs.IDialogService>();
        _mainVM = new MainWindowViewModel(dlg.Object);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PartInstance_DefinitionIdがXMLに含まれる()
    {
        var defId = Guid.NewGuid();
        var pi = new PartInstanceViewModel(defId);
        pi.Left.Value = 10;
        pi.Top.Value = 20;
        pi.Width.Value = 30;
        pi.Height.Value = 40;
        pi.EdgeBrush.Value = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
        pi.FillBrush.Value = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);

        var x = ObjectSerializer.ExtractItem(pi);

        Assert.That(x.Name.LocalName, Is.EqualTo("DesignerItem"));
        Assert.That(x.Element("Type").Value, Is.EqualTo(typeof(PartInstanceViewModel).FullName));
        Assert.That(x.Element("DefinitionId")?.Value, Is.EqualTo(defId.ToString()));
        Assert.That(x.Element("ParameterValues"), Is.Not.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void PartInstance_ParameterValuesが型付きで書き出される()
    {
        var pi = new PartInstanceViewModel(Guid.NewGuid());
        pi.EdgeBrush.Value = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
        pi.FillBrush.Value = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
        var doubleEp = Guid.NewGuid();
        var intEp = Guid.NewGuid();
        var stringEp = Guid.NewGuid();

        pi.GetOrCreateParameterValue(doubleEp, 1.5);
        pi.GetOrCreateParameterValue(intEp, 7);
        pi.GetOrCreateParameterValue(stringEp, "hello");

        var x = ObjectSerializer.ExtractItem(pi);
        var pvs = x.Element("ParameterValues")!.Elements("ParameterValue").ToList();

        Assert.That(pvs.Count, Is.EqualTo(3));
        Assert.That(pvs.Single(p => p.Attribute("ExposedPropertyId")?.Value == doubleEp.ToString())
                       .Attribute("Type")?.Value, Is.EqualTo("Double"));
        Assert.That(pvs.Single(p => p.Attribute("ExposedPropertyId")?.Value == intEp.ToString())
                       .Attribute("Type")?.Value, Is.EqualTo("Int"));
        Assert.That(pvs.Single(p => p.Attribute("ExposedPropertyId")?.Value == stringEp.ToString())
                       .Value, Is.EqualTo("hello"));
    }
}
