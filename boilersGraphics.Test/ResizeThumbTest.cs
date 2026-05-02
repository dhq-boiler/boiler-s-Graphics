using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ResizeThumbTest
    {
        [Test]
        public void Vertical()
        {
            var picture = new PictureDesignerItemViewModel();
            picture.Left.Value = 10;
            picture.Top.Value = 20;
            picture.Width.Value = 100;
            picture.Height.Value = 200;
            ResizeThumb.AffectVertical(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.VerticalAlignment.Top, 0, 10, picture);
            Assert.That(picture.Left.Value, Is.EqualTo(10));
            Assert.That(picture.Top.Value, Is.EqualTo(23));
            Assert.That(picture.Width.Value, Is.EqualTo(100));
            Assert.That(picture.Height.Value, Is.EqualTo(197));
        }

        [Test]
        public void Horizontal()
        {
            var picture = new PictureDesignerItemViewModel();
            picture.Left.Value = 10;
            picture.Top.Value = 20;
            picture.Width.Value = 100;
            picture.Height.Value = 200;
            ResizeThumb.AffectHorizontal(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.HorizontalAlignment.Left, 0, 10, picture);
            Assert.That(picture.Left.Value, Is.EqualTo(13));
            Assert.That(picture.Top.Value, Is.EqualTo(20));
            Assert.That(picture.Width.Value, Is.EqualTo(97));
            Assert.That(picture.Height.Value, Is.EqualTo(200));
        }

        // Regression: dragging the right (or left) resize handle was able
        // to collapse the rectangle to Width = 0. At that point the resize
        // thumbs overlap PART_DragThumb at the same screen coordinates and
        // PART_DragThumb wins the WPF hit test, so the user could not drag
        // the handle back outward — the rectangle was effectively stuck.
        // The fix lives in three layers:
        //   1. DesignerItemViewModelBase.Init now defaults MinWidth/MinHeight
        //      to MIN_ONE_SIDE_LENGTH (10) instead of 0.
        //   2. CalculateDragLimits floors the per-item MinWidth/MinHeight at
        //      MIN_ONE_SIDE_LENGTH so legacy zero values are still safe.
        //   3. The actual Width/Height assignment in the resize / snap paths
        //      clamps with Math.Max so a snap or float drift can't dip below
        //      the floor either.

        [Test]
        public void NRectangleViewModel_MinWidthAndMinHeight_DefaultToFloor()
        {
            var rect = new NRectangleViewModel();
            // MinWidth/MinHeight default to MIN_ONE_SIDE_LENGTH (10) so the
            // resize handles are always at least that far from the opposite
            // edge.
            Assert.That(rect.MinWidth, Is.EqualTo(10));
            Assert.That(rect.MinHeight, Is.EqualTo(10));
        }

        [Test]
        public void CalculateDragLimits_FloorsHorizontalAtMinOneSideLength()
        {
            var rect = new NRectangleViewModel();
            rect.Left.Value = 50;
            rect.Top.Value = 0;
            rect.Width.Value = 100;
            rect.Height.Value = 50;
            // Force MinWidth/MinHeight back to 0 to confirm the floor logic
            // inside CalculateDragLimits still kicks in even when callers
            // set the per-item minimum lower than MIN_ONE_SIDE_LENGTH.
            rect.MinWidth = 0;
            rect.MinHeight = 0;

            ResizeThumb.CalculateDragLimits(
                new[] { (SelectableDesignerItemViewModelBase)rect },
                out _, out _, out var minDeltaHorizontal, out var minDeltaVertical);

            // minDeltaHorizontal must leave at least MIN_ONE_SIDE_LENGTH (10)
            // of width — i.e. cannot exceed Width - 10.
            Assert.That(minDeltaHorizontal, Is.EqualTo(100 - 10));
            Assert.That(minDeltaVertical, Is.EqualTo(50 - 10));
        }

        [Test]
        public void CalculateDragLimits_RespectsLargerExplicitMinWidth()
        {
            var rect = new NRectangleViewModel();
            rect.Left.Value = 0;
            rect.Top.Value = 0;
            rect.Width.Value = 100;
            rect.Height.Value = 100;
            rect.MinWidth = 25;
            rect.MinHeight = 30;

            ResizeThumb.CalculateDragLimits(
                new[] { (SelectableDesignerItemViewModelBase)rect },
                out _, out _, out var minDeltaHorizontal, out var minDeltaVertical);

            // Explicit MinWidth (25) is larger than the floor (10), so the
            // helper should respect it instead of overriding it.
            Assert.That(minDeltaHorizontal, Is.EqualTo(100 - 25));
            Assert.That(minDeltaVertical, Is.EqualTo(100 - 30));
        }

        [Test]
        public void VerticalAndHorizontal()
        {
            var picture = new PictureDesignerItemViewModel();
            picture.Left.Value = 10;
            picture.Top.Value = 20;
            picture.Width.Value = 100;
            picture.Height.Value = 200;
            ResizeThumb.AffectVertical(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.VerticalAlignment.Top, 0, 10, picture);
            ResizeThumb.AffectHorizontal(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.HorizontalAlignment.Left, 0, 10, picture);
            Assert.That(picture.Left.Value, Is.EqualTo(13));
            Assert.That(picture.Top.Value, Is.EqualTo(23));
            Assert.That(picture.Width.Value, Is.EqualTo(97));
            Assert.That(picture.Height.Value, Is.EqualTo(197));
        }
    }
}
