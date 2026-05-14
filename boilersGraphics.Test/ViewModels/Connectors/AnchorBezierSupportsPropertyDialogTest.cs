using boilersGraphics.ViewModels.Connectors;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Connectors;

/// <summary>
/// プロパティダイアログ拡充: AnchorBezierConnectorViewModel.SupportsPropertyDialog が true。
/// </summary>
[TestFixture]
public class AnchorBezierSupportsPropertyDialogTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void SupportsPropertyDialogはtrue()
    {
        var bezier = new AnchorBezierConnectorViewModel();
        Assert.That(bezier.SupportsPropertyDialog, Is.True);
    }
}
