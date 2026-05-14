using boilersGraphics.ViewModels;
using R3;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-g: IsNode=true の DesignerItem が選択されたとき、関連コネクタを強調表示する。
/// Phase 3.5 (#4): stash dict 方式をやめて、対象コネクタの IsHighlighted を true/false するだけに変更。
/// 派生プロパティ <see cref="ConnectorBaseViewModel.EffectiveEdgeBrush"/> /
/// <see cref="ConnectorBaseViewModel.EffectiveEdgeThickness"/> が EdgeBrush / EdgeThickness と
/// IsHighlighted の CombineLatest で導出されるため、強調表示中のユーザ手動編集
/// (EdgeBrush 変更等) も解除時に上書きされない。
/// </summary>
public sealed class NodeHighlightController : IDisposable
{
    public const double ThicknessMultiplier = 1.5;

    private readonly IDiagramViewModel _diagram;
    private readonly HashSet<ConnectorBaseViewModel> _highlighted = new();
    private readonly IDisposable _subscription;

    public NodeHighlightController(IDiagramViewModel diagram)
    {
        _diagram = diagram ?? throw new ArgumentNullException(nameof(diagram));
        _subscription = _diagram.SelectedItems.AsObservable().Subscribe(_ => Apply());
    }

    /// <summary>選択状態から目標コネクタを計算して、IsHighlighted フラグの差分を取りつつ更新する。</summary>
    public void Apply()
    {
        var selected = _diagram.SelectedItems.Value;
        var targets = new HashSet<ConnectorBaseViewModel>();
        if (selected is not null)
        {
            foreach (var item in selected.AsValueEnumerable()
                         .OfType<DesignerItemViewModelBase>()
                         .Where(d => d.IsNode.Value))
            {
                foreach (var c in NodeRelatedConnectorFinder.FindRelated(_diagram, item))
                    targets.Add(c);
            }
        }

        // 過去に強調してたが今回対象外: IsHighlighted を false に
        var toClear = new List<ConnectorBaseViewModel>();
        foreach (var c in _highlighted)
            if (!targets.Contains(c)) toClear.Add(c);
        foreach (var c in toClear)
        {
            c.IsHighlighted.Value = false;
            _highlighted.Remove(c);
        }

        // 新規対象: IsHighlighted を true に (派生プロパティが描画値を更新)
        foreach (var c in targets)
        {
            if (_highlighted.Contains(c)) continue;
            _highlighted.Add(c);
            c.IsHighlighted.Value = true;
        }
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        // Dispose 時点で残ってる強調表示は解除する (フラグ降ろすだけ、元値の復元は不要)
        foreach (var c in _highlighted) c.IsHighlighted.Value = false;
        _highlighted.Clear();
    }

    /// <summary>
    /// SolidColorBrush の RGB 反転 (Alpha 保持)。SolidColorBrush 以外は変更せず返す。
    /// EffectiveEdgeBrush の派生計算でも使うので internal で公開。
    /// </summary>
    internal static Brush InvertBrush(Brush b)
    {
        if (b is SolidColorBrush scb)
        {
            var c = scb.Color;
            return new SolidColorBrush(Color.FromArgb(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B)));
        }
        // SolidColorBrush 以外は反転不可なので元のまま (描画は EdgeThickness の変化で識別可能)
        return b;
    }
}
