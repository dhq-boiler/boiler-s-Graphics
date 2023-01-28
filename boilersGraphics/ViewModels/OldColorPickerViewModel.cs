using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.Views;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels;

public class OldColorPickerViewModel : BindableBase, IDialogAware, IDisposable
{
    private WriteableBitmap _ASelector;
    private bool _bgr2hsv;
    private WriteableBitmap _BlueSelector;
    private ColorPicker _colorPicker;
    private readonly CompositeDisposable _disposables = new();
    private bool _flag = true;
    private WriteableBitmap _GreenSelector;
    private bool _hsv2bgr;
    private WriteableBitmap _HueSelector;
    private WriteableBitmap _RedSelector;
    private IEnumerable<ColorSpot> _spots;
    private WriteableBitmap _WhiteBlackColumnMap;

    public OldColorPickerViewModel()
    {
        OkCommand = Color
            .Where(x => x != null)
            .Select(_ => true)
            .ToReactiveCommand();

        OkCommand
            .Subscribe(_ =>
            {
                var colorSpots = new ColorSpots();
                colorSpots.ColorSpot0 = ColorSpot0.Value;
                colorSpots.ColorSpot1 = ColorSpot1.Value;
                colorSpots.ColorSpot2 = ColorSpot2.Value;
                colorSpots.ColorSpot3 = ColorSpot3.Value;
                colorSpots.ColorSpot4 = ColorSpot4.Value;
                colorSpots.ColorSpot5 = ColorSpot5.Value;
                colorSpots.ColorSpot6 = ColorSpot6.Value;
                colorSpots.ColorSpot7 = ColorSpot7.Value;
                colorSpots.ColorSpot8 = ColorSpot8.Value;
                colorSpots.ColorSpot9 = ColorSpot9.Value;
                colorSpots.ColorSpot10 = ColorSpot10.Value;
                colorSpots.ColorSpot11 = ColorSpot11.Value;
                colorSpots.ColorSpot12 = ColorSpot12.Value;
                colorSpots.ColorSpot13 = ColorSpot13.Value;
                colorSpots.ColorSpot14 = ColorSpot14.Value;
                colorSpots.ColorSpot15 = ColorSpot15.Value;
                colorSpots.ColorSpot16 = ColorSpot16.Value;
                colorSpots.ColorSpot17 = ColorSpot17.Value;
                colorSpots.ColorSpot18 = ColorSpot18.Value;
                colorSpots.ColorSpot19 = ColorSpot19.Value;
                colorSpots.ColorSpot20 = ColorSpot20.Value;
                colorSpots.ColorSpot21 = ColorSpot21.Value;
                colorSpots.ColorSpot22 = ColorSpot22.Value;
                colorSpots.ColorSpot23 = ColorSpot23.Value;
                colorSpots.ColorSpot24 = ColorSpot24.Value;
                colorSpots.ColorSpot25 = ColorSpot25.Value;
                colorSpots.ColorSpot26 = ColorSpot26.Value;
                colorSpots.ColorSpot27 = ColorSpot27.Value;
                colorSpots.ColorSpot28 = ColorSpot28.Value;
                colorSpots.ColorSpot29 = ColorSpot29.Value;
                colorSpots.ColorSpot30 = ColorSpot30.Value;
                colorSpots.ColorSpot31 = ColorSpot31.Value;
                colorSpots.ColorSpot32 = ColorSpot32.Value;
                colorSpots.ColorSpot33 = ColorSpot33.Value;
                colorSpots.ColorSpot34 = ColorSpot34.Value;
                colorSpots.ColorSpot35 = ColorSpot35.Value;
                colorSpots.ColorSpot36 = ColorSpot36.Value;
                colorSpots.ColorSpot37 = ColorSpot37.Value;
                colorSpots.ColorSpot38 = ColorSpot38.Value;
                colorSpots.ColorSpot39 = ColorSpot39.Value;
                colorSpots.ColorSpot40 = ColorSpot40.Value;
                colorSpots.ColorSpot41 = ColorSpot41.Value;
                colorSpots.ColorSpot42 = ColorSpot42.Value;
                colorSpots.ColorSpot43 = ColorSpot43.Value;
                colorSpots.ColorSpot44 = ColorSpot44.Value;
                colorSpots.ColorSpot45 = ColorSpot45.Value;
                colorSpots.ColorSpot46 = ColorSpot46.Value;
                colorSpots.ColorSpot47 = ColorSpot47.Value;
                colorSpots.ColorSpot48 = ColorSpot48.Value;
                colorSpots.ColorSpot49 = ColorSpot49.Value;
                colorSpots.ColorSpot50 = ColorSpot50.Value;
                colorSpots.ColorSpot51 = ColorSpot51.Value;
                colorSpots.ColorSpot52 = ColorSpot52.Value;
                colorSpots.ColorSpot53 = ColorSpot53.Value;
                colorSpots.ColorSpot54 = ColorSpot54.Value;
                colorSpots.ColorSpot55 = ColorSpot55.Value;
                colorSpots.ColorSpot56 = ColorSpot56.Value;
                colorSpots.ColorSpot57 = ColorSpot57.Value;
                colorSpots.ColorSpot58 = ColorSpot58.Value;
                colorSpots.ColorSpot59 = ColorSpot59.Value;
                colorSpots.ColorSpot60 = ColorSpot60.Value;
                colorSpots.ColorSpot61 = ColorSpot61.Value;
                colorSpots.ColorSpot62 = ColorSpot62.Value;
                colorSpots.ColorSpot63 = ColorSpot63.Value;
                colorSpots.ColorSpot64 = ColorSpot64.Value;
                colorSpots.ColorSpot65 = ColorSpot65.Value;
                colorSpots.ColorSpot66 = ColorSpot66.Value;
                colorSpots.ColorSpot67 = ColorSpot67.Value;
                colorSpots.ColorSpot68 = ColorSpot68.Value;
                colorSpots.ColorSpot69 = ColorSpot69.Value;
                colorSpots.ColorSpot70 = ColorSpot70.Value;
                colorSpots.ColorSpot71 = ColorSpot71.Value;
                colorSpots.ColorSpot72 = ColorSpot72.Value;
                colorSpots.ColorSpot73 = ColorSpot73.Value;
                colorSpots.ColorSpot74 = ColorSpot74.Value;
                colorSpots.ColorSpot75 = ColorSpot75.Value;
                colorSpots.ColorSpot76 = ColorSpot76.Value;
                colorSpots.ColorSpot77 = ColorSpot77.Value;
                colorSpots.ColorSpot78 = ColorSpot78.Value;
                colorSpots.ColorSpot79 = ColorSpot79.Value;
                colorSpots.ColorSpot80 = ColorSpot80.Value;
                colorSpots.ColorSpot81 = ColorSpot81.Value;
                colorSpots.ColorSpot82 = ColorSpot82.Value;
                colorSpots.ColorSpot83 = ColorSpot83.Value;
                colorSpots.ColorSpot84 = ColorSpot84.Value;
                colorSpots.ColorSpot85 = ColorSpot85.Value;
                colorSpots.ColorSpot86 = ColorSpot86.Value;
                colorSpots.ColorSpot87 = ColorSpot87.Value;
                colorSpots.ColorSpot88 = ColorSpot88.Value;
                colorSpots.ColorSpot89 = ColorSpot89.Value;
                colorSpots.ColorSpot90 = ColorSpot90.Value;
                colorSpots.ColorSpot91 = ColorSpot91.Value;
                colorSpots.ColorSpot92 = ColorSpot92.Value;
                colorSpots.ColorSpot93 = ColorSpot93.Value;
                colorSpots.ColorSpot94 = ColorSpot94.Value;
                colorSpots.ColorSpot95 = ColorSpot95.Value;
                colorSpots.ColorSpot96 = ColorSpot96.Value;
                colorSpots.ColorSpot97 = ColorSpot97.Value;
                colorSpots.ColorSpot98 = ColorSpot98.Value;
                colorSpots.ColorSpot99 = ColorSpot99.Value;

                EditTarget.New = new SolidColorBrush(Output.Value);
                var parameters = new DialogParameters
                {
                    { "ColorExchange", EditTarget },
                    { "ColorSpots", colorSpots }
                };
                var ret = new DialogResult(ButtonResult.OK, parameters);
                RequestClose.Invoke(ret);
            })
            .AddTo(_disposables);

        Hue
            .Subscribe(_ =>
            {
                GenerateSaturationValueMat();
                if (!_bgr2hsv)
                {
                    _hsv2bgr = true;
                    SetRGB();
                    _hsv2bgr = false;
                }

                SetColorToSpot();
            })
            .AddTo(_disposables);

        Saturation
            .Subscribe(_ =>
            {
                if (!_bgr2hsv)
                {
                    _hsv2bgr = true;
                    HSV2RGB();
                    _hsv2bgr = false;
                }

                SetColorToSpot();
            })
            .AddTo(_disposables);

        Value
            .Subscribe(_ =>
            {
                if (!_bgr2hsv)
                {
                    _hsv2bgr = true;
                    HSV2RGB();
                    _hsv2bgr = false;
                }

                SetColorToSpot();
            })
            .AddTo(_disposables);

        A
            .Subscribe(_ => { SetColorToSpot(); })
            .AddTo(_disposables);

        R
            .Subscribe(_ =>
            {
                GenerateASelectorMat();
                if (!_hsv2bgr)
                {
                    _bgr2hsv = true;
                    RecalcHue();
                    RecalcValue();
                    RecalcSaturation();
                }

                SetColorToSpot();
            })
            .AddTo(_disposables);

        G
            .Subscribe(_ =>
            {
                GenerateASelectorMat();
                if (!_hsv2bgr)
                {
                    _bgr2hsv = true;
                    RecalcHue();
                    RecalcValue();
                    RecalcSaturation();
                    _bgr2hsv = false;
                }

                SetColorToSpot();
            })
            .AddTo(_disposables);

        B
            .Subscribe(_ =>
            {
                GenerateASelectorMat();
                if (!_hsv2bgr)
                {
                    _bgr2hsv = true;
                    RecalcHue();
                    RecalcValue();
                    RecalcSaturation();
                    _bgr2hsv = false;
                }

                SetColorToSpot();
            })
            .AddTo(_disposables);

        Color
            .Subscribe(newColor =>
            {
                if (!_hsv2bgr && !_bgr2hsv)
                {
                    _hsv2bgr = true;
                    _bgr2hsv = true;
                    SetRGB();
                    _hsv2bgr = false;
                    _bgr2hsv = false;
                }
            })
            .AddTo(_disposables);

        GenerateHueSelectorMat();
        GenerateSaturationValueMat();
        GenerateRedSelectorMat();
        GenerateGreenSelectorMat();
        GenerateBlueSelectorMat();
        GenerateASelectorMat();

        OpenCloseColorPalleteCommand.Subscribe(_ =>
            {
                if (ColorPalleteVisibility.Value == Visibility.Collapsed)
                    ColorPalleteVisibility.Value = Visibility.Visible;
                else if (ColorPalleteVisibility.Value == Visibility.Visible)
                    ColorPalleteVisibility.Value = Visibility.Collapsed;
            })
            .AddTo(_disposables);
        SpotSelectCommand.Subscribe(x =>
            {
                var colorSpot = x as ColorSpot;
                if (colorSpot.IsSelected.Value)
                {
                    colorSpot.IsSelected.Value = false;
                }
                else
                {
                    if (!colorSpot.IsSelected.Value)
                    {
                        _spots.ToList().ForEach(x => x.IsSelected.Value = false);
                        colorSpot.IsSelected.Value = true;
                        var color = BrushHelper.ExtractColor(colorSpot.Brush);
                        var a = color.A;
                        var r = color.R;
                        var g = color.G;
                        var b = color.B;
                        _flag = false;
                        A.Value = a;
                        R.Value = r;
                        G.Value = g;
                        B.Value = b;
                        _flag = true;
                    }
                }
            })
            .AddTo(_disposables);
        LoadedCommand.Subscribe(x =>
            {
                var source = x.Source;
                _colorPicker = source as ColorPicker;
                _spots = _colorPicker.FindVisualChildren<ColorSpot>();
            })
            .AddTo(_disposables);
    }

