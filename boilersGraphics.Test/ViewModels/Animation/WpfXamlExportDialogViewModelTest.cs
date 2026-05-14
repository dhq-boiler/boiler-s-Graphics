using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using Prism.Services.Dialogs;

namespace boilersGraphics.Test.ViewModels.Animation;

[TestFixture]
public class WpfXamlExportDialogViewModelTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test]
    public void Validate_全て妥当なら_空文字()
    {
        Assert.That(
            WpfXamlExportDialogViewModel.Validate("MyApp.Animations", "FuiAnimation", "public", 4, "C:/tmp/a.xaml"),
            Is.EqualTo(string.Empty));
    }

    [TestCase("", "FuiAnimation", "public", 4, "C:/tmp/a.xaml", "ターゲット名前空間")]
    [TestCase("   ", "FuiAnimation", "public", 4, "C:/tmp/a.xaml", "ターゲット名前空間")]
    [TestCase("1Foo", "FuiAnimation", "public", 4, "C:/tmp/a.xaml", "C# 識別子")]
    [TestCase("MyApp.Animations", "", "public", 4, "C:/tmp/a.xaml", "クラス名")]
    [TestCase("MyApp.Animations", "9Foo", "public", 4, "C:/tmp/a.xaml", "C# 識別子")]
    [TestCase("MyApp.Animations", "Foo", "private", 4, "C:/tmp/a.xaml", "アクセス修飾子")]
    [TestCase("MyApp.Animations", "Foo", "public", 0, "C:/tmp/a.xaml", "インデント幅")]
    [TestCase("MyApp.Animations", "Foo", "public", 9, "C:/tmp/a.xaml", "インデント幅")]
    [TestCase("MyApp.Animations", "Foo", "public", 4, "", "出力ファイルパス")]
    public void Validate_NG_例(string ns, string cn, string am, int iw, string op, string keyword)
    {
        var msg = WpfXamlExportDialogViewModel.Validate(ns, cn, am, iw, op);
        Assert.That(msg, Does.Contain(keyword));
    }

    [Test]
    public void 初期値は_デフォルト_FuiAnimation_等()
    {
        using var vm = new WpfXamlExportDialogViewModel();
        Assert.That(vm.TargetNamespace.Value, Is.EqualTo("MyApp.Animations"));
        Assert.That(vm.ClassName.Value, Is.EqualTo("FuiAnimation"));
        Assert.That(vm.AccessModifier.Value, Is.EqualTo("public"));
        Assert.That(vm.GenerateCodeBehind.Value, Is.True);
        Assert.That(vm.IndentWidth.Value, Is.EqualTo(4));
        Assert.That(vm.IncludeHeaderComment.Value, Is.True);
        Assert.That(vm.OutputPath.Value, Is.Empty);
    }

    [Test]
    public void OutputPath空のときは_ValidationMessageに表示()
    {
        using var vm = new WpfXamlExportDialogViewModel();
        Assert.That(vm.ValidationMessage.Value, Does.Contain("出力ファイルパス"));
    }

    [Test]
    public void OutputPath埋めれば_ValidationMessageは空()
    {
        using var vm = new WpfXamlExportDialogViewModel { OutputPath = { Value = "C:/tmp/a.xaml" } };
        Assert.That(vm.ValidationMessage.Value, Is.Empty);
    }

    [Test]
    public void ExecuteCommand_は_Settings_と_OutputPath_をDialogResultに載せる()
    {
        using var vm = new WpfXamlExportDialogViewModel
        {
            OutputPath = { Value = "C:/tmp/MySpin.xaml" },
            TargetNamespace = { Value = "Acme.Foo" },
            ClassName = { Value = "MySpin" },
            AccessModifier = { Value = "internal" },
            GenerateCodeBehind = { Value = false },
            IndentWidth = { Value = 2 },
            IncludeHeaderComment = { Value = false },
        };

        IDialogResult captured = null;
        vm.RequestClose += r => captured = r;

        vm.ExecuteCommand.Execute(R3.Unit.Default);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Result, Is.EqualTo(ButtonResult.OK));
        var settings = captured.Parameters.GetValue<XamlExportSettings>("Settings");
        Assert.That(settings.TargetNamespace, Is.EqualTo("Acme.Foo"));
        Assert.That(settings.ClassName, Is.EqualTo("MySpin"));
        Assert.That(settings.AccessModifier, Is.EqualTo("internal"));
        Assert.That(settings.GenerateCodeBehind, Is.False);
        Assert.That(settings.IndentWidth, Is.EqualTo(2));
        Assert.That(settings.IncludeHeaderComment, Is.False);
        Assert.That(captured.Parameters.GetValue<string>("OutputPath"), Is.EqualTo("C:/tmp/MySpin.xaml"));
    }

    [Test]
    public void CancelCommand_は_ButtonResultCancelで_RequestCloseを起こす()
    {
        using var vm = new WpfXamlExportDialogViewModel();
        IDialogResult captured = null;
        vm.RequestClose += r => captured = r;

        vm.CancelCommand.Execute(R3.Unit.Default);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Result, Is.EqualTo(ButtonResult.Cancel));
    }

    [Test]
    public void OnDialogOpened_でInitialClassName_OutputPath_反映()
    {
        using var vm = new WpfXamlExportDialogViewModel();
        var p = new DialogParameters
        {
            { "InitialClassName", "OnTheFly" },
            { "InitialOutputPath", "C:/tmp/OnTheFly.xaml" },
        };
        vm.OnDialogOpened(p);
        Assert.That(vm.ClassName.Value, Is.EqualTo("OnTheFly"));
        Assert.That(vm.OutputPath.Value, Is.EqualTo("C:/tmp/OnTheFly.xaml"));
    }

    [Test]
    public void OnDialogOpened_null_は_no_op()
    {
        using var vm = new WpfXamlExportDialogViewModel();
        vm.OnDialogOpened(null);
        Assert.That(vm.ClassName.Value, Is.EqualTo("FuiAnimation"));
    }
}
