using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Text;

/// <summary>
/// プロパティダイアログ拡充: TextMatrixBlockViewModel.SupportsPropertyDialog が true。
/// </summary>
[TestFixture]
public class TextMatrixSupportsPropertyDialogTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void SupportsPropertyDialogはtrue()
    {
        var vm = new TextMatrixBlockViewModel(new TextMatrixBlock());
        Assert.That(vm.SupportsPropertyDialog, Is.True);
    }
}
