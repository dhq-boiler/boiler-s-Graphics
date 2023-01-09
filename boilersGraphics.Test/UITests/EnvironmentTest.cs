using NUnit.Framework;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class EnvironmentTest
    {
        [DllImport("User32.dll")]
        public static extern int GetDpiForSystem();

        [Test]
        public void DPI96であることを確認()
        {
            Assert.That(GetDpiForSystem(),  Is.EqualTo(96), "DPI==96");
        }

        [ConditionalEnvironmentVariable("OnAzureDevOps")]
        [Test]
        public void プライマリモニタ解像度の幅が1920()
        {
            Assert.That(Screen.PrimaryScreen.Bounds.Width, Is.EqualTo(1920), "Screen.PrimaryScreen.Bounds.Width==1920");
        }

        [ConditionalEnvironmentVariable("OnAzureDevOps")]
        [Test]
        public void プライマリモニタ解像度の高さが1080()
        {
            Assert.That(Screen.PrimaryScreen.Bounds.Height, Is.EqualTo(1080), "Screen.PrimaryScreen.Bounds.Height==1080");
        }
    }
}
