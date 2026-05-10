using DependencyPropertyGenerator;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace boilersGraphics.Views;
/*
 * CoMoの日記 リサイズハンドルをAdornerで実装する
 * http://d.hatena.ne.jp/CoMo/20110428/1303996288
 */

[AttachedDependencyProperty<ControlTemplate, FrameworkElement>("Template")]
public partial class AdornedBy : Adorner
{
    //テンプレート描画用のControlオブジェクトへの参照
    private FrameworkElement _Content;

    //コンストラクタ
    public AdornedBy(UIElement adornedElement) : base(adornedElement)
    {
    }

    //描画されるために不可欠なので実装をしておく
    protected override int VisualChildrenCount => 1;

    //添付プロパティTemplateの設定時に初期化処理を行う
    static partial void OnTemplateChanged(FrameworkElement sender, ControlTemplate newValue)
    {
        var adorned = sender;
        var me = new AdornedBy(adorned);

        //装飾層に登録する
        if (adorned.IsInitialized)
            me.AddToAdornerLayer();
        else
            //初期化中の場合は登録処理を遅延させる
            adorned.Loaded += (_, __) => me.AddToAdornerLayer();

        //子Controlオブジェクトを生成して設定されたテンプレートを設定する
        var t = newValue;
        var ctrl = new Control { Template = t };
        var sourceElement = me.AdornedElement as FrameworkElement;
        ctrl.DataContext = sourceElement.DataContext;
        me._Content = ctrl;
        me.AddVisualChild(ctrl);
        me.AddLogicalChild(ctrl);
        me.InvalidateVisual();
    }

    //装飾層に登録する
    private void AddToAdornerLayer()
    {
        var layer = AdornerLayer.GetAdornerLayer(AdornedElement);
        if (layer == null) throw new InvalidOperationException("XAML tree must have at lest one AdornerDecorator.");
        //既存の装飾を除去
        Adorner[] registed = layer.GetAdorners(AdornedElement);
        if (registed != null)
            foreach (var ad in registed)
                if (ad is AdornedBy)
                    layer.Remove(ad);
        //装飾を登録
        layer.Add(this);
    }

    //テンプレート中の要素から装飾対象を取得するヘルパーメソッド
    public static UIElement GetAdornedElementFromTemplateChild(FrameworkElement contained)
    {
        var tp = contained.TemplatedParent as FrameworkElement;
        if (tp == null || tp.GetType() != typeof(Control)) return null;
        var me = tp.Parent as AdornedBy;
        if (me == null) return null;
        return me.AdornedElement;
    }


    //サイズ計測処理の実装 (テンプレートの大きさを装飾対象に一致させる)
    protected override Size MeasureOverride(Size constraint)
    {
        return AdornedElement.DesiredSize;
    }

    //配置処理の実装 (テンプレートの位置を装飾対象に一致させる)
    protected override Size ArrangeOverride(Size finalSize)
    {
        _Content.Arrange(new Rect(AdornedElement.DesiredSize));
        return AdornedElement.DesiredSize;
    }

    protected override Visual GetVisualChild(int index)
    {
        return _Content;
    }
}
