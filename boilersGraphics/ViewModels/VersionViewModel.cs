using boilersGraphics.Dao;
using NLog;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive.Disposables;
using System.Reflection;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class VersionViewModel : BindableBase, IDialogAware
    {
        private CompositeDisposable _disposables = new CompositeDisposable();
        public string Title => "boiler's Graphics のバージョン情報";

        public event Action<IDialogResult> RequestClose;

        public ReactivePropertySlim<string> Version { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<string> License { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<string> Markdown { get; } = new ReactivePropertySlim<string>();

        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();

        public VersionViewModel()
        {
            Version.Value = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            License.Value = LicenseReadToEnd();
            Markdown.Value = LicenseMdReadToEnd();
            OKCommand.Subscribe(() =>
            {
                DialogResult result = new DialogResult(ButtonResult.OK);
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
            UpdateStatisticsCountVersionInformationDialogWasDisplayed();
        }

        private static void UpdateStatisticsCountVersionInformationDialogWasDisplayed()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.NumberOfTimesTheVersionInformationDialogWasDisplayed++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private string LicenseReadToEnd()
        {
            var str = ReadLICENSE();
            if (!string.IsNullOrEmpty(str))
                return str;

            using (var client = new WebClient())
            {
                if (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).IsPreRelease)
                {
                    var url = "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/develop/LICENSE";
                    var file = "LICENSE";
                    DownloadFile(client, url, file);
                }
                else
                {
                    var url = "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/master/LICENSE";
                    var file = "LICENSE";
                    DownloadFile(client, url, file);
                }
            }

            if (File.Exists("LICENSE"))
            {
                str = ReadLICENSE();
                if (!string.IsNullOrEmpty(str))
                    return str;
            }

            str = "LICENSEが見つかりません。";
            MessageBox.Show(str);
            return str;
        }

        private static void DownloadFile(WebClient client, string url, string file)
        {
            client.DownloadFile(url, file);
            LogManager.GetCurrentClassLogger().Info($"Download {file} from {url}");
        }

        private string ReadLICENSE()
        {
            try
            {
                using (var streamReader = new StreamReader(new FileStream("LICENSE", FileMode.Open)))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (FileNotFoundException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                return string.Empty;
            }
        }

        private string LicenseMdReadToEnd()
        {
            if (!File.Exists("LICENSE.md"))
            {
                var str = "LICENSE.mdが見つかりません。";
                MessageBox.Show(str);
                return str;
            }

            using (var streamReader = new StreamReader(new FileStream("LICENSE.md", FileMode.Open)))
            {
                return streamReader.ReadToEnd();
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
