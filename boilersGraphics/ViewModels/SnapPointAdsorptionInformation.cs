﻿using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.ViewModels
{
    class SnapPointAdsorptionInformation : IObserver<TransformNotification>
    {
        public SnapPointAdsorptionInformation(SnapPointPosition snapPointEdgeTo, SnapPointPosition snapPointEdgeFrom, SelectableDesignerItemViewModelBase item)
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
            switch (SnapPointPositionFrom)
            {
                case SnapPointPosition.BeginEdge:
                    Move(0, value);
                    break;
                case SnapPointPosition.EndEdge:
                    Move(1, value);
                    break;
            }
        }

        private void Move(int index, TransformNotification value)
        {
            var connector = Item as ConnectorBaseViewModel;
            switch (value.PropertyName)
            {
                case "Left":
                    connector.Points[index] = new Point(connector.Points[index].X + (double)value.NewValue - (double)value.OldValue, connector.Points[index].Y);
                    if (index == 0)
                    {
                        connector.SnapPoint0VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                        Canvas.SetLeft(fe, Canvas.GetLeft(fe) + (double)value.NewValue - (double)value.OldValue);
                    }
                    else if (index == 1)
                    {
                        connector.SnapPoint1VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
                        Canvas.SetLeft(fe, Canvas.GetLeft(fe) + (double)value.NewValue - (double)value.OldValue);
                    }
                    break;
                case "Top":
                    connector.Points[index] = new Point(connector.Points[index].X, connector.Points[index].Y + (double)value.NewValue - (double)value.OldValue);
                    if (index == 0)
                    {
                        connector.SnapPoint0VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                        Canvas.SetTop(fe, Canvas.GetTop(fe) + (double)value.NewValue - (double)value.OldValue);
                    }
                    else if (index == 1)
                    {
                        connector.SnapPoint1VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
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
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                                Canvas.SetLeft(fe, Canvas.GetLeft(fe) + 0);
                            }
                            else if (index == 1)
                            {
                                connector.SnapPoint1VM.Value.Left.Value += 0;
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
                                Canvas.SetLeft(fe, Canvas.GetLeft(fe) + 0);
                            }
                            break;
                        case SnapPointPosition.Top:
                        case SnapPointPosition.Center:
                        case SnapPointPosition.Bottom:
                            connector.Points[index] = new Point(connector.Points[index].X + ((double)value.NewValue - (double)value.OldValue) / 2, connector.Points[index].Y);
                            if (index == 0)
                            {
                                connector.SnapPoint0VM.Value.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                                Canvas.SetLeft(fe, Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                            }
                            else if (index == 1)
                            {
                                connector.SnapPoint1VM.Value.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
                                Canvas.SetLeft(fe, Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                            }
                            break;
                        case SnapPointPosition.RightTop:
                        case SnapPointPosition.Right:
                        case SnapPointPosition.RightBottom:
                            connector.Points[index] = new Point(connector.Points[index].X + ((double)value.NewValue - (double)value.OldValue), connector.Points[index].Y);
                            if (index == 0)
                            {
                                connector.SnapPoint0VM.Value.Left.Value += ((double)value.NewValue - (double)value.OldValue);
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                                Canvas.SetLeft(fe, Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue));
                            }
                            else if (index == 1)
                            {
                                connector.SnapPoint1VM.Value.Left.Value += ((double)value.NewValue - (double)value.OldValue);
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
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
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                                Canvas.SetTop(fe, Canvas.GetTop(fe) + 0);
                            }
                            else if (index == 1)
                            {
                                connector.SnapPoint1VM.Value.Top.Value += 0;
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
                                Canvas.SetTop(fe, Canvas.GetTop(fe) + 0);
                            }
                            break;
                        case SnapPointPosition.Left:
                        case SnapPointPosition.Center:
                        case SnapPointPosition.Right:
                            connector.Points[index] = new Point(connector.Points[index].X, connector.Points[index].Y + ((double)value.NewValue - (double)value.OldValue) / 2);
                            if (index == 0)
                            {
                                connector.SnapPoint0VM.Value.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                                Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                            }
                            else if (index == 1)
                            {
                                connector.SnapPoint1VM.Value.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
                                Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                            }
                            break;
                        case SnapPointPosition.LeftBottom:
                        case SnapPointPosition.Bottom:
                        case SnapPointPosition.RightBottom:
                            connector.Points[index] = new Point(connector.Points[index].X, connector.Points[index].Y + ((double)value.NewValue - (double)value.OldValue));
                            if (index == 0)
                            {
                                connector.SnapPoint0VM.Value.Top.Value += ((double)value.NewValue - (double)value.OldValue);
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint0VM.Value).First();
                                Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue));
                            }
                            else if (index == 1)
                            {
                                connector.SnapPoint1VM.Value.Top.Value += ((double)value.NewValue - (double)value.OldValue);
                                var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>(connector.SnapPoint1VM.Value).First();
                                Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue));
                            }
                            break;
                    }
                    
                    break;
            }
        }
    }
}
