using boilersGraphics.Dao;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.ViewModels
{
    internal class StatisticsDialogViewModel : BindableBase, IDialogAware
    {
        public DelegateCommand LoadedCommand { get; }

        public ReactivePropertySlim<int> NumberOfBoots { get; } = new ReactivePropertySlim<int>();

        public string Title => "統計";

        public StatisticsDialogViewModel()
        {
            LoadedCommand = new DelegateCommand(() => Load());
        }

        public event Action<IDialogResult> RequestClose;

        public void Load()
        {
            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var appDao = new StatisticsDao();
            var statistics = appDao.FindBy(new Dictionary<string, object>() { { "ID", id } }).First();
            NumberOfBoots.Value = statistics.NumberOfBoots;
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
