﻿using grapher.Models;
using grapher.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public class LetterDesignerItemViewModel : DesignerItemViewModelBase
    {
        private bool _LetterSettingDialogIsOpen = false;

        public bool LetterSettingDialogIsOpen
        {
            get { return _LetterSettingDialogIsOpen; }
            set { SetProperty(ref _LetterSettingDialogIsOpen, value); }
        }

        public event EventHandler LetterSettingDialogClose;

        public LetterDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public LetterDesignerItemViewModel()
        {
            Init();
            PropertyChanged += LetterDesignerItemViewModel_PropertyChanged;
        }

        private void LetterDesignerItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    if (IsSelected)
                    {
                        if (!LetterSettingDialogIsOpen)
                        {
                            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
                            IDialogResult result = null;
                            dialogService.Show(nameof(LetterSetting), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
                            LetterSettingDialogIsOpen = true;
                        }
                    }
                    else
                    {
                        if (LetterSettingDialogIsOpen)
                        {
                            if (LetterSettingDialogClose != null)
                                LetterSettingDialogClose(this, new EventArgs());
                            LetterSettingDialogIsOpen = false;
                        }
                    }
                    break;
            }
        }

        private void Init()
        {
            this.ShowConnectors = false;
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new LetterDesignerItemViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeColor = EdgeColor;
            clone.FillColor = FillColor;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            return clone;
        }

        #endregion //IClonable
    }
}
