﻿using boilersGraphics.Dao;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.IO;
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
            if (!File.Exists("LICENSE"))
            {
                var str = "LICENSEが見つかりません。";
                MessageBox.Show(str);
                return str;
            }

            using (var streamReader = new StreamReader(new FileStream("LICENSE", FileMode.Open)))
            {
                return streamReader.ReadToEnd();
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
