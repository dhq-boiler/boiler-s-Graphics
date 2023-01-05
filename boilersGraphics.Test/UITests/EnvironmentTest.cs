using NUnit.Framework;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class EnvironmentTest
    {
        [DllImport("User32.dll")]
        public static extern int GetDpiForSystem();

        [ConditionalEnvironmentVariable("OnAzureDevOps")]
        [Test]
        public void DPI96であることを確認()
        {
            Assert.That(GetDpiForSystem(),  Is.EqualTo(96), "DPI==96");
            Assert.That(Screen.PrimaryScreen.Bounds.Width, Is.EqualTo(1980), "Screen.PrimaryScreen.Bounds.Width==1980");
            Assert.That(Screen.PrimaryScreen.Bounds.Height, Is.EqualTo(1080), "Screen.PrimaryScreen.Bounds.Height==1080");
        }
    }
}
