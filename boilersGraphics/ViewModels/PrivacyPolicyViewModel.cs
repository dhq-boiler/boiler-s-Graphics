using boilersGraphics.Dao;
using boilersGraphics.Exceptions;
using boilersGraphics.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class PrivacyPolicyViewModel : BindableBase, IDialogAware
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        public string Title => "Privacy Policy";

        public event Action<IDialogResult> RequestClose;

        public ReactiveCommand AgreeCommand { get; } = new ReactiveCommand();
        public ReactiveCommand DisagreeCommand { get; } = new ReactiveCommand();
        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();

        public ReactivePropertySlim<Visibility> AgreeDisagreeVisibility { get; } = new ReactivePropertySlim<Visibility>();

        public ReactivePropertySlim<Visibility> OKVisibility { get; } = new ReactivePropertySlim<Visibility>();

        public ReactivePropertySlim<string> Markdown { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<PrivacyPolicyAgreement> CurrentPrivacyPolicyAgreement { get; } = new ReactivePropertySlim<PrivacyPolicyAgreement>();

        public PrivacyPolicyViewModel()
        {
            AgreeCommand.Subscribe(_ =>
            {
                InsertOrUpdate(true);
                RequestClose.Invoke(new DialogResult(ButtonResult.Yes));
            })
            .AddTo(disposables);
            DisagreeCommand.Subscribe(_ =>
            {
                InsertOrUpdate(false);
                RequestClose.Invoke(new DialogResult(ButtonResult.No));
            })
            .AddTo(disposables);
            OKCommand.Subscribe(_ =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
            })
            .AddTo(disposables);
        }

        private void InsertOrUpdate(bool isAgree)
        {
            var markdown = Markdown.Value;
            DateTime date = ExtractLatestDateOfEnactment(markdown);
            var dao = new PrivacyPolicyAgreementDao();
            var count = dao.CountBy(new Dictionary<string, object>() { { nameof(PrivacyPolicyAgreement.DateOfEnactment), date } });
            if (count == 0)
            {
                var newPrivacyPolicyAgreement = new PrivacyPolicyAgreement()
                {
                    DateOfEnactment = date,
                    IsAgree = isAgree,
                };
                if (isAgree)
                {
                    newPrivacyPolicyAgreement.DateOfAgreement = DateTime.Now;
                }
                dao.Insert(newPrivacyPolicyAgreement);
            }
            else
            {
                CurrentPrivacyPolicyAgreement.Value = PickoutPrivacyPolicyAgreementTop1AgreeOrDisagree();
                CurrentPrivacyPolicyAgreement.Value.IsAgree = isAgree;
                if (isAgree)
                {
                    CurrentPrivacyPolicyAgreement.Value.DateOfAgreement = DateTime.Now;
                }
                dao.Update(CurrentPrivacyPolicyAgreement.Value);
            }
        }

        private PrivacyPolicyAgreement PickoutPrivacyPolicyAgreementTop1AgreeOrDisagree()
        {
            var dao = new PrivacyPolicyAgreementDao();
            var list = dao.GetAgreeOrDisagree();
            return list.FirstOrDefault();
        }

        private static DateTime ExtractLatestDateOfEnactment(string markdown)
        {
            var lines = markdown.Split("\n");
            foreach (var line in lines)
            {
                var regex = new Regex("^制定：(?<year>\\d+?)年(?<month>\\d+?)月(?<day>\\d+?)日$");
                if (regex.IsMatch(line))
                {
                    var mc = regex.Match(line);
                    return DateTime.Parse($"{mc.Groups["year"].Value}/{mc.Groups["month"].Value}/{mc.Groups["day"].Value}");
                }
            }
            throw new UnexpectedException($"Can't extract date of enactment from markdown.");
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
            var currentState = parameters.GetValue<PrivacyPolicyAgreement>("CurrentStatus");
            CurrentPrivacyPolicyAgreement.Value = currentState;
            AgreeDisagreeVisibility.Value = currentState == null || !currentState.IsAgree ? Visibility.Visible : Visibility.Collapsed;
            OKVisibility.Value = currentState == null || !currentState.IsAgree ? Visibility.Collapsed : Visibility.Visible;

            var privacyPolicyUrl = "https://raw.githubusercontent.com/dhq-boiler/boiler-s-Graphics/master/PrivacyPolicy.md";
            using (var client = new WebClient())
            {
                Markdown.Value = client.DownloadString(privacyPolicyUrl);
            }
        }
    }
}
