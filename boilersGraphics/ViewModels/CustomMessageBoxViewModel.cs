using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using ObservableCollections;
using R3;
using ZLinq;

namespace boilersGraphics.ViewModels;

internal class CustomMessageBoxViewModel : IDialogAware, IDisposable
{
    private CompositeDisposable disposables = new();
    private bool disposedValue;

    public CustomMessageBoxViewModel()
    {
        OKCommand.Subscribe(_ => { RequestClose.Invoke(new DialogResult(ButtonResult.OK)); })
            .AddTo(disposables);
    }

    public BindableReactiveProperty<string> Text { get; } = new();

    public ReactiveCommand OKCommand { get; } = new();

    public NotifyCollectionChangedSynchronizedViewList<Button> Buttons { get; set; } = new ObservableList<Button>().ToWritableNotifyCollectionChanged();

    public string Title { get; private set; }

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
        Title = parameters.GetValue<string>("Title");
        Text.Value = parameters.GetValue<string>("Text");
        var buttons = parameters.GetValue<IEnumerable<Button>>("Buttons");
        buttons.AsValueEnumerable().ToList().ForEach(button => Buttons.Add(button));
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) disposables.Dispose();
            disposables = null;
            disposedValue = true;
        }
    }

    public class Button : IDisposable
    {
        private bool disposedValue;

        public Button(string content, ICommand clickCommand)
        {
            Content.Value = content;
            ClickCommand = clickCommand;
        }

        public BindableReactiveProperty<string> Content { get; set; } = new();

        public ICommand ClickCommand { get; set; }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) Content.Dispose();

                Content = null;
                disposedValue = true;
            }
        }
    }
}