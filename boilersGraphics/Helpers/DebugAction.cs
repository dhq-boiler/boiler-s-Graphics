using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.Windows;

namespace boilersGraphics.Helpers;

internal class DebugAction : TriggerAction<DependencyObject>
{
    protected override void Invoke(object parameter)
    {
        Debug.WriteLine("DebugAction Invoked!!!");
    }
}