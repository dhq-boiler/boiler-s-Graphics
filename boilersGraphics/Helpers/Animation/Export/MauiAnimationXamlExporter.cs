using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// Phase 5.5-d-6: MAUI 用 <see cref="IAnimationExporter"/> 実装。
/// XAML (<see cref="MauiAnimationXamlBuilder"/>) と code-behind
/// (<see cref="MauiAnimationCodeBehindBuilder"/>) を両方出す。
/// MAUI の Animation API はコード側依存なので code-behind は必須 = 戻り値は常に 2。
/// </summary>
public sealed class MauiAnimationXamlExporter : IAnimationExporter
{
    private readonly IReadOnlyList<SelectableDesignerItemViewModelBase> _allItems;
    private readonly Func<SelectableDesignerItemViewModelBase, PathGeometry> _pathGeometryResolver;
    private readonly Action<string, string> _writeAllText;

    public MauiAnimationXamlExporter(
        IReadOnlyList<SelectableDesignerItemViewModelBase> allItems,
        Func<SelectableDesignerItemViewModelBase, PathGeometry> pathGeometryResolver = null,
        Action<string, string> writeAllText = null)
    {
        _allItems = allItems ?? throw new ArgumentNullException(nameof(allItems));
        _pathGeometryResolver = pathGeometryResolver;
        _writeAllText = writeAllText ?? DefaultWriteAllText;
    }

    public string FormatName => "MAUI Animation XAML";
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

        var settings = WpfStoryboardXamlExporter.BuildSettings(options);

        var xaml = MauiAnimationXamlBuilder.Build(timeline, _allItems, settings, _pathGeometryResolver);
        _writeAllText(outputPath, xaml);

        var cs = MauiAnimationCodeBehindBuilder.Build(timeline, settings);
        _writeAllText(outputPath + ".cs", cs);

        return 2;
    }

    private static void DefaultWriteAllText(string path, string content)
    {
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
