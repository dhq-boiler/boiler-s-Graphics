using boilersE2E;
using NLog;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public abstract class PageObjectBase
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public const int TimeoutMinutes = 5;

        protected static WindowsDriver<WindowsElement> Session { get; private set; }

        public PageObjectBase(WindowsDriver<WindowsElement> session)
        {
            Session = session;
        }

        public static WindowsElement WaitForObject(Func<WindowsElement> function, int timeOutSeconds = 10)
        {
            Func<WindowsElement> function2 = function;
            WindowsElement waitElement = null;
            try
            {
                DefaultWait<WindowsDriver<WindowsElement>> defaultWait = new DefaultWait<WindowsDriver<WindowsElement>>(Session)
                {
                    Timeout = TimeSpan.FromSeconds(timeOutSeconds),
                    PollingInterval = TimeSpan.FromSeconds(1.0)
                };
                defaultWait.IgnoreExceptionTypes(typeof(WebDriverException));
                defaultWait.IgnoreExceptionTypes(typeof(InvalidOperationException));
                defaultWait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
                defaultWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                defaultWait.IgnoreExceptionTypes(typeof(NotFoundException));
                defaultWait.IgnoreExceptionTypes(typeof(WebException));
                defaultWait.IgnoreExceptionTypes(typeof(WebDriverTimeoutException));
                defaultWait.Until(delegate
                {
                    waitElement = function2();
                    return waitElement != null && waitElement.Enabled && waitElement.Displayed;
                });
                return waitElement;
            }
            catch (Exception value)
            {
                s_logger.Error(value);
                Assert.Fail("Failed to WaitForObject.. Check screenshots");
                return waitElement;
            }
        }

        public static void InputText(WindowsElement element, string text)
        {
            Util.SetTextToClipboard(text);
            element.SendKeys(Keys.Control + "v" + Keys.Control);
        }

        public static WindowsElement GetElementByAutomationID(string automationId, int timeOutSeconds = 10)
        {
            string automationId2 = automationId;
            WindowsElement element = null;
            DefaultWait<WindowsDriver<WindowsElement>> defaultWait = new DefaultWait<WindowsDriver<WindowsElement>>(Session)
            {
                Timeout = TimeSpan.FromSeconds(timeOutSeconds),
                Message = "Element with automationId \"" + automationId2 + "\" not found."
            };
            defaultWait.IgnoreExceptionTypes(typeof(WebDriverException));
            try
            {
                defaultWait.Until(delegate (WindowsDriver<WindowsElement> Driver)
                {
                    element = Driver.FindElementByAccessibilityId(automationId2);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                s_logger.Error(ex);
                s_logger.Error("automationId:" + automationId2);
                Assert.Fail(ex.Message);
            }

            return element;
        }

        public static WindowsElement GetElementByName(string name, int timeOutSeconds = 10)
        {
            string name2 = name;
            WindowsElement element = null;
            DefaultWait<WindowsDriver<WindowsElement>> defaultWait = new DefaultWait<WindowsDriver<WindowsElement>>(Session)
            {
                Timeout = TimeSpan.FromSeconds(timeOutSeconds),
                Message = "Element with Name \"" + name2 + "\" not found."
            };
            defaultWait.IgnoreExceptionTypes(typeof(WebDriverException));
            try
            {
                defaultWait.Until(delegate (WindowsDriver<WindowsElement> Driver)
                {
                    element = Driver.FindElementByName(name2);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                s_logger.Error(ex);
                s_logger.Error("Name:" + name2);
                Assert.Fail(ex.Message);
            }

            return element;
        }

        public static WindowsElement GetElementBy(By by, int timeOutSeconds = 10)
        {
            By by2 = by;
            WindowsElement element = null;
            DefaultWait<WindowsDriver<WindowsElement>> defaultWait = new DefaultWait<WindowsDriver<WindowsElement>>(Session)
            {
                Timeout = TimeSpan.FromSeconds(timeOutSeconds),
                Message = "Element with By " + by2.ToString() + " not found."
            };
            defaultWait.IgnoreExceptionTypes(typeof(WebDriverException));
            try
            {
                defaultWait.Until(delegate (WindowsDriver<WindowsElement> Driver)
                {
                    element = Driver.FindElement(by2);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                s_logger.Error(ex);
                Assert.Fail(ex.Message);
            }

            return element;
        }

        public static IEnumerable<WindowsElement> GetElementsBy(By by, int timeOutSeconds = 10)
        {
            By by2 = by;
            List<WindowsElement> elements = new List<WindowsElement>();
            DefaultWait<WindowsDriver<WindowsElement>> defaultWait = new DefaultWait<WindowsDriver<WindowsElement>>(Session)
            {
                Timeout = TimeSpan.FromSeconds(timeOutSeconds),
                Message = "Element with By " + by2.ToString() + " not found."
            };
            defaultWait.IgnoreExceptionTypes(typeof(WebDriverException));
            try
            {
                defaultWait.Until(delegate (WindowsDriver<WindowsElement> Driver)
                {
                    elements.AddRange(Driver.FindElements(by2));
                    return elements.Count() > 0;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                s_logger.Error(ex);
                Assert.Fail(ex.Message);
            }

            return elements;
        }

        public static bool ExistsElementByAutomationID(string automationId, int timeOutSeconds = 10)
        {
            string automationId2 = automationId;
            WindowsElement element = null;
            DefaultWait<WindowsDriver<WindowsElement>> defaultWait = new DefaultWait<WindowsDriver<WindowsElement>>(Session)
            {
                Timeout = TimeSpan.FromSeconds(timeOutSeconds),
                Message = "Element with automationId \"" + automationId2 + "\" not found."
            };
            defaultWait.IgnoreExceptionTypes(typeof(WebDriverException));
            try
            {
                defaultWait.Until(delegate (WindowsDriver<WindowsElement> Driver)
                {
                    element = Driver.FindElementByAccessibilityId(automationId2);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException)
            {
            }
            catch (WebDriverException)
            {
            }

            return element != null;
        }

        public static bool IsElementPresent(By by)
        {
            try
            {
                Session.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (WebDriverException)
            {
                return false;
            }
        }
    }
}
