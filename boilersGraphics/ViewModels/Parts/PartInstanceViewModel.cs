using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels.Parts;

public class PartInstanceViewModel : DesignerItemViewModelBase
{
    private readonly Dictionary<Guid, BindableReactiveProperty<object>> _parameterValues = new();

    public BindableReactiveProperty<Guid> DefinitionId { get; } = new();

    public ReadOnlyDictionary<Guid, BindableReactiveProperty<object>> ParameterValues { get; }

    public ReactiveCommand MouseDoubleClickCommand { get; } = new();

    public override bool IsResizable => false;

    public override bool SupportsPropertyDialog => false;

    public PartInstanceViewModel()
    {
        ParameterValues = new ReadOnlyDictionary<Guid, BindableReactiveProperty<object>>(_parameterValues);
        InitMouseDoubleClick();
    }

    public PartInstanceViewModel(Guid definitionId) : this()
    {
        DefinitionId.Value = definitionId;
    }

    public PartInstanceViewModel(int id, IDiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        ParameterValues = new ReadOnlyDictionary<Guid, BindableReactiveProperty<object>>(_parameterValues);
        InitMouseDoubleClick();
    }

    private void InitMouseDoubleClick()
    {
        MouseDoubleClickCommand
            .Subscribe(_ => RequestEditDefinition())
            .AddTo(_CompositeDisposable);
    }

    private void RequestEditDefinition()
    {
        if (Owner is not DiagramViewModel diagram) return;

        var definition = diagram.PartDefinitions
            .FirstOrDefault(d => d.Id.Value == DefinitionId.Value);
        if (definition is null) return;

        diagram.OpenPartEditor(definition);
    }

    public BindableReactiveProperty<object> GetOrCreateParameterValue(Guid exposedPropertyId, object defaultValue = null)
    {
        if (_parameterValues.TryGetValue(exposedPropertyId, out var rp))
            return rp;

        rp = new BindableReactiveProperty<object>(defaultValue);
        _parameterValues[exposedPropertyId] = rp;
        return rp;
    }

    public bool TryGetParameterValue(Guid exposedPropertyId, out BindableReactiveProperty<object> value)
        => _parameterValues.TryGetValue(exposedPropertyId, out value);

    public void RemoveParameterValue(Guid exposedPropertyId)
    {
        if (_parameterValues.TryGetValue(exposedPropertyId, out var rp))
        {
            rp.Dispose();
            _parameterValues.Remove(exposedPropertyId);
        }
    }

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        return GeometryCreator.CreateRectangle(this, 0, 0, flag);
    }

    public override Type GetViewType()
    {
        return typeof(Path);
    }

    public override void OpenPropertyDialog()
    {
    }

    public override object Clone()
    {
        var clone = new PartInstanceViewModel(DefinitionId.Value)
        {
            Owner = Owner
        };
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.RotationAngle.Value = RotationAngle.Value;

        foreach (var kv in _parameterValues)
            clone.GetOrCreateParameterValue(kv.Key, kv.Value.Value);

        return clone;
    }

    public override void Dispose()
    {
        foreach (var rp in _parameterValues.Values)
            rp.Dispose();
        _parameterValues.Clear();

        DefinitionId.Dispose();
        base.Dispose();
    }
}
