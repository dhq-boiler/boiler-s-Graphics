using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels;

public class ColorPickerViewModel : BindableBase, IDialogAware, IDisposable
{
    private readonly IRegionManager regionManager;
    private ColorPicker _colorPicker;
    private readonly CompositeDisposable disposables = new();
    private bool disposedValue;

    public ColorPickerViewModel(IRegionManager regionManager)
    {
        LoadedCommand.Subscribe(x =>
            {
                var source = x.Source;
                _colorPicker = source as ColorPicker;
            })
            .AddTo(disposables);
        UnloadedCommand.Subscribe(x => { this.regionManager.Regions.Remove("ColorPickerRegion"); })
            .AddTo(disposables);
        OKCommand.Subscribe(_ =>
            {
                this.regionManager.Regions.Remove("ColorPickerRegion");
                var solidColorPicker = _colorPicker.EnumVisualChildren<SolidColorPicker>().FirstOrDefault();
                if (solidColorPicker != null)
                {
                    var solidColorPickerViewModel = solidColorPicker.DataContext as SolidColorPickerViewModel;
                    var colorSpots = new ColorSpots
                    {
                        ColorSpot0 = solidColorPickerViewModel.ColorSpot0.Value,
                        ColorSpot1 = solidColorPickerViewModel.ColorSpot1.Value,
                        ColorSpot2 = solidColorPickerViewModel.ColorSpot2.Value,
                        ColorSpot3 = solidColorPickerViewModel.ColorSpot3.Value,
                        ColorSpot4 = solidColorPickerViewModel.ColorSpot4.Value,
                        ColorSpot5 = solidColorPickerViewModel.ColorSpot5.Value,
                        ColorSpot6 = solidColorPickerViewModel.ColorSpot6.Value,
                        ColorSpot7 = solidColorPickerViewModel.ColorSpot7.Value,
                        ColorSpot8 = solidColorPickerViewModel.ColorSpot8.Value,
                        ColorSpot9 = solidColorPickerViewModel.ColorSpot9.Value,
                        ColorSpot10 = solidColorPickerViewModel.ColorSpot10.Value,
                        ColorSpot11 = solidColorPickerViewModel.ColorSpot11.Value,
                        ColorSpot12 = solidColorPickerViewModel.ColorSpot12.Value,
                        ColorSpot13 = solidColorPickerViewModel.ColorSpot13.Value,
                        ColorSpot14 = solidColorPickerViewModel.ColorSpot14.Value,
                        ColorSpot15 = solidColorPickerViewModel.ColorSpot15.Value,
                        ColorSpot16 = solidColorPickerViewModel.ColorSpot16.Value,
                        ColorSpot17 = solidColorPickerViewModel.ColorSpot17.Value,
                        ColorSpot18 = solidColorPickerViewModel.ColorSpot18.Value,
                        ColorSpot19 = solidColorPickerViewModel.ColorSpot19.Value,
                        ColorSpot20 = solidColorPickerViewModel.ColorSpot20.Value,
                        ColorSpot21 = solidColorPickerViewModel.ColorSpot21.Value,
                        ColorSpot22 = solidColorPickerViewModel.ColorSpot22.Value,
                        ColorSpot23 = solidColorPickerViewModel.ColorSpot23.Value,
                        ColorSpot24 = solidColorPickerViewModel.ColorSpot24.Value,
                        ColorSpot25 = solidColorPickerViewModel.ColorSpot25.Value,
                        ColorSpot26 = solidColorPickerViewModel.ColorSpot26.Value,
                        ColorSpot27 = solidColorPickerViewModel.ColorSpot27.Value,
                        ColorSpot28 = solidColorPickerViewModel.ColorSpot28.Value,
                        ColorSpot29 = solidColorPickerViewModel.ColorSpot29.Value,
                        ColorSpot30 = solidColorPickerViewModel.ColorSpot30.Value,
                        ColorSpot31 = solidColorPickerViewModel.ColorSpot31.Value,
                        ColorSpot32 = solidColorPickerViewModel.ColorSpot32.Value,
                        ColorSpot33 = solidColorPickerViewModel.ColorSpot33.Value,
                        ColorSpot34 = solidColorPickerViewModel.ColorSpot34.Value,
                        ColorSpot35 = solidColorPickerViewModel.ColorSpot35.Value,
                        ColorSpot36 = solidColorPickerViewModel.ColorSpot36.Value,
                        ColorSpot37 = solidColorPickerViewModel.ColorSpot37.Value,
                        ColorSpot38 = solidColorPickerViewModel.ColorSpot38.Value,
                        ColorSpot39 = solidColorPickerViewModel.ColorSpot39.Value,
                        ColorSpot40 = solidColorPickerViewModel.ColorSpot40.Value,
                        ColorSpot41 = solidColorPickerViewModel.ColorSpot41.Value,
                        ColorSpot42 = solidColorPickerViewModel.ColorSpot42.Value,
                        ColorSpot43 = solidColorPickerViewModel.ColorSpot43.Value,
                        ColorSpot44 = solidColorPickerViewModel.ColorSpot44.Value,
                        ColorSpot45 = solidColorPickerViewModel.ColorSpot45.Value,
                        ColorSpot46 = solidColorPickerViewModel.ColorSpot46.Value,
                        ColorSpot47 = solidColorPickerViewModel.ColorSpot47.Value,
                        ColorSpot48 = solidColorPickerViewModel.ColorSpot48.Value,
                        ColorSpot49 = solidColorPickerViewModel.ColorSpot49.Value,
                        ColorSpot50 = solidColorPickerViewModel.ColorSpot50.Value,
                        ColorSpot51 = solidColorPickerViewModel.ColorSpot51.Value,
                        ColorSpot52 = solidColorPickerViewModel.ColorSpot52.Value,
                        ColorSpot53 = solidColorPickerViewModel.ColorSpot53.Value,
                        ColorSpot54 = solidColorPickerViewModel.ColorSpot54.Value,
                        ColorSpot55 = solidColorPickerViewModel.ColorSpot55.Value,
                        ColorSpot56 = solidColorPickerViewModel.ColorSpot56.Value,
                        ColorSpot57 = solidColorPickerViewModel.ColorSpot57.Value,
                        ColorSpot58 = solidColorPickerViewModel.ColorSpot58.Value,
                        ColorSpot59 = solidColorPickerViewModel.ColorSpot59.Value,
                        ColorSpot60 = solidColorPickerViewModel.ColorSpot60.Value,
                        ColorSpot61 = solidColorPickerViewModel.ColorSpot61.Value,
                        ColorSpot62 = solidColorPickerViewModel.ColorSpot62.Value,
                        ColorSpot63 = solidColorPickerViewModel.ColorSpot63.Value,
                        ColorSpot64 = solidColorPickerViewModel.ColorSpot64.Value,
                        ColorSpot65 = solidColorPickerViewModel.ColorSpot65.Value,
                        ColorSpot66 = solidColorPickerViewModel.ColorSpot66.Value,
                        ColorSpot67 = solidColorPickerViewModel.ColorSpot67.Value,
                        ColorSpot68 = solidColorPickerViewModel.ColorSpot68.Value,
                        ColorSpot69 = solidColorPickerViewModel.ColorSpot69.Value,
                        ColorSpot70 = solidColorPickerViewModel.ColorSpot70.Value,
                        ColorSpot71 = solidColorPickerViewModel.ColorSpot71.Value,
                        ColorSpot72 = solidColorPickerViewModel.ColorSpot72.Value,
                        ColorSpot73 = solidColorPickerViewModel.ColorSpot73.Value,
                        ColorSpot74 = solidColorPickerViewModel.ColorSpot74.Value,
                        ColorSpot75 = solidColorPickerViewModel.ColorSpot75.Value,
                        ColorSpot76 = solidColorPickerViewModel.ColorSpot76.Value,
                        ColorSpot77 = solidColorPickerViewModel.ColorSpot77.Value,
                        ColorSpot78 = solidColorPickerViewModel.ColorSpot78.Value,
                        ColorSpot79 = solidColorPickerViewModel.ColorSpot79.Value,
                        ColorSpot80 = solidColorPickerViewModel.ColorSpot80.Value,
                        ColorSpot81 = solidColorPickerViewModel.ColorSpot81.Value,
                        ColorSpot82 = solidColorPickerViewModel.ColorSpot82.Value,
                        ColorSpot83 = solidColorPickerViewModel.ColorSpot83.Value,
                        ColorSpot84 = solidColorPickerViewModel.ColorSpot84.Value,
                        ColorSpot85 = solidColorPickerViewModel.ColorSpot85.Value,
                        ColorSpot86 = solidColorPickerViewModel.ColorSpot86.Value,
                        ColorSpot87 = solidColorPickerViewModel.ColorSpot87.Value,
                        ColorSpot88 = solidColorPickerViewModel.ColorSpot88.Value,
                        ColorSpot89 = solidColorPickerViewModel.ColorSpot89.Value,
                        ColorSpot90 = solidColorPickerViewModel.ColorSpot90.Value,
                        ColorSpot91 = solidColorPickerViewModel.ColorSpot91.Value,
                        ColorSpot92 = solidColorPickerViewModel.ColorSpot92.Value,
                        ColorSpot93 = solidColorPickerViewModel.ColorSpot93.Value,
                        ColorSpot94 = solidColorPickerViewModel.ColorSpot94.Value,
                        ColorSpot95 = solidColorPickerViewModel.ColorSpot95.Value,
                        ColorSpot96 = solidColorPickerViewModel.ColorSpot96.Value,
                        ColorSpot97 = solidColorPickerViewModel.ColorSpot97.Value,
                        ColorSpot98 = solidColorPickerViewModel.ColorSpot98.Value,
                        ColorSpot99 = solidColorPickerViewModel.ColorSpot99.Value
                    };
                    var parameters = new DialogParameters
                    {
                        { "ColorExchange", solidColorPickerViewModel.EditTarget.Value },
                        { "ColorSpots", colorSpots }
                    };
                    var ret = new DialogResult(ButtonResult.OK, parameters);
                    RequestClose.Invoke(ret);
                }

                var linearGradientBrushPicker =
                    _colorPicker.EnumVisualChildren<LinearGradientBrushPicker>().FirstOrDefault();
                if (linearGradientBrushPicker != null)
                {
                    var linearGradientBrushPickerViewModel =
                        linearGradientBrushPicker.DataContext as LinearGradientBrushPickerViewModel;
                    var colorSpots = new ColorSpots
                    {
                        ColorSpot0 = linearGradientBrushPickerViewModel.ColorSpot0.Value,
                        ColorSpot1 = linearGradientBrushPickerViewModel.ColorSpot1.Value,
                        ColorSpot2 = linearGradientBrushPickerViewModel.ColorSpot2.Value,
                        ColorSpot3 = linearGradientBrushPickerViewModel.ColorSpot3.Value,
                        ColorSpot4 = linearGradientBrushPickerViewModel.ColorSpot4.Value,
                        ColorSpot5 = linearGradientBrushPickerViewModel.ColorSpot5.Value,
                        ColorSpot6 = linearGradientBrushPickerViewModel.ColorSpot6.Value,
                        ColorSpot7 = linearGradientBrushPickerViewModel.ColorSpot7.Value,
                        ColorSpot8 = linearGradientBrushPickerViewModel.ColorSpot8.Value,
                        ColorSpot9 = linearGradientBrushPickerViewModel.ColorSpot9.Value,
                        ColorSpot10 = linearGradientBrushPickerViewModel.ColorSpot10.Value,
                        ColorSpot11 = linearGradientBrushPickerViewModel.ColorSpot11.Value,
                        ColorSpot12 = linearGradientBrushPickerViewModel.ColorSpot12.Value,
                        ColorSpot13 = linearGradientBrushPickerViewModel.ColorSpot13.Value,
                        ColorSpot14 = linearGradientBrushPickerViewModel.ColorSpot14.Value,
                        ColorSpot15 = linearGradientBrushPickerViewModel.ColorSpot15.Value,
                        ColorSpot16 = linearGradientBrushPickerViewModel.ColorSpot16.Value,
                        ColorSpot17 = linearGradientBrushPickerViewModel.ColorSpot17.Value,
                        ColorSpot18 = linearGradientBrushPickerViewModel.ColorSpot18.Value,
                        ColorSpot19 = linearGradientBrushPickerViewModel.ColorSpot19.Value,
                        ColorSpot20 = linearGradientBrushPickerViewModel.ColorSpot20.Value,
                        ColorSpot21 = linearGradientBrushPickerViewModel.ColorSpot21.Value,
                        ColorSpot22 = linearGradientBrushPickerViewModel.ColorSpot22.Value,
                        ColorSpot23 = linearGradientBrushPickerViewModel.ColorSpot23.Value,
                        ColorSpot24 = linearGradientBrushPickerViewModel.ColorSpot24.Value,
                        ColorSpot25 = linearGradientBrushPickerViewModel.ColorSpot25.Value,
                        ColorSpot26 = linearGradientBrushPickerViewModel.ColorSpot26.Value,
                        ColorSpot27 = linearGradientBrushPickerViewModel.ColorSpot27.Value,
                        ColorSpot28 = linearGradientBrushPickerViewModel.ColorSpot28.Value,
                        ColorSpot29 = linearGradientBrushPickerViewModel.ColorSpot29.Value,
                        ColorSpot30 = linearGradientBrushPickerViewModel.ColorSpot30.Value,
                        ColorSpot31 = linearGradientBrushPickerViewModel.ColorSpot31.Value,
                        ColorSpot32 = linearGradientBrushPickerViewModel.ColorSpot32.Value,
                        ColorSpot33 = linearGradientBrushPickerViewModel.ColorSpot33.Value,
                        ColorSpot34 = linearGradientBrushPickerViewModel.ColorSpot34.Value,
                        ColorSpot35 = linearGradientBrushPickerViewModel.ColorSpot35.Value,
                        ColorSpot36 = linearGradientBrushPickerViewModel.ColorSpot36.Value,
                        ColorSpot37 = linearGradientBrushPickerViewModel.ColorSpot37.Value,
                        ColorSpot38 = linearGradientBrushPickerViewModel.ColorSpot38.Value,
                        ColorSpot39 = linearGradientBrushPickerViewModel.ColorSpot39.Value,
                        ColorSpot40 = linearGradientBrushPickerViewModel.ColorSpot40.Value,
                        ColorSpot41 = linearGradientBrushPickerViewModel.ColorSpot41.Value,
                        ColorSpot42 = linearGradientBrushPickerViewModel.ColorSpot42.Value,
                        ColorSpot43 = linearGradientBrushPickerViewModel.ColorSpot43.Value,
                        ColorSpot44 = linearGradientBrushPickerViewModel.ColorSpot44.Value,
                        ColorSpot45 = linearGradientBrushPickerViewModel.ColorSpot45.Value,
                        ColorSpot46 = linearGradientBrushPickerViewModel.ColorSpot46.Value,
                        ColorSpot47 = linearGradientBrushPickerViewModel.ColorSpot47.Value,
                        ColorSpot48 = linearGradientBrushPickerViewModel.ColorSpot48.Value,
                        ColorSpot49 = linearGradientBrushPickerViewModel.ColorSpot49.Value,
                        ColorSpot50 = linearGradientBrushPickerViewModel.ColorSpot50.Value,
                        ColorSpot51 = linearGradientBrushPickerViewModel.ColorSpot51.Value,
                        ColorSpot52 = linearGradientBrushPickerViewModel.ColorSpot52.Value,
                        ColorSpot53 = linearGradientBrushPickerViewModel.ColorSpot53.Value,
                        ColorSpot54 = linearGradientBrushPickerViewModel.ColorSpot54.Value,
                        ColorSpot55 = linearGradientBrushPickerViewModel.ColorSpot55.Value,
                        ColorSpot56 = linearGradientBrushPickerViewModel.ColorSpot56.Value,
                        ColorSpot57 = linearGradientBrushPickerViewModel.ColorSpot57.Value,
                        ColorSpot58 = linearGradientBrushPickerViewModel.ColorSpot58.Value,
                        ColorSpot59 = linearGradientBrushPickerViewModel.ColorSpot59.Value,
                        ColorSpot60 = linearGradientBrushPickerViewModel.ColorSpot60.Value,
                        ColorSpot61 = linearGradientBrushPickerViewModel.ColorSpot61.Value,
                        ColorSpot62 = linearGradientBrushPickerViewModel.ColorSpot62.Value,
                        ColorSpot63 = linearGradientBrushPickerViewModel.ColorSpot63.Value,
                        ColorSpot64 = linearGradientBrushPickerViewModel.ColorSpot64.Value,
                        ColorSpot65 = linearGradientBrushPickerViewModel.ColorSpot65.Value,
                        ColorSpot66 = linearGradientBrushPickerViewModel.ColorSpot66.Value,
                        ColorSpot67 = linearGradientBrushPickerViewModel.ColorSpot67.Value,
                        ColorSpot68 = linearGradientBrushPickerViewModel.ColorSpot68.Value,
                        ColorSpot69 = linearGradientBrushPickerViewModel.ColorSpot69.Value,
                        ColorSpot70 = linearGradientBrushPickerViewModel.ColorSpot70.Value,
                        ColorSpot71 = linearGradientBrushPickerViewModel.ColorSpot71.Value,
                        ColorSpot72 = linearGradientBrushPickerViewModel.ColorSpot72.Value,
                        ColorSpot73 = linearGradientBrushPickerViewModel.ColorSpot73.Value,
                        ColorSpot74 = linearGradientBrushPickerViewModel.ColorSpot74.Value,
                        ColorSpot75 = linearGradientBrushPickerViewModel.ColorSpot75.Value,
                        ColorSpot76 = linearGradientBrushPickerViewModel.ColorSpot76.Value,
                        ColorSpot77 = linearGradientBrushPickerViewModel.ColorSpot77.Value,
                        ColorSpot78 = linearGradientBrushPickerViewModel.ColorSpot78.Value,
                        ColorSpot79 = linearGradientBrushPickerViewModel.ColorSpot79.Value,
                        ColorSpot80 = linearGradientBrushPickerViewModel.ColorSpot80.Value,
                        ColorSpot81 = linearGradientBrushPickerViewModel.ColorSpot81.Value,
                        ColorSpot82 = linearGradientBrushPickerViewModel.ColorSpot82.Value,
                        ColorSpot83 = linearGradientBrushPickerViewModel.ColorSpot83.Value,
                        ColorSpot84 = linearGradientBrushPickerViewModel.ColorSpot84.Value,
                        ColorSpot85 = linearGradientBrushPickerViewModel.ColorSpot85.Value,
                        ColorSpot86 = linearGradientBrushPickerViewModel.ColorSpot86.Value,
                        ColorSpot87 = linearGradientBrushPickerViewModel.ColorSpot87.Value,
                        ColorSpot88 = linearGradientBrushPickerViewModel.ColorSpot88.Value,
                        ColorSpot89 = linearGradientBrushPickerViewModel.ColorSpot89.Value,
                        ColorSpot90 = linearGradientBrushPickerViewModel.ColorSpot90.Value,
                        ColorSpot91 = linearGradientBrushPickerViewModel.ColorSpot91.Value,
                        ColorSpot92 = linearGradientBrushPickerViewModel.ColorSpot92.Value,
                        ColorSpot93 = linearGradientBrushPickerViewModel.ColorSpot93.Value,
                        ColorSpot94 = linearGradientBrushPickerViewModel.ColorSpot94.Value,
                        ColorSpot95 = linearGradientBrushPickerViewModel.ColorSpot95.Value,
                        ColorSpot96 = linearGradientBrushPickerViewModel.ColorSpot96.Value,
                        ColorSpot97 = linearGradientBrushPickerViewModel.ColorSpot97.Value,
                        ColorSpot98 = linearGradientBrushPickerViewModel.ColorSpot98.Value,
                        ColorSpot99 = linearGradientBrushPickerViewModel.ColorSpot99.Value
                    };
                    var gradientStopCollection = new GradientStopCollection();
                    linearGradientBrushPickerViewModel.GradientStops.ToList()
                        .ForEach(x => gradientStopCollection.Add(x.ConvertToGradientStop()));
                    linearGradientBrushPickerViewModel.EditTarget.Value.New = new LinearGradientBrush(
                        gradientStopCollection, linearGradientBrushPickerViewModel.StartPoint.Value,
                        linearGradientBrushPickerViewModel.EndPoint.Value);
                    linearGradientBrushPickerViewModel.EditTarget.Value.Old = EditTarget.Value.Old;
                    var parameters = new DialogParameters
                    {
                        { "ColorExchange", linearGradientBrushPickerViewModel.EditTarget.Value },
                        { "ColorSpots", colorSpots }
                    };
                    var ret = new DialogResult(ButtonResult.OK, parameters);
                    RequestClose.Invoke(ret);
                }

                var radialGradientBrushPicker =
                    _colorPicker.EnumVisualChildren<RadialGradientBrushPicker>().FirstOrDefault();
                if (radialGradientBrushPicker != null)
                {
                    var radialGradientBrushPickerViewModel =
                        radialGradientBrushPicker.DataContext as RadialGradientBrushPickerViewModel;
                    var colorSpots = new ColorSpots
                    {
                        ColorSpot0 = radialGradientBrushPickerViewModel.ColorSpot0.Value,
                        ColorSpot1 = radialGradientBrushPickerViewModel.ColorSpot1.Value,
                        ColorSpot2 = radialGradientBrushPickerViewModel.ColorSpot2.Value,
                        ColorSpot3 = radialGradientBrushPickerViewModel.ColorSpot3.Value,
                        ColorSpot4 = radialGradientBrushPickerViewModel.ColorSpot4.Value,
                        ColorSpot5 = radialGradientBrushPickerViewModel.ColorSpot5.Value,
                        ColorSpot6 = radialGradientBrushPickerViewModel.ColorSpot6.Value,
                        ColorSpot7 = radialGradientBrushPickerViewModel.ColorSpot7.Value,
                        ColorSpot8 = radialGradientBrushPickerViewModel.ColorSpot8.Value,
                        ColorSpot9 = radialGradientBrushPickerViewModel.ColorSpot9.Value,
                        ColorSpot10 = radialGradientBrushPickerViewModel.ColorSpot10.Value,
                        ColorSpot11 = radialGradientBrushPickerViewModel.ColorSpot11.Value,
                        ColorSpot12 = radialGradientBrushPickerViewModel.ColorSpot12.Value,
                        ColorSpot13 = radialGradientBrushPickerViewModel.ColorSpot13.Value,
                        ColorSpot14 = radialGradientBrushPickerViewModel.ColorSpot14.Value,
                        ColorSpot15 = radialGradientBrushPickerViewModel.ColorSpot15.Value,
                        ColorSpot16 = radialGradientBrushPickerViewModel.ColorSpot16.Value,
                        ColorSpot17 = radialGradientBrushPickerViewModel.ColorSpot17.Value,
                        ColorSpot18 = radialGradientBrushPickerViewModel.ColorSpot18.Value,
                        ColorSpot19 = radialGradientBrushPickerViewModel.ColorSpot19.Value,
                        ColorSpot20 = radialGradientBrushPickerViewModel.ColorSpot20.Value,
                        ColorSpot21 = radialGradientBrushPickerViewModel.ColorSpot21.Value,
                        ColorSpot22 = radialGradientBrushPickerViewModel.ColorSpot22.Value,
                        ColorSpot23 = radialGradientBrushPickerViewModel.ColorSpot23.Value,
                        ColorSpot24 = radialGradientBrushPickerViewModel.ColorSpot24.Value,
                        ColorSpot25 = radialGradientBrushPickerViewModel.ColorSpot25.Value,
                        ColorSpot26 = radialGradientBrushPickerViewModel.ColorSpot26.Value,
                        ColorSpot27 = radialGradientBrushPickerViewModel.ColorSpot27.Value,
                        ColorSpot28 = radialGradientBrushPickerViewModel.ColorSpot28.Value,
                        ColorSpot29 = radialGradientBrushPickerViewModel.ColorSpot29.Value,
                        ColorSpot30 = radialGradientBrushPickerViewModel.ColorSpot30.Value,
                        ColorSpot31 = radialGradientBrushPickerViewModel.ColorSpot31.Value,
                        ColorSpot32 = radialGradientBrushPickerViewModel.ColorSpot32.Value,
                        ColorSpot33 = radialGradientBrushPickerViewModel.ColorSpot33.Value,
                        ColorSpot34 = radialGradientBrushPickerViewModel.ColorSpot34.Value,
                        ColorSpot35 = radialGradientBrushPickerViewModel.ColorSpot35.Value,
                        ColorSpot36 = radialGradientBrushPickerViewModel.ColorSpot36.Value,
                        ColorSpot37 = radialGradientBrushPickerViewModel.ColorSpot37.Value,
                        ColorSpot38 = radialGradientBrushPickerViewModel.ColorSpot38.Value,
                        ColorSpot39 = radialGradientBrushPickerViewModel.ColorSpot39.Value,
                        ColorSpot40 = radialGradientBrushPickerViewModel.ColorSpot40.Value,
                        ColorSpot41 = radialGradientBrushPickerViewModel.ColorSpot41.Value,
                        ColorSpot42 = radialGradientBrushPickerViewModel.ColorSpot42.Value,
                        ColorSpot43 = radialGradientBrushPickerViewModel.ColorSpot43.Value,
                        ColorSpot44 = radialGradientBrushPickerViewModel.ColorSpot44.Value,
                        ColorSpot45 = radialGradientBrushPickerViewModel.ColorSpot45.Value,
                        ColorSpot46 = radialGradientBrushPickerViewModel.ColorSpot46.Value,
                        ColorSpot47 = radialGradientBrushPickerViewModel.ColorSpot47.Value,
                        ColorSpot48 = radialGradientBrushPickerViewModel.ColorSpot48.Value,
                        ColorSpot49 = radialGradientBrushPickerViewModel.ColorSpot49.Value,
                        ColorSpot50 = radialGradientBrushPickerViewModel.ColorSpot50.Value,
                        ColorSpot51 = radialGradientBrushPickerViewModel.ColorSpot51.Value,
                        ColorSpot52 = radialGradientBrushPickerViewModel.ColorSpot52.Value,
                        ColorSpot53 = radialGradientBrushPickerViewModel.ColorSpot53.Value,
                        ColorSpot54 = radialGradientBrushPickerViewModel.ColorSpot54.Value,
                        ColorSpot55 = radialGradientBrushPickerViewModel.ColorSpot55.Value,
                        ColorSpot56 = radialGradientBrushPickerViewModel.ColorSpot56.Value,
                        ColorSpot57 = radialGradientBrushPickerViewModel.ColorSpot57.Value,
                        ColorSpot58 = radialGradientBrushPickerViewModel.ColorSpot58.Value,
                        ColorSpot59 = radialGradientBrushPickerViewModel.ColorSpot59.Value,
                        ColorSpot60 = radialGradientBrushPickerViewModel.ColorSpot60.Value,
                        ColorSpot61 = radialGradientBrushPickerViewModel.ColorSpot61.Value,
                        ColorSpot62 = radialGradientBrushPickerViewModel.ColorSpot62.Value,
                        ColorSpot63 = radialGradientBrushPickerViewModel.ColorSpot63.Value,
                        ColorSpot64 = radialGradientBrushPickerViewModel.ColorSpot64.Value,
                        ColorSpot65 = radialGradientBrushPickerViewModel.ColorSpot65.Value,
                        ColorSpot66 = radialGradientBrushPickerViewModel.ColorSpot66.Value,
                        ColorSpot67 = radialGradientBrushPickerViewModel.ColorSpot67.Value,
                        ColorSpot68 = radialGradientBrushPickerViewModel.ColorSpot68.Value,
                        ColorSpot69 = radialGradientBrushPickerViewModel.ColorSpot69.Value,
                        ColorSpot70 = radialGradientBrushPickerViewModel.ColorSpot70.Value,
                        ColorSpot71 = radialGradientBrushPickerViewModel.ColorSpot71.Value,
                        ColorSpot72 = radialGradientBrushPickerViewModel.ColorSpot72.Value,
                        ColorSpot73 = radialGradientBrushPickerViewModel.ColorSpot73.Value,
                        ColorSpot74 = radialGradientBrushPickerViewModel.ColorSpot74.Value,
                        ColorSpot75 = radialGradientBrushPickerViewModel.ColorSpot75.Value,
                        ColorSpot76 = radialGradientBrushPickerViewModel.ColorSpot76.Value,
                        ColorSpot77 = radialGradientBrushPickerViewModel.ColorSpot77.Value,
                        ColorSpot78 = radialGradientBrushPickerViewModel.ColorSpot78.Value,
                        ColorSpot79 = radialGradientBrushPickerViewModel.ColorSpot79.Value,
                        ColorSpot80 = radialGradientBrushPickerViewModel.ColorSpot80.Value,
                        ColorSpot81 = radialGradientBrushPickerViewModel.ColorSpot81.Value,
                        ColorSpot82 = radialGradientBrushPickerViewModel.ColorSpot82.Value,
                        ColorSpot83 = radialGradientBrushPickerViewModel.ColorSpot83.Value,
                        ColorSpot84 = radialGradientBrushPickerViewModel.ColorSpot84.Value,
                        ColorSpot85 = radialGradientBrushPickerViewModel.ColorSpot85.Value,
                        ColorSpot86 = radialGradientBrushPickerViewModel.ColorSpot86.Value,
                        ColorSpot87 = radialGradientBrushPickerViewModel.ColorSpot87.Value,
                        ColorSpot88 = radialGradientBrushPickerViewModel.ColorSpot88.Value,
                        ColorSpot89 = radialGradientBrushPickerViewModel.ColorSpot89.Value,
                        ColorSpot90 = radialGradientBrushPickerViewModel.ColorSpot90.Value,
                        ColorSpot91 = radialGradientBrushPickerViewModel.ColorSpot91.Value,
                        ColorSpot92 = radialGradientBrushPickerViewModel.ColorSpot92.Value,
                        ColorSpot93 = radialGradientBrushPickerViewModel.ColorSpot93.Value,
                        ColorSpot94 = radialGradientBrushPickerViewModel.ColorSpot94.Value,
                        ColorSpot95 = radialGradientBrushPickerViewModel.ColorSpot95.Value,
                        ColorSpot96 = radialGradientBrushPickerViewModel.ColorSpot96.Value,
                        ColorSpot97 = radialGradientBrushPickerViewModel.ColorSpot97.Value,
                        ColorSpot98 = radialGradientBrushPickerViewModel.ColorSpot98.Value,
                        ColorSpot99 = radialGradientBrushPickerViewModel.ColorSpot99.Value
                    };
                    var gradientStopCollection = new GradientStopCollection();
                    radialGradientBrushPickerViewModel.GradientStops.ToList()
                        .ForEach(x => gradientStopCollection.Add(x.ConvertToGradientStop()));
                    var brush = new RadialGradientBrush(gradientStopCollection)
                    {
                        GradientOrigin = radialGradientBrushPickerViewModel.GradientOrigin.Value,
                        Center = radialGradientBrushPickerViewModel.Center.Value,
                        RadiusX = radialGradientBrushPickerViewModel.RadiusX.Value,
                        RadiusY = radialGradientBrushPickerViewModel.RadiusY.Value
                    };
                    radialGradientBrushPickerViewModel.EditTarget.Value.New = brush;
                    radialGradientBrushPickerViewModel.EditTarget.Value.Old = EditTarget.Value.Old;
                    var parameters = new DialogParameters
                    {
                        { "ColorExchange", radialGradientBrushPickerViewModel.EditTarget.Value },
                        { "ColorSpots", colorSpots }
                    };
                    var ret = new DialogResult(ButtonResult.OK, parameters);
                    RequestClose.Invoke(ret);
                }
            })
            .AddTo(disposables);
        this.regionManager = regionManager;
    }

