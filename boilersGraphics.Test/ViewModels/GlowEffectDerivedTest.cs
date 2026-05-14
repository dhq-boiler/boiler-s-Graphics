using boilersGraphics.ViewModels;
using NUnit.Framework;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace boilersGraphics.Test.ViewModels;

/// <summary>
/// Phase 4-e-2: SelectableDesignerItemViewModelBase.GlowEffect 派生プロパティの動作確認。
/// CombineLatest なので GlowRadius / GlowIntensity / GlowColor / EdgeBrush のいずれの変更でも更新される。
/// </summary>
[TestFixture]
public class GlowEffectDerivedTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void GlowRadius0_GlowEffectはnull()
    {
        var vm = new NRectangleViewModel();
        Assert.That(vm.GlowEffect.Value, Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void GlowRadius0より大_DropShadowEffectが返る()
    {
        var vm = new NRectangleViewModel();
        vm.GlowRadius.Value = 5;
        Assert.That(vm.GlowEffect.Value, Is.InstanceOf<DropShadowEffect>());
    }

    [Test, Apartment(ApartmentState.STA)]
    public void DropShadowEffect_BlurRadiusはRadiusの2倍()
    {
        var vm = new NRectangleViewModel();
        vm.GlowRadius.Value = 6;
        var effect = (DropShadowEffect)vm.GlowEffect.Value;
        Assert.That(effect.BlurRadius, Is.EqualTo(12).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void DropShadowEffect_ShadowDepth0で擬似グロー()
    {
        var vm = new NRectangleViewModel();
        vm.GlowRadius.Value = 3;
        var effect = (DropShadowEffect)vm.GlowEffect.Value;
        Assert.That(effect.ShadowDepth, Is.EqualTo(0));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void GlowColorがnullならEdgeBrushの色を使う()
    {
        var vm = new NRectangleViewModel();
        vm.EdgeBrush.Value = new SolidColorBrush(Colors.Red);
        vm.GlowRadius.Value = 4;
        vm.GlowColor.Value = null;
        var effect = (DropShadowEffect)vm.GlowEffect.Value;
        Assert.That(effect.Color, Is.EqualTo(Colors.Red));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void GlowColorがある場合はその色()
    {
        var vm = new NRectangleViewModel();
        vm.EdgeBrush.Value = new SolidColorBrush(Colors.Red);
        vm.GlowRadius.Value = 4;
        vm.GlowColor.Value = Colors.Blue;
        var effect = (DropShadowEffect)vm.GlowEffect.Value;
        Assert.That(effect.Color, Is.EqualTo(Colors.Blue));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void IntensityがOpacityにクランプされる()
    {
        var vm = new NRectangleViewModel();
        vm.GlowRadius.Value = 4;
        vm.GlowIntensity.Value = 1.5;
        var effect = (DropShadowEffect)vm.GlowEffect.Value;
        Assert.That(effect.Opacity, Is.EqualTo(1.0));

        vm.GlowIntensity.Value = -0.3;
        effect = (DropShadowEffect)vm.GlowEffect.Value;
        Assert.That(effect.Opacity, Is.EqualTo(0.0));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void GlowRadiusを0に戻すとEffectがnullに戻る()
    {
        var vm = new NRectangleViewModel();
        vm.GlowRadius.Value = 5;
        Assert.That(vm.GlowEffect.Value, Is.Not.Null);
        vm.GlowRadius.Value = 0;
        Assert.That(vm.GlowEffect.Value, Is.Null);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void EdgeBrushがSolidColorBrush以外ならフォールバック白()
    {
        var vm = new NRectangleViewModel();
        vm.EdgeBrush.Value = new LinearGradientBrush(Colors.Red, Colors.Blue, 0);
        vm.GlowRadius.Value = 4;
        vm.GlowColor.Value = null;
        var effect = (DropShadowEffect)vm.GlowEffect.Value;
        // フォールバックは Colors.White
        Assert.That(effect.Color, Is.EqualTo(Colors.White));
    }
}
