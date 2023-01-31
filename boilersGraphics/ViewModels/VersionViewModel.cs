using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Windows.Services.Store;
using boilersGraphics.Dao;
using boilersGraphics.Exceptions;
using boilersGraphics.Properties;
using NLog;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels;

public class VersionViewModel : BindableBase, IDialogAware
{
    private readonly CompositeDisposable _disposables = new();

    private StoreAppLicense appLicense;

    public VersionViewModel()
    {
        Version.Value = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        License.Value = LicenseReadToEnd();
        Markdown.Value = LicenseMdReadToEnd();
        OKCommand.Subscribe(() =>
            {
                var result = new DialogResult(ButtonResult.OK);
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
        UpdateStatisticsCountVersionInformationDialogWasDisplayed();
        InitializeLicenseMessage();
    }

    public ReactivePropertySlim<string> Version { get; } = new();

    public ReactivePropertySlim<string> License { get; } = new();

    public ReactivePropertySlim<string> Markdown { get; } = new();

    public ReactiveCommand OKCommand { get; } = new();

    public ReactivePropertySlim<string> LicenseMessage { get; } = new();
    public string Title => Resources.Dialog_Title_Version;

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
    }

    private async Task InitializeLicenseMessage()
    {
        var app = Application.Current as App;
        appLicense = await app.StoreContext.GetAppLicenseAsync();
        app.StoreContext.OfflineLicensesChanged += StoreContext_OfflineLicensesChanged;

        if (appLicense.IsActive)
        {
            if (appLicense.IsTrial)
            {
                var timespan = appLicense.ExpirationDate - DateTime.Now;
                LicenseMessage.Value = string.Format(Resources.String_TrialMessage, timespan.Days, timespan.Hours,
                    timespan.Minutes, timespan.Seconds);
            }
            else
            {
                //full license
                LicenseMessage.Value = string.Format(Resources.String_FullLicense);
            }
        }
    }

    private async void StoreContext_OfflineLicensesChanged(StoreContext sender, object args)
    {
        var app = Application.Current as App;
        appLicense = await app.StoreContext.GetAppLicenseAsync();

        if (appLicense.IsActive)
        {
            if (appLicense.IsTrial)
            {
                var timespan = appLicense.ExpirationDate - DateTime.Now;
                LicenseMessage.Value = string.Format(Resources.String_TrialMessage, timespan.Days, timespan.Hours,
                    timespan.Minutes, timespan.Seconds);
            }
            else
            {
                //full license
                LicenseMessage.Value = string.Format(Resources.String_FullLicense);
            }
        }
    }

    private static void UpdateStatisticsCountVersionInformationDialogWasDisplayed()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        statistics.NumberOfTimesTheVersionInformationDialogWasDisplayed++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    private string LicenseReadToEnd()
    {
        var filename = Path.Combine(Helpers.Path.GetRoamingDirectory(), "dhq_boiler\\boilersGraphics\\LICENSE");
        var str = ReadFileToEnd(filename);
        if (!string.IsNullOrEmpty(str))
            return str;

        using (var client = new WebClient())
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var flags = ParseProductVersion(versionInfo.ProductVersion);
            if (flags.isPreRelease)
            {
                var url = "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/develop/LICENSE";
                DownloadFile(client, url, filename);
            }
            else if (flags.isMaster)
            {
                var url = "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/master/LICENSE";
                DownloadFile(client, url, filename);
            }
        }

        if (File.Exists(filename))
        {
            str = ReadFileToEnd(filename);
            if (!string.IsNullOrEmpty(str))
                return str;
        }

        str = $"{filename}が見つかりません。";
        MessageBox.Show(str);
        return str;
    }

    private static (bool isMaster, bool isPreRelease, bool isDebug) ParseProductVersion(string productVersion)
    {
        if (productVersion.Contains("master") || productVersion.Contains("hotfix"))
            return (true, false, false);
        if (productVersion.Contains("develop") || productVersion.Contains("unstable"))
            return (false, true, true);
        if (productVersion.Contains("feature"))
            return (false, false, true);
        throw new UnexpectedException($"ProductVersionをパースできませんでした。productVersion={productVersion}");
    }

    private static void DownloadFile(WebClient client, string url, string file)
    {
        client.DownloadFile(url, file);
        LogManager.GetCurrentClassLogger().Info($"Download {file} from {url}");
    }

    private string ReadFileToEnd(string filename)
    {
        try
        {
            using (var streamReader = new StreamReader(new FileStream(filename, FileMode.Open)))
            {
                return streamReader.ReadToEnd();
            }
        }
        catch (FileNotFoundException ex)
        {
            LogManager.GetCurrentClassLogger().Warn(ex);
            return string.Empty;
        }
    }

    private string LicenseMdReadToEnd()
    {
        var filename = Path.Combine(Helpers.Path.GetRoamingDirectory(), "dhq_boiler\\boilersGraphics\\LICENSE.md");
        var str = ReadFileToEnd(filename);
        if (!string.IsNullOrEmpty(str))
            return str;

        using (var client = new WebClient())
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            var flags = ParseProductVersion(versionInfo.ProductVersion);
            if (flags.isPreRelease)
            {
                var url =
                    "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/develop/boilersGraphics/LICENSE.md";
                DownloadFile(client, url, filename);
            }
            else if (flags.isMaster)
            {
                var url =
                    "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/master/boilersGraphics/LICENSE.md";
                DownloadFile(client, url, filename);
            }
        }

        if (File.Exists(filename))
        {
            str = ReadFileToEnd(filename);
            if (!string.IsNullOrEmpty(str))
                return str;
        }

        str = $"{filename}が見つかりません。";
        MessageBox.Show(str);
        return str;
    }
}