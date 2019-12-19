namespace Spider.SeleniumClient
{
    using System;
    using System.Drawing;
    using Spider.Common.Helper;

    public static class SeleniumConfig
    {
        public static string SeleniumHubAddress => ConfigHelper.GetStringValue("Selenium.hub.Address");
        public static string SeleniumHubPort => ConfigHelper.GetStringValue("Selenium.hub.Port");

        public static Uri SeleniumHubEndPoint => new Uri(string.Format("http://{0}:{1}/wd/hub", SeleniumConfig.SeleniumHubAddress, SeleniumConfig.SeleniumHubPort), UriKind.Absolute);

        public static bool GridEnabled => ConfigHelper.GetBoolValue("Selenium.Grid.Enabled");

        public static string ChromeDriverLocation => ConfigHelper.GetStringValue("Selenium.ChromeDriver.Location");
        public static string LastChromeDriverVersionUrl => ConfigHelper.GetStringValue("Selenium.ChromeDriver.LastChromeDriverVersionUrl");

        public static string ScreenshotLocation => ConfigHelper.GetStringValue("Selenium.Screenshot.Location");

        public static string ReportLocation => ConfigHelper.GetStringValue("Selenium.Report.Location");

        public static Size? BrowserSize => ConfigHelper.GetSizeValue("Selenium.Browser.Size");
    }
}