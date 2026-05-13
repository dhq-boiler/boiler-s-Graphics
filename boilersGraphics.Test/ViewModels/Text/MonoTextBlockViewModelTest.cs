using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test.ViewModels.Text;

[TestFixture]
public class MonoTextBlockViewModelTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void デフォルトコンストラクタ_新規Modelを内部に持つ()
    {
        var vm = new MonoTextBlockViewModel();
        Assert.That(vm.Model, Is.Not.Null);
        Assert.That(vm.Model, Is.TypeOf<MonoTextBlock>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void nullModelでArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MonoTextBlockViewModel(null));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void デフォルトプロパティ_Modelに対応する初期値()
    {
        var vm = new MonoTextBlockViewModel();
        Assert.That(vm.Text.Value, Is.EqualTo(string.Empty));
        Assert.That(vm.FontFamily.Value, Is.EqualTo(TextElementBase.DefaultFontFamily));
        Assert.That(vm.FontFamily.Value, Does.Contain("JetBrains Mono"));
        Assert.That(vm.FontSize.Value, Is.EqualTo(12));
        Assert.That(vm.Foreground.Value, Is.EqualTo(Brushes.White));
        Assert.That(vm.Background.Value, Is.Null);
        Assert.That(vm.LineHeight.Value, Is.Null);
        Assert.That(vm.LetterSpacing.Value, Is.EqualTo(0d));
        Assert.That(vm.TextOpacity.Value, Is.EqualTo(1.0));
        Assert.That(vm.IsWordWrap.Value, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void 既存Modelの値を初期値として読み込む()
    {
        var model = new MonoTextBlock
        {
            Text = "0xABCD",
            FontFamily = "JetBrains Mono",
            FontSize = 16,
            LetterSpacing = 1.5,
            IsWordWrap = true,
        };

        var vm = new MonoTextBlockViewModel(model);

        Assert.That(vm.Text.Value, Is.EqualTo("0xABCD"));
        Assert.That(vm.FontFamily.Value, Is.EqualTo("JetBrains Mono"));
        Assert.That(vm.FontSize.Value, Is.EqualTo(16));
        Assert.That(vm.LetterSpacing.Value, Is.EqualTo(1.5));
        Assert.That(vm.IsWordWrap.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のTextを変更するとModelに同期される()
    {
        var vm = new MonoTextBlockViewModel();
        vm.Text.Value = "FEED";

        Assert.That(vm.Model.Text, Is.EqualTo("FEED"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のFontFamilyを変更するとModelに同期される()
    {
        var vm = new MonoTextBlockViewModel();
        vm.FontFamily.Value = "JetBrains Mono";

        Assert.That(vm.Model.FontFamily, Is.EqualTo("JetBrains Mono"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のForegroundを変更するとModelに同期される()
    {
        var vm = new MonoTextBlockViewModel();
        vm.Foreground.Value = Brushes.Lime;

        Assert.That(vm.Model.Foreground, Is.EqualTo(Brushes.Lime));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void VM側のLineHeightをnullにできる()
    {
        var vm = new MonoTextBlockViewModel();
        vm.LineHeight.Value = 18.0;
        Assert.That(vm.Model.LineHeight, Is.EqualTo(18.0));

        vm.LineHeight.Value = null;
        Assert.That(vm.Model.LineHeight, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsResizableはtrue()
    {
        var vm = new MonoTextBlockViewModel();
        Assert.That(vm.IsResizable, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SupportsPropertyDialogはfalse_Phase2b最小実装()
    {
        var vm = new MonoTextBlockViewModel();
        Assert.That(vm.SupportsPropertyDialog, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void GetViewTypeはPath()
    {
        var vm = new MonoTextBlockViewModel();
        Assert.That(vm.GetViewType(), Is.EqualTo(typeof(System.Windows.Shapes.Path)));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OpenPropertyDialog_例外なし_最小実装はnoop()
    {
        var vm = new MonoTextBlockViewModel();
        Assert.DoesNotThrow(() => vm.OpenPropertyDialog());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_位置サイズと共通テキスト属性をコピーする()
    {
        var vm = new MonoTextBlockViewModel
        {
            Left = { Value = 10 },
            Top = { Value = 20 },
            Width = { Value = 100 },
            Height = { Value = 30 },
            RotationAngle = { Value = 45 },
        };
        vm.Text.Value = "[INFO]";
        vm.FontFamily.Value = "JetBrains Mono";
        vm.FontSize.Value = 18;
        vm.Foreground.Value = Brushes.Cyan;
        vm.Background.Value = Brushes.Black;
        vm.LineHeight.Value = 22.0;
        vm.LetterSpacing.Value = 2.0;
        vm.TextOpacity.Value = 0.7;
        vm.IsWordWrap.Value = true;

        var clone = (MonoTextBlockViewModel)vm.Clone();

        Assert.That(clone.Left.Value, Is.EqualTo(10));
        Assert.That(clone.Top.Value, Is.EqualTo(20));
        Assert.That(clone.Width.Value, Is.EqualTo(100));
        Assert.That(clone.Height.Value, Is.EqualTo(30));
        Assert.That(clone.RotationAngle.Value, Is.EqualTo(45));
        Assert.That(clone.Text.Value, Is.EqualTo("[INFO]"));
        Assert.That(clone.FontFamily.Value, Is.EqualTo("JetBrains Mono"));
        Assert.That(clone.FontSize.Value, Is.EqualTo(18));
        Assert.That(clone.Foreground.Value, Is.EqualTo(Brushes.Cyan));
        Assert.That(clone.Background.Value, Is.EqualTo(Brushes.Black));
        Assert.That(clone.LineHeight.Value, Is.EqualTo(22.0));
        Assert.That(clone.LetterSpacing.Value, Is.EqualTo(2.0));
        Assert.That(clone.TextOpacity.Value, Is.EqualTo(0.7));
        Assert.That(clone.IsWordWrap.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_別Modelインスタンス_元と独立()
    {
        var vm = new MonoTextBlockViewModel();
        vm.Text.Value = "ORIG";

        var clone = (MonoTextBlockViewModel)vm.Clone();
        Assert.That(clone.Model, Is.Not.SameAs(vm.Model));

        clone.Text.Value = "CLONE";
        Assert.That(vm.Text.Value, Is.EqualTo("ORIG"));
        Assert.That(vm.Model.Text, Is.EqualTo("ORIG"));
        Assert.That(clone.Model.Text, Is.EqualTo("CLONE"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void CreateGeometry_非nullの矩形PathGeometryを返す()
    {
        var vm = new MonoTextBlockViewModel();
        vm.Width.Value = 50;
        vm.Height.Value = 20;

        var geo = vm.CreateGeometry();

        Assert.That(geo, Is.Not.Null);
    }
}
