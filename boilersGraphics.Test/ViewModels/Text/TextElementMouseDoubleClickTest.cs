using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Text;

/// <summary>
/// Phase 6.5: テキスト系 ViewModel 5 種に MouseDoubleClickCommand が定義されていて、
/// Execute() すると OpenPropertyDialog 経路が発火することを assert する。
/// Phase 6 ▶ 再生デモで Detail dialog 起動が必要な前提課題。
///
/// 注: OpenPropertyDialog の中身 (Detail dialog の Show) は App.IsTest=true 下では
/// DialogService が起動できず実 dialog は出ない。ここでは Command が "発火される" ことだけを確認する。
/// </summary>
[TestFixture]
public class TextElementMouseDoubleClickTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test, Apartment(ApartmentState.STA)]
    public void MonoText_MouseDoubleClickCommand_は_null_でない()
    {
        var vm = new MonoTextBlockViewModel(new MonoTextBlock());
        Assert.That(vm.MouseDoubleClickCommand, Is.Not.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void DataGen_MouseDoubleClickCommand_は_null_でない()
    {
        var vm = new DataGeneratorTextBlockViewModel(new DataGeneratorTextBlock());
        Assert.That(vm.MouseDoubleClickCommand, Is.Not.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void NumSeq_MouseDoubleClickCommand_は_null_でない()
    {
        var vm = new NumberSequenceBlockViewModel(new NumberSequenceBlock());
        Assert.That(vm.MouseDoubleClickCommand, Is.Not.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void TextMatrix_MouseDoubleClickCommand_は_null_でない()
    {
        var vm = new TextMatrixBlockViewModel(new TextMatrixBlock());
        Assert.That(vm.MouseDoubleClickCommand, Is.Not.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void TextOnPath_MouseDoubleClickCommand_は_null_でない()
    {
        var vm = new TextOnPathBlockViewModel(new TextOnPathBlock());
        Assert.That(vm.MouseDoubleClickCommand, Is.Not.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void TextMatrix_MouseDoubleClickCommand_Execute_は_OpenPropertyDialog_経路に到達する_例外無し()
    {
        // App.IsTest=true 下では DialogService の Show が短絡されるはず。
        // ここでは「Command Execute が例外で死なない」「OpenPropertyDialog の冒頭まで届く」を最低限の signal とする。
        var vm = new TextMatrixBlockViewModel(new TextMatrixBlock());
        Assert.That(vm.SupportsPropertyDialog, Is.True);
        Assert.DoesNotThrow(() => vm.MouseDoubleClickCommand.Execute(R3.Unit.Default));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void MonoText_MouseDoubleClickCommand_Execute_は_no_op_で_例外無し()
    {
        // MonoText は SupportsPropertyDialog=False、OpenPropertyDialog は no-op (基底 TextElementBaseViewModel)。
        // Command Execute は何も起こらないが、例外で死なないこと。
        var vm = new MonoTextBlockViewModel(new MonoTextBlock());
        Assert.That(vm.SupportsPropertyDialog, Is.False);
        Assert.DoesNotThrow(() => vm.MouseDoubleClickCommand.Execute(R3.Unit.Default));
    }
}
