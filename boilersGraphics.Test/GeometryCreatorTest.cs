using boilersGraphics.Helpers;
using NUnit.Framework;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class GeometryCreatorTest
    {
        [Test]
        public void 四角形移動()
        {
            var geometry = PathGeometry.CreateFromGeometry(new RectangleGeometry(new System.Windows.Rect(0, 10, 20, 30)));
            geometry = GeometryCreator.Translate(geometry, 3, 6);
            Assert.That(geometry.Bounds.Left, Is.EqualTo(3));
            Assert.That(geometry.Bounds.Top, Is.EqualTo(16));
            Assert.That(geometry.Bounds.Width, Is.EqualTo(20));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(30));
        }
    }
}
