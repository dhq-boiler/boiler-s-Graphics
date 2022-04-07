using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class HelpersTest
    {
        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_AddNewBrushViewModel()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var desingerCanvas = new DesignerCanvas();
            desingerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            diagramViewModel.EdgeBrush.Value = new SolidColorBrush(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.AddNewBrushViewModel(desingerCanvas, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            Assert.That(vm.Width.Value, Is.EqualTo(1000));
            Assert.That(vm.Height.Value, Is.EqualTo(1000));
            Assert.That((vm.EdgeBrush.Value as SolidColorBrush).Color, Is.EqualTo(new SolidColorBrush(Colors.Transparent).Color));
            Assert.That((vm.FillBrush.Value as SolidColorBrush).Color, Is.EqualTo(new SolidColorBrush(Colors.Red).Color));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Down()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var designerCanvas = new DesignerCanvas();
            designerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            diagramViewModel.EdgeBrush.Value = new SolidColorBrush(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.Down(mainWindowViewModel, designerCanvas, ref vm, () => new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left).MouseDevice.Capture(designerCanvas), new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left), new System.Windows.Point() { X = 50, Y = 50 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M51,49C51,50.10456949966159 50.10456949966159,51 49,51 47.89543050033841,51 47,50.10456949966159 47,49 47,47.89543050033841 47.89543050033841,47 49,47 50.10456949966159,47 51,47.89543050033841 51,49z"));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Down_2回目()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var designerCanvas = new DesignerCanvas();
            designerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            diagramViewModel.EdgeBrush.Value = new SolidColorBrush(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.Down(mainWindowViewModel, designerCanvas, ref vm, () => new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left).MouseDevice.Capture(designerCanvas), new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left), new System.Windows.Point() { X = 50, Y = 50 }) ;

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M51,49C51,50.10456949966159 50.10456949966159,51 49,51 47.89543050033841,51 47,50.10456949966159 47,49 47,47.89543050033841 47.89543050033841,47 49,47 50.10456949966159,47 51,47.89543050033841 51,49z"));

            mainWindowViewModel.DiagramViewModel.AllItems.Value.First().IsSelected.Value = true;

            BrushInternal.Down(mainWindowViewModel, designerCanvas, ref vm, () => new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left).MouseDevice.Capture(designerCanvas), new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left), new System.Windows.Point() { X = 100, Y = 100 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M101,99C101,100.1045694996616 100.1045694996616,101 99,101 97.8954305003384,101 97,100.1045694996616 97,99 97,97.8954305003384 97.8954305003384,97 99,97 100.1045694996616,97 101,97.8954305003384 101,99z"));
        }
    }
}
