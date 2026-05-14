using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using System;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-c: AnimationTrack.EvaluateAt(now) で得た補間値を `DesignerItemViewModelBase` 系の
/// ReactiveProperty へ書き戻す pure dispatch helper。Reflection は使わず、
/// PropertyPath を文字列 switch で扱う (Phase 5-a Q-1 案 B のスコープ)。
///
/// 対応 PropertyPath:
/// - DesignerItem 共通: Left.Value / Top.Value / Width.Value / Height.Value
/// - SelectableDesignerItem: RotationAngle.Value / EdgeBrush.Value / FillBrush.Value /
///   EdgeThickness.Value / GlowRadius.Value / GlowIntensity.Value / GlowColor.Value
/// - PartInstance ExposedProperty: ExposedProperties[{guid}]
///
/// 未対応 (Phase 5 後半 or 別フェーズ):
/// - Opacity.Value (SelectableDesignerItemViewModelBase 未実装、Phase 5-d/e で検討)
/// - DrawProgress.Value (Phase 5-c の別タスクで Stroke 系図形に追加予定、Q-7 案 A)
/// - テキスト系 Block (Value/FontSize/Foreground、Phase 5-d/e で対応)
/// </summary>
public static class PropertyApplier
{
    public static bool Apply(SelectableDesignerItemViewModelBase item, string propertyPath, object value)
    {
        if (item is null || string.IsNullOrEmpty(propertyPath) || value is null) return false;

        // ExposedProperty 経由: PartInstance のパラメータ値を書き戻す
        if (propertyPath.StartsWith("ExposedProperties[") && propertyPath.EndsWith("]"))
        {
            return ApplyExposedProperty(item, propertyPath, value);
        }

        // DesignerItemViewModelBase の共通プロパティ
        if (item is DesignerItemViewModelBase di)
        {
            switch (propertyPath)
            {
                case "Left.Value": di.Left.Value = Convert.ToDouble(value); return true;
                case "Top.Value": di.Top.Value = Convert.ToDouble(value); return true;
                case "Width.Value": di.Width.Value = Convert.ToDouble(value); return true;
                case "Height.Value": di.Height.Value = Convert.ToDouble(value); return true;
            }
        }

        // SelectableDesignerItemViewModelBase 共通プロパティ
        switch (propertyPath)
        {
            case "RotationAngle.Value":
                item.RotationAngle.Value = Convert.ToDouble(value);
                return true;
            case "EdgeBrush.Value":
                if (value is Brush eb) { item.EdgeBrush.Value = eb; return true; }
                break;
            case "FillBrush.Value":
                if (value is Brush fb) { item.FillBrush.Value = fb; return true; }
                break;
            case "EdgeThickness.Value":
                item.EdgeThickness.Value = Convert.ToDouble(value);
                return true;
            case "GlowRadius.Value":
                item.GlowRadius.Value = Convert.ToDouble(value);
                return true;
            case "GlowIntensity.Value":
                item.GlowIntensity.Value = Convert.ToDouble(value);
                return true;
            case "GlowColor.Value":
                if (value is Color gc) { item.GlowColor.Value = gc; return true; }
                if (value is null) { item.GlowColor.Value = null; return true; }
                break;
        }
        return false;
    }

    private static bool ApplyExposedProperty(SelectableDesignerItemViewModelBase item, string propertyPath, object value)
    {
        if (item is not PartInstanceViewModel pi) return false;

        // "ExposedProperties[{guid}]" → 中の guid を抜き出す
        var inner = propertyPath.Substring("ExposedProperties[".Length);
        inner = inner.TrimEnd(']');
        if (!Guid.TryParse(inner, out var exposedId)) return false;

        if (pi.ParameterValues.TryGetValue(exposedId, out var prop))
        {
            prop.Value = value;
            return true;
        }
        return false;
    }
}