    public WriteableBitmap WhiteBlackColorMap
    {
        get => _WhiteBlackColumnMap;
        set => SetProperty(ref _WhiteBlackColumnMap, value);
    }

    public WriteableBitmap HueSelector
    {
        get => _HueSelector;
        set => SetProperty(ref _HueSelector, value);
    }

    public WriteableBitmap RedSelector
    {
        get => _RedSelector;
        set => SetProperty(ref _RedSelector, value);
    }

    public WriteableBitmap GreenSelector
    {
        get => _GreenSelector;
        set => SetProperty(ref _GreenSelector, value);
    }

    public WriteableBitmap BlueSelector
    {
        get => _BlueSelector;
        set => SetProperty(ref _BlueSelector, value);
    }

    public WriteableBitmap ASelector
    {
        get => _ASelector;
        set => SetProperty(ref _ASelector, value);
    }

    /// <summary>
    ///     0 ≦ Hue ≦ 180
    /// </summary>
    public ReactivePropertySlim<byte> Hue { get; } = new();

    public ReactivePropertySlim<byte> Saturation { get; } = new();

    public ReactivePropertySlim<byte> Value { get; } = new();

    public ReactivePropertySlim<byte> A { get; } = new();

    public ReactivePropertySlim<byte> R { get; } = new();

