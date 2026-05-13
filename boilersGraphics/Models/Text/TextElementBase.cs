using Prism.Mvvm;
using System;
using System.Windows.Media;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2-a §4.1: FUI 系テキスト要素 (MonoTextBlock, DataGenerator, NumberSequence など) の
/// 共通基底モデル。既存 Letter ファミリには手を出さない (Q-1 案 A)。
/// FontFamily は string で保持 (シリアライズ容易性のため; VM 側で FontFamilyEx に変換)。
/// </summary>
[Serializable]
public abstract class TextElementBase : BindableBase
{
    private string _Text = string.Empty;
    private string _FontFamily = "Consolas";
    private int _FontSize = 12;
    private Brush _Foreground = Brushes.White;
    private Brush _Background;
    private double? _LineHeight;
    private double _LetterSpacing;
    private double _TextOpacity = 1.0;
    private bool _IsWordWrap;

    public string Text
    {
        get => _Text;
        set => SetProperty(ref _Text, value);
    }

    public string FontFamily
    {
        get => _FontFamily;
        set => SetProperty(ref _FontFamily, value);
    }

    public int FontSize
    {
        get => _FontSize;
        set => SetProperty(ref _FontSize, value);
    }

    public Brush Foreground
    {
        get => _Foreground;
        set => SetProperty(ref _Foreground, value);
    }

    public Brush Background
    {
        get => _Background;
        set => SetProperty(ref _Background, value);
    }

    public double? LineHeight
    {
        get => _LineHeight;
        set => SetProperty(ref _LineHeight, value);
    }

    public double LetterSpacing
    {
        get => _LetterSpacing;
        set => SetProperty(ref _LetterSpacing, value);
    }

    public double TextOpacity
    {
        get => _TextOpacity;
        set => SetProperty(ref _TextOpacity, value);
    }

    public bool IsWordWrap
    {
        get => _IsWordWrap;
        set => SetProperty(ref _IsWordWrap, value);
    }
}
