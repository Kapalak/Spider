﻿namespace Spider.SeleniumClient
{
    using System;
    using System.Drawing;
    using Spider.Common.Helper;

    public static class SeleniumConfig
    {
        //public static string SeleniumHubAddress => ConfigHelper.GetStringValue("Selenium.hub.Address");
        public static string WebDriverLocation => ConfigHelper.GetStringValue("Selenium.WebDriver.Location");
        public static string LastChromeDriverVersionUrl => ConfigHelper.GetStringValue("Selenium.ChromeDriver.LastChromeDriverVersionUrl");
        public static string LastFirefoxDriverVersionUrl => ConfigHelper.GetStringValue("Selenium.ChromeDriver.LastFirefoxDriverVersionUrl");

        public static string ScreenshotLocation => ConfigHelper.GetStringValue("Selenium.Screenshot.Location");

        public static string ReportLocation => ConfigHelper.GetStringValue("Selenium.Report.Location");

        public static Size? BrowserSize => ConfigHelper.GetSizeValue("Selenium.Browser.Size");
    }
}