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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

    /// <summary>
    /// Phase 1 フォローアップ A: 内部図形の各プロパティの公開状態を選択中図形に対して動的に判定する。
    /// 'Left' / 'Top' / 'Width' / 'Height' / 'EdgeThickness' / 'EdgeBrush' / 'FillBrush' をキーに持つ。
    /// </summary>
    private readonly Dictionary<string, BindableReactiveProperty<bool>> _exposureFlags = new()
    {
        { "Left", new BindableReactiveProperty<bool>(false) },
        { "Top", new BindableReactiveProperty<bool>(false) },
        { "Width", new BindableReactiveProperty<bool>(false) },
        { "Height", new BindableReactiveProperty<bool>(false) },
        { "EdgeThickness", new BindableReactiveProperty<bool>(false) },
        { "EdgeBrush", new BindableReactiveProperty<bool>(false) },
        { "FillBrush", new BindableReactiveProperty<bool>(false) },
        // Phase 3-h §5.7 / Q-9: Phase 3 で追加された公開可能プロパティ。
        { "BeginPoint", new BindableReactiveProperty<bool>(false) },
        { "EndPoint", new BindableReactiveProperty<bool>(false) },
        { "BeginControlPoint", new BindableReactiveProperty<bool>(false) },
        { "EndControlPoint", new BindableReactiveProperty<bool>(false) },
        { "CornerRadius", new BindableReactiveProperty<bool>(false) },
        { "RelativeX", new BindableReactiveProperty<bool>(false) },
        { "RelativeY", new BindableReactiveProperty<bool>(false) },
        { "IsNode", new BindableReactiveProperty<bool>(false) },
    };

    public BindableReactiveProperty<bool> IsLeftExposed => _exposureFlags["Left"];
    public BindableReactiveProperty<bool> IsTopExposed => _exposureFlags["Top"];
    public BindableReactiveProperty<bool> IsWidthExposed => _exposureFlags["Width"];
    public BindableReactiveProperty<bool> IsHeightExposed => _exposureFlags["Height"];
    public BindableReactiveProperty<bool> IsEdgeThicknessExposed => _exposureFlags["EdgeThickness"];
    public BindableReactiveProperty<bool> IsEdgeBrushExposed => _exposureFlags["EdgeBrush"];
    public BindableReactiveProperty<bool> IsFillBrushExposed => _exposureFlags["FillBrush"];
    // Phase 3-h §5.7: Phase 3 公開可能プロパティのフラグ (XAML から DataTrigger で参照)。
    public BindableReactiveProperty<bool> IsBeginPointExposed => _exposureFlags["BeginPoint"];
    public BindableReactiveProperty<bool> IsEndPointExposed => _exposureFlags["EndPoint"];
    public BindableReactiveProperty<bool> IsBeginControlPointExposed => _exposureFlags["BeginControlPoint"];
    public BindableReactiveProperty<bool> IsEndControlPointExposed => _exposureFlags["EndControlPoint"];
    public BindableReactiveProperty<bool> IsCornerRadiusExposed => _exposureFlags["CornerRadius"];
    public BindableReactiveProperty<bool> IsRelativeXExposed => _exposureFlags["RelativeX"];
    public BindableReactiveProperty<bool> IsRelativeYExposed => _exposureFlags["RelativeY"];
    public BindableReactiveProperty<bool> IsIsNodeExposed => _exposureFlags["IsNode"];

    public ReactiveCommand<string> TogglePropertyExposureCommand { get; }

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

        TogglePropertyExposureCommand = new ReactiveCommand<string>();
        TogglePropertyExposureCommand
            .Subscribe(TogglePropertyExposure)
            .AddTo(_disposables);

        SelectedItem
            .Subscribe(_ => RecomputeExposureFlags())
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

    /// <summary>
    /// Phase 1 フォローアップ A: SelectedItem の指定プロパティを公開/非公開トグルする。
    /// 非公開→公開: ExposedProperty を新規作成 (DefaultValue は現在値) + Binding を 1 件作成。
    /// 公開→非公開: 該当する Binding を 1 件のみ持つ ExposedProperty を削除。
    /// </summary>
    internal void TogglePropertyExposure(string propertyName)
    {
        if (Definition is null || string.IsNullOrEmpty(propertyName)) return;
        if (SelectedItem.Value is not SelectableDesignerItemViewModelBase item) return;
        if (!_exposureFlags.ContainsKey(propertyName)) return;
        // Phase 3-h §5.7 / Q-9: 公開可能プロパティが Phase 3 で増えたが、すべての型が持つわけではない
        // (例: NRectangle には CornerRadius / BeginPoint がない)。
        // 持たないプロパティのトグルを無視して、無効な Binding 作成を防ぐ。
        if (item.GetType().GetProperty(propertyName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) is null)
            return;

        var existing = FindExposedPropertyFor(item, propertyName);
        if (existing is not null)
        {
            if (Diagram is not null)
                Diagram.RemoveExposedPropertyFromDefinition(Definition, existing);
            else
                Definition.ExposedProperties.Remove(existing);
        }
        else
        {
            var exposedType = MapToExposedType(propertyName);
            var currentValue = GetCurrentValue(item, propertyName);
            var ep = new ExposedPropertyViewModel(new ExposedProperty
            {
                Name = MakeUniqueExposedName(propertyName),
                Type = exposedType,
                DefaultValue = currentValue,
            });
            ep.Bindings.Add(new BindingViewModel(new Binding
            {
                TargetItemId = item.ID,
                TargetProperty = propertyName,
            }));

            if (Diagram is not null)
                Diagram.AddExposedPropertyToDefinition(Definition, ep);
            else
                Definition.ExposedProperties.Add(ep);
        }
    }

    internal ExposedPropertyViewModel FindExposedPropertyFor(
        SelectableDesignerItemViewModelBase item,
        string propertyName)
    {
        if (Definition is null || item is null) return null;
        foreach (var ep in Definition.ExposedProperties)
        {
            foreach (var b in ep.Bindings)
            {
                if (b.TargetItemId.Value == item.ID && b.TargetProperty.Value == propertyName)
                    return ep;
            }
        }
        return null;
    }

    private void RecomputeExposureFlags()
    {
        var item = SelectedItem.Value;
        foreach (var (name, flag) in _exposureFlags)
            flag.Value = item is not null && FindExposedPropertyFor(item, name) is not null;
    }

    private static ExposedPropertyType MapToExposedType(string propertyName) => propertyName switch
    {
        "EdgeBrush" => ExposedPropertyType.Brush,
        "FillBrush" => ExposedPropertyType.Brush,
        // Phase 3-h §5.7 / Q-9: Point / Bool 系を追加。残りはデフォルト Double。
        "BeginPoint" => ExposedPropertyType.Point,
        "EndPoint" => ExposedPropertyType.Point,
        "BeginControlPoint" => ExposedPropertyType.Point,
        "EndControlPoint" => ExposedPropertyType.Point,
        "IsNode" => ExposedPropertyType.Boolean,
        _ => ExposedPropertyType.Double,
    };

    private static object GetCurrentValue(SelectableDesignerItemViewModelBase item, string propertyName)
    {
        var prop = item.GetType().GetProperty(propertyName);
        var reactive = prop?.GetValue(item);
        if (reactive is null) return null;
        var valueProp = reactive.GetType().GetProperty("Value");
        return valueProp?.GetValue(reactive);
    }

    private string MakeUniqueExposedName(string baseName)
    {
        if (Definition is null) return baseName;
        var existingNames = new HashSet<string>(
            Definition.ExposedProperties.Select(ep => ep.Name.Value),
            StringComparer.Ordinal);
        if (!existingNames.Contains(baseName)) return baseName;
        for (var i = 2; ; i++)
        {
            var candidate = baseName + i.ToString();
            if (!existingNames.Contains(candidate)) return candidate;
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

        if (parameters.ContainsKey(DiagramKey))
            Diagram = parameters.GetValue<DiagramViewModel>(DiagramKey);

        UpdateTitle(Definition.Name.Value);
        Definition.Name
            .Subscribe(name => UpdateTitle(name))
            .AddTo(_disposables);

        Definition.ExposedProperties.CollectionChanged += OnExposedPropertiesChanged;
        _disposables.Add(Disposable.Create(() =>
        {
            if (Definition is not null)
                Definition.ExposedProperties.CollectionChanged -= OnExposedPropertiesChanged;
        }));
        RecomputeExposureFlags();
    }

    private void OnExposedPropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RecomputeExposureFlags();
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
        foreach (var flag in _exposureFlags.Values)
            flag.Dispose();
        _disposables.Dispose();
    }
}
