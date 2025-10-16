using boilersGraphics.Controls;
using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using NLog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.ViewModels;

internal class SnapPointAdsorptionInformation : IObserver<TransformNotification>
{
    public SnapPointAdsorptionInformation(SnapPointPosition snapPointEdgeTo, SnapPointPosition snapPointEdgeFrom,
        SelectableDesignerItemViewModelBase item)
    {
        SnapPointPositionTo = snapPointEdgeTo;
        SnapPointPositionFrom = snapPointEdgeFrom;
        Item = item;
    }

    public SelectableDesignerItemViewModelBase Item { get; }
    public SnapPointPosition SnapPointPositionTo { get; }
    public SnapPointPosition SnapPointPositionFrom { get; }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(TransformNotification value)
    {
        var designerItem = Item as DesignerItemViewModelBase;
        if (designerItem != null && value.NewValue != null && value.OldValue != null)
        {
            LogManager.GetCurrentClassLogger().Debug($"{SnapPointPositionFrom} {value.PropertyName}");
            LogManager.GetCurrentClassLogger()
                .Debug(
                    $"Left={designerItem.Left.Value} Top={designerItem.Top.Value} Width={designerItem.Width.Value} Height={designerItem.Height.Value}");
            LogManager.GetCurrentClassLogger().Debug($"{(double)value.NewValue - (double)value.OldValue}");
        }

        switch (SnapPointPositionFrom)
        {
            case SnapPointPosition.LeftTop:
                switch (value.PropertyName)
                {
                    case "Left":
                        designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Top":
                        designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Width":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.Left:
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.LeftBottom:
                                designerItem.Left.Value += 0;
                                break;
                            case SnapPointPosition.Top:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Bottom:
                                designerItem.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.Right:
                            case SnapPointPosition.RightTop:
                            case SnapPointPosition.RightBottom:
                                designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                    case "Height":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.Top:
                            case SnapPointPosition.RightTop:
                                designerItem.Top.Value += 0;
                                break;
                            case SnapPointPosition.Left:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Right:
                                designerItem.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.LeftBottom:
                            case SnapPointPosition.Bottom:
                            case SnapPointPosition.RightBottom:
                                designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                }

                break;
            case SnapPointPosition.RightTop:
                switch (value.PropertyName)
                {
                    case "Left":
                        designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Top":
                        designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Width":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.Left:
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.LeftBottom:
                                designerItem.Left.Value += 0;
                                break;
                            case SnapPointPosition.Top:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Bottom:
                                designerItem.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.Right:
                            case SnapPointPosition.RightTop:
                            case SnapPointPosition.RightBottom:
                                designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                    case "Height":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.Top:
                            case SnapPointPosition.RightTop:
                                designerItem.Top.Value += 0;
                                break;
                            case SnapPointPosition.Left:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Right:
                                designerItem.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.LeftBottom:
                            case SnapPointPosition.Bottom:
                            case SnapPointPosition.RightBottom:
                                designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                }

                break;
            case SnapPointPosition.RightBottom:
                switch (value.PropertyName)
                {
                    case "Left":
                        designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Top":
                        designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Width":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.Left:
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.LeftBottom:
                                designerItem.Left.Value += 0;
                                break;
                            case SnapPointPosition.Top:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Bottom:
                                designerItem.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.Right:
                            case SnapPointPosition.RightTop:
                            case SnapPointPosition.RightBottom:
                                designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                    case "Height":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.Top:
                            case SnapPointPosition.RightTop:
                                designerItem.Top.Value += 0;
                                break;
                            case SnapPointPosition.Left:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Right:
                                designerItem.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.LeftBottom:
                            case SnapPointPosition.Bottom:
                            case SnapPointPosition.RightBottom:
                                designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                }

                break;
            case SnapPointPosition.LeftBottom:
                switch (value.PropertyName)
                {
                    case "Left":
                        designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Top":
                        designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        break;
                    case "Width":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.Left:
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.LeftBottom:
                                designerItem.Left.Value += 0;
                                break;
                            case SnapPointPosition.Top:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Bottom:
                                designerItem.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.Right:
                            case SnapPointPosition.RightTop:
                            case SnapPointPosition.RightBottom:
                                designerItem.Left.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                    case "Height":
                        switch (SnapPointPositionTo)
                        {
                            case SnapPointPosition.LeftTop:
                            case SnapPointPosition.Top:
                            case SnapPointPosition.RightTop:
                                designerItem.Top.Value += 0;
                                break;
                            case SnapPointPosition.Left:
                            case SnapPointPosition.Center:
                            case SnapPointPosition.Right:
                                designerItem.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                break;
                            case SnapPointPosition.LeftBottom:
                            case SnapPointPosition.Bottom:
                            case SnapPointPosition.RightBottom:
                                designerItem.Top.Value += (double)value.NewValue - (double)value.OldValue;
                                break;
                        }

                        break;
                }

                break;
            case SnapPointPosition.BeginEdge:
                Move(0, value);
                break;
            case SnapPointPosition.EndEdge:
                Move(1, value);
                break;
            default:
                throw new UnexpectedException("zzz");
        }
    }

