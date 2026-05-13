using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using R3;
using System;
using System.Windows;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class ExposedParameterValuePropertyOptionTest
{
    private static ExposedParameterValuePropertyOption Make(
        ExposedPropertyType type,
        bool isArray = false,
        object initialValue = null)
    {
        return new ExposedParameterValuePropertyOption(
            "x",
            Guid.NewGuid(),
            type,
            isArray,
            new BindableReactiveProperty<object>(initialValue));
    }

    [Test]
    public void コンストラクタでPropertyValueがnullなら例外()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ExposedParameterValuePropertyOption(
                "x", Guid.NewGuid(), ExposedPropertyType.Double, false, null));
    }

    [Test]
    public void Type_Boolean_非Array_はCheckBox()
    {
        Assert.That(Make(ExposedPropertyType.Boolean).Type, Is.EqualTo("CheckBox"));
    }

    [Test]
    public void Type_Double_非Array_はTextBox()
    {
        Assert.That(Make(ExposedPropertyType.Double).Type, Is.EqualTo("TextBox"));
    }

    [Test]
    public void Type_Int_非Array_はTextBox()
    {
        Assert.That(Make(ExposedPropertyType.Int).Type, Is.EqualTo("TextBox"));
    }

    [Test]
    public void Type_String_非Array_はTextBox()
    {
        Assert.That(Make(ExposedPropertyType.String).Type, Is.EqualTo("TextBox"));
    }

    [Test]
    public void Type_Point_非Array_は読み取り専用TextBox()
    {
        Assert.That(Make(ExposedPropertyType.Point).Type, Is.EqualTo("ReadOnlyTextBox"));
    }

    [Test]
    public void Type_Color_非Array_は読み取り専用TextBox()
    {
        Assert.That(Make(ExposedPropertyType.Color).Type, Is.EqualTo("ReadOnlyTextBox"));
    }

    [Test]
    public void Type_Brush_非Array_は読み取り専用TextBox()
    {
        Assert.That(Make(ExposedPropertyType.Brush).Type, Is.EqualTo("ReadOnlyTextBox"));
    }

    [Test]
    public void Type_Enum_非Array_は読み取り専用TextBox()
    {
        Assert.That(Make(ExposedPropertyType.Enum).Type, Is.EqualTo("ReadOnlyTextBox"));
    }

    [Test]
    public void Type_Array_はAnyTypeでも読み取り専用TextBox()
    {
        Assert.That(Make(ExposedPropertyType.Double, isArray: true).Type, Is.EqualTo("ReadOnlyTextBox"));
        Assert.That(Make(ExposedPropertyType.Boolean, isArray: true).Type, Is.EqualTo("ReadOnlyTextBox"));
        Assert.That(Make(ExposedPropertyType.String, isArray: true).Type, Is.EqualTo("ReadOnlyTextBox"));
    }

    [Test]
    public void HorizontalContentAlignment_DoubleはRight()
    {
        Assert.That(Make(ExposedPropertyType.Double).HorizontalContentAlignment.Value,
            Is.EqualTo(HorizontalAlignment.Right));
    }

    [Test]
    public void HorizontalContentAlignment_IntはRight()
    {
        Assert.That(Make(ExposedPropertyType.Int).HorizontalContentAlignment.Value,
            Is.EqualTo(HorizontalAlignment.Right));
    }

    [Test]
    public void HorizontalContentAlignment_StringはLeft()
    {
        Assert.That(Make(ExposedPropertyType.String).HorizontalContentAlignment.Value,
            Is.EqualTo(HorizontalAlignment.Left));
    }

    [Test]
    public void PropertyName_コンストラクタで設定された名前を返す()
    {
        var rp = new BindableReactiveProperty<object>(0d);
        var opt = new ExposedParameterValuePropertyOption(
            "半径", Guid.NewGuid(), ExposedPropertyType.Double, false, rp);

        Assert.That(opt.PropertyName.Value, Is.EqualTo("半径"));
    }

    [Test]
    public void PropertyValue_注入したRPと同じインスタンスを返す()
    {
        var rp = new BindableReactiveProperty<object>(3.14);
        var opt = new ExposedParameterValuePropertyOption(
            "x", Guid.NewGuid(), ExposedPropertyType.Double, false, rp);

        Assert.That(opt.PropertyValue, Is.SameAs(rp));
        Assert.That(opt.PropertyValue.Value, Is.EqualTo(3.14));
    }

    [Test]
    public void PropertyValue_TwoWay経路の代入で元のRPに反映される()
    {
        var rp = new BindableReactiveProperty<object>(0d);
        var opt = new ExposedParameterValuePropertyOption(
            "x", Guid.NewGuid(), ExposedPropertyType.Double, false, rp);

        opt.PropertyValue.Value = 42d;

        Assert.That(rp.Value, Is.EqualTo(42d));
    }

    [Test]
    public void ExposedPropertyId_注入したIdを保持する()
    {
        var id = Guid.NewGuid();
        var rp = new BindableReactiveProperty<object>(0d);
        var opt = new ExposedParameterValuePropertyOption(
            "x", id, ExposedPropertyType.Double, false, rp);

        Assert.That(opt.ExposedPropertyId, Is.EqualTo(id));
    }
}
