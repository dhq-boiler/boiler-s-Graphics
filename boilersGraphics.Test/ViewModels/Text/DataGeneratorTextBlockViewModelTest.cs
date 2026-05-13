using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Text;

[TestFixture]
public class DataGeneratorTextBlockViewModelTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void デフォルトコンストラクタ_新規Modelを内部に持つ()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        Assert.That(vm.Model, Is.Not.Null);
        Assert.That(vm.Model, Is.TypeOf<DataGeneratorTextBlock>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void コンストラクタ後_TextはGeneratorで生成済み()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        Assert.That(vm.Text.Value, Is.Not.Null);
        Assert.That(vm.Text.Value, Is.Not.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void デフォルトプロパティ_Modelに対応する初期値()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        Assert.That(vm.Type.Value, Is.EqualTo(DataGeneratorType.Hex));
        Assert.That(vm.IsSeedLocked.Value, Is.False);
        Assert.That(vm.Count.Value, Is.EqualTo(8));
        Assert.That(vm.Separator.Value, Is.EqualTo(" "));
        Assert.That(vm.Layout.Value, Is.EqualTo(DataGeneratorLayout.OneLine));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のType変更で再生成_UUID形式になる()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        vm.Count.Value = 1;
        vm.Type.Value = DataGeneratorType.Uuid;

        Assert.That(Guid.TryParseExact(vm.Text.Value, "D", out _), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のSeed変更で再生成_出力が変わる()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        var before = vm.Text.Value;
        vm.Seed.Value = vm.Seed.Value + 12345;
        var after = vm.Text.Value;

        Assert.That(after, Is.Not.EqualTo(before));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のCount変更で再生成_MultiLine行数が変わる()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        vm.Layout.Value = DataGeneratorLayout.MultiLine;
        vm.Count.Value = 3;

        var lines = vm.Text.Value.Split(Environment.NewLine);
        Assert.That(lines.Length, Is.EqualTo(3));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のSeparator変更で再生成_OneLineに反映()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        vm.Count.Value = 3;
        vm.Separator.Value = "::";

        Assert.That(vm.Text.Value.Split("::").Length, Is.EqualTo(3));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsSeedLocked切替は再生成をトリガーしない()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        var before = vm.Text.Value;
        vm.IsSeedLocked.Value = true;
        Assert.That(vm.Text.Value, Is.EqualTo(before));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Regenerate_明示呼出でSeedに応じた再生成()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        vm.Seed.Value = 7;
        var first = vm.Text.Value;
        vm.Regenerate();
        Assert.That(vm.Text.Value, Is.EqualTo(first));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsResizableはtrue()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        Assert.That(vm.IsResizable, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SupportsPropertyDialogはfalse_Phase2c最小実装()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        Assert.That(vm.SupportsPropertyDialog, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_Type_Seed_Count_Layoutをコピーする()
    {
        var vm = new DataGeneratorTextBlockViewModel
        {
            Left = { Value = 10 },
            Top = { Value = 20 },
            Width = { Value = 100 },
            Height = { Value = 30 },
        };
        vm.Type.Value = DataGeneratorType.Ipv4Address;
        vm.Seed.Value = 4242;
        vm.Count.Value = 5;
        vm.Separator.Value = "; ";
        vm.Layout.Value = DataGeneratorLayout.MultiLine;
        vm.IsSeedLocked.Value = true;

        var clone = (DataGeneratorTextBlockViewModel)vm.Clone();

        Assert.That(clone.Left.Value, Is.EqualTo(10));
        Assert.That(clone.Type.Value, Is.EqualTo(DataGeneratorType.Ipv4Address));
        Assert.That(clone.Seed.Value, Is.EqualTo(4242));
        Assert.That(clone.Count.Value, Is.EqualTo(5));
        Assert.That(clone.Separator.Value, Is.EqualTo("; "));
        Assert.That(clone.Layout.Value, Is.EqualTo(DataGeneratorLayout.MultiLine));
        Assert.That(clone.IsSeedLocked.Value, Is.True);
        Assert.That(clone.Text.Value, Is.EqualTo(vm.Text.Value));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GetViewTypeはPath()
    {
        var vm = new DataGeneratorTextBlockViewModel();
        Assert.That(vm.GetViewType(), Is.EqualTo(typeof(System.Windows.Shapes.Path)));
    }
}
