using boilersGraphics.ViewModels;
using NUnit.Framework;
using R3;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class DetailViewModelBaseReflectionTest
{
    // PropertyOptionsValueCombination は abstract なので、必要最小限の Object / PropertyValue だけ
    // 持つテスト用ダミー派生を定義し、internal な TryGetReactiveValue がリフレクションで
    // .Value を引き出せるかを確認する。
    private class MockRow : PropertyOptionsValueCombination
    {
        public MockRow(string name) : base(name) { }
        public BindableReactiveProperty<object> Object { get; } = new();
        public BindableReactiveProperty<object> PropertyValue { get; } = new();
    }

    [Test]
    public void TryGetReactiveValue_Object_は_Value_を取り出す()
    {
        var row = new MockRow("Left.Value");
        var marker = new object();
        row.Object.Value = marker;

        var result = DetailViewModelBase<global::boilersGraphics.ViewModels.SelectableDesignerItemViewModelBase>
            .TryGetReactiveValue(row, "Object");

        Assert.That(result, Is.SameAs(marker));
    }

    [Test]
    public void TryGetReactiveValue_PropertyValue_は_Value_を取り出す()
    {
        var row = new MockRow("Left.Value");
        row.PropertyValue.Value = 42.0;

        var result = DetailViewModelBase<global::boilersGraphics.ViewModels.SelectableDesignerItemViewModelBase>
            .TryGetReactiveValue(row, "PropertyValue");

        Assert.That(result, Is.EqualTo(42.0));
    }

    [Test]
    public void TryGetReactiveValue_存在しないプロパティ名_は_null()
    {
        var row = new MockRow("Left.Value");
        var result = DetailViewModelBase<global::boilersGraphics.ViewModels.SelectableDesignerItemViewModelBase>
            .TryGetReactiveValue(row, "NoSuchProperty");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryGetReactiveValue_row_null_は_null()
    {
        var result = DetailViewModelBase<global::boilersGraphics.ViewModels.SelectableDesignerItemViewModelBase>
            .TryGetReactiveValue(null, "Object");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryGetReactiveValue_holder_に_Value_プロパティ無し_は_holder自身を返す()
    {
        // ReactiveProperty ではない素の object プロパティを持つ MockRow2
        var row = new MockRow2("Foo");
        row.Plain = "hello"; // string は Value プロパティを持たないので、holder 自身が返る

        var result = DetailViewModelBase<global::boilersGraphics.ViewModels.SelectableDesignerItemViewModelBase>
            .TryGetReactiveValue(row, "Plain");

        Assert.That(result, Is.EqualTo("hello"));
    }

    private class MockRow2 : PropertyOptionsValueCombination
    {
        public MockRow2(string name) : base(name) { }
        public string Plain { get; set; }
    }
}
