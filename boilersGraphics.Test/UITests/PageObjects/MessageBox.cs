﻿using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test.UITests.PageObjects
{
    /// <summary>
    /// 現在のキャンパスは破棄されますが、よろしいですか？
    /// </summary>
    public class MessageBox : PageObjectBase
    {
        public MessageBox(WindowsDriver<WindowsElement> Session)
            : base(Session)
        { }

        public LoadDialog Click_OKButton()
        {
            GetElementByAutomationID("1").Click();
            return new LoadDialog(Session);
        }
    }
}
