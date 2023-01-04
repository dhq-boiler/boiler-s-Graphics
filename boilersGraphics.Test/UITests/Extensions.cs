using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
