using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System.Windows;

namespace boilersGraphics.Test.UITests
{
    public static class Extensions
    {
        public static void InjectText(this Actions actions, WindowsElement element, string text)
        {
            Clipboard.SetText(text);
            actions.SendKeys(element, Keys.Control + "v" + Keys.Control);
        }
    }
}
