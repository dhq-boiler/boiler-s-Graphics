using boilersGraphics.Extensions;
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
using Unity;
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

        public App()
        {
            Instance = this;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        public static Application GetCurrentApp()
        {
            return App.Current != null ? App.Current : new Application();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Fatal(e.Exception);
            IDialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", boilersGraphics.Properties.Resources.DialogTitle_Error);
            var title = Uri.EscapeDataString(e.Exception.Message);
            var body = boilersGraphics.Properties.Resources.String_ErrorReporting + "\n" +
                       boilersGraphics.Properties.Resources.String_ErrorReporting1 + "\n" +
                       boilersGraphics.Properties.Resources.String_ErrorReporting2 + System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics.log") + "\n" +
                       boilersGraphics.Properties.Resources.String_ErrorReporting3 + "\n" +
                       e.Exception.ToString();
            dialogParameters.Add("Text", body);
            body = Uri.EscapeDataString(e.Exception.ToString());
            dialogParameters.Add("Buttons", new List<Button>() {new Button(boilersGraphics.Properties.Resources.Button_PostIssue, new DelegateCommand(() =>
            {
                body = boilersGraphics.Properties.Resources.String_PleaseDescribeError +
                       Environment.NewLine +
                       Environment.NewLine +
                       e.Exception.ToString();
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
            throw e.Exception;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            LogManager.GetCurrentClassLogger().Info($"boiler's Graphics {version}");
            LogManager.GetCurrentClassLogger().Info($"Copyright (C) dhq_boiler 2018-2022. All rights reserved.");
            LogManager.GetCurrentClassLogger().Info($"boiler's Graphics IS LAUNCHING");
            base.OnStartup(e);
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
            containerRegistry.RegisterDialog<ColorPicker, ViewModels.ColorPickerViewModel>();
            containerRegistry.RegisterDialog<LetterSetting, ViewModels.LetterSettingViewModel>();
            containerRegistry.RegisterDialog<LetterVerticalSetting, ViewModels.LetterVerticalSettingViewModel>();
            containerRegistry.RegisterDialog<Views.Preference, ViewModels.PreferenceViewModel>();
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
            containerRegistry.RegisterDialog<Views.CustomMessageBox, ViewModels.CustomMessageBoxViewModel>();
            containerRegistry.RegisterDialog<Views.PrivacyPolicy, ViewModels.PrivacyPolicyViewModel>();
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
