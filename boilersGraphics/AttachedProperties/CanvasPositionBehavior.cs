using boilersGraphics.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.AttachedProperties;

/// <summary>
/// BindableReactiveProperty の PropertyChanged を直接購読して
/// Canvas.Left/Top を更新する Attached Behavior。
/// WPF の Style Setter バインディングが BindableReactiveProperty.Value の
/// 変更通知に追従しない問題を回避する。
/// </summary>
public static class CanvasPositionBehavior
{
    public static readonly DependencyProperty TrackPositionProperty =
        DependencyProperty.RegisterAttached(
            "TrackPosition",
            typeof(bool),
            typeof(CanvasPositionBehavior),
            new PropertyMetadata(false, OnTrackPositionChanged));

    public static bool GetTrackPosition(DependencyObject obj) => (bool)obj.GetValue(TrackPositionProperty);
    public static void SetTrackPosition(DependencyObject obj, bool value) => obj.SetValue(TrackPositionProperty, value);

    private static void OnTrackPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element && (bool)e.NewValue)
        {
            element.DataContextChanged += OnDataContextChanged;
            if (element.DataContext is DesignerItemViewModelBase vm)
            {
                Subscribe(element, vm);
            }
        }
    }

    private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;

        if (e.OldValue is DesignerItemViewModelBase oldVm)
        {
            Unsubscribe(element, oldVm);
        }

        if (e.NewValue is DesignerItemViewModelBase newVm)
        {
            Subscribe(element, newVm);
        }
    }

    private static void Subscribe(FrameworkElement element, DesignerItemViewModelBase vm)
    {
        // Set initial values
        Canvas.SetLeft(element, vm.Left.Value);
        Canvas.SetTop(element, vm.Top.Value);

        // Subscribe to changes
        vm.Left.PropertyChanged += CreateHandler(element, vm, isLeft: true);
        vm.Top.PropertyChanged += CreateHandler(element, vm, isLeft: false);
    }

    private static void Unsubscribe(FrameworkElement element, DesignerItemViewModelBase vm)
    {
        // Remove handlers by clearing the stored delegates
        var leftHandler = GetLeftHandler(element);
        var topHandler = GetTopHandler(element);

        if (leftHandler != null) vm.Left.PropertyChanged -= leftHandler;
        if (topHandler != null) vm.Top.PropertyChanged -= topHandler;

        SetLeftHandler(element, null);
        SetTopHandler(element, null);
    }

    private static PropertyChangedEventHandler CreateHandler(FrameworkElement element, DesignerItemViewModelBase vm, bool isLeft)
    {
        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == "Value")
            {
                if (isLeft)
                    Canvas.SetLeft(element, vm.Left.Value);
                else
                    Canvas.SetTop(element, vm.Top.Value);
            }
        };

        if (isLeft)
            SetLeftHandler(element, handler);
        else
            SetTopHandler(element, handler);

        return handler;
    }

    /// <summary>
    /// ViewModel から対応する ContentPresenter を見つけて Canvas.Left/Top を直接更新する。
    /// DragThumb から呼ばれ、PropertyChanged を経由しない即時更新を提供する。
    /// </summary>
    public static void UpdateCanvasPosition(DesignerItemViewModelBase vm, double left, double top)
    {
        var canvas = boilersGraphics.Controls.DesignerCanvas.GetInstance();
        if (canvas == null) return;

        var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(canvas);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(canvas, i) as FrameworkElement;
            if (child != null && child.DataContext == vm)
            {
                Canvas.SetLeft(child, left);
                Canvas.SetTop(child, top);
                break;
            }
        }
    }

    // Store event handlers for cleanup
    private static readonly DependencyProperty LeftHandlerProperty =
        DependencyProperty.RegisterAttached("LeftHandler", typeof(PropertyChangedEventHandler),
            typeof(CanvasPositionBehavior), new PropertyMetadata(null));

    private static readonly DependencyProperty TopHandlerProperty =
        DependencyProperty.RegisterAttached("TopHandler", typeof(PropertyChangedEventHandler),
            typeof(CanvasPositionBehavior), new PropertyMetadata(null));

    private static PropertyChangedEventHandler GetLeftHandler(DependencyObject obj) =>
        (PropertyChangedEventHandler)obj.GetValue(LeftHandlerProperty);

    private static void SetLeftHandler(DependencyObject obj, PropertyChangedEventHandler value) =>
        obj.SetValue(LeftHandlerProperty, value);

    private static PropertyChangedEventHandler GetTopHandler(DependencyObject obj) =>
        (PropertyChangedEventHandler)obj.GetValue(TopHandlerProperty);

    private static void SetTopHandler(DependencyObject obj, PropertyChangedEventHandler value) =>
        obj.SetValue(TopHandlerProperty, value);
}
