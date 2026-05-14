using boilersGraphics.ViewModels.Anchors;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Anchors;

/// <summary>
/// プロパティダイアログ拡充: AnchorViewModel.SupportsPropertyDialog が true で
/// Detail ダイアログ起動経路が公開されたことを確認する。
/// </summary>
[TestFixture]
public class AnchorSupportsPropertyDialogTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void SupportsPropertyDialogはtrue()
    {
        var anchor = new AnchorViewModel();
        Assert.That(anchor.SupportsPropertyDialog, Is.True);
    }
}
