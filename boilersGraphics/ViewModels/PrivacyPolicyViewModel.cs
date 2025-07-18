using boilersGraphics.Dao;
using boilersGraphics.Exceptions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using NLog;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using System.Windows;
using ZLinq;

namespace boilersGraphics.ViewModels;

public class PrivacyPolicyViewModel : BindableBase, IDialogAware
{
    private readonly CompositeDisposable disposables = new();

    public PrivacyPolicyViewModel()
    {
        AgreeCommand.Subscribe(_ =>
            {
                InsertOrUpdate(true);
                GoogleAnalyticsUtil.Beacon(TerminalInfo.Value, BeaconPlace.AgreePrivacyPolicy,
                    BeaconPath.AgreePrivacyPolicy);
                RequestClose.Invoke(new DialogResult(ButtonResult.Yes));
            })
            .AddTo(disposables);
        DisagreeCommand.Subscribe(_ =>
            {
                InsertOrUpdate(false);
                RequestClose.Invoke(new DialogResult(ButtonResult.No));
            })
            .AddTo(disposables);
        OKCommand.Subscribe(_ => { RequestClose.Invoke(new DialogResult(ButtonResult.OK)); })
            .AddTo(disposables);
    }

    public ReactiveCommand AgreeCommand { get; } = new();
    public ReactiveCommand DisagreeCommand { get; } = new();
    public ReactiveCommand OKCommand { get; } = new();

    public ReactivePropertySlim<Visibility> AgreeDisagreeVisibility { get; } = new();

    public ReactivePropertySlim<Visibility> OKVisibility { get; } = new();

    public ReactivePropertySlim<string> Markdown { get; } = new();

    public ReactivePropertySlim<PrivacyPolicyAgreement> CurrentPrivacyPolicyAgreement { get; } = new();

    public ReactivePropertySlim<string> Message { get; } = new();

    public ReactivePropertySlim<TerminalInfo> TerminalInfo { get; } = new();
    public string Title => Resources.Title_PrivacyPolicy;

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
        var currentState = parameters.GetValue<PrivacyPolicyAgreement>("CurrentStatus");
        var latestDate = parameters.GetValue<DateTime?>("latestDate");
        TerminalInfo.Value = parameters.GetValue<TerminalInfo>("terminalInfo");
        CurrentPrivacyPolicyAgreement.Value = currentState;
        AgreeDisagreeVisibility.Value =
            currentState == null || currentState.DateOfEnactment < latestDate || !currentState.IsAgree
                ? Visibility.Visible
                : Visibility.Collapsed;
        OKVisibility.Value = currentState == null || currentState.DateOfEnactment < latestDate || !currentState.IsAgree
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (OKVisibility.Value == Visibility.Visible)
            Message.Value = string.Format(Resources.PrivacyPolicy_AgreedMessage, currentState.DateOfAgreement);

        try
        {
            var privacyPolicyUrl =
                "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/master/PrivacyPolicy.md";
            using (var client = new WebClient())
            {
                Markdown.Value = client.DownloadString(privacyPolicyUrl);
            }
        }
        catch (WebException e)
        {
            LogManager.GetCurrentClassLogger().Warn(e);
            LogManager.GetCurrentClassLogger().Warn("インターネットに接続されていないため、最新のプライバシーポリシーを確認できませんでした。");
            Markdown.Value = Resources.Message_CantDownloadPrivacyPolicy;
            AgreeDisagreeVisibility.Value = Visibility.Collapsed;
            OKVisibility.Value = Visibility.Visible;
        }
    }

    private void InsertOrUpdate(bool isAgree)
    {
        var markdown = Markdown.Value;
        var date = ExtractLatestDateOfEnactment(markdown);
        var dao = new PrivacyPolicyAgreementDao();
        var count = dao.CountBy(new Dictionary<string, object>
            { { nameof(PrivacyPolicyAgreement.DateOfEnactment), date } });
        if (count == 0)
        {
            var newPrivacyPolicyAgreement = new PrivacyPolicyAgreement
            {
                DateOfEnactment = date,
                IsAgree = isAgree
            };
            if (isAgree) newPrivacyPolicyAgreement.DateOfAgreement = DateTime.Now;
            dao.Insert(newPrivacyPolicyAgreement);
        }
        else
        {
            CurrentPrivacyPolicyAgreement.Value = PickoutPrivacyPolicyAgreementTop1AgreeOrDisagree();
            CurrentPrivacyPolicyAgreement.Value.IsAgree = isAgree;
            if (isAgree) CurrentPrivacyPolicyAgreement.Value.DateOfAgreement = DateTime.Now;
            dao.Update(CurrentPrivacyPolicyAgreement.Value);
        }
    }

    private PrivacyPolicyAgreement PickoutPrivacyPolicyAgreementTop1AgreeOrDisagree()
    {
        var dao = new PrivacyPolicyAgreementDao();
        var list = dao.GetAgreeOrDisagree();
        return list.AsValueEnumerable().FirstOrDefault();
    }

    private static DateTime ExtractLatestDateOfEnactment(string markdown)
    {
        var lines = markdown.Split("\n");
        foreach (var line in lines.AsValueEnumerable().Reverse())
        {
            var regex = new Regex("^改定：(?<year>\\d+?)年(?<month>\\d+?)月(?<day>\\d+?)日$");
            if (regex.IsMatch(line))
            {
                var mc = regex.Match(line);
                return DateTime.Parse($"{mc.Groups["year"].Value}/{mc.Groups["month"].Value}/{mc.Groups["day"].Value}");
            }

            regex = new Regex("^制定：(?<year>\\d+?)年(?<month>\\d+?)月(?<day>\\d+?)日$");
            if (regex.IsMatch(line))
            {
                var mc = regex.Match(line);
                return DateTime.Parse($"{mc.Groups["year"].Value}/{mc.Groups["month"].Value}/{mc.Groups["day"].Value}");
            }
        }

        throw new UnexpectedException("Can't extract date of enactment from markdown.");
    }
}