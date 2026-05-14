using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-g / 5-a Q-12: Phase 5 で確定した IR (Timeline / Track / Keyframe) を
/// 別形式に書き出す Exporter の共通契約。
///
/// 既存実装:
/// <list type="bullet">
///   <item><see cref="PngSequenceExporterAdapter"/> — PNG 連番 (Phase 5-f-2)</item>
/// </list>
/// 将来実装 (Phase 5.5):
/// <list type="bullet">
///   <item>WPF Storyboard XAML Exporter</item>
///   <item>MAUI Animation XAML Exporter</item>
/// </list>
///
/// 出力形式が「複数ファイル (連番) なのか / 単一ファイル (XAML) なのか」は
/// <see cref="IsMultiFile"/> で表現する。<c>outputPath</c> は前者ならディレクトリ、
/// 後者なら出力ファイルパスとして解釈する。
/// <c>options</c> は形式固有パラメータ (PNG 連番なら fps / range / prefix、
/// XAML なら namespace / class name など) を緩い <c>IDictionary</c> で渡す。
/// </summary>
public interface IAnimationExporter
{
    /// <summary>UI 表示用の形式名 (例: "PNG Sequence" / "WPF Storyboard XAML")。</summary>
    string FormatName { get; }

    /// <summary>主拡張子 (例: ".png" / ".xaml")。連番形式でもファイル単位はこの拡張子。</summary>
    string DefaultFileExtension { get; }

    /// <summary>true=複数ファイル (= outputPath はディレクトリ)、false=単一ファイル。</summary>
    bool IsMultiFile { get; }

    /// <summary>
    /// 書き出し本体。書き出した「フレーム数 (連番) または 1 (単一ファイル)」を返す。
    /// 失敗時は例外を投げる (戻り値 0 は IR が空 = 書き出すものが無かった状態のみ)。
    /// </summary>
    int Export(
        TimelineViewModel timeline,
        string outputPath,
        Func<Guid, SelectableDesignerItemViewModelBase> resolver,
        IDictionary<string, object> options);
}
