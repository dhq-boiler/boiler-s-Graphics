using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// Phase 5.5-c: <see cref="IAnimationExporter"/> 実装の WPF Storyboard XAML 出力。
/// <see cref="WpfStoryboardXamlBuilder"/> + <see cref="WpfStoryboardCodeBehindBuilder"/> を呼んで
/// <c>.xaml</c> + (オプションで) <c>.xaml.cs</c> をファイル出力する。
///
/// <see cref="IsMultiFile"/> = false (= <paramref name="outputPath"/> はファイルパス)。
/// GenerateCodeBehind=true のとき <c>.xaml.cs</c> も同名で出すので、戻り値は 1 または 2。
///
/// <c>options</c> の想定キー (全部任意、未指定なら <see cref="XamlExportSettings"/> のデフォルト):
/// <list type="bullet">
///   <item>"TargetNamespace" : string</item>
///   <item>"ClassName" : string</item>
///   <item>"AccessModifier" : string (public / internal)</item>
///   <item>"GenerateCodeBehind" : bool</item>
///   <item>"IndentWidth" : int</item>
///   <item>"NewLine" : string ("\r\n" / "\n")</item>
///   <item>"IncludeHeaderComment" : bool</item>
/// </list>
/// </summary>
public sealed class WpfStoryboardXamlExporter : IAnimationExporter
{
    private readonly IReadOnlyList<SelectableDesignerItemViewModelBase> _allItems;
    private readonly Func<SelectableDesignerItemViewModelBase, PathGeometry> _pathGeometryResolver;
    private readonly Action<string, string> _writeAllText;

    public WpfStoryboardXamlExporter(
        IReadOnlyList<SelectableDesignerItemViewModelBase> allItems,
        Func<SelectableDesignerItemViewModelBase, PathGeometry> pathGeometryResolver = null,
        Action<string, string> writeAllText = null)
    {
        _allItems = allItems ?? throw new ArgumentNullException(nameof(allItems));
        _pathGeometryResolver = pathGeometryResolver;
        _writeAllText = writeAllText ?? DefaultWriteAllText;
    }

    public string FormatName => "WPF Storyboard XAML";
    public string DefaultFileExtension => ".xaml";
    public bool IsMultiFile => false;

    public int Export(
        TimelineViewModel timeline,
        string outputPath,
        Func<Guid, SelectableDesignerItemViewModelBase> resolver,
        IDictionary<string, object> options)
    {
        if (timeline is null) throw new ArgumentNullException(nameof(timeline));
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("outputPath (出力ファイルパス) が空です。", nameof(outputPath));

        var settings = BuildSettings(options);

        var xaml = WpfStoryboardXamlBuilder.Build(timeline, _allItems, settings, _pathGeometryResolver);
        _writeAllText(outputPath, xaml);
        var written = 1;

        if (settings.GenerateCodeBehind)
        {
            var csPath = outputPath + ".cs";
            var cs = WpfStoryboardCodeBehindBuilder.Build(settings);
            _writeAllText(csPath, cs);
            written++;
        }

        return written;
    }

    /// <summary>
    /// options dictionary から <see cref="XamlExportSettings"/> を組み立てる。
    /// 未指定キーはデフォルトを採用 (record の <c>with</c> 適用ベース)。
    /// </summary>
    public static XamlExportSettings BuildSettings(IDictionary<string, object> options)
    {
        var s = new XamlExportSettings();
        if (options is null) return s;
        if (TryGet<string>(options, "TargetNamespace", out var ns) && !string.IsNullOrEmpty(ns))
            s = s with { TargetNamespace = ns };
        if (TryGet<string>(options, "ClassName", out var cn) && !string.IsNullOrEmpty(cn))
            s = s with { ClassName = cn };
        if (TryGet<string>(options, "AccessModifier", out var am) && !string.IsNullOrEmpty(am))
            s = s with { AccessModifier = am };
        if (TryGet<bool>(options, "GenerateCodeBehind", out var gcb)) s = s with { GenerateCodeBehind = gcb };
        if (TryGet<int>(options, "IndentWidth", out var iw) && iw > 0) s = s with { IndentWidth = iw };
        if (TryGet<string>(options, "NewLine", out var nl) && !string.IsNullOrEmpty(nl))
            s = s with { NewLine = nl };
        if (TryGet<bool>(options, "IncludeHeaderComment", out var ih))
            s = s with { IncludeHeaderComment = ih };
        return s;
    }

    private static bool TryGet<T>(IDictionary<string, object> options, string key, out T value)
    {
        value = default;
        if (!options.TryGetValue(key, out var raw) || raw is null) return false;
        if (raw is T direct) { value = direct; return true; }
        try
        {
            value = (T)Convert.ChangeType(raw, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void DefaultWriteAllText(string path, string content)
    {
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
