namespace Spider.SeleniumClient
{
    using Spider.Common.Helper;

    public static class SeleniumConfig
    {
        //public static string SeleniumHubAddress => ConfigHelper.GetStringValue("Selenium.hub.Address");
        public static string WebDriverLocation => ConfigHelper.GetStringValue("Selenium.WebDriver.Location");
        public static string LastChromeDriverVersionUrl => ConfigHelper.GetStringValue("Selenium.ChromeDriver.LastChromeDriverVersionUrl");
        public static string LastFirefoxDriverVersionUrl => ConfigHelper.GetStringValue("Selenium.ChromeDriver.LastFirefoxDriverVersionUrl");
    }
}