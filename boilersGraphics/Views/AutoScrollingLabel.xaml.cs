﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
///     Interaction logic for AutoScrollingLabel.xaml
/// </summary>
public partial class AutoScrollingLabel : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
        typeof(string),
        typeof(AutoScrollingLabel),
        new FrameworkPropertyMetadata(null, OnTextChanged));

    private DateTime _beginAutoScroll;
    private DateTime _reachedDefaultPosition;

    private readonly BackgroundWorker _worker = new();

    public AutoScrollingLabel()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as AutoScrollingLabel;
        if (ctrl != null) ctrl.Control_Label.Content = ctrl.Text;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        var labelWidth = Control_Label.ActualWidth;
        if (labelWidth <= ActualWidth)
        {
            var space = ActualWidth - labelWidth;
            Dispatcher.Invoke(() => { Control_Label.SetValue(Canvas.LeftProperty, space / 2d); });
        }
    }

    public void EnableAutoScroll()
    {
        if (Control_Label.ActualWidth <= ActualWidth || _worker.IsBusy) return;

        _worker.DoWork += Worker_DoWork;
        _worker.WorkerSupportsCancellation = true;
        _worker.RunWorkerAsync();
        _beginAutoScroll = DateTime.Now;
    }

    public void DisableAutoScroll()
    {
        if (_worker.WorkerSupportsCancellation)
        {
            _worker.CancelAsync();
            _worker.DoWork -= Worker_DoWork;
        }
    }

    private void Worker_DoWork(object sender, DoWorkEventArgs e)
    {
        while (!_worker.CancellationPending)
        {
            if ((DateTime.Now - _beginAutoScroll).TotalMilliseconds < 1000)
            {
                Thread.Sleep(10);
                continue;
            }

            Dispatcher.Invoke(() =>
            {
                if ((DateTime.Now - _reachedDefaultPosition).TotalMilliseconds < 2000) return;

                var labelWidth = Control_Label.ActualWidth;
                var left = (double)Control_Label.GetValue(Canvas.LeftProperty);
                if (left < -labelWidth) left = ActualWidth + 0.5;

                Control_Label.SetValue(Canvas.LeftProperty, left - 0.5);

                if (left - 0.5 == 0.0) _reachedDefaultPosition = DateTime.Now;
            });
            Thread.Sleep(10);
        }

        Dispatcher.Invoke(() => { Control_Label.SetValue(Canvas.LeftProperty, 0d); });
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        DisableAutoScroll();
    }
}