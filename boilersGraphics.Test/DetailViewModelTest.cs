using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class DetailViewModelTest
    {
        private static IRegionManager NewRegionManager()
        {
            // Mock<IRegionManager>: Regions.Remove(name) と RequestNavigate(...) を no-op で返す
            var regionManager = new Mock<IRegionManager>();
            var regions = new Mock<IRegionCollection>();
            regions.Setup(x => x.Remove(It.IsAny<string>())).Returns(true);
            regionManager.SetupGet(x => x.Regions).Returns(regions.Object);
            regionManager.Setup(x => x.RequestNavigate(It.IsAny<string>(), It.IsAny<string>()))
                .Verifiable();
            return regionManager.Object;
        }

        private static IDialogParameters DialogParametersWith<T>(T viewModel)
        {
            var p = new DialogParameters();
            p.Add("ViewModel", viewModel);
            return p;
        }

        private static MainWindowViewModel NewMainWindowVM()
        {
            App.IsTest = true;
            var dlg = new Mock<IDialogService>();
            return new MainWindowViewModel(dlg.Object);
        }

        // ---- DetailViewModelBase 共通動作 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Base_CanCloseDialogはtrue()
        {
            var vm = new DetailRectangleViewModel(NewRegionManager());
            Assert.That(vm.CanCloseDialog(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Base_IsNavigationTargetはfalse()
        {
            var vm = new DetailRectangleViewModel(NewRegionManager());
            Assert.That(vm.IsNavigationTarget(null), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Base_OnNavigatedToとOnNavigatedFromは例外なし()
        {
            var vm = new DetailRectangleViewModel(NewRegionManager());
            Assert.That(() => vm.OnNavigatedTo(null), Throws.Nothing);
            Assert.That(() => vm.OnNavigatedFrom(null), Throws.Nothing);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Base_Titleは非null()
        {
            var vm = new DetailRectangleViewModel(NewRegionManager());
            Assert.That(vm.Title, Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Base_OnDialogClosedでRegionsRemoveが呼ばれる()
        {
            var rm = new Mock<IRegionManager>();
            var regions = new Mock<IRegionCollection>();
            regions.Setup(x => x.Remove("DetailRegion")).Returns(true).Verifiable();
            rm.SetupGet(x => x.Regions).Returns(regions.Object);

            var vm = new DetailRectangleViewModel(rm.Object);
            vm.OnDialogClosed();

            regions.Verify(x => x.Remove("DetailRegion"), Times.Once);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Base_Dispose冪等()
        {
            var vm = new DetailRectangleViewModel(NewRegionManager());
            Assert.That(() => { vm.Dispose(); vm.Dispose(); }, Throws.Nothing);
        }

        // ---- Detail*ViewModel SetProperties: Properties が埋まる ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailRectangle_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM(); // App.IsTest = true 設定
            var item = new NRectangleViewModel();
            var vm = new DetailRectangleViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
            Assert.That(vm.OKTabIndex.Value, Is.EqualTo(vm.Properties.Count));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailEllipse_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new NEllipseViewModel();
            var vm = new DetailEllipseViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailBezier_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new BezierCurveViewModel();
            var vm = new DetailBezierViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailBlur_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new BlurEffectViewModel();
            var vm = new DetailBlurViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailMosaic_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new MosaicViewModel();
            var vm = new DetailMosaicViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailPie_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new NPieViewModel();
            var vm = new DetailPieViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailPolygon_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new NPolygonViewModel();
            var vm = new DetailPolygonViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailPolyBezier_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new PolyBezierViewModel();
            var vm = new DetailPolyBezierViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailLetter_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new LetterDesignerItemViewModel();
            var vm = new DetailLetterViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailPicture_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new PictureDesignerItemViewModel();
            var vm = new DetailPictureViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailColorCorrect_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new ColorCorrectViewModel();
            var vm = new DetailColorCorrectViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DetailStraightLine_OnDialogOpenedでPropertiesに項目追加()
        {
            NewMainWindowVM();
            var item = new StraightConnectorViewModel();
            var vm = new DetailStraightLineViewModel(NewRegionManager());
            vm.OnDialogOpened(DialogParametersWith(item));
            Assert.That(vm.Properties.Count, Is.GreaterThan(0));
        }
    }
}
