using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace boilersGraphics.ViewModels
{
    public class DetailViewModelBase<T> : BindableBase, IDialogAware, IDisposable where T : SelectableDesignerItemViewModelBase
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        private bool disposedValue;

        public string Title => "プロパティ";

        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();

        public event Action<IDialogResult> RequestClose;

        public DetailViewModelBase()
        {
            OKCommand.Subscribe(_ =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters() { { "ViewModel", ViewModel.Value } }));
            })
            .AddTo(disposables);

            As.Add(new A() { Value1 = "a1", Value2 = "b1" });
            As.Add(new A() { Value1 = "a2", Value2 = "b2" });
            As.Add(new A() { Value1 = "a3", Value2 = "b3" });
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public ReactivePropertySlim<T> ViewModel { get; } = new ReactivePropertySlim<T>();

        public ReactiveCollection<PropertyOptionsValueCombination> Properties { get; } = new ReactiveCollection<PropertyOptionsValueCombination>();

        public ReactiveCollection<A> As { get; } = new ReactiveCollection<A>();

        public class A
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
        }

        public virtual void SetProperties()
        { }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            ViewModel.Value = parameters.GetValue<T>("ViewModel");
            SetProperties();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OKCommand.Dispose();
                    ViewModel.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
