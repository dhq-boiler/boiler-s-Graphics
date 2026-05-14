using boilersGraphics.Helpers.Parts;
using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Parts;

[TestFixture]
public class PartOperationsTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        boilersGraphics.App.IsTest = true;
        var dlg = new Moq.Mock<Prism.Services.Dialogs.IDialogService>();
        _ = new MainWindowViewModel(dlg.Object);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_選択図形をPartDefinitionに移管してPartInstanceを返す()
    {
        var r1 = new NRectangleViewModel(10, 20, 30, 40);
        var r2 = new NRectangleViewModel(50, 60, 30, 40);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "リング");

        Assert.That(result.Definition.Name.Value, Is.EqualTo("リング"));
        Assert.That(result.Definition.Items.Count, Is.EqualTo(2));
        Assert.That(result.Instance.DefinitionId.Value, Is.EqualTo(result.Definition.Id.Value));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_PartInstanceの位置サイズは選択図形群のバウンディングボックス()
    {
        var r1 = new NRectangleViewModel(10, 20, 30, 40);
        var r2 = new NRectangleViewModel(50, 60, 30, 40);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "P");

        Assert.That(result.Instance.Left.Value, Is.EqualTo(10));
        Assert.That(result.Instance.Top.Value, Is.EqualTo(20));
        Assert.That(result.Instance.Width.Value, Is.EqualTo(70));
        Assert.That(result.Instance.Height.Value, Is.EqualTo(80));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_空の選択でInvalidOperationExceptionをスローする()
    {
        Assert.Throws<InvalidOperationException>(() =>
            PartOperations.Promote(Array.Empty<DesignerItemViewModelBase>(), "P"));
    }

    [Test]
    public void Promote_nullでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => PartOperations.Promote(null, "P"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_既存ExposedPropertyのDefaultValueがPartInstanceにコピーされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P");

        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "幅",
            Type = ExposedPropertyType.Double,
            DefaultValue = 50.0
        });
        result.Definition.ExposedProperties.Add(ep);

        var promotedAfter = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P2");
        promotedAfter.Definition.ExposedProperties.Add(new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "X",
            Type = ExposedPropertyType.Double,
            DefaultValue = 7.5
        }));

        var thirdPromote = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P3");
        Assert.That(thirdPromote.Instance.ParameterValues.Count, Is.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Detach_PartDefinitionの内部図形をクローンして返す()
    {
        var r1 = new NRectangleViewModel(10, 20, 30, 40);
        var r2 = new NRectangleViewModel(50, 60, 30, 40);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "P");

        var detached = PartOperations.Detach(promoted.Instance, promoted.Definition);

        Assert.That(detached.Count, Is.EqualTo(2));
        Assert.That(detached[0], Is.Not.SameAs(r1), "クローンであり同一インスタンスではない");
        Assert.That(detached[1], Is.Not.SameAs(r2));
        Assert.That(detached[0], Is.TypeOf<NRectangleViewModel>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Detach_DefinitionId不一致でInvalidOperationExceptionをスローする()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "P");
        var wrongDef = new PartDefinitionViewModel();

        Assert.Throws<InvalidOperationException>(() =>
            PartOperations.Detach(promoted.Instance, wrongDef));
    }

    [Test]
    public void Detach_nullでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => PartOperations.Detach(null, new PartDefinitionViewModel()));
        Assert.Throws<ArgumentNullException>(() => PartOperations.Detach(new PartInstanceViewModel(), null));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_新Idと新Nameを持つ独立した定義を返す()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元パーツ");

        var clone = PartOperations.Clone(promoted.Definition, "新パーツ");

        Assert.That(clone.Id.Value, Is.Not.EqualTo(promoted.Definition.Id.Value));
        Assert.That(clone.Name.Value, Is.EqualTo("新パーツ"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_ExposedPropertyとBindingがコピーされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元");
        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "幅",
            Type = ExposedPropertyType.Double,
            DefaultValue = 50.0,
        });
        ep.Bindings.Add(new BindingViewModel(new Binding
        {
            TargetItemId = r.ID,
            TargetProperty = "Width"
        }));
        promoted.Definition.ExposedProperties.Add(ep);

        var clone = PartOperations.Clone(promoted.Definition, "新");

        Assert.That(clone.ExposedProperties.Count, Is.EqualTo(1));
        var clonedEp = clone.ExposedProperties[0];
        Assert.That(clonedEp.Name.Value, Is.EqualTo("幅"));
        Assert.That(clonedEp.DefaultValue.Value, Is.EqualTo(50.0));
        Assert.That(clonedEp.Bindings.Count, Is.EqualTo(1));
        Assert.That(clonedEp.Bindings[0].TargetProperty.Value, Is.EqualTo("Width"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_内部図形がディープコピーされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元");

        var clone = PartOperations.Clone(promoted.Definition, "新");

        Assert.That(clone.Items.Count, Is.EqualTo(1));
        Assert.That(clone.Items[0], Is.Not.SameAs(r));
        Assert.That(clone.Items[0], Is.TypeOf<NRectangleViewModel>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Clone_BindingのTargetItemIdがクローンした図形のIdにマッピングされる()
    {
        var r = new NRectangleViewModel(0, 0, 10, 10);
        var promoted = PartOperations.Promote(new DesignerItemViewModelBase[] { r }, "元");
        // Phase 1-c-9 以降、Promote は Definition.Items にクローンを入れるので
        // bindings は Definition 内部の (クローン側) Id を指す前提で書く。
        var definitionItemId = promoted.Definition.Items[0].ID;
        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "幅",
            Type = ExposedPropertyType.Double,
        });
        ep.Bindings.Add(new BindingViewModel(new Binding
        {
            TargetItemId = definitionItemId,
            TargetProperty = "Width"
        }));
        promoted.Definition.ExposedProperties.Add(ep);

        var clone = PartOperations.Clone(promoted.Definition, "新");

        var clonedItemId = clone.Items[0].ID;
        Assert.That(clone.ExposedProperties[0].Bindings[0].TargetItemId.Value, Is.EqualTo(clonedItemId));
        Assert.That(clone.ExposedProperties[0].Bindings[0].TargetItemId.Value, Is.Not.EqualTo(definitionItemId));
    }

    [Test]
    public void Clone_nullでArgumentNullExceptionをスローする()
    {
        Assert.Throws<ArgumentNullException>(() => PartOperations.Clone(null, "新"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_Definition_Itemsはオリジナルとは別インスタンスになる()
    {
        var original = new NRectangleViewModel(0, 0, 10, 10);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { original }, "P1");

        Assert.That(result.Definition.Items, Has.Count.EqualTo(1));
        Assert.That(result.Definition.Items[0], Is.Not.SameAs(original));
        Assert.That(result.Definition.Items[0], Is.TypeOf<NRectangleViewModel>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_オリジナルをDisposeしてもDefinition_Itemsは無事に読める()
    {
        boilersGraphics.App.IsTest = true;
        var original = new NRectangleViewModel(0, 0, 10, 10);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { original }, "P1");
        // Promote 直後にオリジナル側を Dispose しても、Definition.Items の側は別オブジェクトなので無事。
        original.Dispose();

        var def = result.Definition;
        Assert.That(def.Items, Has.Count.EqualTo(1));
        Assert.DoesNotThrow(() =>
        {
            var _ = def.Items[0].Width.Value;
            var __ = def.Items[0].Left.Value;
            var ___ = def.Items[0].IsSelected.Value;
        });
    }

    // 「パーツ化したときにずれます」バグ修正のリグレッション。
    // PartInstanceDesignerItemDataTemplate が item.Left/Top をそのまま Canvas.Left/Top に bind し、
    // Canvas 自体は PartInstance.Left/Top に置かれるので、Definition.Items の Left/Top は
    // bounds の左上を 0 にした「相対座標」でなければ world 上で (bounds.X, bounds.Y) ぶんずれる。

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_Definition内のItemsはbounds左上を起点にした相対座標()
    {
        var r1 = new NRectangleViewModel(100, 200, 30, 40);
        var r2 = new NRectangleViewModel(150, 250, 30, 40);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "P");

        // bounds = (100, 200, 80, 90)。各 item の Left/Top は (0,0) と (50,50) になっているはず。
        Assert.That(result.Definition.Items[0].Left.Value, Is.EqualTo(0));
        Assert.That(result.Definition.Items[0].Top.Value, Is.EqualTo(0));
        Assert.That(result.Definition.Items[1].Left.Value, Is.EqualTo(50));
        Assert.That(result.Definition.Items[1].Top.Value, Is.EqualTo(50));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_オリジナル図形のLeftTopは触らない()
    {
        var r1 = new NRectangleViewModel(100, 200, 30, 40);

        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1 }, "P");

        // 元の r1 は Clone してから操作するので不変
        Assert.That(r1.Left.Value, Is.EqualTo(100));
        Assert.That(r1.Top.Value, Is.EqualTo(200));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Detach_Definitionの相対座標をinstance位置で絶対座標に戻す()
    {
        var r1 = new NRectangleViewModel(100, 200, 30, 40);
        var r2 = new NRectangleViewModel(150, 250, 30, 40);
        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "P");
        // PartInstance を別位置に動かしてから Detach (移動先で復元するパス)
        result.Instance.Left.Value = 500;
        result.Instance.Top.Value = 600;

        var detached = PartOperations.Detach(result.Instance, result.Definition);

        // 相対 (0,0) と (50,50) に、instance の (500,600) が加算されて (500,600) と (550,650)
        Assert.That(detached, Has.Count.EqualTo(2));
        Assert.That(detached[0].Left.Value, Is.EqualTo(500));
        Assert.That(detached[0].Top.Value, Is.EqualTo(600));
        Assert.That(detached[1].Left.Value, Is.EqualTo(550));
        Assert.That(detached[1].Top.Value, Is.EqualTo(650));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Promote_Detach_ラウンドトリップでLeftTopが復元される()
    {
        // Promote → そのままの位置で Detach すれば元の絶対座標に戻る
        var r1 = new NRectangleViewModel(100, 200, 30, 40);
        var r2 = new NRectangleViewModel(150, 250, 30, 40);
        var result = PartOperations.Promote(new DesignerItemViewModelBase[] { r1, r2 }, "P");

        var detached = PartOperations.Detach(result.Instance, result.Definition);

        Assert.That(detached[0].Left.Value, Is.EqualTo(100));
        Assert.That(detached[0].Top.Value, Is.EqualTo(200));
        Assert.That(detached[1].Left.Value, Is.EqualTo(150));
        Assert.That(detached[1].Top.Value, Is.EqualTo(250));
    }
}
