using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Models.Parts;
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
    public const string DiagramKey = "Diagram";

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

    /// <summary>
    /// Phase 1-c-6-d-6: 親 DiagramViewModel への参照。ExposedProperty の Add/Remove を Instance に同期するために使う。
    /// OnDialogOpened で渡される。null の場合 (テスト時など) は同期をスキップして Definition のみ更新する。
    /// </summary>
    public DiagramViewModel Diagram { get; private set; }

    public DelegateCommand CloseCommand { get; }

    public ReactiveCommand AddRectangleCommand { get; }

    public ReactiveCommand DeleteSelectedCommand { get; }

    public ReactiveCommand SelectEdgeColorCommand { get; }

    public ReactiveCommand SelectFillColorCommand { get; }

    public ReactiveCommand AddExposedPropertyCommand { get; }

    public ReactiveCommand<ExposedPropertyViewModel> RemoveExposedPropertyCommand { get; }

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

        AddExposedPropertyCommand = new ReactiveCommand();
        AddExposedPropertyCommand
            .Subscribe(_ => AddExposedProperty())
            .AddTo(_disposables);

        RemoveExposedPropertyCommand = new ReactiveCommand<ExposedPropertyViewModel>();
        RemoveExposedPropertyCommand
            .Subscribe(RemoveExposedProperty)
            .AddTo(_disposables);
    }

    public void SelectItem(SelectableDesignerItemViewModelBase item)
    {
        if (Definition is null) return;

        var previous = SelectedItem.Value;
        if (previous is not null && !ReferenceEquals(previous, item))
            previous.IsSelected.Value = false;

        if (item is not null)
            item.IsSelected.Value = true;

        SelectedItem.Value = item;
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
        target.IsSelected.Value = false;
        SelectedItem.Value = null;
    }

    private void SelectColor(bool isEdge)
    {
        if (_dialogService is null) return;

        var target = SelectedItem.Value;
        if (target is null) return;

        var currentBrush = isEdge ? target.EdgeBrush.Value : target.FillBrush.Value;

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

        if (isEdge) target.EdgeBrush.Value = exchange.New;
        else target.FillBrush.Value = exchange.New;
    }

    private void AddExposedProperty()
    {
        if (Definition is null) return;

        ExposedProperty model = null;
        if (_dialogService is not null)
        {
            IDialogResult dialogResult = null;
            _dialogService.ShowDialog(
                nameof(Views.AddExposedProperty),
                new DialogParameters(),
                ret => dialogResult = ret);

            if (dialogResult is null || dialogResult.Result != ButtonResult.OK) return;
            model = dialogResult.Parameters.GetValue<ExposedProperty>(
                AddExposedPropertyDialogViewModel.ExposedPropertyKey);
        }

        if (model is null) return;

        var ep = new ExposedPropertyViewModel(model);
        if (Diagram is not null)
            Diagram.AddExposedPropertyToDefinition(Definition, ep);
        else
            Definition.ExposedProperties.Add(ep);
    }

    /// <summary>
    /// Test hook: ダイアログを経由せずに直接 ExposedProperty を追加する。実機呼び出しは AddExposedPropertyCommand 経由。
    /// </summary>
    internal void AddExposedPropertyDirect(ExposedPropertyViewModel ep)
    {
        if (Definition is null || ep is null) return;
        if (Diagram is not null)
            Diagram.AddExposedPropertyToDefinition(Definition, ep);
        else
            Definition.ExposedProperties.Add(ep);
    }

    private void RemoveExposedProperty(ExposedPropertyViewModel ep)
    {
        if (Definition is null || ep is null) return;

        if (Diagram is not null)
            Diagram.RemoveExposedPropertyFromDefinition(Definition, ep);
        else
            Definition.ExposedProperties.Remove(ep);
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

        if (parameters.ContainsKey(DiagramKey))
            Diagram = parameters.GetValue<DiagramViewModel>(DiagramKey);

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
