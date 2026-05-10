using System.Windows;
using DependencyPropertyGenerator;

namespace boilersGraphics.Helpers;

[DependencyProperty<object>("Data", DefaultValue = null)]
public partial class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}
