using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using NLog;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace boilersGraphics
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : PrismApplication
    {
        public static bool IsTest { get; set; }

        public static App Instance { get; set; }

        public App()
        {
            Instance = this;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Fatal(e.Exception);
            MessageBox.Show("申し訳ありません。\n" +
                            "お使いのアプリケーションは異常を検知したため終了します\n" +
                            "-- エラー内容 --\n" +
                            e.Exception.ToString(),
                            "異常終了");
            Environment.Exit(1);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            LogManager.GetCurrentClassLogger().Info($"boiler's Graphics {version}");
            LogManager.GetCurrentClassLogger().Info($"Copyright (C) dhq_boiler 2018-2021. All rights reserved.");
            LogManager.GetCurrentClassLogger().Info($"boiler's Graphics IS LAUNCHING");
        }

        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindow>();
            return w;
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            var container = new UnityContainer();
            container.AddExtension(new Diagnostic());
            return new UnityContainerExtension(container);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ColorPicker, ViewModels.ColorPickerViewModel>();
            containerRegistry.RegisterDialog<LetterSetting, ViewModels.LetterSettingViewModel>();
            containerRegistry.RegisterDialog<LetterVerticalSetting, ViewModels.LetterVerticalSettingViewModel>();
            containerRegistry.RegisterDialog<Views.Setting, ViewModels.SettingViewModel>();
            containerRegistry.RegisterDialog<Export, ViewModels.ExportViewModel>();
            containerRegistry.RegisterDialog<PolygonSetting, ViewModels.PolygonSettingViewModel>();
            containerRegistry.RegisterDialog<Layers, ViewModels.LayersViewModel>();
            containerRegistry.RegisterDialog<SetSnapPoint, ViewModels.SetSnapPointViewModel>();
            containerRegistry.RegisterDialog<Views.Version, ViewModels.VersionViewModel>();
            containerRegistry.RegisterDialog<Views.Thickness, ViewModels.ThicknessViewModel>();
            containerRegistry.RegisterDialog<Views.DetailRectangle, ViewModels.DetailRectangleViewModel>();
            containerRegistry.RegisterDialog<Views.DetailEllipse, ViewModels.DetailEllipseViewModel>();
            containerRegistry.RegisterDialog<Views.DetailPolygon, ViewModels.DetailPolygonViewModel>();
            containerRegistry.RegisterDialog<Views.DetailStraightLine, ViewModels.DetailStraightLineViewModel>();
            containerRegistry.RegisterDialog<Views.DetailBezier, ViewModels.DetailBezierViewModel>();
            containerRegistry.RegisterDialog<Views.DetailPicture, ViewModels.DetailPictureViewModel>();
            containerRegistry.RegisterDialog<Views.DetailLetter, ViewModels.DetailLetterViewModel>();
            containerRegistry.RegisterDialog<Views.Statistics, ViewModels.StatisticsDialogViewModel>();
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
