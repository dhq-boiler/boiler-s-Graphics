using boilersGraphics.ViewModels;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using System.Windows.Media;

namespace boilersGraphics.ViewModels.Parts;

public class PartEditorViewModel : BindableBase, IDialogAware, IDisposable
{
    public const string PartDefinitionKey = "PartDefinition";

    private const double DefaultShapeLeft = 50d;
    private const double DefaultShapeTop = 50d;
    private const double DefaultShapeWidth = 100d;
    private const double DefaultShapeHeight = 60d;
    private const double DefaultEdgeThickness = 1d;
    private static readonly Color DefaultEdgeColor = Colors.Gray;
    private static readonly Color DefaultFillColor = Color.FromArgb(0x30, 0x80, 0x80, 0x80);

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

    public ReactiveCommand AddRectangleCommand { get; }

    public ReactiveCommand DeleteSelectedCommand { get; }

    public BindableReactiveProperty<SelectableDesignerItemViewModelBase> SelectedItem { get; } = new();

    public event Action<IDialogResult> RequestClose;

    public PartEditorViewModel()
    {
        CloseCommand = new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        AddRectangleCommand = new ReactiveCommand();
        AddRectangleCommand
            .Subscribe(_ => AddRectangle())
            .AddTo(_disposables);

        DeleteSelectedCommand = new ReactiveCommand();
        DeleteSelectedCommand
            .Subscribe(_ => DeleteSelected())
            .AddTo(_disposables);
    }

    public void SelectItem(SelectableDesignerItemViewModelBase item)
    {
        if (Definition is null) return;

        var previous = SelectedItem.Value;
        if (previous is not null && !ReferenceEquals(previous, item))
            TrySetIsSelected(previous, false);

        if (item is not null)
            TrySetIsSelected(item, true);

        SelectedItem.Value = item;
    }

    // Items が Promote / Detach 等で先に Dispose 済みになっているケースを許容する。
    private static void TrySetIsSelected(SelectableDesignerItemViewModelBase target, bool value)
    {
        try
        {
            target.IsSelected.Value = value;
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void AddRectangle()
    {
        if (Definition is null) return;

        var rect = new NRectangleViewModel(
            DefaultShapeLeft,
            DefaultShapeTop,
            DefaultShapeWidth,
            DefaultShapeHeight);
        rect.EdgeThickness.Value = DefaultEdgeThickness;
        rect.EdgeBrush.Value = new SolidColorBrush(DefaultEdgeColor);
        rect.FillBrush.Value = new SolidColorBrush(DefaultFillColor);
        Definition.Items.Add(rect);
    }

    private void DeleteSelected()
    {
        if (Definition is null) return;

        var target = SelectedItem.Value;
        if (target is null) return;

        Definition.Items.Remove(target as DesignerItemViewModelBase);
        TrySetIsSelected(target, false);
        SelectedItem.Value = null;
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
        SelectedItem.Dispose();
        _disposables.Dispose();
    }
}
