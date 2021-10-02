using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var diagramViewModel = new DiagramViewModel(new MainWindowViewModel(null), 100, 100);
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

            Assert.That(vm.Width.Value, Is.EqualTo(100));
            Assert.That(vm.Height.Value, Is.EqualTo(100));
            Assert.That(vm.EdgeColor.Value, Is.EqualTo(Colors.Transparent));
            Assert.That(vm.FillColor.Value, Is.EqualTo(Colors.Red));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Draw()
        {
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 100, 100);
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

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("F1M99,97C99.5522842407227,97 100.052284240723,97.2238616943359 100.414215087891,97.5857849121094 100.776138305664,97.9477157592773 101,98.4477157592773 101,99 101,99.5522842407227 100.776138305664,100.052284240723 100.414215087891,100.414215087891C100.052284240723,100.776138305664,99.5522842407227,101,99,101C98.4477157592773,101,97.9477157592773,100.776138305664,97.5857849121094,100.414215087891C97.2238616943359,100.052284240723,97,99.5522842407227,97,99C97,98.4477157592773,97.2238616943359,97.9477157592773,97.5857849121094,97.5857849121094C97.9477157592773,97.2238616943359,98.4477157592773,97,99,97z"));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Down()
        {
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 100, 100);
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

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M51,49C51,50.1045694996616 50.1045694996616,51 49,51 47.8954305003384,51 47,50.1045694996616 47,49 47,47.8954305003384 47.8954305003384,47 49,47 50.1045694996616,47 51,47.8954305003384 51,49z"));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Down_2回目()
        {
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 100, 100);
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

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M51,49C51,50.1045694996616 50.1045694996616,51 49,51 47.8954305003384,51 47,50.1045694996616 47,49 47,47.8954305003384 47.8954305003384,47 49,47 50.1045694996616,47 51,47.8954305003384 51,49z"));

            mainWindowViewModel.DiagramViewModel.AllItems.Value.First().IsSelected.Value = true;

            BrushInternal.Down(mainWindowViewModel, designerCanvas, ref vm, () => new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left).MouseDevice.Capture(designerCanvas), new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left), new System.Windows.Point() { X = 100, Y = 100 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M101,99C101,100.104569499662 100.104569499662,101 99,101 97.8954305003384,101 97,100.104569499662 97,99 97,97.8954305003384 97.8954305003384,97 99,97 100.104569499662,97 101,97.8954305003384 101,99z"));
        }

        //

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void EraserInternal_Erase()
        {
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 100, 100);
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

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("F1M101,99C101,99.5522842407227,100.776138305664,100.052284240723,100.414215087891,100.414215087891C100.052284240723,100.776138305664,99.5522842407227,101,99,101C100.104568481445,101,101,100.104568481445,101,99z M97,99C97,100.104568481445,97.8954315185547,101,99,101C98.4477157592773,101,97.9477157592773,100.776138305664,97.5857849121094,100.414215087891C97.2238616943359,100.052284240723,97,99.5522842407227,97,99z M99,97C97.8954315185547,97 97,97.8954315185547 97,99 97,98.4477157592773 97.2238616943359,97.9477157592773 97.5857849121094,97.5857849121094C97.9477157592773,97.2238616943359,98.4477157592773,97,99,97z M99,97C99.5522842407227,97 100.052284240723,97.2238616943359 100.414215087891,97.5857849121094 100.776138305664,97.9477157592773 101,98.4477157592773 101,99C101,97.8954315185547,100.104568481445,97,99,97z"));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void EraserInternal_Down()
        {
            var mainWindowViewModel = new MainWindowViewModel(null);
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

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("F1M101,99C101,99.5522842407227,100.776138305664,100.052284240723,100.414215087891,100.414215087891C100.052284240723,100.776138305664,99.5522842407227,101,99,101C100.104568481445,101,101,100.104568481445,101,99z M97,99C97,100.104568481445,97.8954315185547,101,99,101C98.4477157592773,101,97.9477157592773,100.776138305664,97.5857849121094,100.414215087891C97.2238616943359,100.052284240723,97,99.5522842407227,97,99z M99,97C97.8954315185547,97 97,97.8954315185547 97,99 97,98.4477157592773 97.2238616943359,97.9477157592773 97.5857849121094,97.5857849121094C97.9477157592773,97.2238616943359,98.4477157592773,97,99,97z M99,97C99.5522842407227,97 100.052284240723,97.2238616943359 100.414215087891,97.5857849121094 100.776138305664,97.9477157592773 101,98.4477157592773 101,99C101,97.8954315185547,100.104568481445,97,99,97z"));
        }
    }
}
