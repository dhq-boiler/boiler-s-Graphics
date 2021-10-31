using boilersGraphics.Controls;
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
        public SnapPointAdsorptionInformation(SnapPointEdge snapPointEdge, SelectableDesignerItemViewModelBase item)
        {
            SnapPointEdge = snapPointEdge;
            Item = item;
        }

        public SnapPointEdge SnapPointEdge { get; }
        public SelectableDesignerItemViewModelBase Item { get; }

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
            switch (SnapPointEdge)
            {
                case SnapPointEdge.BeginEdge:
                    Move(0, value);
                    break;
                case SnapPointEdge.EndEdge:
                    Move(1, value);
                    break;
            }
        }

        private void Move(int index, TransformNotification value)
        {
            switch (value.PropertyName)
            {
                case "Left":
                    (Item as ConnectorBaseViewModel).Points[index] = new Point((Item as ConnectorBaseViewModel).Points[index].X + (double)value.NewValue - (double)value.OldValue, (Item as ConnectorBaseViewModel).Points[index].Y);
                    if (index == 0)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint0VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint0VM.Value).First();
                        Canvas.SetLeft(fe, Canvas.GetLeft(fe) + (double)value.NewValue - (double)value.OldValue);
                    }
                    else if (index == 1)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint1VM.Value.Left.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint1VM.Value).First();
                        Canvas.SetLeft(fe, Canvas.GetLeft(fe) + (double)value.NewValue - (double)value.OldValue);
                    }
                    break;
                case "Top":
                    (Item as ConnectorBaseViewModel).Points[index] = new Point((Item as ConnectorBaseViewModel).Points[index].X, (Item as ConnectorBaseViewModel).Points[index].Y + (double)value.NewValue - (double)value.OldValue);
                    if (index == 0)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint0VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint0VM.Value).First();
                        Canvas.SetTop(fe, Canvas.GetTop(fe) + (double)value.NewValue - (double)value.OldValue);
                    }
                    else if (index == 1)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint1VM.Value.Top.Value += (double)value.NewValue - (double)value.OldValue;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint1VM.Value).First();
                        Canvas.SetTop(fe, Canvas.GetTop(fe) + (double)value.NewValue - (double)value.OldValue);
                    }
                    break;
                case "Width":
                    (Item as ConnectorBaseViewModel).Points[index] = new Point((Item as ConnectorBaseViewModel).Points[index].X + ((double)value.NewValue - (double)value.OldValue) / 2, (Item as ConnectorBaseViewModel).Points[index].Y);
                    if (index == 0)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint0VM.Value.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint0VM.Value).First();
                        Canvas.SetLeft(fe, Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                    }
                    else if (index == 1)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint1VM.Value.Left.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint1VM.Value).First();
                        Canvas.SetLeft(fe, Canvas.GetLeft(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                    }
                    break;
                case "Height":
                    (Item as ConnectorBaseViewModel).Points[index] = new Point((Item as ConnectorBaseViewModel).Points[index].X, (Item as ConnectorBaseViewModel).Points[index].Y + ((double)value.NewValue - (double)value.OldValue) / 2);
                    if (index == 0)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint0VM.Value.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint0VM.Value).First();
                        Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                    }
                    else if (index == 1)
                    {
                        (Item as ConnectorBaseViewModel).SnapPoint1VM.Value.Top.Value += ((double)value.NewValue - (double)value.OldValue) / 2;
                        var fe = App.Current.MainWindow.GetCorrespondingViews<LineResizeHandle>((Item as ConnectorBaseViewModel).SnapPoint1VM.Value).First();
                        Canvas.SetTop(fe, Canvas.GetTop(fe) + ((double)value.NewValue - (double)value.OldValue) / 2);
                    }
                    break;
            }
        }
    }
}
