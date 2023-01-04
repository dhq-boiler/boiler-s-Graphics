using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests
{
    public static class Extensions
    {
        public static Actions ClickAndHold(this AppiumWebElement element, IWebDriver session)
        {
            var actions = new Actions(session);
            actions.ClickAndHold(element);
            return actions;
        }
    }
}
