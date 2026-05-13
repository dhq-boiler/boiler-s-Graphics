using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows.Media;

namespace boilersGraphics.Test.ViewModels.Parts;

/// <summary>
/// Phase 2-f: PartInstance.InitializeRenderedItems / ApplyValueToProperty の挙動を pin する。
/// PartDefinition.Items を ID 引き継ぎでクローンして RenderedItems に詰め、
/// ExposedProperty.Bindings を辿って ParameterValues の変更を内部 Item に値伝搬する。
/// </summary>
[TestFixture]
public class PartInstanceRenderedItemsTest
{
    [SetUp]
    public void SetUp() => boilersGraphics.App.IsTest = true;

    private static (PartDefinitionViewModel definition, NRectangleViewModel rect, ExposedPropertyViewModel ep)
        BuildDefinitionWithWidthBinding()
    {
        var definition = new PartDefinitionViewModel();
        var rect = new NRectangleViewModel();
        rect.Width.Value = 50;
        definition.Items.Add(rect);

        var ep = new ExposedPropertyViewModel
        {
            Name = { Value = "Width" },
            Type = { Value = ExposedPropertyType.Double },
            DefaultValue = { Value = 80.0 },
        };
        ep.Bindings.Add(new BindingViewModel(new Binding
        {
            TargetItemId = rect.ID,
            TargetProperty = "Width",
        }));
        definition.ExposedProperties.Add(ep);

        return (definition, rect, ep);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RenderedItemsはコンストラクタ後は空()
    {
        var vm = new PartInstanceViewModel();
        Assert.That(vm.RenderedItems, Is.Not.Null);
        Assert.That(vm.RenderedItems.Count, Is.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Initialize_DefinitionのItemsをID引継ぎCloneして格納()
    {
        var (definition, rect, _) = BuildDefinitionWithWidthBinding();
        var instance = new PartInstanceViewModel();

        instance.InitializeRenderedItems(definition);

        Assert.That(instance.RenderedItems.Count, Is.EqualTo(1));
        Assert.That(instance.RenderedItems[0], Is.Not.SameAs(rect), "別インスタンスのクローンになる");
        Assert.That(instance.RenderedItems[0].ID, Is.EqualTo(rect.ID), "Binding.TargetItemId 解決のため ID は引き継がれる");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Initialize_DefaultValueがParameterValueに反映される()
    {
        var (definition, _, ep) = BuildDefinitionWithWidthBinding();
        var instance = new PartInstanceViewModel();

        instance.InitializeRenderedItems(definition);

        Assert.That(instance.TryGetParameterValue(ep.Id.Value, out var rp), Is.True);
        Assert.That(rp.Value, Is.EqualTo(80.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ParameterValue変更_RenderedItemのプロパティに伝搬()
    {
        var (definition, _, ep) = BuildDefinitionWithWidthBinding();
        var instance = new PartInstanceViewModel();
        instance.InitializeRenderedItems(definition);

        instance.GetOrCreateParameterValue(ep.Id.Value).Value = 200.0;

        var renderedRect = (NRectangleViewModel)instance.RenderedItems[0];
        Assert.That(renderedRect.Width.Value, Is.EqualTo(200.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Initialize_複数回呼んでも安全_RenderedItemsは再構築()
    {
        var (definition, _, _) = BuildDefinitionWithWidthBinding();
        var instance = new PartInstanceViewModel();

        instance.InitializeRenderedItems(definition);
        instance.InitializeRenderedItems(definition);

        Assert.That(instance.RenderedItems.Count, Is.EqualTo(1));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Initialize_nullでArgumentNullException()
    {
        var instance = new PartInstanceViewModel();
        Assert.Throws<ArgumentNullException>(() => instance.InitializeRenderedItems(null));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Initialize_複数Bindingで同じExposedPropertyから値が複数Itemに伝搬()
    {
        var definition = new PartDefinitionViewModel();
        var rectA = new NRectangleViewModel();
        var rectB = new NRectangleViewModel();
        definition.Items.Add(rectA);
        definition.Items.Add(rectB);

        var ep = new ExposedPropertyViewModel
        {
            Name = { Value = "EdgeThickness" },
            Type = { Value = ExposedPropertyType.Double },
            DefaultValue = { Value = 0.0 },
        };
        ep.Bindings.Add(new BindingViewModel(new Binding { TargetItemId = rectA.ID, TargetProperty = "EdgeThickness" }));
        ep.Bindings.Add(new BindingViewModel(new Binding { TargetItemId = rectB.ID, TargetProperty = "EdgeThickness" }));
        definition.ExposedProperties.Add(ep);

        var instance = new PartInstanceViewModel();
        instance.InitializeRenderedItems(definition);

        instance.GetOrCreateParameterValue(ep.Id.Value).Value = 5.0;

        Assert.That(((NRectangleViewModel)instance.RenderedItems[0]).EdgeThickness.Value, Is.EqualTo(5.0));
        Assert.That(((NRectangleViewModel)instance.RenderedItems[1]).EdgeThickness.Value, Is.EqualTo(5.0));
    }

    // ---- ApplyValueToProperty 単体 ----

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_BindableReactivePropertyのValueに代入()
    {
        var rect = new NRectangleViewModel();
        PartInstanceViewModel.ApplyValueToProperty(rect, "Width", 123.0);
        Assert.That(rect.Width.Value, Is.EqualTo(123.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_int値をdoubleに変換できる()
    {
        var rect = new NRectangleViewModel();
        PartInstanceViewModel.ApplyValueToProperty(rect, "Width", 99); // int → double
        Assert.That(rect.Width.Value, Is.EqualTo(99.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_Brush型もそのまま代入できる()
    {
        var rect = new NRectangleViewModel();
        var brush = new SolidColorBrush(Colors.Red);
        PartInstanceViewModel.ApplyValueToProperty(rect, "FillBrush", brush);
        Assert.That(rect.FillBrush.Value, Is.SameAs(brush));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_存在しないプロパティ名は静かに無視()
    {
        var rect = new NRectangleViewModel();
        Assert.DoesNotThrow(() => PartInstanceViewModel.ApplyValueToProperty(rect, "DoesNotExist", 1));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_nullターゲットや空プロパティ名は何もしない()
    {
        Assert.DoesNotThrow(() => PartInstanceViewModel.ApplyValueToProperty(null, "Width", 1));
        Assert.DoesNotThrow(() => PartInstanceViewModel.ApplyValueToProperty(new NRectangleViewModel(), null, 1));
        Assert.DoesNotThrow(() => PartInstanceViewModel.ApplyValueToProperty(new NRectangleViewModel(), string.Empty, 1));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Apply_型不一致でConvert失敗時は何もしない()
    {
        var rect = new NRectangleViewModel();
        var before = rect.Width.Value;
        // string "abc" は double に変換不能
        Assert.DoesNotThrow(() => PartInstanceViewModel.ApplyValueToProperty(rect, "Width", "abc"));
        Assert.That(rect.Width.Value, Is.EqualTo(before), "失敗時は元の値を保つ");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Dispose_RenderedItemsをクリア()
    {
        var (definition, _, _) = BuildDefinitionWithWidthBinding();
        var instance = new PartInstanceViewModel();
        instance.InitializeRenderedItems(definition);

        Assert.That(instance.RenderedItems.Count, Is.EqualTo(1));
        instance.Dispose();
        Assert.That(instance.RenderedItems.Count, Is.EqualTo(0));
    }

    // ---- Phase 2-f-2: HasRenderedItems / IsRenderedItemsEmpty ----

    [Test, RequiresThread(ApartmentState.STA)]
    public void HasRenderedItems_初期はfalse_IsEmptyはtrue()
    {
        var vm = new PartInstanceViewModel();
        Assert.That(vm.HasRenderedItems.Value, Is.False);
        Assert.That(vm.IsRenderedItemsEmpty.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void HasRenderedItems_Initialize後はtrue_IsEmptyはfalse()
    {
        var (definition, _, _) = BuildDefinitionWithWidthBinding();
        var instance = new PartInstanceViewModel();

        instance.InitializeRenderedItems(definition);

        Assert.That(instance.HasRenderedItems.Value, Is.True);
        Assert.That(instance.IsRenderedItemsEmpty.Value, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void HasRenderedItems_空Definitionなら依然false()
    {
        var emptyDef = new PartDefinitionViewModel();
        var instance = new PartInstanceViewModel();

        instance.InitializeRenderedItems(emptyDef);

        Assert.That(instance.HasRenderedItems.Value, Is.False);
        Assert.That(instance.IsRenderedItemsEmpty.Value, Is.True);
    }

    // ---- Phase 2-f-3: 後付け Definition / デシリアライズ経路の再初期化 ----

    [Test, RequiresThread(ApartmentState.STA)]
    public void PartDefinition後付けで既存PartInstanceが再Initializeされる()
    {
        var diagram = new MainWindowViewModel(new Moq.Mock<Prism.Services.Dialogs.IDialogService>().Object).DiagramViewModel;

        // PartInstance を Layer に追加 (Definition なしの状態)
        var (definition, _, ep) = BuildDefinitionWithWidthBinding();
        var instance = new PartInstanceViewModel(definition.Id.Value) { Owner = diagram };
        instance.Left.Value = 10;
        instance.Top.Value = 20;
        instance.Width.Value = 200;
        instance.Height.Value = 100;
        diagram.AddItemCommand.Execute(instance);

        // 1) Definition 未登録なので RenderedItems は空
        Assert.That(instance.RenderedItems.Count, Is.EqualTo(0));

        // 2) PartDefinitions に Definition を後から追加
        diagram.PartDefinitions.Add(definition);

        // 3) PartInstance が自動的に Initialize される
        Assert.That(instance.RenderedItems.Count, Is.EqualTo(1));
        Assert.That(instance.HasRenderedItems.Value, Is.True);
        Assert.That(instance.TryGetParameterValue(ep.Id.Value, out _), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void ObjectDeserializer経路_Definition先読み済みなら即時Initialize()
    {
        var diagram = new MainWindowViewModel(new Moq.Mock<Prism.Services.Dialogs.IDialogService>().Object).DiagramViewModel;
        var (definition, _, _) = BuildDefinitionWithWidthBinding();
        diagram.PartDefinitions.Add(definition); // 先に登録

        var xml = new System.Xml.Linq.XElement("DesignerItem",
            new System.Xml.Linq.XElement("ID", Guid.NewGuid()),
            new System.Xml.Linq.XElement("ParentID", Guid.Empty),
            new System.Xml.Linq.XElement("Type", typeof(PartInstanceViewModel).FullName),
            new System.Xml.Linq.XElement("Left", 0),
            new System.Xml.Linq.XElement("Top", 0),
            new System.Xml.Linq.XElement("Width", 100),
            new System.Xml.Linq.XElement("Height", 100),
            new System.Xml.Linq.XElement("ZIndex", 0),
            new System.Xml.Linq.XElement("EdgeBrush", System.Xml.Linq.XElement.Parse(
                boilersGraphics.Helpers.WpfObjectSerializer.Serialize(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent)))),
            new System.Xml.Linq.XElement("FillBrush", System.Xml.Linq.XElement.Parse(
                boilersGraphics.Helpers.WpfObjectSerializer.Serialize(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent)))),
            new System.Xml.Linq.XElement("EdgeThickness", 0),
            new System.Xml.Linq.XElement("PathGeometryNoRotate", "M 0,0 L 100,0 L 100,100 L 0,100 Z"),
            new System.Xml.Linq.XElement("PathGeometryRotate", "M 0,0 L 100,0 L 100,100 L 0,100 Z"),
            new System.Xml.Linq.XElement("RotationAngle", 0),
            new System.Xml.Linq.XElement("StrokeLineJoin", "Miter"),
            new System.Xml.Linq.XElement("StrokeMiterLimit", 10),
            new System.Xml.Linq.XElement("StrokeDashArray", string.Empty),
            new System.Xml.Linq.XElement("DefinitionId", definition.Id.Value));

        var restored = (PartInstanceViewModel)boilersGraphics.Helpers.ObjectDeserializer.ExtractDesignerItemViewModelBase(diagram, xml);

        Assert.That(restored.DefinitionId.Value, Is.EqualTo(definition.Id.Value));
        Assert.That(restored.RenderedItems.Count, Is.EqualTo(1), "Definition 先読みなら即時 Initialize される");
    }
}
