using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test.UITests
{
    public class AppSession
    {
        // Note: append /wd/hub to the URL if you're directing the test at Appium
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        private static readonly string AppPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boilersGraphics.exe");//"Microsoft.WindowsCalculator_8wekyb3d8bbwe!App";

        protected static WindowsDriver<WindowsElement> session;

        [SetUp]
        public static void Setup()
        {
            // Launch Calculator application if it is not yet launched
            if (session == null)
            {
                // Create a new session to bring up an instance of the Calculator application
                // Note: Multiple calculator windows (instances) share the same process Id
                DesiredCapabilities appCapabilities = new DesiredCapabilities();
                appCapabilities.SetCapability("app", AppPath);
                appCapabilities.SetCapability("deviceName", "WindowsPC");
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities, TimeSpan.FromMinutes(5));
                Assert.That(session, Is.Not.Null);

                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times

                session.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);

                session.Manage().Window.Maximize();
            }
        }

        [TearDown]
        public static void TearDown()
        {
            // Close the application and delete the session
            if (session != null)
            {
                session.Quit();
                session = null;
            }
        }
    }
}
