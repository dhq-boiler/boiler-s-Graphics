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

namespace boilersGraphics
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : PrismApplication
    {
        public static bool IsTest { get; set; }

        public static App Instance { get; set; }

        public StoreContext StoreContext { get; private set; }

        public App()
        {
            Instance = this;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public static Application GetCurrentApp()
        {
            return App.Current != null ? App.Current : new Application();
        }

        public Window GetDialog()
        {
            foreach (var window in this.Windows)
            {
                if (window.GetType() == typeof(Prism.Services.Dialogs.DialogWindow))
                    return window as Window;
            }
            return null;
        }


        /// <summary>
        /// WPF UIスレッドでの未処理例外スロー時のイベントハンドラ
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
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
                       boilersGraphics.Properties.Resources.String_ErrorReporting2 + System.IO.Path.Combine(boilersGraphics.Helpers.Path.GetRoamingDirectory(), "dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics.log") + "\n" +
                       boilersGraphics.Properties.Resources.String_ErrorReporting3 + "\n" +
                       e.ToString();
            dialogParameters.Add("Text", body);
            body = Uri.EscapeDataString(e.ToString());
            dialogParameters.Add("Buttons", new List<Button>() {new Button(boilersGraphics.Properties.Resources.Button_PostIssue, new DelegateCommand(() =>
            {
                body = boilersGraphics.Properties.Resources.String_PleaseDescribeError +
                       Environment.NewLine +
                       Environment.NewLine +
                       e.ToString();
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
            }))});
            IDialogResult dialogResult = new DialogResult();
            Container.Resolve<IDialogService>().ShowDialog(nameof(CustomMessageBox), dialogParameters, ret => dialogResult = ret);
            string message = GoogleAnalyticsUtil.GetStringLimit500Bytes(e.Message);
            GoogleAnalyticsUtil.Beacon((App.Current.MainWindow.DataContext as MainWindowViewModel).TerminalInfo.Value, BeaconPlace.Crash, BeaconPath.Crash, message);
        }

        /// <summary>
        /// UIスレッド以外の未処理例外スロー時のイベントハンドラ
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.ReportUnhandledException(e.ExceptionObject as Exception);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            LogManager.GetCurrentClassLogger().Info($"{GetAppNameAndVersion()}");
            LogManager.GetCurrentClassLogger().Info($"Copyright (C) dhq_boiler 2018-2022. All rights reserved.");
            LogManager.GetCurrentClassLogger().Info($"boiler's Graphics IS LAUNCHING");

            StoreContext context = StoreContext.GetDefault();
            IInitializeWithWindow initWindow = context.As<IInitializeWithWindow>();
            initWindow.Initialize(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            StoreContext = context;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Info("boiler's Graphics IS SHUTTING DOWN");

            this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
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

            containerRegistry.RegisterDialog<ColorPicker, ViewModels.ColorPickerViewModel>();
            containerRegistry.RegisterDialog<OldColorPicker, ViewModels.OldColorPickerViewModel>();
            containerRegistry.RegisterDialog<SolidColorPicker, ViewModels.SolidColorPickerViewModel>();
            containerRegistry.RegisterDialog<LetterSetting, ViewModels.LetterSettingViewModel>();
            containerRegistry.RegisterDialog<LetterVerticalSetting, ViewModels.LetterVerticalSettingViewModel>();
            containerRegistry.RegisterDialog<Views.Preference, ViewModels.PreferenceViewModel>();
            containerRegistry.RegisterDialog<Export, ViewModels.ExportViewModel>();
            containerRegistry.RegisterDialog<PolygonSetting, ViewModels.PolygonSettingViewModel>();
            containerRegistry.RegisterDialog<Layers, ViewModels.LayersViewModel>();
            containerRegistry.RegisterDialog<SetSnapPoint, ViewModels.SetSnapPointViewModel>();
            containerRegistry.RegisterDialog<Views.Version, ViewModels.VersionViewModel>();
            containerRegistry.RegisterDialog<Views.Thickness, ViewModels.ThicknessViewModel>();
            containerRegistry.RegisterDialog<Views.Statistics, ViewModels.StatisticsDialogViewModel>();
            containerRegistry.RegisterDialog<Views.CustomMessageBox, ViewModels.CustomMessageBoxViewModel>();
            containerRegistry.RegisterDialog<Views.PrivacyPolicy, ViewModels.PrivacyPolicyViewModel>();
            containerRegistry.RegisterDialog<Views.UndoHistory, ViewModels.UndoHistoryViewModel>();

            containerRegistry.RegisterForNavigation<Detail>();
            containerRegistry.RegisterDialog<Views.DetailRectangle, ViewModels.DetailRectangleViewModel>();
            containerRegistry.RegisterDialog<Views.DetailEllipse, ViewModels.DetailEllipseViewModel>();
            containerRegistry.RegisterDialog<Views.DetailPolygon, ViewModels.DetailPolygonViewModel>();
            containerRegistry.RegisterDialog<Views.DetailStraightLine, ViewModels.DetailStraightLineViewModel>();
            containerRegistry.RegisterDialog<Views.DetailBezier, ViewModels.DetailBezierViewModel>();
            containerRegistry.RegisterDialog<Views.DetailPicture, ViewModels.DetailPictureViewModel>();
            containerRegistry.RegisterDialog<Views.DetailLetter, ViewModels.DetailLetterViewModel>();
            containerRegistry.RegisterDialog<Views.DetailPie, ViewModels.DetailPieViewModel>();
            containerRegistry.RegisterDialog<Views.DetailPolyBezier, ViewModels.DetailPolyBezierViewModel>();
            containerRegistry.RegisterDialog<Views.DetailMosaic, ViewModels.DetailMosaicViewModel>();
            containerRegistry.RegisterDialog<Views.DetailBlur, ViewModels.DetailBlurViewModel>();
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
}
