using Prism.Mvvm;
using System;

namespace boilersGraphics.Models.Parts;

[Serializable]
public class Binding : BindableBase
{
    private Guid _TargetItemId;
    private string _TargetProperty;

    public Guid TargetItemId
    {
        get => _TargetItemId;
        set => SetProperty(ref _TargetItemId, value);
    }

    public string TargetProperty
    {
        get => _TargetProperty;
        set => SetProperty(ref _TargetProperty, value);
    }
}
