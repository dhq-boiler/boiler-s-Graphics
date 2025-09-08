using boilersGraphics.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Windows;
using R3;

namespace boilersGraphics.ViewModels;

internal class SetSnapPointViewModel : BindableBase, IDialogAware, IDisposable
{
    private CompositeDisposable _disposables = new();
    private bool disposedValue;

    public SetSnapPointViewModel()
    {
        OKCommand = X.CombineLatest(Y, (x, y) => x > 0 && y > 0)
            .ToReactiveCommand();
        OKCommand.Subscribe(x =>
            {
                var result = new DialogResult(ButtonResult.OK,
                    new DialogParameters { { "Point", new Point(X.Value, Y.Value) } });
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
        CancelCommand = new ReactiveCommand();
        CancelCommand.Subscribe(x =>
            {
                var result = new DialogResult(ButtonResult.Cancel, null);
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
    }

    public LayerItem LayerItem { get; set; }

    public BindableReactiveProperty<double> X { get; } = new();

    public BindableReactiveProperty<double> Y { get; } = new();

    public ReactiveCommand OKCommand { get; }
    public ReactiveCommand CancelCommand { get; }

    public string Title => $"{LayerItem.Name.Value}の移動";

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
        var point = parameters.GetValue<Point>("Point");
        LayerItem = parameters.GetValue<LayerItem>("LayerItem");
        X.Value = point.X;
        Y.Value = point.Y;
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
            if (disposing) _disposables.Dispose();

            _disposables = null;
            disposedValue = true;
        }
    }
}