using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class MathTest
    {
        [Test]
        public void Cos0()
        {
            Assert.That(Math.Cos(0d * Math.PI / 180.0), Is.EqualTo(1d).Within(0.0005));
        }

        [Test]
        public void Cos90()
        {
            Assert.That(Math.Cos(90d * Math.PI / 180.0), Is.EqualTo(0d).Within(0.0005));
        }

        [Test]
        public void Cos180()
        {
            Assert.That(Math.Cos(180d * Math.PI / 180.0), Is.EqualTo(-1d).Within(0.0005));
        }

        [Test]
        public void Cos270()
        {
            Assert.That(Math.Cos(270d * Math.PI / 180.0), Is.EqualTo(0d).Within(0.0005));
        }
        [Test]
        public void Sin0()
        {
            Assert.That(Math.Sin(0d * Math.PI / 180.0), Is.EqualTo(0d).Within(0.0005));
        }

        [Test]
        public void Sin90()
        {
            Assert.That(Math.Sin(90d * Math.PI / 180.0), Is.EqualTo(1d).Within(0.0005));
        }

        [Test]
        public void Sin180()
        {
            Assert.That(Math.Sin(180d * Math.PI / 180.0), Is.EqualTo(0d).Within(0.0005));
        }

        [Test]
        public void Sin270()
        {
            Assert.That(Math.Sin(270d * Math.PI / 180.0), Is.EqualTo(-1d).Within(0.0005));
        }
    }
}
