using boilersGraphics.Converters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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

        [Test]
        public void DoubleToStringConverter_Convert()
        {
            var converter = new DoubleToStringConverter();
            Assert.That(converter.Convert(1d, typeof(double), null, null), Is.EqualTo("1"));
            Assert.That(converter.Convert(1000d, typeof(double), null, null), Is.EqualTo("1000"));
            Assert.That(converter.Convert(0.1d, typeof(double), null, null), Is.EqualTo("0.1"));
            Assert.That(converter.Convert(0.01d, typeof(double), null, null), Is.EqualTo("0.01"));
            Assert.That(converter.Convert(0.001d, typeof(double), null, null), Is.EqualTo("0.001"));
            Assert.That(converter.Convert(0.0001d, typeof(double), null, null), Is.EqualTo("0.0001"));
            Assert.That(converter.Convert(0.00001d, typeof(double), null, null), Is.EqualTo("1E-05"));
        }

        [Test]
        public void DoubleToStringConverter_ConvertBack()
        {
            var converter = new DoubleToStringConverter();
            Assert.That(converter.ConvertBack("", typeof(string), null, null), Is.EqualTo(0));
            Assert.That(converter.ConvertBack("0.", typeof(string), null, null), Is.EqualTo(Binding.DoNothing));
            Assert.That(converter.ConvertBack("-", typeof(string), null, null), Is.EqualTo(Binding.DoNothing));
            Assert.That(converter.ConvertBack("0.0001", typeof(string), null, null), Is.EqualTo(0.0001));
            Assert.That(converter.ConvertBack("0.00001", typeof(string), null, null), Is.EqualTo(0.00001));
            Assert.That(converter.ConvertBack("0.0001*", typeof(string), null, null), Is.EqualTo(0));
        }

        [Test]
        public void ExtensionConverter_Convert()
        {
            var converter = new ExtensionConverter();
            Assert.That(converter.Convert(@"Z:\Git\boilersGraphics\boilersGraphics.Test\ConverterTest.cs", typeof(string), null, null), Is.EqualTo(".cs"));
        }

        [Test]
        public void ExtensionConverter_ConvertBack()
        {
            var converter = new ExtensionConverter();
            Assert.That(() => converter.ConvertBack(string.Empty, typeof(string), null, null), Throws.InstanceOf<NotImplementedException>());
        }

        [Test]
        public void IntToStringConverter_Convert()
        {
            var converter = new IntToStringConverter();
            Assert.That(converter.Convert(1, typeof(int), null, null), Is.EqualTo("1"));
            Assert.That(converter.Convert(int.MaxValue, typeof(int), null, null), Is.EqualTo("2147483647"));
            Assert.That(converter.Convert(int.MinValue, typeof(int), null, null), Is.EqualTo("-2147483648"));
        }

        [Test]
        public void IntToStringConverter_ConvertBack()
        {
            var converter = new IntToStringConverter();
            Assert.That(converter.ConvertBack("", typeof(string), null, null), Is.EqualTo(0));
            Assert.That(converter.ConvertBack("1", typeof(string), null, null), Is.EqualTo(1));
            Assert.That(converter.ConvertBack("2147483647", typeof(string), null, null), Is.EqualTo(2147483647));
            Assert.That(converter.ConvertBack("-2147483648", typeof(string), null, null), Is.EqualTo(-2147483648));
            Assert.That(converter.ConvertBack(" ", typeof(string), null, null), Is.EqualTo(0));
            Assert.That(converter.ConvertBack("*", typeof(string), null, null), Is.EqualTo(0));
        }

        [Test]
        public void IsSelectedToBorderBrushConverter_Convert()
        {
            var converter = new IsSelectedToBorderBrushConverter();
            Assert.That(converter.Convert(true, typeof(bool), null, null), Is.EqualTo(Brushes.Red));
            Assert.That(converter.Convert(false, typeof(bool), null, null), Is.EqualTo(Brushes.Black));
        }

        [Test]
        public void IsSelectedToBorderBrushConverter_ConvertBack()
        {
            var converter = new IsSelectedToBorderBrushConverter();
            Assert.That(() => converter.ConvertBack(Brushes.Red, typeof(Brush), null, null), Throws.InstanceOf<NotImplementedException>());
        }

        [Test]
        public void ShiftCoordinateConverter_Convert()
        {
            var converter = new ShiftCoordinateConverter();
            Assert.That(converter.Convert(1d, typeof(double), 2d, null), Is.EqualTo(0d));
            Assert.That(converter.Convert(100d, typeof(double), 1d, null), Is.EqualTo(99.5d));
        }

        [Test]
        public void ShiftCoordinateConverter_ConvertBack()
        {
            var converter = new ShiftCoordinateConverter();
            Assert.That(() => converter.ConvertBack(0d, typeof(double), null, null), Throws.InstanceOf<NotImplementedException>());
        }

        [Test]
        public void StringToByteConverter_Convert()
        {
            var converter = new StringToByteConverter();
            Assert.That(converter.Convert("1", typeof(string), null, null), Is.EqualTo(1));
        }

        [Test]
        public void StringToByteConverter_ConvertBack()
        {
            var converter = new StringToByteConverter();
            byte val = 1;
            Assert.That(() => converter.ConvertBack(val, typeof(byte), null, null), Is.EqualTo("1"));
        }

        [Test]
        public void StrokeColorConverter_Convert()
        {
            var converter = new StrokeColorConverter();
            ObservableCollection<Color> collection = new ObservableCollection<Color>();
            Assert.That(converter.Convert(collection, typeof(ObservableCollection<Color>), null, null), Has.Property("Color").EqualTo(Colors.Transparent));
            collection.Add(Colors.Red);
            Assert.That(converter.Convert(collection, typeof(ObservableCollection<Color>), null, null), Has.Property("Color").EqualTo(Colors.Red));
            collection.Add(Colors.Blue);
            Assert.That(converter.Convert(collection, typeof(ObservableCollection<Color>), null, null), Has.Property("Color").EqualTo(Colors.Transparent));
        }

        [Test]
        public void StrokeColorConverter_ConvertBack()
        {
            var converter = new StrokeColorConverter();
            Assert.That(() => converter.ConvertBack(new SolidColorBrush(Colors.Transparent), typeof(SolidColorBrush), null, null), Throws.InstanceOf<NotImplementedException>());
        }

        [Test]
        public void ToSolidColorBrushConverter_Convert()
        {
            var converter = new ToSolidColorBrushConverter();
            Assert.That(converter.Convert(Colors.Transparent, typeof(Color), null, null), Has.Property("Color").EqualTo(Colors.Transparent));
            Assert.That(converter.Convert(Colors.Black, typeof(Color), null, null), Has.Property("Color").EqualTo(Colors.Black));
            Assert.That(converter.Convert(Colors.White, typeof(Color), null, null), Has.Property("Color").EqualTo(Colors.White));
        }

        [Test]
        public void ToSolidColorBrushConverter_ConvertBack()
        {
            var converter = new ToSolidColorBrushConverter();
            Assert.That(() => converter.ConvertBack(new SolidColorBrush(Colors.Transparent), typeof(SolidColorBrush), null, null), Throws.InstanceOf<NotImplementedException>());
        }
    }
}
