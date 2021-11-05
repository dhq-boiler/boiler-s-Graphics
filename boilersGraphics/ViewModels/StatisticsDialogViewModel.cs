using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Reactive.Linq;

namespace boilersGraphics.ViewModels
{
    internal class StatisticsDialogViewModel : BindableBase, IDialogAware
    {
        public MainWindowViewModel MainWindowViewModel { get; set; }
        public DelegateCommand LoadedCommand { get; }

        public ReactivePropertySlim<int> NumberOfBoots { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfTimesTheFileWasOpenedBySpecifyingIt { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfClicksWithThePointerTool { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> CumulativeTotalOfItemsSelectedWithTheLassoTool { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<TimeSpan> Uptime { get; } = new ReactivePropertySlim<TimeSpan>();

        public string Title => "統計";

        public StatisticsDialogViewModel()
        {
            LoadedCommand = new DelegateCommand(() => Load());
        }

#pragma warning disable CS0067

        public event Action<IDialogResult> RequestClose;

#pragma warning restore CS0067

        public void Load()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            NumberOfBoots.Value = statistics.NumberOfBoots;
            NumberOfTimesTheFileWasOpenedBySpecifyingIt.Value = statistics.NumberOfTimesTheFileWasOpenedBySpecifyingIt;
            NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened.Value = statistics.NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened;
            NumberOfClicksWithThePointerTool.Value = statistics.NumberOfClicksWithThePointerTool;
            CumulativeTotalOfItemsSelectedWithTheLassoTool.Value = statistics.CumulativeTotalOfItemsSelectedWithTheLassoTool;
            Observable.Timer(DateTime.Now, TimeSpan.FromSeconds(1))
                      .Subscribe(_ =>
                      {
                          App.Current.Dispatcher.Invoke(() =>
                          {
                              lock (App.Current)
                              {
                                  var statistics = MainWindowViewModel.Statistics.Value;
                                  Uptime.Value = TimeSpan.FromTicks(statistics.UptimeTicks);
                              }
                          });
                      });
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
            MainWindowViewModel = parameters.GetValue<MainWindowViewModel>("MainWindowViewModel");
        }
    }
}
