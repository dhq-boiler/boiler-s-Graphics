using boilersGraphics.Models.Parts;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace boilersGraphics.ViewModels.Parts;

public class PartDefinitionViewModel : BindableBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private bool _disposed;

    public PartDefinition Model { get; }

    public BindableReactiveProperty<Guid> Id { get; }
    public BindableReactiveProperty<string> Name { get; }

    public ObservableCollection<DesignerItemViewModelBase> Items { get; } = new();

    public ObservableCollection<ExposedPropertyViewModel> ExposedProperties { get; } = new();

    public PartDefinitionViewModel() : this(new PartDefinition())
    {
    }

    public PartDefinitionViewModel(PartDefinition model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));

        Id = new BindableReactiveProperty<Guid>(Model.Id);
        Name = new BindableReactiveProperty<string>(Model.Name);

        Id.Subscribe(v => Model.Id = v).AddTo(_disposables);
        Name.Subscribe(v => Model.Name = v).AddTo(_disposables);

        foreach (var ep in Model.ExposedProperties)
            ExposedProperties.Add(new ExposedPropertyViewModel(ep));

        ExposedProperties.CollectionChanged += OnExposedPropertiesCollectionChanged;
    }

    private void OnExposedPropertiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (ExposedPropertyViewModel vm in e.NewItems!)
                    Model.ExposedProperties.Add(vm.Model);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (ExposedPropertyViewModel vm in e.OldItems!)
                    Model.ExposedProperties.Remove(vm.Model);
                break;
            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.NewItems!.Count; i++)
                    Model.ExposedProperties[e.NewStartingIndex + i] =
                        ((ExposedPropertyViewModel)e.NewItems[i]!).Model;
                break;
            case NotifyCollectionChangedAction.Move:
                var moved = Model.ExposedProperties[e.OldStartingIndex];
                Model.ExposedProperties.RemoveAt(e.OldStartingIndex);
                Model.ExposedProperties.Insert(e.NewStartingIndex, moved);
                break;
            case NotifyCollectionChangedAction.Reset:
                Model.ExposedProperties.Clear();
                foreach (var vm in ExposedProperties)
                    Model.ExposedProperties.Add(vm.Model);
                break;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        ExposedProperties.CollectionChanged -= OnExposedPropertiesCollectionChanged;
        foreach (var vm in ExposedProperties)
            vm.Dispose();

        _disposables.Dispose();
        Id.Dispose();
        Name.Dispose();
    }
}
