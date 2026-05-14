using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Text;

/// <summary>
/// プロパティダイアログ拡充: TextOnPathBlockViewModel.SupportsPropertyDialog が true。
/// </summary>
[TestFixture]
public class TextOnPathSupportsPropertyDialogTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void SupportsPropertyDialogはtrue()
    {
        var vm = new TextOnPathBlockViewModel(new TextOnPathBlock());
        Assert.That(vm.SupportsPropertyDialog, Is.True);
    }
}