    public ReactiveCommand OKCommand { get; } = new();
    public ReactiveCommand SelectSolidColorCommand { get; } = new();
    public ReactiveCommand SelectLinearGradientCommand { get; } = new();
    public ReactiveCommand SelectRadialGradientCommand { get; } = new();
    public ReactivePropertySlim<ColorExchange> EditTarget { get; } = new();
    public ReactivePropertySlim<ColorSpots> ColorSpots { get; } = new();

    public ReactiveCommand<RoutedEventArgs> UnloadedCommand { get; } = new();
    public ReactiveCommand<RoutedEventArgs> LoadedCommand { get; } = new();

    public string Title => "カラーピッカー";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        EditTarget.Value = parameters.GetValue<ColorExchange>("ColorExchange");
        ColorSpots.Value = parameters.GetValue<ColorSpots>("ColorSpots");

        if (EditTarget.Value.Old is SolidColorBrush scb)
            regionManager.RequestNavigate("ColorPickerRegion", nameof(SolidColorPicker), new NavigationParameters
            {
                { "ColorExchange", EditTarget.Value },
                { "ColorSpots", ColorSpots.Value }
            });
        else if (EditTarget.Value.Old is LinearGradientBrush lgb)
            regionManager.RequestNavigate("ColorPickerRegion", nameof(LinearGradientBrushPicker),
                new NavigationParameters
                {
                    { "ColorExchange", EditTarget.Value },
                    { "ColorSpots", ColorSpots.Value }
                });
        else if (EditTarget.Value.Old is RadialGradientBrush rgb)
            regionManager.RequestNavigate("ColorPickerRegion", nameof(RadialGradientBrushPicker),
                new NavigationParameters
                {
                    { "ColorExchange", EditTarget.Value },
                    { "ColorSpots", ColorSpots.Value }
                });

