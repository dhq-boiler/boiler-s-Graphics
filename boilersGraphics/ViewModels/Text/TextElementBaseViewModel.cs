using boilersGraphics.Helpers;
using boilersGraphics.Models.Text;
using R3;
using System;
using System.Windows.Media;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels.Text;

/// <summary>
/// Phase 2-a §4.1: TextElementBase (Model) を VM 層でラップする抽象 ViewModel。
/// Letter ファミリと同様、DesignerItemViewModelBase を直接派生して位置・サイズ・選択など既存基盤を踏襲する。
/// Phase 2-b 最小実装ではプロパティダイアログ等の UI 経路はまだ持たない。
/// </summary>
public abstract class TextElementBaseViewModel : DesignerItemViewModelBase
{
    public TextElementBase Model { get; }

    public BindableReactiveProperty<string> Text { get; }
    public BindableReactiveProperty<string> FontFamily { get; }
    public BindableReactiveProperty<int> FontSize { get; }
    public BindableReactiveProperty<Brush> Foreground { get; }
    public BindableReactiveProperty<Brush> Background { get; }
    public BindableReactiveProperty<double?> LineHeight { get; }
    public BindableReactiveProperty<double> LetterSpacing { get; }
    public BindableReactiveProperty<double> TextOpacity { get; }
    public BindableReactiveProperty<bool> IsWordWrap { get; }

    /// <summary>
    /// Phase 6.5: テキスト系図形のキャンバス上ダブルクリックで OpenPropertyDialog を発火させる Command。
    /// DataTemplate 側で <c>{Binding MouseDoubleClickCommand}</c> として InvokeCommandAction に渡される。
    /// SupportsPropertyDialog=False の派生 (Mono/DataGen/NumSeq) では OpenPropertyDialog が no-op、
    /// True の TextMatrix/TextOnPath では Detail dialog が起動する。
    /// </summary>
    public ReactiveCommand MouseDoubleClickCommand { get; } = new();

    protected TextElementBaseViewModel(TextElementBase model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));

        Text = new BindableReactiveProperty<string>(Model.Text);
        FontFamily = new BindableReactiveProperty<string>(Model.FontFamily);
        FontSize = new BindableReactiveProperty<int>(Model.FontSize);
        Foreground = new BindableReactiveProperty<Brush>(Model.Foreground);
        Background = new BindableReactiveProperty<Brush>(Model.Background);
        LineHeight = new BindableReactiveProperty<double?>(Model.LineHeight);
        LetterSpacing = new BindableReactiveProperty<double>(Model.LetterSpacing);
        TextOpacity = new BindableReactiveProperty<double>(Model.TextOpacity);
        IsWordWrap = new BindableReactiveProperty<bool>(Model.IsWordWrap);

        Text.Subscribe(v => Model.Text = v).AddTo(_CompositeDisposable);
        FontFamily.Subscribe(v => Model.FontFamily = v).AddTo(_CompositeDisposable);
        FontSize.Subscribe(v => Model.FontSize = v).AddTo(_CompositeDisposable);
        Foreground.Subscribe(v => Model.Foreground = v).AddTo(_CompositeDisposable);
        Background.Subscribe(v => Model.Background = v).AddTo(_CompositeDisposable);
        LineHeight.Subscribe(v => Model.LineHeight = v).AddTo(_CompositeDisposable);
        LetterSpacing.Subscribe(v => Model.LetterSpacing = v).AddTo(_CompositeDisposable);
        TextOpacity.Subscribe(v => Model.TextOpacity = v).AddTo(_CompositeDisposable);
        IsWordWrap.Subscribe(v => Model.IsWordWrap = v).AddTo(_CompositeDisposable);

        // Phase 6.5: ダブルクリック -> OpenPropertyDialog 経路を有効化する。
        // NRectangleViewModel と同じ pattern。SupportsPropertyDialog の判定は OpenPropertyDialog 実装側で行う。
        MouseDoubleClickCommand.Subscribe(_ => OpenPropertyDialog()).AddTo(_CompositeDisposable);
    }

    public override System.Windows.Media.PathGeometry CreateGeometry(bool flag = false)
    {
        return GeometryCreator.CreateRectangle(this, 0, 0, flag);
    }

    public override Type GetViewType() => typeof(Path);

    public override void OpenPropertyDialog()
    {
    }

    /// <summary>
    /// 派生 Clone から呼ぶ。位置/サイズ/共通テキスト属性のコピーを一括で扱う。
    /// </summary>
    protected void CopyCommonPropertiesTo(TextElementBaseViewModel target)
    {
        target.Owner = Owner;
        target.Left.Value = Left.Value;
        target.Top.Value = Top.Value;
        target.Width.Value = Width.Value;
        target.Height.Value = Height.Value;
        target.RotationAngle.Value = RotationAngle.Value;

        target.Text.Value = Text.Value;
        target.FontFamily.Value = FontFamily.Value;
        target.FontSize.Value = FontSize.Value;
        target.Foreground.Value = Foreground.Value;
        target.Background.Value = Background.Value;
        target.LineHeight.Value = LineHeight.Value;
        target.LetterSpacing.Value = LetterSpacing.Value;
        target.TextOpacity.Value = TextOpacity.Value;
        target.IsWordWrap.Value = IsWordWrap.Value;
    }
}
