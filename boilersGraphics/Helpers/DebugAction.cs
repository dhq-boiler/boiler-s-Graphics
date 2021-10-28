using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Helpers
{
    class DebugAction : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            Debug.WriteLine("DebugAction Invoked!!!");
        }
    }
}
