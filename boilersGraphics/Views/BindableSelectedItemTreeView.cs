using DependencyPropertyGenerator;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// https://takap-tech.com/entry/2017/06/29/233511
/// <summary>
///     SelectedItem をバインド可能にする TreeView の拡張コントロールです。
/// </summary>
[DependencyProperty<object>("BindableSelectedItem")]
public partial class BindableSelectedItemTreeView : TreeView
{
    public BindableSelectedItemTreeView()
    {
        SelectedItemChanged += OnSelectedItemChanged;
    }

    protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (SelectedItem == null) return;

        SetValue(BindableSelectedItemProperty, SelectedItem);
    }
}
