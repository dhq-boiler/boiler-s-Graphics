using boilersGraphics.ViewModels;
using R3;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-g: IsNode=true の DesignerItem が選択されたとき、関連コネクタを強調表示する。
/// EdgeThickness × <see cref="ThicknessMultiplier"/>, EdgeBrush の色反転 (SolidColorBrush 限定) を
/// 直接コネクタ VM に書き戻し、選択解除/別ノード選択時は元の値を復元する。
/// 注意: 強調表示中にユーザが EdgeThickness / EdgeBrush を手動変更すると、解除時に元の値で
///       上書きされる (Phase 3-g の最小実装スコープ。Phase 3.5 で改善余地)。
/// </summary>
public sealed class NodeHighlightController : IDisposable
{
    public const double ThicknessMultiplier = 1.5;

    private readonly IDiagramViewModel _diagram;
    private readonly Dictionary<ConnectorBaseViewModel, (Brush Brush, double Thickness)> _stash = new();
    private readonly IDisposable _subscription;

    public NodeHighlightController(IDiagramViewModel diagram)
    {
        _diagram = diagram ?? throw new ArgumentNullException(nameof(diagram));
        _subscription = _diagram.SelectedItems.AsObservable().Subscribe(_ => Apply());
    }

    /// <summary>選択状態から目標コネクタを計算して、stash と差分を取りつつ強調表示を更新する。</summary>
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

        // 過去に強調してたが今回対象外: 復元
        var toRestore = new List<ConnectorBaseViewModel>();
        foreach (var c in _stash.Keys)
            if (!targets.Contains(c)) toRestore.Add(c);
        foreach (var c in toRestore)
        {
            var (origBrush, origThickness) = _stash[c];
            c.EdgeBrush.Value = origBrush;
            c.EdgeThickness.Value = origThickness;
            _stash.Remove(c);
        }

        // 新規対象: stash に保存して強調
        foreach (var c in targets)
        {
            if (_stash.ContainsKey(c)) continue;
            _stash[c] = (c.EdgeBrush.Value, c.EdgeThickness.Value);
            c.EdgeBrush.Value = InvertBrush(c.EdgeBrush.Value);
            c.EdgeThickness.Value = c.EdgeThickness.Value * ThicknessMultiplier;
        }
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        // Dispose 時点で残ってる強調表示は復元しておく
        foreach (var kv in _stash)
        {
            kv.Key.EdgeBrush.Value = kv.Value.Brush;
            kv.Key.EdgeThickness.Value = kv.Value.Thickness;
        }
        _stash.Clear();
    }

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
