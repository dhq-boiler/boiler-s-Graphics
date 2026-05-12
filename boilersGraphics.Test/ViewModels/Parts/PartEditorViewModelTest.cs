using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class PartEditorViewModelTest
{
    [Test, RequiresThread(ApartmentState.STA)]
    public void Title_初期値はパーツ編集()
    {
        var vm = new PartEditorViewModel();
        Assert.That(vm.Title, Is.EqualTo("パーツ編集"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Definition_初期値はnull()
    {
        var vm = new PartEditorViewModel();
        Assert.That(vm.Definition, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_PartDefinition付き_DefinitionとTitleが反映される()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "リング" };
        var defVm = new PartDefinitionViewModel(model);

        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        Assert.That(vm.Definition, Is.SameAs(defVm));
        Assert.That(vm.Title, Is.EqualTo("パーツ編集: リング"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_DefinitionのNameが変わるとTitleが連動する()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "リング" };
        var defVm = new PartDefinitionViewModel(model);

        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        defVm.Name.Value = "目盛り";

        Assert.That(vm.Title, Is.EqualTo("パーツ編集: 目盛り"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_NameがnullまたはWhitespaceならパーツ編集()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "" };
        var defVm = new PartDefinitionViewModel(model);

        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        Assert.That(vm.Title, Is.EqualTo("パーツ編集"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_パラメータがnull_例外なし()
    {
        var vm = new PartEditorViewModel();
        Assert.DoesNotThrow(() => vm.OnDialogOpened(null));
        Assert.That(vm.Definition, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_PartDefinitionKeyなし_例外なし()
    {
        var vm = new PartEditorViewModel();
        var parameters = new DialogParameters
        {
            { "OtherKey", "value" }
        };
        Assert.DoesNotThrow(() => vm.OnDialogOpened(parameters));
        Assert.That(vm.Definition, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void CloseCommand_RequestCloseでButtonResultOKを返す()
    {
        var vm = new PartEditorViewModel();
        IDialogResult result = null;
        vm.RequestClose += r => result = r;

        vm.CloseCommand.Execute();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.EqualTo(ButtonResult.OK));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void CanCloseDialog_常にtrue()
    {
        var vm = new PartEditorViewModel();
        Assert.That(vm.CanCloseDialog(), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogClosed_例外なくDisposeできる()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "リング" };
        var defVm = new PartDefinitionViewModel(model);
        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        Assert.DoesNotThrow(() => vm.OnDialogClosed());
        Assert.DoesNotThrow(() => vm.OnDialogClosed()); // 二重 Dispose も問題なし
    }
}
