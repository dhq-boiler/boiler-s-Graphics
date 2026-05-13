using boilersGraphics.Models.Parts;
using Prism.Mvvm;
using R3;
using System;

namespace boilersGraphics.ViewModels.Parts;

public class BindingViewModel : BindableBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private bool _disposed;

    public Binding Model { get; }

    public BindableReactiveProperty<Guid> TargetItemId { get; }

    public BindableReactiveProperty<string> TargetProperty { get; }

    public BindingViewModel() : this(new Binding())
    {
    }

    public BindingViewModel(Binding model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));

        TargetItemId = new BindableReactiveProperty<Guid>(Model.TargetItemId);
        TargetProperty = new BindableReactiveProperty<string>(Model.TargetProperty);

        TargetItemId.Subscribe(v => Model.TargetItemId = v).AddTo(_disposables);
        TargetProperty.Subscribe(v => Model.TargetProperty = v).AddTo(_disposables);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _disposables.Dispose();
        TargetItemId.Dispose();
        TargetProperty.Dispose();
    }
}
