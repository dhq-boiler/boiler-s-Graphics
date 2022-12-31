using NLog;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace boilersGraphics.Test.UITests
{
    [Obsolete]
    public class AppSession
    {
        // Note: append /wd/hub to the URL if you're directing the test at Appium
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private static readonly string AppPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boilersGraphics.exe");//"Microsoft.WindowsCalculator_8wekyb3d8bbwe!App";

        protected static WindowsDriver<WindowsElement> session;
        private static Process wad;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("BOILERSGRAPHICS_TEST_IS_VALID");
            if (environmentVariable == "true")
            {
                wad = Process.Start(new ProcessStartInfo(@"C:\Program Files\Windows Application Driver\WinAppDriver.exe"));
            }
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("BOILERSGRAPHICS_TEST_IS_VALID");
            if (environmentVariable == "true")
            {
                wad.Kill();
            }
        }

        [SetUp]
        public static void Setup()
        {
            // Launch Calculator application if it is not yet launched
            if (session == null)
            {
                // Create a new session to bring up an instance of the Calculator application
                // Note: Multiple calculator windows (instances) share the same process Id
                var options = new AppiumOptions();
                options.AddAdditionalCapability("app", AppPath);
                options.AddAdditionalCapability("appWorkingDir", Path.GetDirectoryName(AppPath));
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), options);
                Assert.That(session, Is.Not.Null);

                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times

                //session.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(1);

                SkipPrivacyPolicyIfExists();

                var environmentVariable = Environment.GetEnvironmentVariable("BOILERSGRAPHICS_TEST_IS_VALID");
                if (environmentVariable == "true")
                {
                    session.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
                }
                else
                {
                    session.SwitchTo().Window(session.WindowHandles.First()).Manage().Window.Maximize();
                }
            }
        }

        public static void SkipPrivacyPolicyIfExists()
        {
            if (ExistsElementByAutomationID("Agree"))
            {
                GetElementByAutomationID("Agree").Click();
            }
        }

        [TearDown]
        public static void TearDown()
        {
            // Close the application and delete the session
            if (session != null)
            {
                while (session.WindowHandles.Count() > 0)
                {
                    var actions = new Actions(session);
                    actions.SendKeys(OpenQA.Selenium.Keys.Alt + OpenQA.Selenium.Keys.F4 + OpenQA.Selenium.Keys.Alt);
                    actions.Perform();
                }
                session.WindowHandles.Select(x => session.SwitchTo().Window(x)).ToList().ForEach(x => x.Dispose());
                session.Quit();
                session = null;
            }
        }

        // Wait for an Object to be accessible, use a custom timeout
        public static WindowsElement WaitForObject(Func<WindowsElement> element, int timeout)
        {

            WindowsElement waitElement = null;

            try
            {
                var wait = new DefaultWait<WindowsDriver<WindowsElement>>(session)
                {
                    Timeout = TimeSpan.FromSeconds(timeout),
                    PollingInterval = TimeSpan.FromSeconds(1)
                };

                wait.IgnoreExceptionTypes(typeof(WebDriverException));
                wait.IgnoreExceptionTypes(typeof(InvalidOperationException));
                wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                wait.IgnoreExceptionTypes(typeof(NotFoundException));
                wait.IgnoreExceptionTypes(typeof(WebException));
                wait.IgnoreExceptionTypes(typeof(WebDriverTimeoutException));

                wait.Until(driver =>
                {
                    waitElement = element();

                    return waitElement != null && waitElement.Enabled && waitElement.Displayed;
                });

                return waitElement;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail("Failed to WaitForObject.. Check screenshots");
                return waitElement;
            }
        }

        public static void InjectText(WindowsElement element, string text)
        {
            Clipboard.SetText(text);
            element.SendKeys(Keys.Control + "v" + Keys.Control);
        }

        public static WindowsElement GetElementByAutomationID(string automationId, int timeOut = 10000)
        {
            WindowsElement element = null;

            var wait = new DefaultWait<WindowsDriver<WindowsElement>>(session)
            {
                Timeout = TimeSpan.FromMilliseconds(timeOut),
                Message = $"Element with automationId \"{automationId}\" not found."
            };

            wait.IgnoreExceptionTypes(typeof(WebDriverException));

            try
            {
                wait.Until(Driver =>
                {
                    element = Driver.FindElementByAccessibilityId(automationId);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                LogManager.GetCurrentClassLogger().Error($"automationId:{automationId}");
                Assert.Fail(ex.Message);
            }

            return element;
        }

        public static WindowsElement GetElementByName(string name, int timeOut = 10000)
        {
            WindowsElement element = null;

            var wait = new DefaultWait<WindowsDriver<WindowsElement>>(session)
            {
                Timeout = TimeSpan.FromMilliseconds(timeOut),
                Message = $"Element with Name \"{name}\" not found."
            };

            wait.IgnoreExceptionTypes(typeof(WebDriverException));

            try
            {
                wait.Until(Driver =>
                {
                    element = Driver.FindElementByName(name);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                LogManager.GetCurrentClassLogger().Error($"Name:{name}");
                Assert.Fail(ex.Message);
            }

            return element;
        }

        public static WindowsElement GetElementBy(By by, int timeOut = 10000)
        {
            WindowsElement element = null;

            var wait = new DefaultWait<WindowsDriver<WindowsElement>>(session)
            {
                Timeout = TimeSpan.FromMilliseconds(timeOut),
                Message = $"Element with By {by.ToString()} not found."
            };

            wait.IgnoreExceptionTypes(typeof(WebDriverException));

            try
            {
                wait.Until(Driver =>
                {
                    element = Driver.FindElement(by);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                Assert.Fail(ex.Message);
            }

            return element;
        }

        public static bool ExistsElementByAutomationID(string automationId, int timeOut = 10000)
        {
            WindowsElement element = null;

            var wait = new DefaultWait<WindowsDriver<WindowsElement>>(session)
            {
                Timeout = TimeSpan.FromMilliseconds(timeOut),
                Message = $"Element with automationId \"{automationId}\" not found."
            };

            wait.IgnoreExceptionTypes(typeof(WebDriverException));

            try
            {
                wait.Until(Driver =>
                {
                    element = Driver.FindElementByAccessibilityId(automationId);
                    return element != null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
            }
            catch (WebDriverException ex)
            {
            }

            return element != null;
        }

        public static bool IsElementPresent(By by)
        {
            try
            {
                session.FindElement(by);
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

        public static void TakeScreenShot(string filename)
        {
            session.GetScreenshot().SaveAsFile($"{AppDomain.CurrentDomain.BaseDirectory}\\{filename}");
            TestContext.AddTestAttachment($"{AppDomain.CurrentDomain.BaseDirectory}\\{filename}");
        }

        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);
        [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint thread);
        public static CultureInfo GetCurrentKeyboardLayout()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                uint foregroundProcess = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                int keyboardLayout = GetKeyboardLayout(foregroundProcess).ToInt32() & 0xFFFF;
                return new CultureInfo(keyboardLayout);
            }
            catch (Exception _)
            {
                return new CultureInfo(1033); // Assume English if something went wrong.
            }
        }
    }
}