    public ReactivePropertySlim<byte> G { get; } = new();

    public ReactivePropertySlim<byte> B { get; } = new();

    public ReactivePropertySlim<Brush> Color { get; } = new();

    public ReactivePropertySlim<Color> Output { get; } = new();

    public ReactivePropertySlim<Visibility> ColorPalleteVisibility { get; } = new(Visibility.Collapsed);

    public ReactivePropertySlim<Brush> ColorSpot0 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot1 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot2 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot3 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot4 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot5 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot6 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot7 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot8 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot9 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot10 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot11 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot12 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot13 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot14 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot15 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot16 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot17 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot18 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot19 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot20 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot21 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot22 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot23 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot24 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot25 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot26 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot27 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot28 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot29 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot30 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot31 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot32 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot33 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot34 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot35 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot36 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot37 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot38 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot39 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot40 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot41 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot42 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot43 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot44 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot45 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot46 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot47 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot48 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot49 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot50 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot51 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot52 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot53 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot54 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot55 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot56 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot57 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot58 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot59 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot60 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot61 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot62 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot63 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot64 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot65 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot66 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot67 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot68 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot69 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot70 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot71 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot72 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot73 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot74 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot75 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot76 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot77 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot78 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot79 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot80 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot81 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot82 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot83 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot84 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot85 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot86 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot87 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot88 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot89 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot90 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot91 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot92 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot93 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot94 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot95 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot96 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot97 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot98 { get; } = new();
    public ReactivePropertySlim<Brush> ColorSpot99 { get; } = new();


