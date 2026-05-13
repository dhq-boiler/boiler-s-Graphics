using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Models.Parts;

[Serializable]
public class ExposedProperty : BindableBase
{
    private Guid _Id = Guid.NewGuid();
    private string _Name;
    private ExposedPropertyType _Type;
    private bool _IsArray;
    private object _DefaultValue;
    private double? _MinValue;
    private double? _MaxValue;
    private double? _Step;
    private List<Binding> _Bindings = new();

    public Guid Id
    {
        get => _Id;
        set => SetProperty(ref _Id, value);
    }

    public string Name
    {
        get => _Name;
        set => SetProperty(ref _Name, value);
    }

    public ExposedPropertyType Type
    {
        get => _Type;
        set => SetProperty(ref _Type, value);
    }

    public bool IsArray
    {
        get => _IsArray;
        set => SetProperty(ref _IsArray, value);
    }

    public object DefaultValue
    {
        get => _DefaultValue;
        set => SetProperty(ref _DefaultValue, value);
    }

    public double? MinValue
    {
        get => _MinValue;
        set => SetProperty(ref _MinValue, value);
    }

    public double? MaxValue
    {
        get => _MaxValue;
        set => SetProperty(ref _MaxValue, value);
    }

    public double? Step
    {
        get => _Step;
        set => SetProperty(ref _Step, value);
    }

    public List<Binding> Bindings
    {
        get => _Bindings;
        set => SetProperty(ref _Bindings, value);
    }
}
