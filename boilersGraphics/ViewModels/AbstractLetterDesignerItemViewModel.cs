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
        public ReactivePropertySlim<bool> LetterSettingDialogIsOpen { get; } = new ReactivePropertySlim<bool>(false);

        public ReactivePropertySlim<string> LetterString { get; } = new ReactivePropertySlim<string>(string.Empty);

        public ReactivePropertySlim<FontFamilyEx> SelectedFontFamily { get; } = new ReactivePropertySlim<FontFamilyEx>();

        public ReactivePropertySlim<bool> IsBold { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> IsItalic { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<int> FontSize { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<bool> IsAutoLineBreak { get; } = new ReactivePropertySlim<bool>();

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
                if (!LetterSettingDialogIsOpen.Value)
                {
                    OpenSettingDialog();
                }
            })
            .AddTo(_CompositeDisposable);
            LostFocusCommand.Subscribe(x =>
            {
                if (LetterSettingDialogIsOpen.Value)
                {
                    CloseLetterSettingDialog();
                }
            })
            .AddTo(_CompositeDisposable);
            this.ShowConnectors = false;
            this.ObserveProperty(x => x.LetterString.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.SelectedFontFamily.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.IsBold.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.IsItalic.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.FontSize.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.Width.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.IsAutoLineBreak.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            EnablePathGeometryUpdate.Value = true;
        }

        public void CloseLetterSettingDialog()
        {
            if (LetterSettingDialogIsOpen.Value)
            {
                LetterSettingDialogClose?.Invoke(this, new EventArgs());
                LetterSettingDialogIsOpen.Value = false;
            }
        }

        private void RenderLetter()
        {
            if (SelectedFontFamily.Value is not null && FontSize.Value > 0)
            {
                var fontStyle = IsItalic.Value ? FontStyles.Italic : FontStyles.Normal;
                var fontWeight = IsBold.Value ? FontWeights.Bold : FontWeights.Normal;
                var typeface = new Typeface(new FontFamilyEx(SelectedFontFamily.Value.FamilyName), fontStyle, fontWeight, FontStretches.Normal);
                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                    return;
                if (IsAutoLineBreak.Value)
                    WithLineBreak(glyphTypeface);
                else
                    WithoutLineBreak(glyphTypeface);
            }
            else
            {
                PathGeometryNoRotate.Value = new PathGeometry();
            }
        }

        public abstract void WithLineBreak(GlyphTypeface glyphTypeface);
        public abstract void WithoutLineBreak(GlyphTypeface glyphTypeface);

        public override PathGeometry CreateGeometry(bool flag = false)
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
