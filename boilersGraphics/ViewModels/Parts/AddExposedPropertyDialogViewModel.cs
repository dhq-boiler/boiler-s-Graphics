using boilersGraphics.Models.Parts;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace boilersGraphics.ViewModels.Parts;

public class AddExposedPropertyDialogViewModel : BindableBase, IDialogAware
{
    public const string ExposedPropertyKey = "ExposedProperty";

    public BindableReactiveProperty<string> Name { get; } = new(string.Empty);
    public BindableReactiveProperty<ExposedPropertyType> SelectedType { get; }
        = new(ExposedPropertyType.Double);
    public BindableReactiveProperty<string> DefaultValueText { get; } = new(string.Empty);
    public BindableReactiveProperty<bool> IsArray { get; } = new(false);
    public BindableReactiveProperty<string> ErrorMessage { get; } = new(string.Empty);

    public IReadOnlyList<ExposedPropertyType> AvailableTypes { get; } = new[]
    {
        ExposedPropertyType.Double,
        ExposedPropertyType.Int,
        ExposedPropertyType.Boolean,
        ExposedPropertyType.Point,
        ExposedPropertyType.Color,
        ExposedPropertyType.Brush,
        ExposedPropertyType.String,
        ExposedPropertyType.Enum,
    };

    public DelegateCommand OkCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public string Title => "公開パラメータの追加";

    public event Action<IDialogResult> RequestClose;

    public AddExposedPropertyDialogViewModel()
    {
        OkCommand = new DelegateCommand(ExecuteOk);
        CancelCommand = new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        });
    }

    private void ExecuteOk()
    {
        var name = (Name.Value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorMessage.Value = "名前を入力してください";
            return;
        }

        var ep = new ExposedProperty
        {
            Name = name,
            Type = SelectedType.Value,
            IsArray = IsArray.Value,
            DefaultValue = ParseDefaultValue(SelectedType.Value, DefaultValueText.Value),
        };

        var parameters = new DialogParameters
        {
            { ExposedPropertyKey, ep },
        };
        RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
    }

    internal static object ParseDefaultValue(ExposedPropertyType type, string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return DefaultFor(type);

        switch (type)
        {
            case ExposedPropertyType.Double:
                return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                    ? d
                    : DefaultFor(type);
            case ExposedPropertyType.Int:
                return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                    ? i
                    : DefaultFor(type);
            case ExposedPropertyType.Boolean:
                return bool.TryParse(raw, out var b) ? b : DefaultFor(type);
            default:
                return raw;
        }
    }

    private static object DefaultFor(ExposedPropertyType type) => type switch
    {
        ExposedPropertyType.Double => 0d,
        ExposedPropertyType.Int => 0,
        ExposedPropertyType.Boolean => false,
        ExposedPropertyType.String => string.Empty,
        _ => null,
    };

    public bool CanCloseDialog() => true;
    public void OnDialogClosed() { }
    public void OnDialogOpened(IDialogParameters parameters) { }
}
