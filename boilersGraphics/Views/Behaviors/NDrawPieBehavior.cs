﻿using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Views.Behaviors;

public class NDrawPieBehavior : Behavior<DesignerCanvas>
{
    private Point? _rectangleStartPoint;
    private readonly SnapAction snapAction;

    public NDrawPieBehavior()
    {
        snapAction = new SnapAction();
    }

    protected override void OnAttached()
    {
        AssociatedObject.StylusDown += AssociatedObject_StylusDown;
        AssociatedObject.StylusMove += AssociatedObject_StylusMove;
        AssociatedObject.TouchDown += AssociatedObject_TouchDown;
        AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
        AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
        AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
        AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        base.OnDetaching();
    }

    private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            _rectangleStartPoint = e.GetPosition(AssociatedObject);
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            var touchPoint = e.GetTouchPoint(AssociatedObject);
            _rectangleStartPoint = touchPoint.Position;
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            if (e.Source == AssociatedObject)
            {
                _rectangleStartPoint = e.GetPosition(AssociatedObject);

                e.Handled = true;
            }
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.StylusDevice != null)
            return;

        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        snapAction.OnMouseMove(ref current);

        if (e.LeftButton != MouseButtonState.Pressed)
            _rectangleStartPoint = null;

        if (_rectangleStartPoint.HasValue)
        {
            _rectangleStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new PieAdorner(canvas, _rectangleStartPoint);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        snapAction.OnMouseMove(ref current);

        if (e.InAir)
            _rectangleStartPoint = null;

        if (_rectangleStartPoint.HasValue)
        {
            _rectangleStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new PieAdorner(canvas, _rectangleStartPoint);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }
    }
}