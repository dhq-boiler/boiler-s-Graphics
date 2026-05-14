using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels.Animation;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-b: TimelineViewModel を `&lt;Timeline&gt;` セクションへシリアライズ / デシリアライズする静的 helper。
/// CanvasPage / DiagramViewModel への組み込みは Phase 5-d/e で行うため、本クラスは Timeline 単独で完結する。
/// 値型は Q-4 確定済み (Double/Int/Boolean/Point/Color/Brush/String/Enum)。
/// </summary>
public static class TimelineSerializer
{
    public static XElement SerializeTimeline(TimelineViewModel timeline)
    {
        var elem = new XElement("Timeline");
        if (timeline is null) return elem;

        elem.Add(new XElement("Duration", timeline.Duration.Value.ToString(CultureInfo.InvariantCulture)));
        elem.Add(new XElement("Fps", timeline.Fps.Value.ToString(CultureInfo.InvariantCulture)));
        elem.Add(new XElement("PlayRangeStart", timeline.PlayRangeStart.Value.ToString(CultureInfo.InvariantCulture)));
        elem.Add(new XElement("PlayRangeEnd", timeline.PlayRangeEnd.Value.ToString(CultureInfo.InvariantCulture)));
        elem.Add(new XElement("Loop", timeline.Loop.Value));

        var tracksElem = new XElement("Tracks");
        foreach (var track in timeline.Tracks)
        {
            tracksElem.Add(SerializeTrack(track));
        }
        elem.Add(tracksElem);
        return elem;
    }

    public static TimelineViewModel DeserializeTimeline(XElement elem)
    {
        if (elem is null || elem.Name.LocalName != "Timeline") return new TimelineViewModel();

        var duration = ParseDouble(elem.Element("Duration")?.Value, 0.0);
        var fps = ParseInt(elem.Element("Fps")?.Value, 30);
        var playStart = ParseDouble(elem.Element("PlayRangeStart")?.Value, 0.0);
        var playEnd = ParseDouble(elem.Element("PlayRangeEnd")?.Value, duration);
        var loop = ParseBool(elem.Element("Loop")?.Value, false);

        var timeline = new TimelineViewModel(duration, fps);
        timeline.PlayRangeStart.Value = playStart;
        timeline.PlayRangeEnd.Value = playEnd;
        timeline.Loop.Value = loop;

        var tracksElem = elem.Element("Tracks");
        if (tracksElem is not null)
        {
            foreach (var trackElem in tracksElem.Elements("Track"))
            {
                var track = DeserializeTrack(trackElem);
                if (track is not null) timeline.Tracks.Add(track);
            }
        }
        return timeline;
    }

    public static XElement SerializeTrack(AnimationTrack track)
    {
        var elem = new XElement("Track");
        elem.Add(new XElement("ItemId", track.Target.ItemId));
        elem.Add(new XElement("PropertyPath", track.Target.PropertyPath));
        elem.Add(new XElement("ValueType", track.Target.ValueType.ToString()));

        var kfsElem = new XElement("Keyframes");
        foreach (var kf in track.Keyframes)
        {
            kfsElem.Add(SerializeKeyframe(kf, track.Target.ValueType));
        }
        elem.Add(kfsElem);
        return elem;
    }

    public static AnimationTrack DeserializeTrack(XElement elem)
    {
        if (elem is null) return null;
        var itemIdStr = elem.Element("ItemId")?.Value;
        var propPath = elem.Element("PropertyPath")?.Value;
        var valueTypeStr = elem.Element("ValueType")?.Value;

        if (string.IsNullOrEmpty(itemIdStr) || string.IsNullOrEmpty(propPath) || string.IsNullOrEmpty(valueTypeStr)) return null;
        if (!Guid.TryParse(itemIdStr, out var itemId)) return null;
        if (!Enum.TryParse<AnimatedValueType>(valueTypeStr, out var valueType)) return null;

        var pref = new PropertyRef(itemId, propPath, valueType);
        var track = new AnimationTrack(pref);

        var kfsElem = elem.Element("Keyframes");
        if (kfsElem is not null)
        {
            foreach (var kfElem in kfsElem.Elements("Keyframe"))
            {
                var kf = DeserializeKeyframe(kfElem, valueType);
                if (kf is not null) track.Keyframes.Add(kf);
            }
        }
        return track;
    }

