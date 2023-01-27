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
                var solidColorPicker = _colorPicker.FindVisualChildren<SolidColorPicker>().FirstOrDefault();
                if (solidColorPicker != null)
                {
                    var solidColorPickerViewModel = solidColorPicker.DataContext as SolidColorPickerViewModel;
                    var colorSpots = new ColorSpots();
                    colorSpots.ColorSpot0 = solidColorPickerViewModel.ColorSpot0.Value;
                    colorSpots.ColorSpot1 = solidColorPickerViewModel.ColorSpot1.Value;
                    colorSpots.ColorSpot2 = solidColorPickerViewModel.ColorSpot2.Value;
                    colorSpots.ColorSpot3 = solidColorPickerViewModel.ColorSpot3.Value;
                    colorSpots.ColorSpot4 = solidColorPickerViewModel.ColorSpot4.Value;
                    colorSpots.ColorSpot5 = solidColorPickerViewModel.ColorSpot5.Value;
                    colorSpots.ColorSpot6 = solidColorPickerViewModel.ColorSpot6.Value;
                    colorSpots.ColorSpot7 = solidColorPickerViewModel.ColorSpot7.Value;
                    colorSpots.ColorSpot8 = solidColorPickerViewModel.ColorSpot8.Value;
                    colorSpots.ColorSpot9 = solidColorPickerViewModel.ColorSpot9.Value;
                    colorSpots.ColorSpot10 = solidColorPickerViewModel.ColorSpot10.Value;
                    colorSpots.ColorSpot11 = solidColorPickerViewModel.ColorSpot11.Value;
                    colorSpots.ColorSpot12 = solidColorPickerViewModel.ColorSpot12.Value;
                    colorSpots.ColorSpot13 = solidColorPickerViewModel.ColorSpot13.Value;
                    colorSpots.ColorSpot14 = solidColorPickerViewModel.ColorSpot14.Value;
                    colorSpots.ColorSpot15 = solidColorPickerViewModel.ColorSpot15.Value;
                    colorSpots.ColorSpot16 = solidColorPickerViewModel.ColorSpot16.Value;
                    colorSpots.ColorSpot17 = solidColorPickerViewModel.ColorSpot17.Value;
                    colorSpots.ColorSpot18 = solidColorPickerViewModel.ColorSpot18.Value;
                    colorSpots.ColorSpot19 = solidColorPickerViewModel.ColorSpot19.Value;
                    colorSpots.ColorSpot20 = solidColorPickerViewModel.ColorSpot20.Value;
                    colorSpots.ColorSpot21 = solidColorPickerViewModel.ColorSpot21.Value;
                    colorSpots.ColorSpot22 = solidColorPickerViewModel.ColorSpot22.Value;
                    colorSpots.ColorSpot23 = solidColorPickerViewModel.ColorSpot23.Value;
                    colorSpots.ColorSpot24 = solidColorPickerViewModel.ColorSpot24.Value;
                    colorSpots.ColorSpot25 = solidColorPickerViewModel.ColorSpot25.Value;
                    colorSpots.ColorSpot26 = solidColorPickerViewModel.ColorSpot26.Value;
                    colorSpots.ColorSpot27 = solidColorPickerViewModel.ColorSpot27.Value;
                    colorSpots.ColorSpot28 = solidColorPickerViewModel.ColorSpot28.Value;
                    colorSpots.ColorSpot29 = solidColorPickerViewModel.ColorSpot29.Value;
                    colorSpots.ColorSpot30 = solidColorPickerViewModel.ColorSpot30.Value;
                    colorSpots.ColorSpot31 = solidColorPickerViewModel.ColorSpot31.Value;
                    colorSpots.ColorSpot32 = solidColorPickerViewModel.ColorSpot32.Value;
                    colorSpots.ColorSpot33 = solidColorPickerViewModel.ColorSpot33.Value;
                    colorSpots.ColorSpot34 = solidColorPickerViewModel.ColorSpot34.Value;
                    colorSpots.ColorSpot35 = solidColorPickerViewModel.ColorSpot35.Value;
                    colorSpots.ColorSpot36 = solidColorPickerViewModel.ColorSpot36.Value;
                    colorSpots.ColorSpot37 = solidColorPickerViewModel.ColorSpot37.Value;
                    colorSpots.ColorSpot38 = solidColorPickerViewModel.ColorSpot38.Value;
                    colorSpots.ColorSpot39 = solidColorPickerViewModel.ColorSpot39.Value;
                    colorSpots.ColorSpot40 = solidColorPickerViewModel.ColorSpot40.Value;
                    colorSpots.ColorSpot41 = solidColorPickerViewModel.ColorSpot41.Value;
                    colorSpots.ColorSpot42 = solidColorPickerViewModel.ColorSpot42.Value;
                    colorSpots.ColorSpot43 = solidColorPickerViewModel.ColorSpot43.Value;
                    colorSpots.ColorSpot44 = solidColorPickerViewModel.ColorSpot44.Value;
                    colorSpots.ColorSpot45 = solidColorPickerViewModel.ColorSpot45.Value;
                    colorSpots.ColorSpot46 = solidColorPickerViewModel.ColorSpot46.Value;
                    colorSpots.ColorSpot47 = solidColorPickerViewModel.ColorSpot47.Value;
                    colorSpots.ColorSpot48 = solidColorPickerViewModel.ColorSpot48.Value;
                    colorSpots.ColorSpot49 = solidColorPickerViewModel.ColorSpot49.Value;
                    colorSpots.ColorSpot50 = solidColorPickerViewModel.ColorSpot50.Value;
                    colorSpots.ColorSpot51 = solidColorPickerViewModel.ColorSpot51.Value;
                    colorSpots.ColorSpot52 = solidColorPickerViewModel.ColorSpot52.Value;
                    colorSpots.ColorSpot53 = solidColorPickerViewModel.ColorSpot53.Value;
                    colorSpots.ColorSpot54 = solidColorPickerViewModel.ColorSpot54.Value;
                    colorSpots.ColorSpot55 = solidColorPickerViewModel.ColorSpot55.Value;
                    colorSpots.ColorSpot56 = solidColorPickerViewModel.ColorSpot56.Value;
                    colorSpots.ColorSpot57 = solidColorPickerViewModel.ColorSpot57.Value;
                    colorSpots.ColorSpot58 = solidColorPickerViewModel.ColorSpot58.Value;
                    colorSpots.ColorSpot59 = solidColorPickerViewModel.ColorSpot59.Value;
                    colorSpots.ColorSpot60 = solidColorPickerViewModel.ColorSpot60.Value;
                    colorSpots.ColorSpot61 = solidColorPickerViewModel.ColorSpot61.Value;
                    colorSpots.ColorSpot62 = solidColorPickerViewModel.ColorSpot62.Value;
                    colorSpots.ColorSpot63 = solidColorPickerViewModel.ColorSpot63.Value;
                    colorSpots.ColorSpot64 = solidColorPickerViewModel.ColorSpot64.Value;
                    colorSpots.ColorSpot65 = solidColorPickerViewModel.ColorSpot65.Value;
                    colorSpots.ColorSpot66 = solidColorPickerViewModel.ColorSpot66.Value;
                    colorSpots.ColorSpot67 = solidColorPickerViewModel.ColorSpot67.Value;
                    colorSpots.ColorSpot68 = solidColorPickerViewModel.ColorSpot68.Value;
                    colorSpots.ColorSpot69 = solidColorPickerViewModel.ColorSpot69.Value;
                    colorSpots.ColorSpot70 = solidColorPickerViewModel.ColorSpot70.Value;
                    colorSpots.ColorSpot71 = solidColorPickerViewModel.ColorSpot71.Value;
                    colorSpots.ColorSpot72 = solidColorPickerViewModel.ColorSpot72.Value;
                    colorSpots.ColorSpot73 = solidColorPickerViewModel.ColorSpot73.Value;
                    colorSpots.ColorSpot74 = solidColorPickerViewModel.ColorSpot74.Value;
                    colorSpots.ColorSpot75 = solidColorPickerViewModel.ColorSpot75.Value;
                    colorSpots.ColorSpot76 = solidColorPickerViewModel.ColorSpot76.Value;
                    colorSpots.ColorSpot77 = solidColorPickerViewModel.ColorSpot77.Value;
                    colorSpots.ColorSpot78 = solidColorPickerViewModel.ColorSpot78.Value;
                    colorSpots.ColorSpot79 = solidColorPickerViewModel.ColorSpot79.Value;
                    colorSpots.ColorSpot80 = solidColorPickerViewModel.ColorSpot80.Value;
                    colorSpots.ColorSpot81 = solidColorPickerViewModel.ColorSpot81.Value;
                    colorSpots.ColorSpot82 = solidColorPickerViewModel.ColorSpot82.Value;
                    colorSpots.ColorSpot83 = solidColorPickerViewModel.ColorSpot83.Value;
                    colorSpots.ColorSpot84 = solidColorPickerViewModel.ColorSpot84.Value;
                    colorSpots.ColorSpot85 = solidColorPickerViewModel.ColorSpot85.Value;
                    colorSpots.ColorSpot86 = solidColorPickerViewModel.ColorSpot86.Value;
                    colorSpots.ColorSpot87 = solidColorPickerViewModel.ColorSpot87.Value;
                    colorSpots.ColorSpot88 = solidColorPickerViewModel.ColorSpot88.Value;
                    colorSpots.ColorSpot89 = solidColorPickerViewModel.ColorSpot89.Value;
                    colorSpots.ColorSpot90 = solidColorPickerViewModel.ColorSpot90.Value;
                    colorSpots.ColorSpot91 = solidColorPickerViewModel.ColorSpot91.Value;
                    colorSpots.ColorSpot92 = solidColorPickerViewModel.ColorSpot92.Value;
                    colorSpots.ColorSpot93 = solidColorPickerViewModel.ColorSpot93.Value;
                    colorSpots.ColorSpot94 = solidColorPickerViewModel.ColorSpot94.Value;
                    colorSpots.ColorSpot95 = solidColorPickerViewModel.ColorSpot95.Value;
                    colorSpots.ColorSpot96 = solidColorPickerViewModel.ColorSpot96.Value;
                    colorSpots.ColorSpot97 = solidColorPickerViewModel.ColorSpot97.Value;
                    colorSpots.ColorSpot98 = solidColorPickerViewModel.ColorSpot98.Value;
                    colorSpots.ColorSpot99 = solidColorPickerViewModel.ColorSpot99.Value;
                    var parameters = new DialogParameters
                    {
                        { "ColorExchange", solidColorPickerViewModel.EditTarget.Value },
                        { "ColorSpots", colorSpots }
                    };
                    var ret = new DialogResult(ButtonResult.OK, parameters);
                    RequestClose.Invoke(ret);
                }

                var linearGradientBrushPicker =
                    _colorPicker.FindVisualChildren<LinearGradientBrushPicker>().FirstOrDefault();
                if (linearGradientBrushPicker != null)
                {
                    var linearGradientBrushPickerViewModel =
                        linearGradientBrushPicker.DataContext as LinearGradientBrushPickerViewModel;
                    var colorSpots = new ColorSpots();
                    colorSpots.ColorSpot0 = linearGradientBrushPickerViewModel.ColorSpot0.Value;
                    colorSpots.ColorSpot1 = linearGradientBrushPickerViewModel.ColorSpot1.Value;
                    colorSpots.ColorSpot2 = linearGradientBrushPickerViewModel.ColorSpot2.Value;
                    colorSpots.ColorSpot3 = linearGradientBrushPickerViewModel.ColorSpot3.Value;
                    colorSpots.ColorSpot4 = linearGradientBrushPickerViewModel.ColorSpot4.Value;
                    colorSpots.ColorSpot5 = linearGradientBrushPickerViewModel.ColorSpot5.Value;
                    colorSpots.ColorSpot6 = linearGradientBrushPickerViewModel.ColorSpot6.Value;
                    colorSpots.ColorSpot7 = linearGradientBrushPickerViewModel.ColorSpot7.Value;
                    colorSpots.ColorSpot8 = linearGradientBrushPickerViewModel.ColorSpot8.Value;
                    colorSpots.ColorSpot9 = linearGradientBrushPickerViewModel.ColorSpot9.Value;
                    colorSpots.ColorSpot10 = linearGradientBrushPickerViewModel.ColorSpot10.Value;
                    colorSpots.ColorSpot11 = linearGradientBrushPickerViewModel.ColorSpot11.Value;
                    colorSpots.ColorSpot12 = linearGradientBrushPickerViewModel.ColorSpot12.Value;
                    colorSpots.ColorSpot13 = linearGradientBrushPickerViewModel.ColorSpot13.Value;
                    colorSpots.ColorSpot14 = linearGradientBrushPickerViewModel.ColorSpot14.Value;
                    colorSpots.ColorSpot15 = linearGradientBrushPickerViewModel.ColorSpot15.Value;
                    colorSpots.ColorSpot16 = linearGradientBrushPickerViewModel.ColorSpot16.Value;
                    colorSpots.ColorSpot17 = linearGradientBrushPickerViewModel.ColorSpot17.Value;
                    colorSpots.ColorSpot18 = linearGradientBrushPickerViewModel.ColorSpot18.Value;
                    colorSpots.ColorSpot19 = linearGradientBrushPickerViewModel.ColorSpot19.Value;
                    colorSpots.ColorSpot20 = linearGradientBrushPickerViewModel.ColorSpot20.Value;
                    colorSpots.ColorSpot21 = linearGradientBrushPickerViewModel.ColorSpot21.Value;
                    colorSpots.ColorSpot22 = linearGradientBrushPickerViewModel.ColorSpot22.Value;
                    colorSpots.ColorSpot23 = linearGradientBrushPickerViewModel.ColorSpot23.Value;
                    colorSpots.ColorSpot24 = linearGradientBrushPickerViewModel.ColorSpot24.Value;
                    colorSpots.ColorSpot25 = linearGradientBrushPickerViewModel.ColorSpot25.Value;
                    colorSpots.ColorSpot26 = linearGradientBrushPickerViewModel.ColorSpot26.Value;
                    colorSpots.ColorSpot27 = linearGradientBrushPickerViewModel.ColorSpot27.Value;
                    colorSpots.ColorSpot28 = linearGradientBrushPickerViewModel.ColorSpot28.Value;
                    colorSpots.ColorSpot29 = linearGradientBrushPickerViewModel.ColorSpot29.Value;
                    colorSpots.ColorSpot30 = linearGradientBrushPickerViewModel.ColorSpot30.Value;
                    colorSpots.ColorSpot31 = linearGradientBrushPickerViewModel.ColorSpot31.Value;
                    colorSpots.ColorSpot32 = linearGradientBrushPickerViewModel.ColorSpot32.Value;
                    colorSpots.ColorSpot33 = linearGradientBrushPickerViewModel.ColorSpot33.Value;
                    colorSpots.ColorSpot34 = linearGradientBrushPickerViewModel.ColorSpot34.Value;
                    colorSpots.ColorSpot35 = linearGradientBrushPickerViewModel.ColorSpot35.Value;
                    colorSpots.ColorSpot36 = linearGradientBrushPickerViewModel.ColorSpot36.Value;
                    colorSpots.ColorSpot37 = linearGradientBrushPickerViewModel.ColorSpot37.Value;
                    colorSpots.ColorSpot38 = linearGradientBrushPickerViewModel.ColorSpot38.Value;
                    colorSpots.ColorSpot39 = linearGradientBrushPickerViewModel.ColorSpot39.Value;
                    colorSpots.ColorSpot40 = linearGradientBrushPickerViewModel.ColorSpot40.Value;
                    colorSpots.ColorSpot41 = linearGradientBrushPickerViewModel.ColorSpot41.Value;
                    colorSpots.ColorSpot42 = linearGradientBrushPickerViewModel.ColorSpot42.Value;
                    colorSpots.ColorSpot43 = linearGradientBrushPickerViewModel.ColorSpot43.Value;
                    colorSpots.ColorSpot44 = linearGradientBrushPickerViewModel.ColorSpot44.Value;
                    colorSpots.ColorSpot45 = linearGradientBrushPickerViewModel.ColorSpot45.Value;
                    colorSpots.ColorSpot46 = linearGradientBrushPickerViewModel.ColorSpot46.Value;
                    colorSpots.ColorSpot47 = linearGradientBrushPickerViewModel.ColorSpot47.Value;
                    colorSpots.ColorSpot48 = linearGradientBrushPickerViewModel.ColorSpot48.Value;
                    colorSpots.ColorSpot49 = linearGradientBrushPickerViewModel.ColorSpot49.Value;
                    colorSpots.ColorSpot50 = linearGradientBrushPickerViewModel.ColorSpot50.Value;
                    colorSpots.ColorSpot51 = linearGradientBrushPickerViewModel.ColorSpot51.Value;
                    colorSpots.ColorSpot52 = linearGradientBrushPickerViewModel.ColorSpot52.Value;
                    colorSpots.ColorSpot53 = linearGradientBrushPickerViewModel.ColorSpot53.Value;
                    colorSpots.ColorSpot54 = linearGradientBrushPickerViewModel.ColorSpot54.Value;
                    colorSpots.ColorSpot55 = linearGradientBrushPickerViewModel.ColorSpot55.Value;
                    colorSpots.ColorSpot56 = linearGradientBrushPickerViewModel.ColorSpot56.Value;
                    colorSpots.ColorSpot57 = linearGradientBrushPickerViewModel.ColorSpot57.Value;
                    colorSpots.ColorSpot58 = linearGradientBrushPickerViewModel.ColorSpot58.Value;
                    colorSpots.ColorSpot59 = linearGradientBrushPickerViewModel.ColorSpot59.Value;
                    colorSpots.ColorSpot60 = linearGradientBrushPickerViewModel.ColorSpot60.Value;
                    colorSpots.ColorSpot61 = linearGradientBrushPickerViewModel.ColorSpot61.Value;
                    colorSpots.ColorSpot62 = linearGradientBrushPickerViewModel.ColorSpot62.Value;
                    colorSpots.ColorSpot63 = linearGradientBrushPickerViewModel.ColorSpot63.Value;
                    colorSpots.ColorSpot64 = linearGradientBrushPickerViewModel.ColorSpot64.Value;
                    colorSpots.ColorSpot65 = linearGradientBrushPickerViewModel.ColorSpot65.Value;
                    colorSpots.ColorSpot66 = linearGradientBrushPickerViewModel.ColorSpot66.Value;
                    colorSpots.ColorSpot67 = linearGradientBrushPickerViewModel.ColorSpot67.Value;
                    colorSpots.ColorSpot68 = linearGradientBrushPickerViewModel.ColorSpot68.Value;
                    colorSpots.ColorSpot69 = linearGradientBrushPickerViewModel.ColorSpot69.Value;
                    colorSpots.ColorSpot70 = linearGradientBrushPickerViewModel.ColorSpot70.Value;
                    colorSpots.ColorSpot71 = linearGradientBrushPickerViewModel.ColorSpot71.Value;
                    colorSpots.ColorSpot72 = linearGradientBrushPickerViewModel.ColorSpot72.Value;
                    colorSpots.ColorSpot73 = linearGradientBrushPickerViewModel.ColorSpot73.Value;
                    colorSpots.ColorSpot74 = linearGradientBrushPickerViewModel.ColorSpot74.Value;
                    colorSpots.ColorSpot75 = linearGradientBrushPickerViewModel.ColorSpot75.Value;
                    colorSpots.ColorSpot76 = linearGradientBrushPickerViewModel.ColorSpot76.Value;
                    colorSpots.ColorSpot77 = linearGradientBrushPickerViewModel.ColorSpot77.Value;
                    colorSpots.ColorSpot78 = linearGradientBrushPickerViewModel.ColorSpot78.Value;
                    colorSpots.ColorSpot79 = linearGradientBrushPickerViewModel.ColorSpot79.Value;
                    colorSpots.ColorSpot80 = linearGradientBrushPickerViewModel.ColorSpot80.Value;
                    colorSpots.ColorSpot81 = linearGradientBrushPickerViewModel.ColorSpot81.Value;
                    colorSpots.ColorSpot82 = linearGradientBrushPickerViewModel.ColorSpot82.Value;
                    colorSpots.ColorSpot83 = linearGradientBrushPickerViewModel.ColorSpot83.Value;
                    colorSpots.ColorSpot84 = linearGradientBrushPickerViewModel.ColorSpot84.Value;
                    colorSpots.ColorSpot85 = linearGradientBrushPickerViewModel.ColorSpot85.Value;
                    colorSpots.ColorSpot86 = linearGradientBrushPickerViewModel.ColorSpot86.Value;
                    colorSpots.ColorSpot87 = linearGradientBrushPickerViewModel.ColorSpot87.Value;
                    colorSpots.ColorSpot88 = linearGradientBrushPickerViewModel.ColorSpot88.Value;
                    colorSpots.ColorSpot89 = linearGradientBrushPickerViewModel.ColorSpot89.Value;
                    colorSpots.ColorSpot90 = linearGradientBrushPickerViewModel.ColorSpot90.Value;
                    colorSpots.ColorSpot91 = linearGradientBrushPickerViewModel.ColorSpot91.Value;
                    colorSpots.ColorSpot92 = linearGradientBrushPickerViewModel.ColorSpot92.Value;
                    colorSpots.ColorSpot93 = linearGradientBrushPickerViewModel.ColorSpot93.Value;
                    colorSpots.ColorSpot94 = linearGradientBrushPickerViewModel.ColorSpot94.Value;
                    colorSpots.ColorSpot95 = linearGradientBrushPickerViewModel.ColorSpot95.Value;
                    colorSpots.ColorSpot96 = linearGradientBrushPickerViewModel.ColorSpot96.Value;
                    colorSpots.ColorSpot97 = linearGradientBrushPickerViewModel.ColorSpot97.Value;
                    colorSpots.ColorSpot98 = linearGradientBrushPickerViewModel.ColorSpot98.Value;
                    colorSpots.ColorSpot99 = linearGradientBrushPickerViewModel.ColorSpot99.Value;
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
                    _colorPicker.FindVisualChildren<RadialGradientBrushPicker>().FirstOrDefault();
                if (radialGradientBrushPicker != null)
                {
                    var radialGradientBrushPickerViewModel =
                        radialGradientBrushPicker.DataContext as RadialGradientBrushPickerViewModel;
                    var colorSpots = new ColorSpots();
                    colorSpots.ColorSpot0 = radialGradientBrushPickerViewModel.ColorSpot0.Value;
                    colorSpots.ColorSpot1 = radialGradientBrushPickerViewModel.ColorSpot1.Value;
                    colorSpots.ColorSpot2 = radialGradientBrushPickerViewModel.ColorSpot2.Value;
                    colorSpots.ColorSpot3 = radialGradientBrushPickerViewModel.ColorSpot3.Value;
                    colorSpots.ColorSpot4 = radialGradientBrushPickerViewModel.ColorSpot4.Value;
                    colorSpots.ColorSpot5 = radialGradientBrushPickerViewModel.ColorSpot5.Value;
                    colorSpots.ColorSpot6 = radialGradientBrushPickerViewModel.ColorSpot6.Value;
                    colorSpots.ColorSpot7 = radialGradientBrushPickerViewModel.ColorSpot7.Value;
                    colorSpots.ColorSpot8 = radialGradientBrushPickerViewModel.ColorSpot8.Value;
                    colorSpots.ColorSpot9 = radialGradientBrushPickerViewModel.ColorSpot9.Value;
                    colorSpots.ColorSpot10 = radialGradientBrushPickerViewModel.ColorSpot10.Value;
                    colorSpots.ColorSpot11 = radialGradientBrushPickerViewModel.ColorSpot11.Value;
                    colorSpots.ColorSpot12 = radialGradientBrushPickerViewModel.ColorSpot12.Value;
                    colorSpots.ColorSpot13 = radialGradientBrushPickerViewModel.ColorSpot13.Value;
                    colorSpots.ColorSpot14 = radialGradientBrushPickerViewModel.ColorSpot14.Value;
                    colorSpots.ColorSpot15 = radialGradientBrushPickerViewModel.ColorSpot15.Value;
                    colorSpots.ColorSpot16 = radialGradientBrushPickerViewModel.ColorSpot16.Value;
                    colorSpots.ColorSpot17 = radialGradientBrushPickerViewModel.ColorSpot17.Value;
                    colorSpots.ColorSpot18 = radialGradientBrushPickerViewModel.ColorSpot18.Value;
                    colorSpots.ColorSpot19 = radialGradientBrushPickerViewModel.ColorSpot19.Value;
                    colorSpots.ColorSpot20 = radialGradientBrushPickerViewModel.ColorSpot20.Value;
                    colorSpots.ColorSpot21 = radialGradientBrushPickerViewModel.ColorSpot21.Value;
                    colorSpots.ColorSpot22 = radialGradientBrushPickerViewModel.ColorSpot22.Value;
                    colorSpots.ColorSpot23 = radialGradientBrushPickerViewModel.ColorSpot23.Value;
                    colorSpots.ColorSpot24 = radialGradientBrushPickerViewModel.ColorSpot24.Value;
                    colorSpots.ColorSpot25 = radialGradientBrushPickerViewModel.ColorSpot25.Value;
                    colorSpots.ColorSpot26 = radialGradientBrushPickerViewModel.ColorSpot26.Value;
                    colorSpots.ColorSpot27 = radialGradientBrushPickerViewModel.ColorSpot27.Value;
                    colorSpots.ColorSpot28 = radialGradientBrushPickerViewModel.ColorSpot28.Value;
                    colorSpots.ColorSpot29 = radialGradientBrushPickerViewModel.ColorSpot29.Value;
                    colorSpots.ColorSpot30 = radialGradientBrushPickerViewModel.ColorSpot30.Value;
                    colorSpots.ColorSpot31 = radialGradientBrushPickerViewModel.ColorSpot31.Value;
                    colorSpots.ColorSpot32 = radialGradientBrushPickerViewModel.ColorSpot32.Value;
                    colorSpots.ColorSpot33 = radialGradientBrushPickerViewModel.ColorSpot33.Value;
                    colorSpots.ColorSpot34 = radialGradientBrushPickerViewModel.ColorSpot34.Value;
                    colorSpots.ColorSpot35 = radialGradientBrushPickerViewModel.ColorSpot35.Value;
                    colorSpots.ColorSpot36 = radialGradientBrushPickerViewModel.ColorSpot36.Value;
                    colorSpots.ColorSpot37 = radialGradientBrushPickerViewModel.ColorSpot37.Value;
                    colorSpots.ColorSpot38 = radialGradientBrushPickerViewModel.ColorSpot38.Value;
                    colorSpots.ColorSpot39 = radialGradientBrushPickerViewModel.ColorSpot39.Value;
                    colorSpots.ColorSpot40 = radialGradientBrushPickerViewModel.ColorSpot40.Value;
                    colorSpots.ColorSpot41 = radialGradientBrushPickerViewModel.ColorSpot41.Value;
                    colorSpots.ColorSpot42 = radialGradientBrushPickerViewModel.ColorSpot42.Value;
                    colorSpots.ColorSpot43 = radialGradientBrushPickerViewModel.ColorSpot43.Value;
                    colorSpots.ColorSpot44 = radialGradientBrushPickerViewModel.ColorSpot44.Value;
                    colorSpots.ColorSpot45 = radialGradientBrushPickerViewModel.ColorSpot45.Value;
                    colorSpots.ColorSpot46 = radialGradientBrushPickerViewModel.ColorSpot46.Value;
                    colorSpots.ColorSpot47 = radialGradientBrushPickerViewModel.ColorSpot47.Value;
                    colorSpots.ColorSpot48 = radialGradientBrushPickerViewModel.ColorSpot48.Value;
                    colorSpots.ColorSpot49 = radialGradientBrushPickerViewModel.ColorSpot49.Value;
                    colorSpots.ColorSpot50 = radialGradientBrushPickerViewModel.ColorSpot50.Value;
                    colorSpots.ColorSpot51 = radialGradientBrushPickerViewModel.ColorSpot51.Value;
                    colorSpots.ColorSpot52 = radialGradientBrushPickerViewModel.ColorSpot52.Value;
                    colorSpots.ColorSpot53 = radialGradientBrushPickerViewModel.ColorSpot53.Value;
                    colorSpots.ColorSpot54 = radialGradientBrushPickerViewModel.ColorSpot54.Value;
                    colorSpots.ColorSpot55 = radialGradientBrushPickerViewModel.ColorSpot55.Value;
                    colorSpots.ColorSpot56 = radialGradientBrushPickerViewModel.ColorSpot56.Value;
                    colorSpots.ColorSpot57 = radialGradientBrushPickerViewModel.ColorSpot57.Value;
                    colorSpots.ColorSpot58 = radialGradientBrushPickerViewModel.ColorSpot58.Value;
                    colorSpots.ColorSpot59 = radialGradientBrushPickerViewModel.ColorSpot59.Value;
                    colorSpots.ColorSpot60 = radialGradientBrushPickerViewModel.ColorSpot60.Value;
                    colorSpots.ColorSpot61 = radialGradientBrushPickerViewModel.ColorSpot61.Value;
                    colorSpots.ColorSpot62 = radialGradientBrushPickerViewModel.ColorSpot62.Value;
                    colorSpots.ColorSpot63 = radialGradientBrushPickerViewModel.ColorSpot63.Value;
                    colorSpots.ColorSpot64 = radialGradientBrushPickerViewModel.ColorSpot64.Value;
                    colorSpots.ColorSpot65 = radialGradientBrushPickerViewModel.ColorSpot65.Value;
                    colorSpots.ColorSpot66 = radialGradientBrushPickerViewModel.ColorSpot66.Value;
                    colorSpots.ColorSpot67 = radialGradientBrushPickerViewModel.ColorSpot67.Value;
                    colorSpots.ColorSpot68 = radialGradientBrushPickerViewModel.ColorSpot68.Value;
                    colorSpots.ColorSpot69 = radialGradientBrushPickerViewModel.ColorSpot69.Value;
                    colorSpots.ColorSpot70 = radialGradientBrushPickerViewModel.ColorSpot70.Value;
                    colorSpots.ColorSpot71 = radialGradientBrushPickerViewModel.ColorSpot71.Value;
                    colorSpots.ColorSpot72 = radialGradientBrushPickerViewModel.ColorSpot72.Value;
                    colorSpots.ColorSpot73 = radialGradientBrushPickerViewModel.ColorSpot73.Value;
                    colorSpots.ColorSpot74 = radialGradientBrushPickerViewModel.ColorSpot74.Value;
                    colorSpots.ColorSpot75 = radialGradientBrushPickerViewModel.ColorSpot75.Value;
                    colorSpots.ColorSpot76 = radialGradientBrushPickerViewModel.ColorSpot76.Value;
                    colorSpots.ColorSpot77 = radialGradientBrushPickerViewModel.ColorSpot77.Value;
                    colorSpots.ColorSpot78 = radialGradientBrushPickerViewModel.ColorSpot78.Value;
                    colorSpots.ColorSpot79 = radialGradientBrushPickerViewModel.ColorSpot79.Value;
                    colorSpots.ColorSpot80 = radialGradientBrushPickerViewModel.ColorSpot80.Value;
                    colorSpots.ColorSpot81 = radialGradientBrushPickerViewModel.ColorSpot81.Value;
                    colorSpots.ColorSpot82 = radialGradientBrushPickerViewModel.ColorSpot82.Value;
                    colorSpots.ColorSpot83 = radialGradientBrushPickerViewModel.ColorSpot83.Value;
                    colorSpots.ColorSpot84 = radialGradientBrushPickerViewModel.ColorSpot84.Value;
                    colorSpots.ColorSpot85 = radialGradientBrushPickerViewModel.ColorSpot85.Value;
                    colorSpots.ColorSpot86 = radialGradientBrushPickerViewModel.ColorSpot86.Value;
                    colorSpots.ColorSpot87 = radialGradientBrushPickerViewModel.ColorSpot87.Value;
                    colorSpots.ColorSpot88 = radialGradientBrushPickerViewModel.ColorSpot88.Value;
                    colorSpots.ColorSpot89 = radialGradientBrushPickerViewModel.ColorSpot89.Value;
                    colorSpots.ColorSpot90 = radialGradientBrushPickerViewModel.ColorSpot90.Value;
                    colorSpots.ColorSpot91 = radialGradientBrushPickerViewModel.ColorSpot91.Value;
                    colorSpots.ColorSpot92 = radialGradientBrushPickerViewModel.ColorSpot92.Value;
                    colorSpots.ColorSpot93 = radialGradientBrushPickerViewModel.ColorSpot93.Value;
                    colorSpots.ColorSpot94 = radialGradientBrushPickerViewModel.ColorSpot94.Value;
                    colorSpots.ColorSpot95 = radialGradientBrushPickerViewModel.ColorSpot95.Value;
                    colorSpots.ColorSpot96 = radialGradientBrushPickerViewModel.ColorSpot96.Value;
                    colorSpots.ColorSpot97 = radialGradientBrushPickerViewModel.ColorSpot97.Value;
                    colorSpots.ColorSpot98 = radialGradientBrushPickerViewModel.ColorSpot98.Value;
                    colorSpots.ColorSpot99 = radialGradientBrushPickerViewModel.ColorSpot99.Value;
                    var gradientStopCollection = new GradientStopCollection();
                    radialGradientBrushPickerViewModel.GradientStops.ToList()
                        .ForEach(x => gradientStopCollection.Add(x.ConvertToGradientStop()));
                    var brush = new RadialGradientBrush(gradientStopCollection);
                    brush.GradientOrigin = radialGradientBrushPickerViewModel.GradientOrigin.Value;
                    brush.Center = radialGradientBrushPickerViewModel.Center.Value;
                    brush.RadiusX = radialGradientBrushPickerViewModel.RadiusX.Value;
                    brush.RadiusY = radialGradientBrushPickerViewModel.RadiusY.Value;
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