using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Text;

[TestFixture]
public class NumberSequenceBlockViewModelTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void デフォルトコンストラクタ_新規Modelを内部に持つ()
    {
        var vm = new NumberSequenceBlockViewModel();
        Assert.That(vm.Model, Is.Not.Null);
        Assert.That(vm.Model, Is.TypeOf<NumberSequenceBlock>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void コンストラクタ後_Textはデフォルト範囲を反映()
    {
        var vm = new NumberSequenceBlockViewModel();
        Assert.That(vm.Text.Value, Is.EqualTo("0 1 2 3 4 5 6 7 8 9 10"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Step変更で再生成_要素が増減()
    {
        var vm = new NumberSequenceBlockViewModel();
        vm.End.Value = 4;
        vm.Step.Value = 2;
        Assert.That(vm.Text.Value, Is.EqualTo("0 2 4"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Direction_Verticalで改行区切り()
    {
        var vm = new NumberSequenceBlockViewModel();
        vm.End.Value = 2;
        vm.Direction.Value = NumberSequenceDirection.Vertical;

        var lines = vm.Text.Value.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "0", "1", "2" }));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Format_D2で2桁0パディング()
    {
        var vm = new NumberSequenceBlockViewModel();
        vm.End.Value = 3;
        vm.Format.Value = "D2";

        Assert.That(vm.Text.Value, Is.EqualTo("00 01 02 03"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Grid_3x2で行列出力()
    {
        var vm = new NumberSequenceBlockViewModel();
        vm.End.Value = 5;
        vm.Separator.Value = ",";
        vm.Direction.Value = NumberSequenceDirection.Grid;
        vm.GridRows.Value = 3;
        vm.GridColumns.Value = 2;

        var lines = vm.Text.Value.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "0,1", "2,3", "4,5" }));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Separator変更で再生成()
    {
        var vm = new NumberSequenceBlockViewModel();
        vm.End.Value = 2;
        vm.Separator.Value = "::";
        Assert.That(vm.Text.Value, Is.EqualTo("0::1::2"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Start_End変更でも再生成()
    {
        var vm = new NumberSequenceBlockViewModel();
        vm.Start.Value = 10;
        vm.End.Value = 12;
        Assert.That(vm.Text.Value, Is.EqualTo("10 11 12"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsResizableはtrue()
    {
        var vm = new NumberSequenceBlockViewModel();
        Assert.That(vm.IsResizable, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SupportsPropertyDialogはfalse_Phase2d最小実装()
    {
        var vm = new NumberSequenceBlockViewModel();
        Assert.That(vm.SupportsPropertyDialog, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_Start_End_Step_Format_Separator_Direction_Grid全てコピー()
    {
        var vm = new NumberSequenceBlockViewModel
        {
            Left = { Value = 10 },
            Top = { Value = 20 },
            Width = { Value = 100 },
            Height = { Value = 30 },
        };
        vm.Start.Value = 5;
        vm.End.Value = 25;
        vm.Step.Value = 5;
        vm.Format.Value = "D3";
        vm.Separator.Value = "; ";
        vm.Direction.Value = NumberSequenceDirection.Grid;
        vm.GridRows.Value = 2;
        vm.GridColumns.Value = 3;

        var clone = (NumberSequenceBlockViewModel)vm.Clone();

        Assert.That(clone.Start.Value, Is.EqualTo(5));
        Assert.That(clone.End.Value, Is.EqualTo(25));
        Assert.That(clone.Step.Value, Is.EqualTo(5));
        Assert.That(clone.Format.Value, Is.EqualTo("D3"));
        Assert.That(clone.Separator.Value, Is.EqualTo("; "));
        Assert.That(clone.Direction.Value, Is.EqualTo(NumberSequenceDirection.Grid));
        Assert.That(clone.GridRows.Value, Is.EqualTo(2));
        Assert.That(clone.GridColumns.Value, Is.EqualTo(3));
        Assert.That(clone.Text.Value, Is.EqualTo(vm.Text.Value));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GetViewTypeはPath()
    {
        var vm = new NumberSequenceBlockViewModel();
        Assert.That(vm.GetViewType(), Is.EqualTo(typeof(System.Windows.Shapes.Path)));
    }
}
