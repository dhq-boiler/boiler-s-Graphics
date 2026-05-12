using boilersGraphics.Models.Parts;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace boilersGraphics.ViewModels.Parts;

public class ExposedPropertyViewModel : BindableBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private bool _disposed;

    public ExposedProperty Model { get; }

    public BindableReactiveProperty<Guid> Id { get; }
    public BindableReactiveProperty<string> Name { get; }
    public BindableReactiveProperty<ExposedPropertyType> Type { get; }
    public BindableReactiveProperty<bool> IsArray { get; }
    public BindableReactiveProperty<object> DefaultValue { get; }
    public BindableReactiveProperty<double?> MinValue { get; }
    public BindableReactiveProperty<double?> MaxValue { get; }
    public BindableReactiveProperty<double?> Step { get; }

    public ObservableCollection<BindingViewModel> Bindings { get; } = new();

    public ExposedPropertyViewModel() : this(new ExposedProperty())
    {
    }

    public ExposedPropertyViewModel(ExposedProperty model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));

        Id = new BindableReactiveProperty<Guid>(Model.Id);
        Name = new BindableReactiveProperty<string>(Model.Name);
        Type = new BindableReactiveProperty<ExposedPropertyType>(Model.Type);
        IsArray = new BindableReactiveProperty<bool>(Model.IsArray);
        DefaultValue = new BindableReactiveProperty<object>(Model.DefaultValue);
        MinValue = new BindableReactiveProperty<double?>(Model.MinValue);
        MaxValue = new BindableReactiveProperty<double?>(Model.MaxValue);
        Step = new BindableReactiveProperty<double?>(Model.Step);

        Id.Subscribe(v => Model.Id = v).AddTo(_disposables);
        Name.Subscribe(v => Model.Name = v).AddTo(_disposables);
        Type.Subscribe(v => Model.Type = v).AddTo(_disposables);
        IsArray.Subscribe(v => Model.IsArray = v).AddTo(_disposables);
        DefaultValue.Subscribe(v => Model.DefaultValue = v).AddTo(_disposables);
        MinValue.Subscribe(v => Model.MinValue = v).AddTo(_disposables);
        MaxValue.Subscribe(v => Model.MaxValue = v).AddTo(_disposables);
        Step.Subscribe(v => Model.Step = v).AddTo(_disposables);

        foreach (var b in Model.Bindings)
            Bindings.Add(new BindingViewModel(b));

        Bindings.CollectionChanged += OnBindingsCollectionChanged;
    }

    private void OnBindingsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (BindingViewModel vm in e.NewItems!)
                    Model.Bindings.Add(vm.Model);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (BindingViewModel vm in e.OldItems!)
                    Model.Bindings.Remove(vm.Model);
                break;
            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.NewItems!.Count; i++)
                    Model.Bindings[e.NewStartingIndex + i] = ((BindingViewModel)e.NewItems[i]!).Model;
                break;
            case NotifyCollectionChangedAction.Move:
                var moved = Model.Bindings[e.OldStartingIndex];
                Model.Bindings.RemoveAt(e.OldStartingIndex);
                Model.Bindings.Insert(e.NewStartingIndex, moved);
                break;
            case NotifyCollectionChangedAction.Reset:
                Model.Bindings.Clear();
                foreach (var vm in Bindings)
                    Model.Bindings.Add(vm.Model);
                break;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Bindings.CollectionChanged -= OnBindingsCollectionChanged;
        foreach (var vm in Bindings)
            vm.Dispose();

        _disposables.Dispose();
        Id.Dispose();
        Name.Dispose();
        Type.Dispose();
        IsArray.Dispose();
        DefaultValue.Dispose();
        MinValue.Dispose();
        MaxValue.Dispose();
        Step.Dispose();
    }
}
