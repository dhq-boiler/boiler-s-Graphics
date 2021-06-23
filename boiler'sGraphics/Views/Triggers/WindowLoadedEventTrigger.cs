using boiler_sGraphics.ViewModels;
using System;
using Microsoft.Xaml.Behaviors;

namespace boiler_sGraphics.Views.Triggers
{
    internal class WindowLoadedEventTrigger : EventTrigger
    {
        protected override void OnEvent(EventArgs eventArgs)
        {
            //((AssociatedObject as MainWindow).DataContext as MainWindowViewModel).Initialize();
        }
    }
}
