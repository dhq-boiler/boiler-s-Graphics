using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using boilersGraphics.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels;

public abstract class AbstractLetterDesignerItemViewModel : DesignerItemViewModelBase
{
    public AbstractLetterDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        Init();
    }

    public AbstractLetterDesignerItemViewModel()
    {
        Init();
    }

    public ReactivePropertySlim<bool> LetterSettingDialogIsOpen { get; } = new();

    public ReactivePropertySlim<string> LetterString { get; } = new(string.Empty);

    public ReactivePropertySlim<FontFamilyEx> SelectedFontFamily { get; } = new();

    public ReactivePropertySlim<bool> IsBold { get; } = new();

    public ReactivePropertySlim<bool> IsItalic { get; } = new();

    public ReactivePropertySlim<int> FontSize { get; } = new();

    public ReactivePropertySlim<bool> IsAutoLineBreak { get; } = new();

    public ReactiveCommand GotFocusCommand { get; } = new();
    public ReactiveCommand LostFocusCommand { get; } = new();

    public override bool SupportsPropertyDialog => false;

    public event EventHandler LetterSettingDialogClose;

    public abstract void OpenSettingDialog();

    protected void Init()
    {
        GotFocusCommand.Subscribe(x =>
            {
                if (!LetterSettingDialogIsOpen.Value) OpenSettingDialog();
            })
            .AddTo(_CompositeDisposable);
        LostFocusCommand.Subscribe(x =>
            {
                if (LetterSettingDialogIsOpen.Value) CloseLetterSettingDialog();
            })
            .AddTo(_CompositeDisposable);
        ShowConnectors = false;
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
        UpdatingStrategy.Value = PathGeometryUpdatingStrategy.Initial;
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
            var typeface = new Typeface(new FontFamilyEx(SelectedFontFamily.Value.FamilyName), fontStyle, fontWeight,
                FontStretches.Normal);
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
        rotatePathGeometry.Transform = new RotateTransform(angle, CenterPoint.Value.X, CenterPoint.Value.Y);
        return rotatePathGeometry;
    }

    public override Type GetViewType()
    {
        return typeof(Path);
    }
}