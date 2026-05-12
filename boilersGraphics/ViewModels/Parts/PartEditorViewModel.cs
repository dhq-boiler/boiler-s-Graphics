using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;

namespace boilersGraphics.ViewModels.Parts;

public class PartEditorViewModel : BindableBase, IDialogAware, IDisposable
{
    public const string PartDefinitionKey = "PartDefinition";

    private readonly CompositeDisposable _disposables = new();
    private bool _disposed;

    private string _title = "パーツ編集";

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public PartDefinitionViewModel Definition { get; private set; }

    public DelegateCommand CloseCommand { get; }

    public event Action<IDialogResult> RequestClose;

    public PartEditorViewModel()
    {
        CloseCommand = new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
        Dispose();
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters is null || !parameters.ContainsKey(PartDefinitionKey))
            return;

        Definition = parameters.GetValue<PartDefinitionViewModel>(PartDefinitionKey);
        if (Definition is null) return;

        UpdateTitle(Definition.Name.Value);
        Definition.Name
            .Subscribe(name => UpdateTitle(name))
            .AddTo(_disposables);
    }

    private void UpdateTitle(string name)
    {
        Title = string.IsNullOrWhiteSpace(name)
            ? "パーツ編集"
            : $"パーツ編集: {name}";
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _disposables.Dispose();
    }
}
