using grapher.Controls;
using grapher.Extensions;
using grapher.Models;
using grapher.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public class LetterDesignerItemViewModel : DesignerItemViewModelBase
    {
        private bool _LetterSettingDialogIsOpen = false;
        private string _LetterString = string.Empty;
        private System.Windows.Media.FontFamily _SelectedFontFamily;
        private bool _IsBold;
        private bool _IsItalic;
        private int _FontSize;
        private PathGeometry _PathGeometry;

        public bool LetterSettingDialogIsOpen
        {
            get { return _LetterSettingDialogIsOpen; }
            set { SetProperty(ref _LetterSettingDialogIsOpen, value); }
        }

        public string LetterString
        {
            get { return _LetterString; }
            set { SetProperty(ref _LetterString, value); }
        }

        public System.Windows.Media.FontFamily SelectedFontFamily
        {
            get { return _SelectedFontFamily; }
            set { SetProperty(ref _SelectedFontFamily, value); }
        }

        public bool IsBold
        {
            get { return _IsBold; }
            set { SetProperty(ref _IsBold, value); }
        }

        public bool IsItalic
        {
            get { return _IsItalic; }
            set { SetProperty(ref _IsItalic, value); }
        }

        public int FontSize
        {
            get { return _FontSize; }
            set { SetProperty(ref _FontSize, value); }
        }

        public PathGeometry PathGeometry
        {
            get { return _PathGeometry; }
            set { SetProperty(ref _PathGeometry, value); }
        }

        public event EventHandler LetterSettingDialogClose;

        public LetterDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public LetterDesignerItemViewModel()
            : base()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
            this.ObserveProperty(x => x.IsSelected)
                .Subscribe(isSelected =>
                {
                    if (isSelected)
                    {
                        if (!LetterSettingDialogIsOpen)
                        {
                            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
                            IDialogResult result = null;
                            dialogService.Show(nameof(LetterSetting), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
                            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                            designerCanvas.Focus();
                            LetterSettingDialogIsOpen = true;
                        }
                    }
                    else
                    {
                        CloseLetterSettingDialog();
                    }
                })
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.LetterString)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.SelectedFontFamily)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.IsBold)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.IsItalic)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.FontSize)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
        }

        public void CloseLetterSettingDialog()
        {
            if (LetterSettingDialogIsOpen)
            {
                LetterSettingDialogClose?.Invoke(this, new EventArgs());
                LetterSettingDialogIsOpen = false;
            }
        }

        private void RenderLetter()
        {
            if (!string.IsNullOrEmpty(LetterString) &&
                SelectedFontFamily != null && SelectedFontFamily.BaseUri != null &&
                FontSize > 0)
            {
                foreach (var @char in LetterString)
                {
                    //ここから GlyphTypeface.ctor -> GlyphTypeface.Initialize -> FontCacheUtil.SplitFontFaceIndex のLine507-518までを抜き出した
                    //var components = SelectedFontFamily.BaseUri.GetComponents(UriComponents.Fragment, UriFormat.SafeUnescaped);
                    //if (!string.IsNullOrEmpty(components))
                    //{
                    //    var faceIndex = 0;
                    //    if (!int.TryParse(components, NumberStyles.None, CultureInfo.InvariantCulture, out faceIndex))
                    //    {
                    //        throw new ArgumentException("FaceIndexMustBePositiveOrZero", "fontUri"); //ここで例外発生
                    //    }
                    //}
                    //ここまで
                    GlyphTypeface glyphTypeface = new GlyphTypeface(SelectedFontFamily.BaseUri);
                    ushort glyphIndex;
                    glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                    Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize, FontSize);
                    PathGeometry pg = geometry.GetOutlinedPathGeometry();
                    PathGeometry = pg;
                }
            }
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
