﻿using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace boilersGraphics.ViewModels;

public class ThicknessViewModel : BindableBase, IDialogAware
{
    private readonly CompositeDisposable disposables = new();

    public ThicknessViewModel()
    {
        Left.Subscribe(x => { StaticLeft = x; })
            .AddTo(disposables);
        Top.Subscribe(x => { StaticTop = x; })
            .AddTo(disposables);
    }

    public static double StaticLeft { get; set; }

    public static double StaticTop { get; set; }

    public ReactivePropertySlim<BrushViewModel> ViewModel { get; } = new();

    public ReactivePropertySlim<double> Left { get; } = new();
    public ReactivePropertySlim<double> Top { get; } = new();

    public string Title => "ブラシの太さ";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
        ViewModel.Value.ThicknessDialogClose -= Value_ThicknessDialogClose;
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        ViewModel.Value = parameters.GetValue<BrushViewModel>("ViewModel");
        ViewModel.Value.ThicknessDialogClose += Value_ThicknessDialogClose;
        Left.Value = StaticLeft;
        Top.Value = StaticTop;
    }

    private void Value_ThicknessDialogClose(object sender, EventArgs e)
    {
        IDialogResult result = new DialogResult(ButtonResult.OK);
        RequestClose.Invoke(result);
    }
}