using boilersGraphics.ViewModels;
using System;
using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Views.Triggers
{
    internal class WindowLoadedEventTrigger : EventTrigger
    {
        protected override void OnEvent(EventArgs eventArgs)
        {
            //((AssociatedObject as MainWindow).DataContext as MainWindowViewModel).Initialize();
        }
    }
}
