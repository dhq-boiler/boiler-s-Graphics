using Microsoft.Xaml.Behaviors;
using System;

namespace boilersGraphics.Views.Triggers;

internal class WindowLoadedEventTrigger : EventTrigger
{
    protected override void OnEvent(EventArgs eventArgs)
    {
        //((AssociatedObject as MainWindow).DataContext as MainWindowViewModel).Initialize();
    }
}