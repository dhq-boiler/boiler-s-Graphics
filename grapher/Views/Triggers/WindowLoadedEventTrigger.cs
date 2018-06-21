using grapher.ViewModels;
using System;
using System.Windows.Interactivity;

namespace grapher.Views.Triggers
{
    internal class WindowLoadedEventTrigger : EventTrigger
    {
        protected override void OnEvent(EventArgs eventArgs)
        {
            //((AssociatedObject as MainWindow).DataContext as MainWindowViewModel).Initialize();
        }
    }
}
