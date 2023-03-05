using boilersE2E.NUnit;
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
        private static readonly NLog.Logger s_logger = LogManager.GetCurrentClassLogger();

        public const int TimeoutMinutes = 5;

        protected static WindowsDriver<WindowsElement> Session { get; private set; }

        protected E2ETestFixture TestFixture { get; private set; }

        public PageObjectBase(WindowsDriver<WindowsElement> session, E2ETestFixture testFixture)
        {
            Session = session;
            TestFixture = testFixture;
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

        public void InputText(WindowsElement element, string text)
        {
            TestFixture.InputText(element, text);
        }

        public  WindowsElement GetElementByAutomationID(string automationId, int timeOutSeconds = 10)
        {
            return TestFixture.GetElementByAutomationID(automationId, timeOutSeconds);
        }

        public WindowsElement GetElementByName(string name, int timeOutSeconds = 10)
        {
            return TestFixture.GetElementByName(name, timeOutSeconds);
        }

        public  WindowsElement GetElementBy(By by, int timeOutSeconds = 10)
        {
            return TestFixture.GetElementBy(by, timeOutSeconds);
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

        public bool ExistsElementByAutomationID(string automationId, int timeOutSeconds = 10)
        {
            return TestFixture.ExistsElementByAutomationID(automationId, timeOutSeconds);
        }

        public bool IsElementPresent(By by)
        {
            return TestFixture.IsElementPresent(by);
        }
    }
}
