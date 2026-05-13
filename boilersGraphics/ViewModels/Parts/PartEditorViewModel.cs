using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
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

    private readonly IDialogService _dialogService;
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

    public ReactiveCommand SelectEdgeColorCommand { get; }

    public ReactiveCommand SelectFillColorCommand { get; }

    public BindableReactiveProperty<SelectableDesignerItemViewModelBase> SelectedItem { get; } = new();

    public event Action<IDialogResult> RequestClose;

    public PartEditorViewModel() : this(null)
    {
    }

    public PartEditorViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;

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

        SelectEdgeColorCommand = new ReactiveCommand();
        SelectEdgeColorCommand
            .Subscribe(_ => SelectColor(isEdge: true))
            .AddTo(_disposables);

        SelectFillColorCommand = new ReactiveCommand();
        SelectFillColorCommand
            .Subscribe(_ => SelectColor(isEdge: false))
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

    private void SelectColor(bool isEdge)
    {
        if (_dialogService is null) return;

        var target = SelectedItem.Value;
        if (target is null) return;

        Brush currentBrush;
        try
        {
            currentBrush = isEdge ? target.EdgeBrush.Value : target.FillBrush.Value;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        IDialogResult dialogResult = null;
        _dialogService.ShowDialog(
            nameof(ColorPicker),
            new DialogParameters
            {
                { "ColorExchange", new ColorExchange { Old = currentBrush } },
                // ColorPicker のサブ ViewModel (SolidColorPickerViewModel) は ColorSpots を非 null 前提で参照する。
                // PartEditor からは DiagramViewModel.ColorSpots を引っ張ってこないので空の ColorSpots を渡す。
                { "ColorSpots", new ColorSpots() }
            },
            ret => dialogResult = ret);

        var exchange = dialogResult?.Parameters.GetValue<ColorExchange>("ColorExchange");
        if (exchange?.New is null) return;

        try
        {
            if (isEdge) target.EdgeBrush.Value = exchange.New;
            else target.FillBrush.Value = exchange.New;
        }
        catch (ObjectDisposedException)
        {
            // SelectedItem が直前に Dispose されたケース。何もしない。
        }
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
