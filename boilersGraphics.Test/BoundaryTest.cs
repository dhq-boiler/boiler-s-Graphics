using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Windows;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class BoundaryTest
    {
        [Test]
        public void Basic()
        {
            var diagrams = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel(10, 10, 10, 10)
            };

            var actual = DiagramViewModel.GetBoundingRectangle(diagrams);

            Assert.That(actual, Is.EqualTo(new Rect(10, 10, 10, 10)));
        }

        [Test]
        public void Rotate_45()
        {
            var diagrams = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel(10, 10, 10, 10, 45)
            };

            var actual = DiagramViewModel.GetBoundingRectangle(diagrams);

            Assert.That(actual, Is.EqualTo(new Rect(15 - 5 * Math.Sqrt(2), 15 - 5 * Math.Sqrt(2), 10 * Math.Sqrt(2), 10 * Math.Sqrt(2))));
        }

        [Test]
        public void Rotate_90()
        {
            var diagrams = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel(10, 10, 10, 10, 90)
            };

            var actual = DiagramViewModel.GetBoundingRectangle(diagrams);

            Assert.That(actual, Is.EqualTo(new Rect(10, 10, 10, 10)));
        }

        [Test]
        public void Rotate_180()
        {
            var diagrams = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel(10, 10, 10, 10, 180)
            };

            var actual = DiagramViewModel.GetBoundingRectangle(diagrams);

            Assert.That(actual, Is.EqualTo(new Rect(10, 10, 10, 10)));
        }

        [Test]
        public void Multiple_2()
        {
            var diagrams = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel(10, 10, 10, 10),
                new NRectangleViewModel(20, 20, 10, 10),
            };

            var actual = DiagramViewModel.GetBoundingRectangle(diagrams);

            Assert.That(actual, Is.EqualTo(new Rect(10, 10, 20, 20)));
        }

        [Test]
        public void Multiple_3()
        {
            var diagrams = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel(10, 10, 10, 10),
                new NRectangleViewModel(20, 20, 50, 10),
            };

            var actual = DiagramViewModel.GetBoundingRectangle(diagrams);

            Assert.That(actual.Left, Is.EqualTo(10d).Within(0.00000000000001));
            Assert.That(actual.Top, Is.EqualTo(10d).Within(0.00000000000001));
            Assert.That(actual.Width, Is.EqualTo(60d).Within(0.00000000000001));
            Assert.That(actual.Height, Is.EqualTo(20d).Within(0.00000000000001));
        }
    }
}
