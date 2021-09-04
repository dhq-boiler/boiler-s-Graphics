using boilersGraphics.Converters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ConverterTest
    {
        [Test]
        public void ColorToStringConverter_Convert()
        {
            var converter = new ColorToStringConverter();
            Assert.That(converter.Convert(Colors.Red, typeof(Color), null, null), Is.EqualTo("#FFFF0000"));
            Assert.That(converter.Convert(Colors.Lime, typeof(Color), null, null), Is.EqualTo("#FF00FF00"));
            Assert.That(converter.Convert(Colors.Green, typeof(Color), null, null), Is.EqualTo("#FF008000"));
            Assert.That(converter.Convert(Colors.Blue, typeof(Color), null, null), Is.EqualTo("#FF0000FF"));
            Assert.That(converter.Convert(Colors.White, typeof(Color), null, null), Is.EqualTo("#FFFFFFFF"));
            Assert.That(converter.Convert(Colors.Black, typeof(Color), null, null), Is.EqualTo("#FF000000"));
            Assert.That(converter.Convert(Colors.Transparent, typeof(Color), null, null), Is.EqualTo("#00FFFFFF"));
        }

        [Test]
        public void ColorToStringConverter_ConvertBack()
        {
            var converter = new ColorToStringConverter();
            Assert.That(() => converter.ConvertBack("#FFFF0000", typeof(string), null, null), Throws.InstanceOf<NotImplementedException>());
        }
    }
}
