using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using NLog;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Unity;
using Windows.Services.Store;
using WinRT;
using static boilersGraphics.ViewModels.CustomMessageBoxViewModel;
using Path = System.IO.Path;
using Preference = boilersGraphics.Views.Preference;
using Statistics = boilersGraphics.Views.Statistics;
using Thickness = boilersGraphics.Views.Thickness;
using Version = boilersGraphics.Views.Version;

namespace boilersGraphics;

/// <summary>
///     App.xaml の相互作用ロジック
/// </summary>
public partial class App : PrismApplication
{
    public App()
    {
        Instance = this;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    public static bool IsTest { get; set; }

    public static App Instance { get; set; }

    public StoreContext StoreContext { get; private set; }

    public static Application GetCurrentApp()
    {
        return Current != null ? Current : new Application();
    }

    public Window GetDialog()
    {
        foreach (var window in Windows)
            if (window.GetType() == typeof(DialogWindow))
                return window as Window;
        return null;
    }


    /// <summary>
    ///     WPF UIスレッドでの未処理例外スロー時のイベントハンドラ
    /// </summary>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportUnhandledException(e.Exception);
        e.Handled = true;
        throw e.Exception;
    }

    private void ReportUnhandledException(Exception e)
    {
        LogManager.GetCurrentClassLogger().Fatal(e);
        IDialogParameters dialogParameters = new DialogParameters();
        dialogParameters.Add("Title", boilersGraphics.Properties.Resources.DialogTitle_Error);
        var title = Uri.EscapeDataString(e.Message);
        var body = boilersGraphics.Properties.Resources.String_ErrorReporting + "\n" +
                   boilersGraphics.Properties.Resources.String_ErrorReporting1 + "\n" +
                   boilersGraphics.Properties.Resources.String_ErrorReporting2 +
                   Path.Combine(Helpers.Path.GetRoamingDirectory(),
                       "dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics.log") + "\n" +
                   boilersGraphics.Properties.Resources.String_ErrorReporting3 + "\n" +
                   e;
        dialogParameters.Add("Text", body);
        body = Uri.EscapeDataString(e.ToString());
        dialogParameters.Add("Buttons", new List<Button>
        {
            new(boilersGraphics.Properties.Resources.Button_PostIssue, new DelegateCommand(() =>
            {
                body = boilersGraphics.Properties.Resources.String_PleaseDescribeError +
                       Environment.NewLine +
                       Environment.NewLine +
                       e;
                const int maxExceptionDetailsLength = 4000;
                if (body.Length > maxExceptionDetailsLength)
                {
                    body = body.Substring(0, maxExceptionDetailsLength);
                    body += Environment.NewLine;
                    body += $"Exception details truncated at {maxExceptionDetailsLength} chars.";
                }

                body = Uri.EscapeDataString(body);

                var url = $"https://github.com/dhq-boiler/boiler-s-Graphics/issues/new?title={title}&body={body}";

                var processStartInfo = new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                };
                using (Process.Start(processStartInfo))
                {
                }
            }))
        });
        IDialogResult dialogResult = new DialogResult();
        Container.Resolve<IDialogService>()
            .ShowDialog(nameof(CustomMessageBox), dialogParameters, ret => dialogResult = ret);
        var message = GoogleAnalyticsUtil.GetStringLimit500Bytes(e.Message);
        GoogleAnalyticsUtil.Beacon((Current.MainWindow.DataContext as MainWindowViewModel).TerminalInfo.Value,
            BeaconPlace.Crash, BeaconPath.Crash, message);
    }

    /// <summary>
    ///     UIスレッド以外の未処理例外スロー時のイベントハンドラ
    /// </summary>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ReportUnhandledException(e.ExceptionObject as Exception);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        LogManager.GetCurrentClassLogger().Info($"{GetAppNameAndVersion()}");
        LogManager.GetCurrentClassLogger().Info("Copyright (C) dhq_boiler 2018-2022. All rights reserved.");
        LogManager.GetCurrentClassLogger().Info("boiler's Graphics IS LAUNCHING");

        var context = StoreContext.GetDefault();
        var initWindow = context.As<IInitializeWithWindow>();
        initWindow.Initialize(Process.GetCurrentProcess().MainWindowHandle);
        StoreContext = context;

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Info("boiler's Graphics IS SHUTTING DOWN");

        DispatcherUnhandledException -= App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

        base.OnExit(e);
    }

    protected override IContainerExtension CreateContainerExtension()
    {
        var container = new UnityContainer();
        container.AddExtension(new Diagnostic());
        container.AddExtension(new LogResolvesUnityContainerExtension());
        return new UnityContainerExtension(container);
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<SolidColorPicker>();
        containerRegistry.RegisterForNavigation<LinearGradientBrushPicker>();
        containerRegistry.RegisterForNavigation<RadialGradientBrushPicker>();

        containerRegistry.RegisterDialog<ColorPicker, ColorPickerViewModel>();
        containerRegistry.RegisterDialog<OldColorPicker, OldColorPickerViewModel>();
        containerRegistry.RegisterDialog<SolidColorPicker, SolidColorPickerViewModel>();
        containerRegistry.RegisterDialog<LetterSetting, LetterSettingViewModel>();
        containerRegistry.RegisterDialog<LetterVerticalSetting, LetterVerticalSettingViewModel>();
        containerRegistry.RegisterDialog<Preference, PreferenceViewModel>();
        containerRegistry.RegisterDialog<Export, ExportViewModel>();
        containerRegistry.RegisterDialog<PolygonSetting, PolygonSettingViewModel>();
        containerRegistry.RegisterDialog<Layers, LayersViewModel>();
        containerRegistry.RegisterDialog<SetSnapPoint, SetSnapPointViewModel>();
        containerRegistry.RegisterDialog<Version, VersionViewModel>();
        containerRegistry.RegisterDialog<Thickness, ThicknessViewModel>();
        containerRegistry.RegisterDialog<Statistics, StatisticsDialogViewModel>();
        containerRegistry.RegisterDialog<CustomMessageBox, CustomMessageBoxViewModel>();
        containerRegistry.RegisterDialog<PrivacyPolicy, PrivacyPolicyViewModel>();
        containerRegistry.RegisterDialog<UndoHistory, UndoHistoryViewModel>();

        containerRegistry.RegisterForNavigation<Detail>();
        containerRegistry.RegisterDialog<DetailRectangle, DetailRectangleViewModel>();
        containerRegistry.RegisterDialog<DetailEllipse, DetailEllipseViewModel>();
        containerRegistry.RegisterDialog<DetailPolygon, DetailPolygonViewModel>();
        containerRegistry.RegisterDialog<DetailStraightLine, DetailStraightLineViewModel>();
        containerRegistry.RegisterDialog<DetailBezier, DetailBezierViewModel>();
        containerRegistry.RegisterDialog<DetailPicture, DetailPictureViewModel>();
        containerRegistry.RegisterDialog<DetailLetter, DetailLetterViewModel>();
        containerRegistry.RegisterDialog<DetailPie, DetailPieViewModel>();
        containerRegistry.RegisterDialog<DetailPolyBezier, DetailPolyBezierViewModel>();
        containerRegistry.RegisterDialog<DetailMosaic, DetailMosaicViewModel>();
        containerRegistry.RegisterDialog<DetailBlur, DetailBlurViewModel>();

        containerRegistry.RegisterForNavigation<Views.ColorCorrect.Hsv, ViewModels.ColorCorrect.HsvViewModel>();
        containerRegistry.RegisterForNavigation<Views.ColorCorrect.ToneCurve, ViewModels.ColorCorrect.ToneCurveViewModel>();
        containerRegistry.RegisterForNavigation<Views.ColorCorrect.NegativePositiveConversion, ViewModels.ColorCorrect.NegativePositiveConversion>();
        containerRegistry.RegisterDialog<ColorCorrectInstruction, ColorCorrectInstructionViewModel>();

        //containerRegistry.RegisterSingleton<ProgressBarWithOutputViewModel>();
        containerRegistry.RegisterDialog<ProgressBarWithOutput, ProgressBarWithOutputViewModel>();
    }

    protected override Window CreateShell()
    {
        var w = Container.Resolve<MainWindow>();
        return w;
    }

    public static string GetAppNameAndVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var majorMinorBuild = $"{version.Major}.{version.Minor}.{version.Build}";
        var appnameAndVersion = $"boiler's Graphics {majorMinorBuild}";
        return appnameAndVersion;
    }
}