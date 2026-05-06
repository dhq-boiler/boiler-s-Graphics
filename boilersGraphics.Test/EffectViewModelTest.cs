using boilersGraphics.ViewModels;
using NUnit.Framework;
using System.Windows.Controls;

namespace boilersGraphics.Test
{
    // Lightweight smoke coverage for the three concrete Effect view-models
    // (Blur, Mosaic, ColorCorrect). Render() and Initialize() pull in
    // Application.Current.MainWindow, OpenCV, and a real visual tree, so they
    // can't run unmodified inside the unit-test process. These tests instead
    // pin down the parts that are pure model state: defaults, property
    // round-trips, GetViewType, and SupportsPropertyDialog. If any of these
    // values change in production code the tests should be updated to match.
    [TestFixture]
    public class EffectViewModelTest
    {
        #region BlurEffectViewModel

        [Test]
        public void Blur_Defaults_MatchProductionValues()
        {
            var blur = new BlurEffectViewModel();

            Assert.That(blur.KernelWidth.Value, Is.EqualTo(111d));
            Assert.That(blur.KernelHeight.Value, Is.EqualTo(111d));
            Assert.That(blur.Sigma.Value, Is.EqualTo(16d));
        }

        [Test]
        public void Blur_GetViewType_IsImage()
        {
            var blur = new BlurEffectViewModel();
            Assert.That(blur.GetViewType(), Is.EqualTo(typeof(Image)));
        }

        [Test]
        public void Blur_SupportsPropertyDialog_IsTrue()
        {
            var blur = new BlurEffectViewModel();
            Assert.That(blur.SupportsPropertyDialog, Is.True);
        }

        [Test]
        public void Blur_Properties_AssignmentRoundTrips()
        {
            var blur = new BlurEffectViewModel();

            blur.KernelWidth.Value = 31;
            blur.KernelHeight.Value = 21;
            blur.Sigma.Value = 4.5;

            Assert.That(blur.KernelWidth.Value, Is.EqualTo(31d));
            Assert.That(blur.KernelHeight.Value, Is.EqualTo(21d));
            Assert.That(blur.Sigma.Value, Is.EqualTo(4.5d));
        }

        #endregion

        #region MosaicViewModel

        [Test]
        public void Mosaic_Defaults_MatchProductionValues()
        {
            var mosaic = new MosaicViewModel();

            Assert.That(mosaic.ColumnPixels.Value, Is.EqualTo(30d));
            Assert.That(mosaic.RowPixels.Value, Is.EqualTo(30d));
        }

        [Test]
        public void Mosaic_GetViewType_IsImage()
        {
            var mosaic = new MosaicViewModel();
            Assert.That(mosaic.GetViewType(), Is.EqualTo(typeof(Image)));
        }

        [Test]
        public void Mosaic_SupportsPropertyDialog_IsTrue()
        {
            var mosaic = new MosaicViewModel();
            Assert.That(mosaic.SupportsPropertyDialog, Is.True);
        }

        [Test]
        public void Mosaic_Properties_AssignmentRoundTrips()
        {
            var mosaic = new MosaicViewModel();

            mosaic.ColumnPixels.Value = 50;
            mosaic.RowPixels.Value = 75;

            Assert.That(mosaic.ColumnPixels.Value, Is.EqualTo(50d));
            Assert.That(mosaic.RowPixels.Value, Is.EqualTo(75d));
        }

        #endregion

        #region ColorCorrectViewModel

        [Test]
        public void ColorCorrect_Defaults_MatchProductionValues()
        {
            var cc = new ColorCorrectViewModel();

            // CCType / TargetChannel are the dropdowns at the top of the
            // dialog; their initial values determine which sub-panel renders.
            Assert.That(cc.CCType.Value, Is.InstanceOf<Hsv>());
            Assert.That(cc.TargetChannel.Value, Is.EqualTo(Channel.RGB));

            // HSV defaults: no offset on any channel.
            Assert.That(cc.AddHue.Value, Is.EqualTo(0));
            Assert.That(cc.AddSaturation.Value, Is.EqualTo(0));
            Assert.That(cc.AddValue.Value, Is.EqualTo(0));

            // Binarization defaults: Otsu off, threshold = 0, max = 255,
            // mode = Binary. These map to OpenCV's cv::threshold defaults.
            Assert.That(cc.Threshold.Value, Is.EqualTo(0d));
            Assert.That(cc.MaxValue.Value, Is.EqualTo(255d));
            Assert.That(cc.ThresholdTypes.Value, Is.EqualTo(boilersGraphics.ViewModels.ThresholdTypes.Binary));
            Assert.That(cc.OtsuEnabled.Value, Is.False);
        }

        [Test]
        public void ColorCorrect_GetViewType_IsImage()
        {
            var cc = new ColorCorrectViewModel();
            Assert.That(cc.GetViewType(), Is.EqualTo(typeof(Image)));
        }

        [Test]
        public void ColorCorrect_SupportsPropertyDialog_IsTrue()
        {
            var cc = new ColorCorrectViewModel();
            Assert.That(cc.SupportsPropertyDialog, Is.True);
        }

        [Test]
        public void ColorCorrect_CCType_SwitchesAcrossSupportedModes()
        {
            var cc = new ColorCorrectViewModel();

            cc.CCType.Value = ColorCorrectType.ToneCurve;
            Assert.That(cc.CCType.Value, Is.InstanceOf<ToneCurve>());

            cc.CCType.Value = ColorCorrectType.NegativePositiveConversion;
            Assert.That(cc.CCType.Value, Is.InstanceOf<NegativePositiveConversion>());

            cc.CCType.Value = ColorCorrectType.Binarization;
            Assert.That(cc.CCType.Value, Is.InstanceOf<Binarization>());

            cc.CCType.Value = ColorCorrectType.HSV;
            Assert.That(cc.CCType.Value, Is.InstanceOf<Hsv>());
        }

        [Test]
        public void ColorCorrect_HsvOffsets_AssignmentRoundTrips()
        {
            var cc = new ColorCorrectViewModel();

            cc.AddHue.Value = 45;
            cc.AddSaturation.Value = -30;
            cc.AddValue.Value = 90;

            Assert.That(cc.AddHue.Value, Is.EqualTo(45));
            Assert.That(cc.AddSaturation.Value, Is.EqualTo(-30));
            Assert.That(cc.AddValue.Value, Is.EqualTo(90));
        }

        [Test]
        public void ColorCorrect_BinarizationProperties_AssignmentRoundTrips()
        {
            var cc = new ColorCorrectViewModel();

            cc.Threshold.Value = 128d;
            cc.MaxValue.Value = 200d;
            cc.OtsuEnabled.Value = true;
            cc.ThresholdTypes.Value = boilersGraphics.ViewModels.ThresholdTypes.BinaryInv;

            Assert.That(cc.Threshold.Value, Is.EqualTo(128d));
            Assert.That(cc.MaxValue.Value, Is.EqualTo(200d));
            Assert.That(cc.OtsuEnabled.Value, Is.True);
            Assert.That(cc.ThresholdTypes.Value, Is.EqualTo(boilersGraphics.ViewModels.ThresholdTypes.BinaryInv));
        }

        #endregion
    }
}