    public ReactiveCommand OkCommand { get; }

    public ReactiveCommand OpenCloseColorPalleteCommand { get; } = new();

    public ReactiveCommand SpotSelectCommand { get; } = new();

    public ReactiveCommand<RoutedEventArgs> LoadedCommand { get; } = new();

    public ColorExchange EditTarget { get; set; }

    public event Action<IDialogResult> RequestClose;

    public string Title => Resources.Title_ColorPicker;

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        EditTarget = parameters.GetValue<ColorExchange>("ColorExchange");
        A.Value = (EditTarget.Old as SolidColorBrush).Color.A;
        R.Value = (EditTarget.Old as SolidColorBrush).Color.R;
        G.Value = (EditTarget.Old as SolidColorBrush).Color.G;
        B.Value = (EditTarget.Old as SolidColorBrush).Color.B;
        var colorspots = parameters.GetValue<ColorSpots>("ColorSpots");
        ColorSpot0.Value = colorspots.ColorSpot0;
        ColorSpot1.Value = colorspots.ColorSpot1;
        ColorSpot2.Value = colorspots.ColorSpot2;
        ColorSpot3.Value = colorspots.ColorSpot3;
        ColorSpot4.Value = colorspots.ColorSpot4;
        ColorSpot5.Value = colorspots.ColorSpot5;
        ColorSpot6.Value = colorspots.ColorSpot6;
        ColorSpot7.Value = colorspots.ColorSpot7;
        ColorSpot8.Value = colorspots.ColorSpot8;
        ColorSpot9.Value = colorspots.ColorSpot9;
        ColorSpot10.Value = colorspots.ColorSpot10;
        ColorSpot11.Value = colorspots.ColorSpot11;
        ColorSpot12.Value = colorspots.ColorSpot12;
        ColorSpot13.Value = colorspots.ColorSpot13;
        ColorSpot14.Value = colorspots.ColorSpot14;
        ColorSpot15.Value = colorspots.ColorSpot15;
        ColorSpot16.Value = colorspots.ColorSpot16;
        ColorSpot17.Value = colorspots.ColorSpot17;
        ColorSpot18.Value = colorspots.ColorSpot18;
        ColorSpot19.Value = colorspots.ColorSpot19;
        ColorSpot20.Value = colorspots.ColorSpot20;
        ColorSpot21.Value = colorspots.ColorSpot21;
        ColorSpot22.Value = colorspots.ColorSpot22;
        ColorSpot23.Value = colorspots.ColorSpot23;
        ColorSpot24.Value = colorspots.ColorSpot24;
        ColorSpot25.Value = colorspots.ColorSpot25;
        ColorSpot26.Value = colorspots.ColorSpot26;
        ColorSpot27.Value = colorspots.ColorSpot27;
        ColorSpot28.Value = colorspots.ColorSpot28;
        ColorSpot29.Value = colorspots.ColorSpot29;
        ColorSpot30.Value = colorspots.ColorSpot30;
        ColorSpot31.Value = colorspots.ColorSpot31;
        ColorSpot32.Value = colorspots.ColorSpot32;
        ColorSpot33.Value = colorspots.ColorSpot33;
        ColorSpot34.Value = colorspots.ColorSpot34;
        ColorSpot35.Value = colorspots.ColorSpot35;
        ColorSpot36.Value = colorspots.ColorSpot36;
        ColorSpot37.Value = colorspots.ColorSpot37;
        ColorSpot38.Value = colorspots.ColorSpot38;
        ColorSpot39.Value = colorspots.ColorSpot39;
        ColorSpot40.Value = colorspots.ColorSpot40;
        ColorSpot41.Value = colorspots.ColorSpot41;
        ColorSpot42.Value = colorspots.ColorSpot42;
        ColorSpot43.Value = colorspots.ColorSpot43;
        ColorSpot44.Value = colorspots.ColorSpot44;
        ColorSpot45.Value = colorspots.ColorSpot45;
        ColorSpot46.Value = colorspots.ColorSpot46;
        ColorSpot47.Value = colorspots.ColorSpot47;
        ColorSpot48.Value = colorspots.ColorSpot48;
        ColorSpot49.Value = colorspots.ColorSpot49;
        ColorSpot50.Value = colorspots.ColorSpot50;
        ColorSpot51.Value = colorspots.ColorSpot51;
        ColorSpot52.Value = colorspots.ColorSpot52;
        ColorSpot53.Value = colorspots.ColorSpot53;
        ColorSpot54.Value = colorspots.ColorSpot54;
        ColorSpot55.Value = colorspots.ColorSpot55;
        ColorSpot56.Value = colorspots.ColorSpot56;
        ColorSpot57.Value = colorspots.ColorSpot57;
        ColorSpot58.Value = colorspots.ColorSpot58;
        ColorSpot59.Value = colorspots.ColorSpot59;
        ColorSpot60.Value = colorspots.ColorSpot60;
        ColorSpot61.Value = colorspots.ColorSpot61;
        ColorSpot62.Value = colorspots.ColorSpot62;
        ColorSpot63.Value = colorspots.ColorSpot63;
        ColorSpot64.Value = colorspots.ColorSpot64;
        ColorSpot65.Value = colorspots.ColorSpot65;
        ColorSpot66.Value = colorspots.ColorSpot66;
        ColorSpot67.Value = colorspots.ColorSpot67;
        ColorSpot68.Value = colorspots.ColorSpot68;
        ColorSpot69.Value = colorspots.ColorSpot69;
        ColorSpot70.Value = colorspots.ColorSpot70;
        ColorSpot71.Value = colorspots.ColorSpot71;
        ColorSpot72.Value = colorspots.ColorSpot72;
        ColorSpot73.Value = colorspots.ColorSpot73;
        ColorSpot74.Value = colorspots.ColorSpot74;
        ColorSpot75.Value = colorspots.ColorSpot75;
        ColorSpot76.Value = colorspots.ColorSpot76;
        ColorSpot77.Value = colorspots.ColorSpot77;
        ColorSpot78.Value = colorspots.ColorSpot78;
        ColorSpot79.Value = colorspots.ColorSpot79;
        ColorSpot80.Value = colorspots.ColorSpot80;
        ColorSpot81.Value = colorspots.ColorSpot81;
        ColorSpot82.Value = colorspots.ColorSpot82;
        ColorSpot83.Value = colorspots.ColorSpot83;
        ColorSpot84.Value = colorspots.ColorSpot84;
        ColorSpot85.Value = colorspots.ColorSpot85;
        ColorSpot86.Value = colorspots.ColorSpot86;
        ColorSpot87.Value = colorspots.ColorSpot87;
        ColorSpot88.Value = colorspots.ColorSpot88;
        ColorSpot89.Value = colorspots.ColorSpot89;
        ColorSpot90.Value = colorspots.ColorSpot90;
        ColorSpot91.Value = colorspots.ColorSpot91;
        ColorSpot92.Value = colorspots.ColorSpot92;
        ColorSpot93.Value = colorspots.ColorSpot93;
        ColorSpot94.Value = colorspots.ColorSpot94;
        ColorSpot95.Value = colorspots.ColorSpot95;
        ColorSpot96.Value = colorspots.ColorSpot96;
        ColorSpot97.Value = colorspots.ColorSpot97;
        ColorSpot98.Value = colorspots.ColorSpot98;
        ColorSpot99.Value = colorspots.ColorSpot99;
    }

    #region IDisposable

    public void Dispose()
    {
        _disposables.Dispose();
    }

    #endregion //IDisposable

    private void SetColorToSpot()
    {
        if (_spots != null && _flag)
            _spots.Where(x => x.IsSelected.Value)
                .ToList()
                .ForEach(x => { x.Brush = new SolidColorBrush(Output.Value); });
    }

    private void GenerateRedSelectorMat()
    {
        using (var bgrMat = new Mat(10, 255, MatType.CV_8UC3))
        {
            unsafe
            {
                var p = (byte*)bgrMat.Data.ToPointer();

                for (var y = 0; y < 10; ++y)
                {
                    var py = p + y * bgrMat.Step();

                    for (var x = 0; x < 255; ++x)
                    {
                        *(py + x * 3) = 0;
                        *(py + x * 3 + 1) = 0;
                        *(py + x * 3 + 2) = (byte)x;
                    }
                }
            }

            RedSelector = bgrMat.ToWriteableBitmap();
        }
    }

    private void GenerateGreenSelectorMat()
    {
        using (var bgrMat = new Mat(10, 255, MatType.CV_8UC3))
        {
            unsafe
            {
                var p = (byte*)bgrMat.Data.ToPointer();

                for (var y = 0; y < 10; ++y)
                {
                    var py = p + y * bgrMat.Step();

                    for (var x = 0; x < 255; ++x)
                    {
                        *(py + x * 3) = 0;
                        *(py + x * 3 + 1) = (byte)x;
                        *(py + x * 3 + 2) = 0;
                    }
                }
            }

            GreenSelector = bgrMat.ToWriteableBitmap();
        }
    }

    private void GenerateBlueSelectorMat()
    {
        using (var bgrMat = new Mat(10, 255, MatType.CV_8UC3))
        {
            unsafe
            {
                var p = (byte*)bgrMat.Data.ToPointer();

                for (var y = 0; y < 10; ++y)
                {
                    var py = p + y * bgrMat.Step();

                    for (var x = 0; x < 255; ++x)
                    {
                        *(py + x * 3) = (byte)x;
                        *(py + x * 3 + 1) = 0;
                        *(py + x * 3 + 2) = 0;
                    }
                }
            }

            BlueSelector = bgrMat.ToWriteableBitmap();
        }
    }

    private void GenerateASelectorMat()
    {
        using (var bgrMat = new Mat(10, 255, MatType.CV_8UC4))
        {
            unsafe
            {
                var p = (byte*)bgrMat.Data.ToPointer();

                for (var y = 0; y < 10; ++y)
                {
                    var py = p + y * bgrMat.Step();

                    for (var x = 0; x < 255; ++x)
                        if ((y + x) % 2 == 0)
                        {
                            *(py + x * 4) = B.Value;
                            *(py + x * 4 + 1) = G.Value;
                            *(py + x * 4 + 2) = R.Value;
                            *(py + x * 4 + 3) = (byte)x;
                        }
                        else
                        {
                            *(py + x * 4) = 255 / 2;
                            *(py + x * 4 + 1) = 255 / 2;
                            *(py + x * 4 + 2) = 255 / 2;
                            *(py + x * 4 + 3) = (byte)x;
                        }
                }
            }

            ASelector = bgrMat.ToWriteableBitmap();
        }
    }

    private void GenerateHueSelectorMat()
    {
        using (var hsvMat = new Mat(10, 180, MatType.CV_8UC3))
        {
            unsafe
            {
                var p = (byte*)hsvMat.Data.ToPointer();

                for (var y = 0; y < 10; ++y)
                {
                    var py = p + y * hsvMat.Step();

                    for (var x = 0; x < 180; ++x)
                    {
                        *(py + x * 3) = (byte)x; //hue
                        *(py + x * 3 + 1) = 255;
                        *(py + x * 3 + 2) = 255;
                    }
                }
            }

            using (var bgrMat = new Mat())
            {
                Cv2.CvtColor(hsvMat, bgrMat, ColorConversionCodes.HSV2BGR);
                HueSelector = bgrMat.ToWriteableBitmap();
            }
        }
    }

    private void GenerateSaturationValueMat()
    {
        using (var hsvMat = new Mat(256, 256, MatType.CV_8UC3))
        {
            unsafe
            {
                var p = (byte*)hsvMat.Data.ToPointer();

                for (var y = 0; y < 256; ++y)
                {
                    var py = p + y * hsvMat.Step();

                    for (var x = 0; x < 256; ++x)
                    {
                        *(py + x * 3) = Hue.Value;
                        *(py + x * 3 + 1) = (byte)x; //saturation
                        *(py + x * 3 + 2) = (byte)(255 - y); //value
                    }
                }
            }

            using (var bgrMat = new Mat())
            {
                Cv2.CvtColor(hsvMat, bgrMat, ColorConversionCodes.HSV2BGR);
                WhiteBlackColorMap = bgrMat.ToWriteableBitmap();
            }
        }
    }

    private void RecalcHue()
    {
        var r = R.Value / 255d;
        var g = G.Value / 255d;
        var b = B.Value / 255d;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        if (max == 0 || max == min) return;

        var hue = 0d;

        if (r == max)
            hue = (g - b) / (max - min);
        else if (g == max)
            hue = 2.0 + (b - r) / (max - min);
        else if (b == max) hue = 4.0 + (r - g) / (max - min);

        hue *= 60d;
        if (hue < 0d) hue = hue + 360d;

        hue /= 2d;

        Hue.Value = (byte)Math.Round(hue);
    }

    private void RecalcSaturation()
    {
        if (Value.Value == 0)
        {
            Saturation.Value = 0;
        }
        else
        {
            var r = R.Value / 255d;
            var g = G.Value / 255d;
            var b = B.Value / 255d;
            var v = Value.Value / 255d;

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));

            var saturation = (max - min) / v;

            Saturation.Value = (byte)Math.Round(saturation * 255d);
        }
    }

    private void RecalcValue()
    {
        Value.Value = Math.Max(R.Value, Math.Max(G.Value, B.Value));
    }

    private void SetRGB()
    {
        HSV2RGB();
    }

    private void HSV2RGB()
    {
        var hue = Hue.Value * 2d;
        var saturation = Saturation.Value / 255d;
        var value = Value.Value / 255d;

        var C = value * saturation;
        var X = C * (1 - Math.Abs(hue / 60d % 2d - 1d));
        var m = value - C;

        double _r = 0, _g = 0, _b = 0;
        if ((0 <= hue && hue < 60) || hue == 360)
        {
            _r = C;
            _g = X;
            _b = 0;
        }
        else if (60 <= hue && hue < 120)
        {
            _r = X;
            _g = C;
            _b = 0;
        }
        else if (120 <= hue && hue < 180)
        {
            _r = 0;
            _g = C;
            _b = X;
        }
        else if (180 <= hue && hue < 240)
        {
            _r = 0;
            _g = X;
            _b = C;
        }
        else if (240 <= hue && hue < 300)
        {
            _r = X;
            _g = 0;
            _b = C;
        }
        else if (300 <= hue && hue < 360)
        {
            _r = C;
            _g = 0;
            _b = X;
        }

        R.Value = (byte)((_r + m) * 255d);
        G.Value = (byte)((_g + m) * 255d);
        B.Value = (byte)((_b + m) * 255d);
    }
}