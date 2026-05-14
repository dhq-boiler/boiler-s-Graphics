using boilersGraphics.ViewModels.Connectors;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Connectors;

/// <summary>
/// プロパティダイアログ拡充: OrthogonalConnectorViewModel.SupportsPropertyDialog が true で
/// Detail ダイアログ起動経路が公開されたことを確認する。
/// </summary>
[TestFixture]
public class OrthogonalSupportsPropertyDialogTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void SupportsPropertyDialogはtrue()
    {
        var orthogonal = new OrthogonalConnectorViewModel();
        Assert.That(orthogonal.SupportsPropertyDialog, Is.True);
    }
}
