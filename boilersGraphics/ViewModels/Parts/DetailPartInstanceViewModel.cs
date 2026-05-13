using Prism.Regions;

namespace boilersGraphics.ViewModels.Parts;

public class DetailPartInstanceViewModel : DetailViewModelBase<PartInstanceViewModel>
{
    public DetailPartInstanceViewModel(IRegionManager regionManager) : base(regionManager)
    {
    }

    public override void SetProperties()
    {
        var instance = ViewModel.Value;
        if (instance is null) return;
        if (instance.Owner is not DiagramViewModel diagram) return;
        if (!diagram.TryGetPartDefinition(instance.DefinitionId.Value, out var definition)) return;

        foreach (var ep in definition.ExposedProperties)
        {
            var rp = instance.GetOrCreateParameterValue(ep.Id.Value, ep.DefaultValue.Value);
            Properties.Add(new ExposedParameterValuePropertyOption(
                ep.Name.Value,
                ep.Id.Value,
                ep.Type.Value,
                ep.IsArray.Value,
                rp));
        }
    }
}
