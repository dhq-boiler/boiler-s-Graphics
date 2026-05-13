using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Application = System.Windows.Application;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels.Parts;

public class PartInstanceViewModel : DesignerItemViewModelBase
{
    private readonly Dictionary<Guid, BindableReactiveProperty<object>> _parameterValues = new();
    private readonly BindableReactiveProperty<int> _exposedParameterCount = new(0);
    private CompositeDisposable _renderBindings = new();

    public BindableReactiveProperty<Guid> DefinitionId { get; } = new();

    public ReadOnlyDictionary<Guid, BindableReactiveProperty<object>> ParameterValues { get; }

    public IReadOnlyBindableReactiveProperty<int> ExposedParameterCount { get; }

    public IReadOnlyBindableReactiveProperty<bool> HasExposedParameters { get; }

    /// <summary>
    /// Phase 2-f: PartDefinition.Items を ID 引き継ぎでクローンしたインスタンス専用のレンダリング用コレクション。
    /// DataTemplate がこれを ItemsControl 経由で描画する (UI 配線は別タスク Phase 2-f-2)。
    /// Bindings は ParameterValues の値変更をこれの対応プロパティへ伝搬する。
    /// </summary>
    public ObservableCollection<DesignerItemViewModelBase> RenderedItems { get; } = new();

    public ReactiveCommand MouseDoubleClickCommand { get; } = new();

    public override bool IsResizable => false;

    public override bool SupportsPropertyDialog => true;

    public PartInstanceViewModel()
    {
        ParameterValues = new ReadOnlyDictionary<Guid, BindableReactiveProperty<object>>(_parameterValues);
        ExposedParameterCount = _exposedParameterCount.ToReadOnlyBindableReactiveProperty();
        HasExposedParameters = _exposedParameterCount
            .Select(c => c > 0)
            .ToReadOnlyBindableReactiveProperty();
        IsHitTestVisible.Value = true;
        InitMouseDoubleClick();
    }

    public PartInstanceViewModel(Guid definitionId) : this()
    {
        DefinitionId.Value = definitionId;
    }

    public PartInstanceViewModel(int id, IDiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        ParameterValues = new ReadOnlyDictionary<Guid, BindableReactiveProperty<object>>(_parameterValues);
        ExposedParameterCount = _exposedParameterCount.ToReadOnlyBindableReactiveProperty();
        HasExposedParameters = _exposedParameterCount
            .Select(c => c > 0)
            .ToReadOnlyBindableReactiveProperty();
        IsHitTestVisible.Value = true;
        InitMouseDoubleClick();
    }

    private void InitMouseDoubleClick()
    {
        MouseDoubleClickCommand
            .Subscribe(_ => RequestEditDefinition())
            .AddTo(_CompositeDisposable);
    }

    private void RequestEditDefinition()
    {
        if (Owner is not DiagramViewModel diagram) return;
        if (!diagram.TryGetPartDefinition(DefinitionId.Value, out var definition)) return;

        diagram.OpenPartEditor(definition);
    }

    public BindableReactiveProperty<object> GetOrCreateParameterValue(Guid exposedPropertyId, object defaultValue = null)
    {
        if (_parameterValues.TryGetValue(exposedPropertyId, out var rp))
            return rp;

        rp = new BindableReactiveProperty<object>(defaultValue);
        _parameterValues[exposedPropertyId] = rp;
        _exposedParameterCount.Value = _parameterValues.Count;
        return rp;
    }

    public bool TryGetParameterValue(Guid exposedPropertyId, out BindableReactiveProperty<object> value)
        => _parameterValues.TryGetValue(exposedPropertyId, out value);

