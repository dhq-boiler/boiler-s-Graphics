using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace boilersGraphics.Helpers.Parts;

internal static class PartOperations
{
    public static PromoteResult Promote(
        IReadOnlyList<DesignerItemViewModelBase> items,
        string name)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        if (items.Count == 0)
            throw new InvalidOperationException("Cannot promote an empty selection.");

        var bounds = ComputeBounds(items);

        var definition = new PartDefinitionViewModel();
        definition.Name.Value = name;

        // Clone every item into Definition.Items. We must NOT take the
        // selected items by reference here: the caller will remove (and
        // Dispose) the originals from the layer after Promote returns,
        // which would otherwise leave Disposed instances inside
        // Definition.Items and make subsequent edits in PartEditor throw
        // ObjectDisposedException at the first ReactiveProperty access.
        // Detach / Clone follow the same defensive-clone pattern.
        //
        // BUG FIX: clones must be re-anchored to the selection bounds
        // (subtract bounds.X / bounds.Y). PartInstanceDesignerItemData
        // Template binds Canvas.Left/Top straight to item.Left/Top, and
        // the Canvas itself is positioned at PartInstance.Left/Top.
        // Without this normalization the original absolute coords add
        // up with PartInstance.Left/Top and the part appears shifted by
        // (bounds.X, bounds.Y) after Promote.
        foreach (var item in items)
        {
            if (item.Clone() is DesignerItemViewModelBase clone)
            {
                clone.Left.Value -= bounds.X;
                clone.Top.Value -= bounds.Y;
                definition.Items.Add(clone);
            }
        }

        var instance = new PartInstanceViewModel(definition.Id.Value)
        {
            Owner = items[0].Owner
        };
        instance.Left.Value = bounds.X;
        instance.Top.Value = bounds.Y;
        instance.Width.Value = bounds.Width;
        instance.Height.Value = bounds.Height;

        foreach (var exposed in definition.ExposedProperties)
            instance.GetOrCreateParameterValue(exposed.Id.Value, exposed.DefaultValue.Value);

        return new PromoteResult(definition, instance);
    }

    public static IReadOnlyList<DesignerItemViewModelBase> Detach(
        PartInstanceViewModel instance,
        PartDefinitionViewModel definition)
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));
        if (definition is null) throw new ArgumentNullException(nameof(definition));
        if (instance.DefinitionId.Value != definition.Id.Value)
            throw new InvalidOperationException(
                "PartInstance.DefinitionId does not match the supplied definition.");

        // Inverse of the Promote normalization: Definition.Items hold
        // bounds-relative Left/Top, so when Detaching back into the
        // layer we need to re-apply instance.Left/Top to land each
        // child at its original world position.
        var detached = new List<DesignerItemViewModelBase>(definition.Items.Count);
        var offsetX = instance.Left.Value;
        var offsetY = instance.Top.Value;
        foreach (var item in definition.Items)
        {
            if (item.Clone() is DesignerItemViewModelBase clone)
            {
                clone.Left.Value += offsetX;
                clone.Top.Value += offsetY;
                clone.Owner = instance.Owner;
                detached.Add(clone);
            }
        }
        return detached;
    }

    public static PartDefinitionViewModel Clone(
        PartDefinitionViewModel original,
        string newName)
    {
        if (original is null) throw new ArgumentNullException(nameof(original));

        var clone = new PartDefinitionViewModel();
        clone.Name.Value = newName;

        var itemIdMap = new Dictionary<Guid, Guid>();
        foreach (var item in original.Items)
        {
            if (item.Clone() is DesignerItemViewModelBase cloneItem)
            {
                itemIdMap[item.ID] = cloneItem.ID;
                clone.Items.Add(cloneItem);
            }
        }

        foreach (var exposed in original.ExposedProperties)
        {
            var exposedClone = new ExposedPropertyViewModel(new ExposedProperty
            {
                Name = exposed.Name.Value,
                Type = exposed.Type.Value,
                IsArray = exposed.IsArray.Value,
                DefaultValue = exposed.DefaultValue.Value,
                MinValue = exposed.MinValue.Value,
                MaxValue = exposed.MaxValue.Value,
                Step = exposed.Step.Value,
            });

            foreach (var binding in exposed.Bindings)
            {
                var mappedTarget = itemIdMap.TryGetValue(binding.TargetItemId.Value, out var mapped)
                    ? mapped
                    : binding.TargetItemId.Value;
                exposedClone.Bindings.Add(new BindingViewModel(new Binding
                {
                    TargetItemId = mappedTarget,
                    TargetProperty = binding.TargetProperty.Value,
                }));
            }

            clone.ExposedProperties.Add(exposedClone);
        }

        return clone;
    }

    public readonly record struct PromoteResult(
        PartDefinitionViewModel Definition,
        PartInstanceViewModel Instance);

    public const int MaxNestingDepth = 32;

    public static bool WouldCreateCycle(
        Guid hostDefinitionId,
        Guid newChildDefinitionId,
        IReadOnlyDictionary<Guid, PartDefinitionViewModel> registry)
    {
        if (registry is null) throw new ArgumentNullException(nameof(registry));
        if (hostDefinitionId == newChildDefinitionId) return true;
        var visited = new HashSet<Guid>();
        return DependsOn(newChildDefinitionId, hostDefinitionId, registry, visited, 0);
    }

    private static bool DependsOn(
        Guid current,
        Guid target,
        IReadOnlyDictionary<Guid, PartDefinitionViewModel> registry,
        HashSet<Guid> visited,
        int depth)
    {
        if (depth >= MaxNestingDepth) return true;
        if (!visited.Add(current)) return false;
        if (!registry.TryGetValue(current, out var def)) return false;
        foreach (var item in def.Items)
        {
            if (item is not PartInstanceViewModel pi) continue;
            var childDefId = pi.DefinitionId.Value;
            if (childDefId == target) return true;
            if (DependsOn(childDefId, target, registry, visited, depth + 1)) return true;
        }
        return false;
    }

    private static (double X, double Y, double Width, double Height) ComputeBounds(
        IReadOnlyList<DesignerItemViewModelBase> items)
    {
        var minX = items.Min(i => i.Left.Value);
        var minY = items.Min(i => i.Top.Value);
        var maxX = items.Max(i => i.Left.Value + i.Width.Value);
        var maxY = items.Max(i => i.Top.Value + i.Height.Value);
        return (minX, minY, maxX - minX, maxY - minY);
    }
}