    public static XElement SerializeKeyframe(Keyframe kf, AnimatedValueType valueType)
    {
        var elem = new XElement("Keyframe");
        elem.Add(new XElement("Time", kf.Time.Value.ToString(CultureInfo.InvariantCulture)));
        elem.Add(new XElement("Value", SerializeValue(kf.Value.Value, valueType)));
        elem.Add(new XElement("Easing", kf.Easing.Value.ToString()));
        elem.Add(new XElement("EasingMode", kf.Mode.Value.ToString()));
        return elem;
    }

    public static Keyframe DeserializeKeyframe(XElement elem, AnimatedValueType valueType)
    {
        if (elem is null) return null;
        var time = ParseDouble(elem.Element("Time")?.Value, 0.0);
        var valStr = elem.Element("Value")?.Value;
        var value = DeserializeValue(valStr, valueType);
        var easing = Enum.TryParse<EasingKind>(elem.Element("Easing")?.Value, out var ek) ? ek : EasingKind.LinearEase;
        var mode = Enum.TryParse<EasingMode>(elem.Element("EasingMode")?.Value, out var em) ? em : EasingMode.EaseIn;
        return new Keyframe(time, value, easing, mode);
    }

    private static string SerializeValue(object value, AnimatedValueType type)
    {
        if (value is null) return string.Empty;
        return type switch
        {
            AnimatedValueType.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            AnimatedValueType.Int => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            AnimatedValueType.Boolean => Convert.ToBoolean(value).ToString(),
            AnimatedValueType.Point => value is Point p ? $"{p.X.ToString(CultureInfo.InvariantCulture)},{p.Y.ToString(CultureInfo.InvariantCulture)}" : value.ToString(),
            AnimatedValueType.Color => value is Color c ? c.ToString() : value.ToString(),
            AnimatedValueType.Brush => value is SolidColorBrush scb ? scb.Color.ToString() : value?.ToString() ?? string.Empty,
            AnimatedValueType.String => value?.ToString() ?? string.Empty,
            AnimatedValueType.Enum => value?.ToString() ?? string.Empty,
            _ => value?.ToString() ?? string.Empty,
        };
    }

    /// <summary>
    /// Enum 型は Phase 5-b の段階では「enum 型本体を知らない」状態なので、文字列のまま復元する。
    /// 実際の Enum 値への変換は Phase 5-c (PropertyApplier 経由で適用先の型を知った時点) で行う。
    /// </summary>
    private static object DeserializeValue(string s, AnimatedValueType type)
    {
        if (s is null) return null;
        return type switch
        {
            AnimatedValueType.Double => ParseDouble(s, 0.0),
            AnimatedValueType.Int => ParseInt(s, 0),
            AnimatedValueType.Boolean => ParseBool(s, false),
            AnimatedValueType.Point => ParsePoint(s),
            AnimatedValueType.Color => ParseColor(s),
            AnimatedValueType.Brush => new SolidColorBrush(ParseColor(s)),
            AnimatedValueType.String => s,
            AnimatedValueType.Enum => s,
            _ => s,
        };
    }

    private static double ParseDouble(string s, double def) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : def;

    private static int ParseInt(string s, int def) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : def;

    private static bool ParseBool(string s, bool def) =>
        bool.TryParse(s, out var v) ? v : def;

    private static Point ParsePoint(string s)
    {
        if (string.IsNullOrEmpty(s)) return new Point(0, 0);
        var parts = s.Split(',');
        if (parts.Length != 2) return new Point(0, 0);
        return new Point(ParseDouble(parts[0], 0), ParseDouble(parts[1], 0));
    }

    private static Color ParseColor(string s)
    {
        if (string.IsNullOrEmpty(s)) return Colors.Transparent;
        try
        {
            var obj = ColorConverter.ConvertFromString(s);
            if (obj is Color c) return c;
        }
        catch
        {
        }
        return Colors.Transparent;
    }
}
