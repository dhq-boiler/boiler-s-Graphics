﻿using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Reactive.Disposables;

namespace boilersGraphics.ViewModels
{
    public class ThicknessViewModel : BindableBase, IDialogAware
    {
        private CompositeDisposable disposables = new CompositeDisposable();

        public ReactivePropertySlim<BrushViewModel> ViewModel { get; } = new ReactivePropertySlim<BrushViewModel>();

        public ThicknessViewModel()
        {
        }

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
        }

        private void Value_ThicknessDialogClose(object sender, EventArgs e)
        {
            IDialogResult result = new DialogResult(ButtonResult.OK);
            RequestClose.Invoke(result);
        }
    }
}
