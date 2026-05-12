using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;

namespace boilersGraphics.ViewModels;

public class PromoteToPartDialogViewModel : BindableBase, IDialogAware
{
    public const string SelectedPartNameKey = "PartName";

    public BindableReactiveProperty<string> PartName { get; } = new(string.Empty);

    public DelegateCommand OkCommand { get; }

    public DelegateCommand CancelCommand { get; }

    public string Title => "パーツ化";

    public event Action<IDialogResult> RequestClose;

    public PromoteToPartDialogViewModel()
    {
        OkCommand = new DelegateCommand(() =>
        {
            var parameters = new DialogParameters
            {
                { SelectedPartNameKey, PartName.Value ?? string.Empty },
            };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
        });

        CancelCommand = new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        });
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters is not null && parameters.ContainsKey(SelectedPartNameKey))
            PartName.Value = parameters.GetValue<string>(SelectedPartNameKey) ?? string.Empty;
    }
}