        SelectSolidColorCommand.Subscribe(_ =>
            {
                regionManager.RequestNavigate("ColorPickerRegion", nameof(SolidColorPicker), new NavigationParameters
                {
                    { "ColorExchange", EditTarget.Value },
                    { "ColorSpots", ColorSpots.Value }
                });
            })
            .AddTo(disposables);
        SelectLinearGradientCommand.Subscribe(_ =>
            {
                regionManager.RequestNavigate("ColorPickerRegion", nameof(LinearGradientBrushPicker),
                    new NavigationParameters
                    {
                        { "ColorExchange", EditTarget.Value },
                        { "ColorSpots", ColorSpots.Value }
                    });
            })
            .AddTo(disposables);
        SelectRadialGradientCommand.Subscribe(_ =>
            {
                regionManager.RequestNavigate("ColorPickerRegion", nameof(RadialGradientBrushPicker),
                    new NavigationParameters
                    {
                        { "ColorExchange", EditTarget.Value },
                        { "ColorSpots", ColorSpots.Value }
                    });
            })
            .AddTo(disposables);
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                SelectSolidColorCommand.Dispose();
                SelectLinearGradientCommand.Dispose();
                SelectRadialGradientCommand.Dispose();
                disposables.Dispose();
            }

            disposedValue = true;
        }
    }
}