using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class HelpersTest
    {
        [Test]
        public void GeometryCreator_CreateEllipse()
        {
            App.IsTest = true;
            var ellipse = new NEllipseViewModel();
            ellipse.Left.Value = 10;
            ellipse.Top.Value = 10;
            ellipse.Width.Value = 10;
            ellipse.Height.Value = 10;
            var pg = GeometryCreator.CreateEllipse(ellipse);
            Assert.That(pg.Bounds.Left, Is.EqualTo(10));
            Assert.That(pg.Bounds.Top, Is.EqualTo(10));
            Assert.That(pg.Bounds.Width, Is.EqualTo(10));
            Assert.That(pg.Bounds.Height, Is.EqualTo(10));
        }
        [Test]
        public void GeometryCreator_CreateEllipse_Angle()
        {
            App.IsTest = true;
            var ellipse = new NEllipseViewModel();
            ellipse.Left.Value = 10;
            ellipse.Top.Value = 10;
            ellipse.Width.Value = 20;
            ellipse.Height.Value = 10;
            var pg = GeometryCreator.CreateEllipse(ellipse);
            Assert.That(pg.Bounds.Left, Is.EqualTo(10));
            Assert.That(pg.Bounds.Top, Is.EqualTo(10));
            Assert.That(pg.Bounds.Width, Is.EqualTo(20));
            Assert.That(pg.Bounds.Height, Is.EqualTo(10));
            pg = GeometryCreator.CreateEllipse(ellipse, 90);
            Assert.That(pg.Bounds.Left, Is.EqualTo(15));
            Assert.That(pg.Bounds.Top, Is.EqualTo(5));
            Assert.That(pg.Bounds.Width, Is.EqualTo(10));
            Assert.That(pg.Bounds.Height, Is.EqualTo(20));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_AddNewBrushViewModel()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var desingerCanvas = new DesignerCanvas();
            desingerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillColors.Clear();
            diagramViewModel.FillColors.Add(Colors.Red);
            diagramViewModel.EdgeColors.Clear();
            diagramViewModel.EdgeColors.Add(Colors.Transparent);
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
            Assert.That(vm.EdgeColor.Value, Is.EqualTo(Colors.Transparent));
            Assert.That(vm.FillColor.Value, Is.EqualTo(Colors.Red));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Draw()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var desingerCanvas = new DesignerCanvas();
            desingerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillColors.Add(Colors.Red);
            diagramViewModel.EdgeColors.Add(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.AddNewBrushViewModel(desingerCanvas, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            BrushInternal.Draw(mainWindowViewModel, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("F1M99,97C99.55228424072266,97 100.05228424072266,97.22386169433594 100.41421508789062,97.58578491210938 100.77613830566406,97.94771575927734 101,98.44771575927734 101,99 101,99.55228424072266 100.77613830566406,100.05228424072266 100.41421508789062,100.41421508789062C100.05228424072266,100.77613830566406,99.55228424072266,101,99,101C98.44771575927734,101,97.94771575927734,100.77613830566406,97.58578491210938,100.41421508789062C97.22386169433594,100.05228424072266,97,99.55228424072266,97,99C97,98.44771575927734,97.22386169433594,97.94771575927734,97.58578491210938,97.58578491210938C97.94771575927734,97.22386169433594,98.44771575927734,97,99,97z"));
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
            diagramViewModel.FillColors.Add(Colors.Red);
            diagramViewModel.EdgeColors.Add(Colors.Transparent);
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
            diagramViewModel.FillColors.Add(Colors.Red);
            diagramViewModel.EdgeColors.Add(Colors.Transparent);
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

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void EraserInternal_Erase()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var desingerCanvas = new DesignerCanvas();
            desingerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillColors.Add(Colors.Red);
            diagramViewModel.EdgeColors.Add(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.AddNewBrushViewModel(desingerCanvas, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            BrushInternal.Draw(mainWindowViewModel, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            EraserInternal.Erase(mainWindowViewModel, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("F1M101,99C101,99.55228424072266,100.77613830566406,100.05228424072266,100.41421508789062,100.41421508789062C100.05228424072266,100.77613830566406,99.55228424072266,101,99,101C100.10456848144531,101,101,100.10456848144531,101,99z M97,99C97,100.10456848144531,97.89543151855469,101,99,101C98.44771575927734,101,97.94771575927734,100.77613830566406,97.58578491210938,100.41421508789062C97.22386169433594,100.05228424072266,97,99.55228424072266,97,99z M99,97C97.89543151855469,97 97,97.89543151855469 97,99 97,98.44771575927734 97.22386169433594,97.94771575927734 97.58578491210938,97.58578491210938C97.94771575927734,97.22386169433594,98.44771575927734,97,99,97z M99,97C99.55228424072266,97 100.05228424072266,97.22386169433594 100.41421508789062,97.58578491210938 100.77613830566406,97.94771575927734 101,98.44771575927734 101,99C101,97.89543151855469,100.10456848144531,97,99,97z"));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void EraserInternal_Down()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var designerCanvas = new DesignerCanvas();
            designerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillColors.Add(Colors.Red);
            diagramViewModel.EdgeColors.Add(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.AddNewBrushViewModel(designerCanvas, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            (designerCanvas.DataContext as DiagramViewModel).SelectedItems.Value.OfType<BrushViewModel>().First().IsSelected.Value = true;
            vm.IsSelected.Value = true;

            designerCanvas.Children.Add(new System.Windows.Shapes.Path() { DataContext = (designerCanvas.DataContext as DiagramViewModel).SelectedItems.Value.OfType<BrushViewModel>().First() });

            BrushInternal.Draw(mainWindowViewModel, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            EraserInternal.Down(mainWindowViewModel,
                                designerCanvas,
                                ref vm,
                                new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent },
                                new Point() { X = 100, Y = 100 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("F1M101,99C101,99.55228424072266,100.77613830566406,100.05228424072266,100.41421508789062,100.41421508789062C100.05228424072266,100.77613830566406,99.55228424072266,101,99,101C100.10456848144531,101,101,100.10456848144531,101,99z M97,99C97,100.10456848144531,97.89543151855469,101,99,101C98.44771575927734,101,97.94771575927734,100.77613830566406,97.58578491210938,100.41421508789062C97.22386169433594,100.05228424072266,97,99.55228424072266,97,99z M99,97C97.89543151855469,97 97,97.89543151855469 97,99 97,98.44771575927734 97.22386169433594,97.94771575927734 97.58578491210938,97.58578491210938C97.94771575927734,97.22386169433594,98.44771575927734,97,99,97z M99,97C99.55228424072266,97 100.05228424072266,97.22386169433594 100.41421508789062,97.58578491210938 100.77613830566406,97.94771575927734 101,98.44771575927734 101,99C101,97.89543151855469,100.10456848144531,97,99,97z"));
        }
    }
}
