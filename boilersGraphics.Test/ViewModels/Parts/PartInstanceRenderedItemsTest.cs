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
}
