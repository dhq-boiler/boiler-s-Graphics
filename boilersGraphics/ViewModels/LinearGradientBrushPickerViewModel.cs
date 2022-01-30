using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Views;
using NLog;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    internal class LinearGradientBrushPickerViewModel : BindableBase, INavigationAware
    {
        private CompositeDisposable _disposables = new CompositeDisposable();
        private LinearGradientBrushPicker _colorPicker;
        private IEnumerable<ColorSpot> _spots;
        private readonly IDialogService dialogService;

        public ReactivePropertySlim<Brush> TargetBrush { get; } = new ReactivePropertySlim<Brush>();
        //public ReactiveCommand OKCommand { get; }
        public ReactiveCommand<boilersGraphics.Models.GradientStop> SelectGradientStopColorCommand { get; } = new ReactiveCommand<boilersGraphics.Models.GradientStop>();
        public ReactiveCommand SpotSelectCommand { get; } = new ReactiveCommand();
        public ReactiveCommand<RoutedEventArgs> LoadedCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
        public ReactiveCommand OpenCloseColorPalleteCommand { get; } = new ReactiveCommand();
        public ReactiveCommand AddGradientStopCommand { get; } = new ReactiveCommand();
        public ReactiveCommand<TextChangedEventArgs> TextChangedCommand { get; } = new ReactiveCommand<TextChangedEventArgs>();
        public ReactiveCollection<boilersGraphics.Models.GradientStop> GradientStops { get; } = new ReactiveCollection<boilersGraphics.Models.GradientStop>();
        public ReactivePropertySlim<Visibility> ColorPalleteVisibility { get; } = new ReactivePropertySlim<Visibility>(Visibility.Collapsed);
        public ReactivePropertySlim<ColorSpots> ColorSpots { get; } = new ReactivePropertySlim<ColorSpots>();
        public ReactivePropertySlim<Brush> ColorSpot0 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot1 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot2 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot3 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot4 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot5 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot6 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot7 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot8 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot9 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot10 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot11 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot12 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot13 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot14 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot15 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot16 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot17 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot18 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot19 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot20 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot21 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot22 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot23 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot24 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot25 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot26 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot27 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot28 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot29 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot30 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot31 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot32 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot33 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot34 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot35 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot36 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot37 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot38 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot39 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot40 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot41 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot42 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot43 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot44 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot45 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot46 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot47 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot48 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot49 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot50 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot51 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot52 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot53 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot54 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot55 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot56 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot57 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot58 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot59 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot60 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot61 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot62 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot63 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot64 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot65 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot66 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot67 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot68 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot69 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot70 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot71 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot72 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot73 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot74 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot75 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot76 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot77 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot78 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot79 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot80 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot81 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot82 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot83 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot84 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot85 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot86 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot87 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot88 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot89 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot90 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot91 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot92 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot93 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot94 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot95 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot96 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot97 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot98 { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> ColorSpot99 { get; } = new ReactivePropertySlim<Brush>();

        public ReactivePropertySlim<boilersGraphics.Models.GradientStop> SelectedGradientStop { get; } = new ReactivePropertySlim<Models.GradientStop>();
        public ReactivePropertySlim<Point> StartPoint { get; } = new ReactivePropertySlim<Point>();
        public ReactivePropertySlim<Point> EndPoint { get; } = new ReactivePropertySlim<Point>();
        public ReactivePropertySlim<ColorExchange> EditTarget { get; } = new ReactivePropertySlim<ColorExchange>(new ColorExchange());

        public LinearGradientBrushPickerViewModel(IDialogService dialogService)
        {
            //OKCommand = new ReactiveCommand();

            //OKCommand
            //    .Subscribe(_ =>
            //    {
            //        var colorSpots = new ColorSpots();
            //        colorSpots.ColorSpot0 = ColorSpot0.Value;
            //        colorSpots.ColorSpot1 = ColorSpot1.Value;
            //        colorSpots.ColorSpot2 = ColorSpot2.Value;
            //        colorSpots.ColorSpot3 = ColorSpot3.Value;
            //        colorSpots.ColorSpot4 = ColorSpot4.Value;
            //        colorSpots.ColorSpot5 = ColorSpot5.Value;
            //        colorSpots.ColorSpot6 = ColorSpot6.Value;
            //        colorSpots.ColorSpot7 = ColorSpot7.Value;
            //        colorSpots.ColorSpot8 = ColorSpot8.Value;
            //        colorSpots.ColorSpot9 = ColorSpot9.Value;
            //        colorSpots.ColorSpot10 = ColorSpot10.Value;
            //        colorSpots.ColorSpot11 = ColorSpot11.Value;
            //        colorSpots.ColorSpot12 = ColorSpot12.Value;
            //        colorSpots.ColorSpot13 = ColorSpot13.Value;
            //        colorSpots.ColorSpot14 = ColorSpot14.Value;
            //        colorSpots.ColorSpot15 = ColorSpot15.Value;
            //        colorSpots.ColorSpot16 = ColorSpot16.Value;
            //        colorSpots.ColorSpot17 = ColorSpot17.Value;
            //        colorSpots.ColorSpot18 = ColorSpot18.Value;
            //        colorSpots.ColorSpot19 = ColorSpot19.Value;
            //        colorSpots.ColorSpot20 = ColorSpot20.Value;
            //        colorSpots.ColorSpot21 = ColorSpot21.Value;
            //        colorSpots.ColorSpot22 = ColorSpot22.Value;
            //        colorSpots.ColorSpot23 = ColorSpot23.Value;
            //        colorSpots.ColorSpot24 = ColorSpot24.Value;
            //        colorSpots.ColorSpot25 = ColorSpot25.Value;
            //        colorSpots.ColorSpot26 = ColorSpot26.Value;
            //        colorSpots.ColorSpot27 = ColorSpot27.Value;
            //        colorSpots.ColorSpot28 = ColorSpot28.Value;
            //        colorSpots.ColorSpot29 = ColorSpot29.Value;
            //        colorSpots.ColorSpot30 = ColorSpot30.Value;
            //        colorSpots.ColorSpot31 = ColorSpot31.Value;
            //        colorSpots.ColorSpot32 = ColorSpot32.Value;
            //        colorSpots.ColorSpot33 = ColorSpot33.Value;
            //        colorSpots.ColorSpot34 = ColorSpot34.Value;
            //        colorSpots.ColorSpot35 = ColorSpot35.Value;
            //        colorSpots.ColorSpot36 = ColorSpot36.Value;
            //        colorSpots.ColorSpot37 = ColorSpot37.Value;
            //        colorSpots.ColorSpot38 = ColorSpot38.Value;
            //        colorSpots.ColorSpot39 = ColorSpot39.Value;
            //        colorSpots.ColorSpot40 = ColorSpot40.Value;
            //        colorSpots.ColorSpot41 = ColorSpot41.Value;
            //        colorSpots.ColorSpot42 = ColorSpot42.Value;
            //        colorSpots.ColorSpot43 = ColorSpot43.Value;
            //        colorSpots.ColorSpot44 = ColorSpot44.Value;
            //        colorSpots.ColorSpot45 = ColorSpot45.Value;
            //        colorSpots.ColorSpot46 = ColorSpot46.Value;
            //        colorSpots.ColorSpot47 = ColorSpot47.Value;
            //        colorSpots.ColorSpot48 = ColorSpot48.Value;
            //        colorSpots.ColorSpot49 = ColorSpot49.Value;
            //        colorSpots.ColorSpot50 = ColorSpot50.Value;
            //        colorSpots.ColorSpot51 = ColorSpot51.Value;
            //        colorSpots.ColorSpot52 = ColorSpot52.Value;
            //        colorSpots.ColorSpot53 = ColorSpot53.Value;
            //        colorSpots.ColorSpot54 = ColorSpot54.Value;
            //        colorSpots.ColorSpot55 = ColorSpot55.Value;
            //        colorSpots.ColorSpot56 = ColorSpot56.Value;
            //        colorSpots.ColorSpot57 = ColorSpot57.Value;
            //        colorSpots.ColorSpot58 = ColorSpot58.Value;
            //        colorSpots.ColorSpot59 = ColorSpot59.Value;
            //        colorSpots.ColorSpot60 = ColorSpot60.Value;
            //        colorSpots.ColorSpot61 = ColorSpot61.Value;
            //        colorSpots.ColorSpot62 = ColorSpot62.Value;
            //        colorSpots.ColorSpot63 = ColorSpot63.Value;
            //        colorSpots.ColorSpot64 = ColorSpot64.Value;
            //        colorSpots.ColorSpot65 = ColorSpot65.Value;
            //        colorSpots.ColorSpot66 = ColorSpot66.Value;
            //        colorSpots.ColorSpot67 = ColorSpot67.Value;
            //        colorSpots.ColorSpot68 = ColorSpot68.Value;
            //        colorSpots.ColorSpot69 = ColorSpot69.Value;
            //        colorSpots.ColorSpot70 = ColorSpot70.Value;
            //        colorSpots.ColorSpot71 = ColorSpot71.Value;
            //        colorSpots.ColorSpot72 = ColorSpot72.Value;
            //        colorSpots.ColorSpot73 = ColorSpot73.Value;
            //        colorSpots.ColorSpot74 = ColorSpot74.Value;
            //        colorSpots.ColorSpot75 = ColorSpot75.Value;
            //        colorSpots.ColorSpot76 = ColorSpot76.Value;
            //        colorSpots.ColorSpot77 = ColorSpot77.Value;
            //        colorSpots.ColorSpot78 = ColorSpot78.Value;
            //        colorSpots.ColorSpot79 = ColorSpot79.Value;
            //        colorSpots.ColorSpot80 = ColorSpot80.Value;
            //        colorSpots.ColorSpot81 = ColorSpot81.Value;
            //        colorSpots.ColorSpot82 = ColorSpot82.Value;
            //        colorSpots.ColorSpot83 = ColorSpot83.Value;
            //        colorSpots.ColorSpot84 = ColorSpot84.Value;
            //        colorSpots.ColorSpot85 = ColorSpot85.Value;
            //        colorSpots.ColorSpot86 = ColorSpot86.Value;
            //        colorSpots.ColorSpot87 = ColorSpot87.Value;
            //        colorSpots.ColorSpot88 = ColorSpot88.Value;
            //        colorSpots.ColorSpot89 = ColorSpot89.Value;
            //        colorSpots.ColorSpot90 = ColorSpot90.Value;
            //        colorSpots.ColorSpot91 = ColorSpot91.Value;
            //        colorSpots.ColorSpot92 = ColorSpot92.Value;
            //        colorSpots.ColorSpot93 = ColorSpot93.Value;
            //        colorSpots.ColorSpot94 = ColorSpot94.Value;
            //        colorSpots.ColorSpot95 = ColorSpot95.Value;
            //        colorSpots.ColorSpot96 = ColorSpot96.Value;
            //        colorSpots.ColorSpot97 = ColorSpot97.Value;
            //        colorSpots.ColorSpot98 = ColorSpot98.Value;
            //        colorSpots.ColorSpot99 = ColorSpot99.Value;

            //        //something should be written
            //        var gradientStopCollection = new GradientStopCollection();
            //        GradientStops.ToList().ForEach(x => gradientStopCollection.Add(x.ConvertToGradientStop()));
            //        EditTarget.Value.New = new LinearGradientBrush(gradientStopCollection, StartPoint.Value, EndPoint.Value);
            //    })
            //    .AddTo(_disposables);

            TextChangedCommand.Subscribe(args =>
            {
                BuildTargetBrush();
                args.Handled = false;
            })
            .AddTo(_disposables);

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
                if (colorSpot.IsSelected.Value == true)
                    colorSpot.IsSelected.Value = false;
                else
                {
                    if (!colorSpot.IsSelected.Value)
                    {
                        _spots.ToList().ForEach(x => x.IsSelected.Value = false);
                        colorSpot.IsSelected.Value = true;
                        if (SelectedGradientStop.Value != null)
                        {
                            SelectedGradientStop.Value.Color.Value = colorSpot.Color;
                            BuildTargetBrush();
                        }
                    }
                }
            })
            .AddTo(_disposables);
            LoadedCommand.Subscribe(x =>
            {
                var source = x.Source;
                _colorPicker = source as LinearGradientBrushPicker;
                _spots = _colorPicker.FindVisualChildren<ColorSpot>();
            })
            .AddTo(_disposables);
            AddGradientStopCommand.Subscribe(x =>
            {
                GradientStops.Add(new boilersGraphics.Models.GradientStop(Colors.White, 0));
            })
            .AddTo(_disposables);
            this.dialogService = dialogService;
            SelectGradientStopColorCommand.Subscribe(gradientStop =>
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
                IDialogResult result = null;
                this.dialogService.ShowDialog(nameof(OldColorPicker),
                                           new DialogParameters()
                                           {
                                               {
                                                   "ColorExchange",
                                                   new ColorExchange()
                                                   {
                                                       Old = new SolidColorBrush(gradientStop.Color.Value)
                                                   }
                                               },
                                               {
                                                   "ColorSpots",
                                                   colorSpots
                                               }
                                           },
                                           ret => result = ret);
                if (result != null)
                {
                    var exchange = result.Parameters.GetValue<ColorExchange>("ColorExchange");
                    if (exchange != null)
                    {
                        gradientStop.Color.Value = (exchange.New as SolidColorBrush).Color;
                        BuildTargetBrush();
                    }
                    var _colorSpots = result.Parameters.GetValue<ColorSpots>("ColorSpots");
                    if (_colorSpots != null)
                    {
                        ColorSpots.Value = _colorSpots;
                    }
                }
            })
            .AddTo(_disposables);
        }

        private void BuildTargetBrush()
        {
            LogManager.GetCurrentClassLogger().Debug($"BuildTargetBrush StartPoint={StartPoint.Value}, EndPoint={EndPoint.Value}");
            var gradientStopCollection = new GradientStopCollection();
            GradientStops.ToList().ForEach(x => gradientStopCollection.Add(x.ConvertToGradientStop()));
            TargetBrush.Value = new LinearGradientBrush(gradientStopCollection, StartPoint.Value, EndPoint.Value);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var colorSpots = navigationContext.Parameters.GetValue<ColorSpots>("ColorSpots");
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
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var colorspots = navigationContext.Parameters.GetValue<ColorSpots>("ColorSpots");
            this.ColorSpot0.Value = colorspots.ColorSpot0;
            this.ColorSpot1.Value = colorspots.ColorSpot1;
            this.ColorSpot2.Value = colorspots.ColorSpot2;
            this.ColorSpot3.Value = colorspots.ColorSpot3;
            this.ColorSpot4.Value = colorspots.ColorSpot4;
            this.ColorSpot5.Value = colorspots.ColorSpot5;
            this.ColorSpot6.Value = colorspots.ColorSpot6;
            this.ColorSpot7.Value = colorspots.ColorSpot7;
            this.ColorSpot8.Value = colorspots.ColorSpot8;
            this.ColorSpot9.Value = colorspots.ColorSpot9;
            this.ColorSpot10.Value = colorspots.ColorSpot10;
            this.ColorSpot11.Value = colorspots.ColorSpot11;
            this.ColorSpot12.Value = colorspots.ColorSpot12;
            this.ColorSpot13.Value = colorspots.ColorSpot13;
            this.ColorSpot14.Value = colorspots.ColorSpot14;
            this.ColorSpot15.Value = colorspots.ColorSpot15;
            this.ColorSpot16.Value = colorspots.ColorSpot16;
            this.ColorSpot17.Value = colorspots.ColorSpot17;
            this.ColorSpot18.Value = colorspots.ColorSpot18;
            this.ColorSpot19.Value = colorspots.ColorSpot19;
            this.ColorSpot20.Value = colorspots.ColorSpot20;
            this.ColorSpot21.Value = colorspots.ColorSpot21;
            this.ColorSpot22.Value = colorspots.ColorSpot22;
            this.ColorSpot23.Value = colorspots.ColorSpot23;
            this.ColorSpot24.Value = colorspots.ColorSpot24;
            this.ColorSpot25.Value = colorspots.ColorSpot25;
            this.ColorSpot26.Value = colorspots.ColorSpot26;
            this.ColorSpot27.Value = colorspots.ColorSpot27;
            this.ColorSpot28.Value = colorspots.ColorSpot28;
            this.ColorSpot29.Value = colorspots.ColorSpot29;
            this.ColorSpot30.Value = colorspots.ColorSpot30;
            this.ColorSpot31.Value = colorspots.ColorSpot31;
            this.ColorSpot32.Value = colorspots.ColorSpot32;
            this.ColorSpot33.Value = colorspots.ColorSpot33;
            this.ColorSpot34.Value = colorspots.ColorSpot34;
            this.ColorSpot35.Value = colorspots.ColorSpot35;
            this.ColorSpot36.Value = colorspots.ColorSpot36;
            this.ColorSpot37.Value = colorspots.ColorSpot37;
            this.ColorSpot38.Value = colorspots.ColorSpot38;
            this.ColorSpot39.Value = colorspots.ColorSpot39;
            this.ColorSpot40.Value = colorspots.ColorSpot40;
            this.ColorSpot41.Value = colorspots.ColorSpot41;
            this.ColorSpot42.Value = colorspots.ColorSpot42;
            this.ColorSpot43.Value = colorspots.ColorSpot43;
            this.ColorSpot44.Value = colorspots.ColorSpot44;
            this.ColorSpot45.Value = colorspots.ColorSpot45;
            this.ColorSpot46.Value = colorspots.ColorSpot46;
            this.ColorSpot47.Value = colorspots.ColorSpot47;
            this.ColorSpot48.Value = colorspots.ColorSpot48;
            this.ColorSpot49.Value = colorspots.ColorSpot49;
            this.ColorSpot50.Value = colorspots.ColorSpot50;
            this.ColorSpot51.Value = colorspots.ColorSpot51;
            this.ColorSpot52.Value = colorspots.ColorSpot52;
            this.ColorSpot53.Value = colorspots.ColorSpot53;
            this.ColorSpot54.Value = colorspots.ColorSpot54;
            this.ColorSpot55.Value = colorspots.ColorSpot55;
            this.ColorSpot56.Value = colorspots.ColorSpot56;
            this.ColorSpot57.Value = colorspots.ColorSpot57;
            this.ColorSpot58.Value = colorspots.ColorSpot58;
            this.ColorSpot59.Value = colorspots.ColorSpot59;
            this.ColorSpot60.Value = colorspots.ColorSpot60;
            this.ColorSpot61.Value = colorspots.ColorSpot61;
            this.ColorSpot62.Value = colorspots.ColorSpot62;
            this.ColorSpot63.Value = colorspots.ColorSpot63;
            this.ColorSpot64.Value = colorspots.ColorSpot64;
            this.ColorSpot65.Value = colorspots.ColorSpot65;
            this.ColorSpot66.Value = colorspots.ColorSpot66;
            this.ColorSpot67.Value = colorspots.ColorSpot67;
            this.ColorSpot68.Value = colorspots.ColorSpot68;
            this.ColorSpot69.Value = colorspots.ColorSpot69;
            this.ColorSpot70.Value = colorspots.ColorSpot70;
            this.ColorSpot71.Value = colorspots.ColorSpot71;
            this.ColorSpot72.Value = colorspots.ColorSpot72;
            this.ColorSpot73.Value = colorspots.ColorSpot73;
            this.ColorSpot74.Value = colorspots.ColorSpot74;
            this.ColorSpot75.Value = colorspots.ColorSpot75;
            this.ColorSpot76.Value = colorspots.ColorSpot76;
            this.ColorSpot77.Value = colorspots.ColorSpot77;
            this.ColorSpot78.Value = colorspots.ColorSpot78;
            this.ColorSpot79.Value = colorspots.ColorSpot79;
            this.ColorSpot80.Value = colorspots.ColorSpot80;
            this.ColorSpot81.Value = colorspots.ColorSpot81;
            this.ColorSpot82.Value = colorspots.ColorSpot82;
            this.ColorSpot83.Value = colorspots.ColorSpot83;
            this.ColorSpot84.Value = colorspots.ColorSpot84;
            this.ColorSpot85.Value = colorspots.ColorSpot85;
            this.ColorSpot86.Value = colorspots.ColorSpot86;
            this.ColorSpot87.Value = colorspots.ColorSpot87;
            this.ColorSpot88.Value = colorspots.ColorSpot88;
            this.ColorSpot89.Value = colorspots.ColorSpot89;
            this.ColorSpot90.Value = colorspots.ColorSpot90;
            this.ColorSpot91.Value = colorspots.ColorSpot91;
            this.ColorSpot92.Value = colorspots.ColorSpot92;
            this.ColorSpot93.Value = colorspots.ColorSpot93;
            this.ColorSpot94.Value = colorspots.ColorSpot94;
            this.ColorSpot95.Value = colorspots.ColorSpot95;
            this.ColorSpot96.Value = colorspots.ColorSpot96;
            this.ColorSpot97.Value = colorspots.ColorSpot97;
            this.ColorSpot98.Value = colorspots.ColorSpot98;
            this.ColorSpot99.Value = colorspots.ColorSpot99;
        }
    }
}
