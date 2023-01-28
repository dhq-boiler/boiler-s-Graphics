using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// https://takap-tech.com/entry/2017/06/29/233511
/// <summary>
///     SelectedItem をバインド可能にする TreeView の拡張コントロールです。
/// </summary>
public class BindableSelectedItemTreeView : TreeView
{
    //
    // Bindable Definitions
    // - - - - - - - - - - - - - - - - - - - -

    public static readonly DependencyProperty BindableSelectedItemProperty = DependencyProperty.Register(
        nameof(BindableSelectedItem),
        typeof(object), typeof(BindableSelectedItemTreeView), new UIPropertyMetadata(null));

    //
    // Constructors
    // - - - - - - - - - - - - - - - - - - - -

    public BindableSelectedItemTreeView()
    {
        SelectedItemChanged += OnSelectedItemChanged;
    }


    //
    // Properties
    // - - - - - - - - - - - - - - - - - - - -

    /// <summary>
    ///     Bind 可能な SelectedItem を表し、SelectedItem を設定または取得します。
    /// </summary>
    public object BindableSelectedItem
    {
        get => (object)GetValue(BindableSelectedItemProperty);
        set => SetValue(BindableSelectedItemProperty, value);
    }

    //
    // Event Handlers
    // - - - - - - - - - - - - - - - - - - - -

    protected virtual void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (SelectedItem == null) return;

        SetValue(BindableSelectedItemProperty, SelectedItem);
    }
}