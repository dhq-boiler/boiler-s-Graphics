using NLog;
using NUnit.Framework;
using OpenCvSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace boilersGraphics.Test.UITests
{
    public static class Extensions
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public static Actions ClickAndHold(this AppiumWebElement element, IWebDriver session)
        {
            var actions = new Actions(session);
            actions.ClickAndHold(element);
            return actions;
        }

        public static AppiumWebElement GetElementBy(this AppiumWebElement element, By by, int timeOutSeconds = 10)
        {
            By by2 = by;
            AppiumWebElement found = null;
            DefaultWait<AppiumWebElement> defaultWait = new DefaultWait<AppiumWebElement>(element)
            {
                Timeout = TimeSpan.FromSeconds(timeOutSeconds),
                Message = "Element with By " + by2.ToString() + " not found."
            };
            defaultWait.IgnoreExceptionTypes(typeof(WebDriverException));
            try
            {
                defaultWait.Until(elm =>
                {
                    found = elm.FindElement(by2);
                    return found != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                s_logger.Error(ex);
                Assert.Fail(ex.Message);
            }

            return found;
        }

        public static Mat ToMat(this Screenshot screenshot, string filename)
        {
            screenshot.SaveAsFile(filename);
            return new Mat(filename);
        }
    }
}