    private void Move(int index, TransformNotification value)
    {
        var connector = Item as ConnectorBaseViewModel;
        switch (value.PropertyName)
        {
            case "Left":
                connector.Points[index] =
                    new Point(connector.Points[index].X + (double)value.NewValue - (double)value.OldValue,
                        connector.Points[index].Y);
                if (index == 0)
                {
                    connector.SnapPoint0VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                    var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                    Canvas.SetLeft(fe, Canvas.GetLeft(fe) + (double)value.NewValue - (double)value.OldValue);
                }
                else if (index == 1)
                {
                    connector.SnapPoint1VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                    var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                    Canvas.SetLeft(fe, Canvas.GetLeft(fe) + (double)value.NewValue - (double)value.OldValue);
                }

                break;
            case "Top":
                connector.Points[index] = new Point(connector.Points[index].X,
                    connector.Points[index].Y + (double)value.NewValue - (double)value.OldValue);
                if (index == 0)
                {
                    connector.SnapPoint0VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                    var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                    Canvas.SetTop(fe, Canvas.GetTop(fe) + (double)value.NewValue - (double)value.OldValue);
                }
                else if (index == 1)
                {
                    connector.SnapPoint1VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                    var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                    Canvas.SetTop(fe, Canvas.GetTop(fe) + (double)value.NewValue - (double)value.OldValue);
                }

                break;
            case "Width":
                switch (SnapPointPositionTo)
                {
                    case SnapPointPosition.Left:
                    case SnapPointPosition.LeftTop:
                    case SnapPointPosition.LeftBottom:
                        connector.Points[index] = new Point(connector.Points[index].X, connector.Points[index].Y);
                        if (index == 0)
                        {
                            connector.SnapPoint0VM.Value.Left.Value += 0;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                            Canvas.SetLeft(fe, Canvas.GetLeft(fe) + 0);
                        }
                        else if (index == 1)
                        {
                            connector.SnapPoint1VM.Value.Left.Value += 0;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                            Canvas.SetLeft(fe, Canvas.GetLeft(fe) + 0);
                        }

                        break;
                    case SnapPointPosition.Top:
                    case SnapPointPosition.Center:
                    case SnapPointPosition.Bottom:
                        connector.Points[index] =
                            new Point(connector.Points[index].X + ((double)value.NewValue - (double)value.OldValue) / 2,
                                connector.Points[index].Y);
                        if (index == 0)
                        {
                            connector.SnapPoint0VM.Value.Left.Value +=
                                ((double)value.NewValue - (double)value.OldValue) / 2;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                            Canvas.SetLeft(fe,
                                Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                        }
                        else if (index == 1)
                        {
                            connector.SnapPoint1VM.Value.Left.Value +=
                                ((double)value.NewValue - (double)value.OldValue) / 2;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                            Canvas.SetLeft(fe,
                                Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                        }

                        break;
                    case SnapPointPosition.RightTop:
                    case SnapPointPosition.Right:
                    case SnapPointPosition.RightBottom:
                        connector.Points[index] =
                            new Point(connector.Points[index].X + ((double)value.NewValue - (double)value.OldValue),
                                connector.Points[index].Y);
                        if (index == 0)
                        {
                            connector.SnapPoint0VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                            Canvas.SetLeft(fe, Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue));
                        }
                        else if (index == 1)
                        {
                            connector.SnapPoint1VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                            Canvas.SetLeft(fe, Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue));
                        }

                        break;
                }

                break;
            case "Height":
                switch (SnapPointPositionTo)
                {
                    case SnapPointPosition.LeftTop:
                    case SnapPointPosition.Top:
                    case SnapPointPosition.RightTop:
                        connector.Points[index] = new Point(connector.Points[index].X, connector.Points[index].Y + 0);
                        if (index == 0)
                        {
                            connector.SnapPoint0VM.Value.Top.Value += 0;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                            Canvas.SetTop(fe, Canvas.GetTop(fe) + 0);
                        }
                        else if (index == 1)
                        {
                            connector.SnapPoint1VM.Value.Top.Value += 0;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                            Canvas.SetTop(fe, Canvas.GetTop(fe) + 0);
                        }

                        break;
                    case SnapPointPosition.Left:
                    case SnapPointPosition.Center:
                    case SnapPointPosition.Right:
                        connector.Points[index] = new Point(connector.Points[index].X,
                            connector.Points[index].Y + ((double)value.NewValue - (double)value.OldValue) / 2);
                        if (index == 0)
                        {
                            connector.SnapPoint0VM.Value.Top.Value +=
                                ((double)value.NewValue - (double)value.OldValue) / 2;
                            var fe = Application.Current.MainWindow.GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                            Canvas.SetTop(fe,
                                Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                        }
                        else if (index == 1)
                        {
                            connector.SnapPoint1VM.Value.Top.Value +=
                                ((double)value.NewValue - (double)value.OldValue) / 2;
                            var fe = Application.Current.MainWindow
                                .GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                            Canvas.SetTop(fe,
                                Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                        }

                        break;
                    case SnapPointPosition.LeftBottom:
                    case SnapPointPosition.Bottom:
                    case SnapPointPosition.RightBottom:
                        connector.Points[index] = new Point(connector.Points[index].X,
                            connector.Points[index].Y + ((double)value.NewValue - (double)value.OldValue));
                        if (index == 0)
                        {
                            connector.SnapPoint0VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                            var fe = Application.Current.MainWindow
                                .GetVisualChild<LineResizeHandle>(connector.SnapPoint0VM.Value);
                            Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue));
                        }
                        else if (index == 1)
                        {
                            connector.SnapPoint1VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                            var fe = Application.Current.MainWindow
                                .GetVisualChild<LineResizeHandle>(connector.SnapPoint1VM.Value);
                            Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue));
                        }

                        break;
                }

                break;
        }
    }
}