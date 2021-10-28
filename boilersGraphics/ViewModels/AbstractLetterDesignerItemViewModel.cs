using boilersGraphics.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public abstract class AbstractLetterDesignerItemViewModel : DesignerItemViewModelBase
    {
        private bool _LetterSettingDialogIsOpen = false;
        private string _LetterString = string.Empty;
        private FontFamilyEx _SelectedFontFamily;
        private bool _IsBold;
        private bool _IsItalic;
        private int _FontSize;
        private bool _AutoLineBreak;

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

        public FontFamilyEx SelectedFontFamily
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

        public bool AutoLineBreak
        {
            get { return _AutoLineBreak; }
            set { SetProperty(ref _AutoLineBreak, value); }
        }

        public event EventHandler LetterSettingDialogClose;

        public ReactiveCommand GotFocusCommand { get; } = new ReactiveCommand();
        public ReactiveCommand LostFocusCommand { get; } = new ReactiveCommand();

        public override bool SupportsPropertyDialog => false;

        public AbstractLetterDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public AbstractLetterDesignerItemViewModel()
            : base()
        {
            Init();
        }

        public abstract void OpenSettingDialog();

        protected void Init()
        {
            GotFocusCommand.Subscribe(x =>
            {
                if (!LetterSettingDialogIsOpen)
                {
                    OpenSettingDialog();
                }
            })
            .AddTo(_CompositeDisposable);
            LostFocusCommand.Subscribe(x =>
            {
                if (LetterSettingDialogIsOpen)
                {
                    CloseLetterSettingDialog();
                }
            })
            .AddTo(_CompositeDisposable);
            this.ShowConnectors = false;
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
            this.ObserveProperty(x => x.Width.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.AutoLineBreak)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            EnablePathGeometryUpdate.Value = true;
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
            if (SelectedFontFamily != null && FontSize > 0)
            {
                var fontStyle = IsItalic ? FontStyles.Italic : FontStyles.Normal;
                var fontWeight = IsBold ? FontWeights.Bold : FontWeights.Normal;
                var typeface = new Typeface(new FontFamilyEx(SelectedFontFamily.FamilyName), fontStyle, fontWeight, FontStretches.Normal);
                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                    return;
                if (AutoLineBreak)
                    WithLineBreak(glyphTypeface);
                else
                    WithoutLineBreak(glyphTypeface);
            }
            else
            {
                PathGeometry.Value = new PathGeometry();
            }
        }

        public abstract void WithLineBreak(GlyphTypeface glyphTypeface);
        public abstract void WithoutLineBreak(GlyphTypeface glyphTypeface);

        public override PathGeometry CreateGeometry()
        {
            RenderLetter();
            return PathGeometry.Value;
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            RenderLetter();
            var rotatePathGeometry = PathGeometry.Value.Clone();
            rotatePathGeometry.Transform = new RotateTransform(angle, this.CenterPoint.Value.X, this.CenterPoint.Value.Y);
            return rotatePathGeometry;
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }
    }
}
