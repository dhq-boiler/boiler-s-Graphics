using boilersGraphics.Helpers.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Parts;

[TestFixture]
public class PartNestingTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Moq.Mock<Prism.Services.Dialogs.IDialogService>();
        _ = new MainWindowViewModel(dlg.Object);
    }

    [Test]
    public void WouldCreateCycle_自分自身を入れる場合はtrue()
    {
        var a = new PartDefinitionViewModel();
        var registry = new Dictionary<Guid, PartDefinitionViewModel> { { a.Id.Value, a } };

        Assert.That(PartOperations.WouldCreateCycle(a.Id.Value, a.Id.Value, registry), Is.True);
    }

    [Test]
    public void WouldCreateCycle_独立した2定義は循環なし()
    {
        var a = new PartDefinitionViewModel();
        var b = new PartDefinitionViewModel();
        var registry = new Dictionary<Guid, PartDefinitionViewModel>
        {
            { a.Id.Value, a },
            { b.Id.Value, b },
        };

        Assert.That(PartOperations.WouldCreateCycle(a.Id.Value, b.Id.Value, registry), Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void WouldCreateCycle_AにBが含まれBにAを入れようとすると循環()
    {
        var a = new PartDefinitionViewModel();
        var b = new PartDefinitionViewModel();
        // A の Items に B のインスタンスを含めて、A は B に依存している
        a.Items.Add(new PartInstanceViewModel(b.Id.Value));
        var registry = new Dictionary<Guid, PartDefinitionViewModel>
        {
            { a.Id.Value, a },
            { b.Id.Value, b },
        };

        // B の Items に A のインスタンスを入れようとする → 循環
        Assert.That(PartOperations.WouldCreateCycle(b.Id.Value, a.Id.Value, registry), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void WouldCreateCycle_三層チェインの末端から先頭に戻すと循環()
    {
        var a = new PartDefinitionViewModel();
        var b = new PartDefinitionViewModel();
        var c = new PartDefinitionViewModel();
        a.Items.Add(new PartInstanceViewModel(b.Id.Value));
        b.Items.Add(new PartInstanceViewModel(c.Id.Value));
        var registry = new Dictionary<Guid, PartDefinitionViewModel>
        {
            { a.Id.Value, a },
            { b.Id.Value, b },
            { c.Id.Value, c },
        };

        // C の Items に A のインスタンスを入れようとする → 循環
        Assert.That(PartOperations.WouldCreateCycle(c.Id.Value, a.Id.Value, registry), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void WouldCreateCycle_未登録の定義IDは循環なし扱い()
    {
        var a = new PartDefinitionViewModel();
        var registry = new Dictionary<Guid, PartDefinitionViewModel> { { a.Id.Value, a } };

        Assert.That(PartOperations.WouldCreateCycle(a.Id.Value, Guid.NewGuid(), registry), Is.False);
    }

    [Test]
    public void WouldCreateCycle_nullレジストリでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PartOperations.WouldCreateCycle(Guid.NewGuid(), Guid.NewGuid(), null));
    }
}
