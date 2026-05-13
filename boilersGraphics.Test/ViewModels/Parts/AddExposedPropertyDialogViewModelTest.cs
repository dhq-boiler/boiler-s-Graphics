using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class AddExposedPropertyDialogViewModelTest
{
    [Test, RequiresThread(ApartmentState.STA)]
    public void Title_初期値は公開パラメータの追加()
    {
        var vm = new AddExposedPropertyDialogViewModel();
        Assert.That(vm.Title, Is.EqualTo("公開パラメータの追加"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AvailableTypes_8つの型を提示する()
    {
        var vm = new AddExposedPropertyDialogViewModel();
        Assert.That(vm.AvailableTypes, Has.Count.EqualTo(8));
        Assert.That(vm.AvailableTypes, Does.Contain(ExposedPropertyType.Double));
        Assert.That(vm.AvailableTypes, Does.Contain(ExposedPropertyType.Enum));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OkCommand_Name入力なし_ErrorMessageが設定されRequestCloseは呼ばれない()
    {
        var vm = new AddExposedPropertyDialogViewModel();
        IDialogResult result = null;
        vm.RequestClose += r => result = r;

        vm.OkCommand.Execute();

        Assert.That(result, Is.Null);
        Assert.That(vm.ErrorMessage.Value, Is.Not.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OkCommand_Name入力あり_ExposedPropertyが返る()
    {
        var vm = new AddExposedPropertyDialogViewModel();
        vm.Name.Value = "半径";
        vm.SelectedType.Value = ExposedPropertyType.Double;
        vm.DefaultValueText.Value = "12.5";

        IDialogResult result = null;
        vm.RequestClose += r => result = r;

        vm.OkCommand.Execute();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.EqualTo(ButtonResult.OK));
        var ep = result.Parameters.GetValue<ExposedProperty>(
            AddExposedPropertyDialogViewModel.ExposedPropertyKey);
        Assert.That(ep, Is.Not.Null);
        Assert.That(ep!.Name, Is.EqualTo("半径"));
        Assert.That(ep.Type, Is.EqualTo(ExposedPropertyType.Double));
        Assert.That(ep.DefaultValue, Is.EqualTo(12.5d));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OkCommand_Nameの前後空白はトリムされる()
    {
        var vm = new AddExposedPropertyDialogViewModel();
        vm.Name.Value = "  幅  ";

        IDialogResult result = null;
        vm.RequestClose += r => result = r;
        vm.OkCommand.Execute();

        var ep = result!.Parameters.GetValue<ExposedProperty>(
            AddExposedPropertyDialogViewModel.ExposedPropertyKey);
        Assert.That(ep!.Name, Is.EqualTo("幅"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void CancelCommand_RequestCloseはCancel()
    {
        var vm = new AddExposedPropertyDialogViewModel();
        IDialogResult result = null;
        vm.RequestClose += r => result = r;

        vm.CancelCommand.Execute();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.EqualTo(ButtonResult.Cancel));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ParseDefaultValue_Double型_数値文字列が変換される()
    {
        Assert.That(AddExposedPropertyDialogViewModel.ParseDefaultValue(ExposedPropertyType.Double, "3.14"),
            Is.EqualTo(3.14d));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ParseDefaultValue_Int型_整数文字列が変換される()
    {
        Assert.That(AddExposedPropertyDialogViewModel.ParseDefaultValue(ExposedPropertyType.Int, "42"),
            Is.EqualTo(42));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ParseDefaultValue_Boolean型_true文字列がboolに変換される()
    {
        Assert.That(AddExposedPropertyDialogViewModel.ParseDefaultValue(ExposedPropertyType.Boolean, "true"),
            Is.EqualTo(true));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ParseDefaultValue_String型_そのまま返る()
    {
        Assert.That(AddExposedPropertyDialogViewModel.ParseDefaultValue(ExposedPropertyType.String, "abc"),
            Is.EqualTo("abc"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ParseDefaultValue_Double型でパース不能_デフォルト0()
    {
        Assert.That(AddExposedPropertyDialogViewModel.ParseDefaultValue(ExposedPropertyType.Double, "xyz"),
            Is.EqualTo(0d));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ParseDefaultValue_空文字_Doubleは0_Stringは空文字()
    {
        Assert.That(AddExposedPropertyDialogViewModel.ParseDefaultValue(ExposedPropertyType.Double, ""),
            Is.EqualTo(0d));
        Assert.That(AddExposedPropertyDialogViewModel.ParseDefaultValue(ExposedPropertyType.String, ""),
            Is.EqualTo(string.Empty));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OkCommand_IsArrayチェック付き_ExposedProperty_IsArray_true()
    {
        var vm = new AddExposedPropertyDialogViewModel();
        vm.Name.Value = "頂点";
        vm.SelectedType.Value = ExposedPropertyType.Point;
        vm.IsArray.Value = true;

        IDialogResult result = null;
        vm.RequestClose += r => result = r;
        vm.OkCommand.Execute();

        var ep = result!.Parameters.GetValue<ExposedProperty>(
            AddExposedPropertyDialogViewModel.ExposedPropertyKey);
        Assert.That(ep!.IsArray, Is.True);
    }
}
