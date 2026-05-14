using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-g: PNG 連番書出を <see cref="IAnimationExporter"/> 経由でも呼べるようにする薄いラッパ。
/// 既存呼び出し側 (DiagramViewModel) は引き続き static <see cref="PngSequenceExporter.Export"/> を直接使ってよく、
/// このアダプタは「複数 Exporter を切替えるダイアログ」(Phase 5.5 で来る予定) や、
/// Exporter を抽象的に扱いたい新規コードのためのもの。
///
/// Renderer 呼び出しの副作用 (Renderer.Render → PngBitmapEncoder → FileStream) は外注なので、
/// コンストラクタで <c>renderAndSaveFrame</c> デリゲートを受け取る。
///
/// <c>options</c> の想定キー:
/// <list type="bullet">
///   <item>"Start" : double (必須)</item>
///   <item>"End"   : double (必須)</item>
///   <item>"Fps"   : int    (必須)</item>
///   <item>"FilenamePrefix" : string (任意、デフォルト "frame_")</item>
/// </list>
/// <c>outputPath</c> は出力ディレクトリ。<see cref="IsMultiFile"/> = true なので。
/// </summary>
public sealed class PngSequenceExporterAdapter : IAnimationExporter
{
    private readonly Action<double, string> _renderAndSaveFrame;

    public PngSequenceExporterAdapter(Action<double, string> renderAndSaveFrame)
    {
        _renderAndSaveFrame = renderAndSaveFrame ?? throw new ArgumentNullException(nameof(renderAndSaveFrame));
    }

    public string FormatName => "PNG Sequence";
    public string DefaultFileExtension => ".png";
    public bool IsMultiFile => true;

    public int Export(
        TimelineViewModel timeline,
        string outputPath,
        Func<Guid, SelectableDesignerItemViewModelBase> resolver,
        IDictionary<string, object> options)
    {
        if (timeline is null) throw new ArgumentNullException(nameof(timeline));
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("outputPath (出力ディレクトリ) が空です。", nameof(outputPath));
        if (options is null) throw new ArgumentNullException(nameof(options));

        var settings = new PngSequenceExportSettings
        {
            Start = GetRequired<double>(options, "Start"),
            End = GetRequired<double>(options, "End"),
            Fps = GetRequired<int>(options, "Fps"),
            OutputDirectory = outputPath,
            FilenamePrefix = GetOptional<string>(options, "FilenamePrefix", "frame_"),
        };

        return PngSequenceExporter.Export(timeline, settings, resolver, _renderAndSaveFrame);
    }

    private static T GetRequired<T>(IDictionary<string, object> options, string key)
    {
        if (!options.TryGetValue(key, out var raw) || raw is null)
            throw new ArgumentException($"必須オプション '{key}' が未指定です。", nameof(options));
        return Coerce<T>(raw, key);
    }

    private static T GetOptional<T>(IDictionary<string, object> options, string key, T defaultValue)
    {
        if (!options.TryGetValue(key, out var raw) || raw is null) return defaultValue;
        return Coerce<T>(raw, key);
    }

    private static T Coerce<T>(object raw, string key)
    {
        if (raw is T direct) return direct;
        try
        {
            return (T)Convert.ChangeType(raw, typeof(T));
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"オプション '{key}' の型を {typeof(T).Name} に変換できません: {ex.Message}", "options", ex);
        }
    }
}
