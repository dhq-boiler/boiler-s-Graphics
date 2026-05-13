using boilersGraphics.ViewModels;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Threading;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class PromoteToPartDialogViewModelTest
{
    [Test, RequiresThread(ApartmentState.STA)]
    public void 初期PartNameは空文字()
    {
        var vm = new PromoteToPartDialogViewModel();
        Assert.That(vm.PartName.Value, Is.EqualTo(string.Empty));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_既定名パラメータがPartNameに反映される()
    {
        var vm = new PromoteToPartDialogViewModel();
        var parameters = new DialogParameters
        {
            { PromoteToPartDialogViewModel.SelectedPartNameKey, "パーツ1" }
        };

        vm.OnDialogOpened(parameters);

        Assert.That(vm.PartName.Value, Is.EqualTo("パーツ1"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OkCommand_DialogResultOKとPartNameを返す()
    {
        var vm = new PromoteToPartDialogViewModel();
        vm.PartName.Value = "リング";
        IDialogResult result = null;
        vm.RequestClose += r => result = r;

        vm.OkCommand.Execute();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.EqualTo(ButtonResult.OK));
        Assert.That(result.Parameters.GetValue<string>(PromoteToPartDialogViewModel.SelectedPartNameKey),
            Is.EqualTo("リング"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void CancelCommand_DialogResultCancelを返す()
    {
        var vm = new PromoteToPartDialogViewModel();
        IDialogResult result = null;
        vm.RequestClose += r => result = r;

        vm.CancelCommand.Execute();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.EqualTo(ButtonResult.Cancel));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Title_パーツ化_を返す()
    {
        var vm = new PromoteToPartDialogViewModel();
        Assert.That(vm.Title, Is.EqualTo("パーツ化"));
    }
}