    public void RemoveParameterValue(Guid exposedPropertyId)
    {
        if (_parameterValues.TryGetValue(exposedPropertyId, out var rp))
        {
            rp.Dispose();
            _parameterValues.Remove(exposedPropertyId);
            _exposedParameterCount.Value = _parameterValues.Count;
        }
    }

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        return GeometryCreator.CreateRectangle(this, 0, 0, flag);
    }

    public override Type GetViewType()
    {
        return typeof(Path);
    }

    public override void OpenPropertyDialog()
    {
        if (App.IsTest) return;
        var app = Application.Current as PrismApplication;
        if (app?.Container is not IContainerExtension container) return;

        var dialogService = new DialogService(container);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailPartInstance),
            new DialogParameters { { "ViewModel", this } },
            ret => result = ret);
    }

    public override object Clone()
    {
        var clone = new PartInstanceViewModel(DefinitionId.Value)
        {
            Owner = Owner
        };
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.RotationAngle.Value = RotationAngle.Value;

        foreach (var kv in _parameterValues)
            clone.GetOrCreateParameterValue(kv.Key, kv.Value.Value);

        return clone;
    }

    /// <summary>
    /// Phase 2-f: PartDefinition の Items を ID 引き継ぎでクローンして RenderedItems に詰め、
    /// ExposedProperty.Bindings を辿って ParameterValues[ep.Id] の変更を内部 Item の対応プロパティへ伝搬する Subscribe を貼る。
    /// 複数回呼んでも安全 (既存 RenderedItems を破棄して再構築)。
    /// </summary>
    public void InitializeRenderedItems(PartDefinitionViewModel definition)
    {
        if (definition is null) throw new ArgumentNullException(nameof(definition));

        foreach (var existing in RenderedItems)
            existing.Dispose();
        RenderedItems.Clear();

        _renderBindings.Dispose();
        _renderBindings = new CompositeDisposable();

        foreach (var src in definition.Items)
        {
            var clone = (DesignerItemViewModelBase)src.Clone();
            clone.ID = src.ID;
            RenderedItems.Add(clone);
        }

        foreach (var ep in definition.ExposedProperties)
        {
            var paramRP = GetOrCreateParameterValue(ep.Id.Value, ep.DefaultValue.Value);
            var bindings = ep.Bindings.Select(b => b.Model).ToList();

            paramRP.Subscribe(value =>
            {
                foreach (var b in bindings)
                {
                    var target = RenderedItems.FirstOrDefault(it => it.ID == b.TargetItemId);
                    if (target is null) continue;
                    ApplyValueToProperty(target, b.TargetProperty, value);
                }
            }).AddTo(_renderBindings);
        }
    }

    /// <summary>
    /// Phase 2-f: VM の BindableReactiveProperty&lt;T&gt; を public プロパティ経由で受け取り、Value プロパティに value を書き込む。
    /// 既存図形群の RP は Reflection で取得 (Phase 2-a §6 / Q-9 の主要プロパティはすべて public RP)。
    /// 型不一致時は Convert.ChangeType で試行、失敗したら何もしない (デザイナの誤設定で例外を投げない)。
    /// </summary>
    internal static void ApplyValueToProperty(SelectableDesignerItemViewModelBase target, string propertyName, object value)
    {
        if (target is null || string.IsNullOrEmpty(propertyName)) return;

        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return;

        var rp = prop.GetValue(target);
        if (rp is null) return;

        var valueProp = rp.GetType().GetProperty("Value");
        if (valueProp is null || !valueProp.CanWrite) return;

        object converted = value;
        if (value is not null)
        {
            var targetType = valueProp.PropertyType;
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (!targetType.IsInstanceOfType(value))
            {
                try { converted = Convert.ChangeType(value, underlying); }
                catch { return; }
            }
        }

        try { valueProp.SetValue(rp, converted); }
        catch { /* セッターが拒否したら静かに諦める */ }
    }

    public override void Dispose()
    {
        _renderBindings.Dispose();
        foreach (var rp in _parameterValues.Values)
            rp.Dispose();
        _parameterValues.Clear();

        foreach (var item in RenderedItems)
            item.Dispose();
        RenderedItems.Clear();

        _exposedParameterCount.Dispose();
        DefinitionId.Dispose();
        base.Dispose();
    }
}
