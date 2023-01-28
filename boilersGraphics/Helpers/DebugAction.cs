using System.Diagnostics;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Helpers;

internal class DebugAction : TriggerAction<DependencyObject>
{
    protected override void Invoke(object parameter)
    {
        Debug.WriteLine("DebugAction Invoked!!!");
    }
}