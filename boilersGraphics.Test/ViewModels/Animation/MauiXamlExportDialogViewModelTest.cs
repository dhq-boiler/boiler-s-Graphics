using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using Prism.Services.Dialogs;

namespace boilersGraphics.Test.ViewModels.Animation;

[TestFixture]
public class MauiXamlExportDialogViewModelTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test]
    public void 初期値はFuiAnimation_GenerateCodeBehindフィールドはない()
    {
        using var vm = new MauiXamlExportDialogViewModel();
        Assert.That(vm.TargetNamespace.Value, Is.EqualTo("MyApp.Animations"));
        Assert.That(vm.ClassName.Value, Is.EqualTo("FuiAnimation"));
        Assert.That(vm.AccessModifier.Value, Is.EqualTo("public"));
        Assert.That(vm.IndentWidth.Value, Is.EqualTo(4));
        Assert.That(vm.IncludeHeaderComment.Value, Is.True);
        Assert.That(vm.OutputPath.Value, Is.Empty);
    }

    [Test]
    public void OutputPath空のときValidationMessage_埋めれば空()
    {
        using var vm = new MauiXamlExportDialogViewModel();
        Assert.That(vm.ValidationMessage.Value, Does.Contain("出力ファイルパス"));
        vm.OutputPath.Value = "C:/tmp/a.xaml";
        Assert.That(vm.ValidationMessage.Value, Is.Empty);
    }

    [Test]
    public void ExecuteCommand_は_Settings_GenerateCodeBehind_常にtrue()
    {
        using var vm = new MauiXamlExportDialogViewModel
        {
            OutputPath = { Value = "C:/tmp/Foo.xaml" },
            ClassName = { Value = "MySpin" },
        };
        IDialogResult captured = null;
        vm.RequestClose += r => captured = r;
        vm.ExecuteCommand.Execute(R3.Unit.Default);
        Assert.That(captured!.Result, Is.EqualTo(ButtonResult.OK));
        var settings = captured.Parameters.GetValue<XamlExportSettings>("Settings");
        Assert.That(settings.ClassName, Is.EqualTo("MySpin"));
        Assert.That(settings.GenerateCodeBehind, Is.True);
        Assert.That(captured.Parameters.GetValue<string>("OutputPath"), Is.EqualTo("C:/tmp/Foo.xaml"));
    }

    [Test]
    public void CancelCommand_は_ButtonResultCancel()
    {
        using var vm = new MauiXamlExportDialogViewModel();
        IDialogResult captured = null;
        vm.RequestClose += r => captured = r;
        vm.CancelCommand.Execute(R3.Unit.Default);
        Assert.That(captured!.Result, Is.EqualTo(ButtonResult.Cancel));
    }

    [Test]
    public void OnDialogOpened_でInitialClassName_OutputPath_反映()
    {
        using var vm = new MauiXamlExportDialogViewModel();
        var p = new DialogParameters
        {
            { "InitialClassName", "OnTheFly" },
            { "InitialOutputPath", "C:/tmp/OnTheFly.xaml" },
        };
        vm.OnDialogOpened(p);
        Assert.That(vm.ClassName.Value, Is.EqualTo("OnTheFly"));
        Assert.That(vm.OutputPath.Value, Is.EqualTo("C:/tmp/OnTheFly.xaml"));
    }
}
