using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Models.Parts;

[Serializable]
public class PartDefinition : BindableBase
{
    private Guid _Id = Guid.NewGuid();
    private string _Name;
    private List<RenderItem> _Items = new();
    private List<ExposedProperty> _ExposedProperties = new();

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

    public List<RenderItem> Items
    {
        get => _Items;
        set => SetProperty(ref _Items, value);
    }

    public List<ExposedProperty> ExposedProperties
    {
        get => _ExposedProperties;
        set => SetProperty(ref _ExposedProperties, value);
    }
}
