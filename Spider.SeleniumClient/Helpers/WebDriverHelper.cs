namespace Spider.SeleniumClient.Helpers
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using Spider.Common.Helper;
    using Spider.Common.Model;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class WebDriverHelper
    {
        public static int FetchRetries => ConfigHelper.GetIntValue("SeleniumClient.Fetch.MaxRetries", 1);
        public static int RetrySleepInterval => ConfigHelper.GetIntValue("SeleniumClient.Fetch.RetrySleepInterval", 500);

        public static IWebDriver CreateSession()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument(string.Format("--lang={0}", CultureInfo.CurrentCulture));
            DesiredCapabilities desiredCapabilities = new DesiredCapabilities("chrome", string.Empty, new Platform(PlatformType.Any));
            desiredCapabilities.SetCapability(ChromeOptions.Capability,
                string.Format(CultureInfo.InvariantCulture, "--lang={0}", CultureInfo.CurrentCulture));

            var projectOutputDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var chromeDriverPath = Path.Combine(projectOutputDirectory, SeleniumConfig.ChromeDriverLocation);
            
            IWebDriver webDriver = SeleniumConfig.GridEnabled ?
                new RemoteWebDriver(SeleniumConfig.SeleniumHubEndPoint, desiredCapabilities) :
                new ChromeDriver(chromeDriverPath, options, TimeSpan.FromSeconds(60));
            return webDriver;
        }

        public static void ResizeWindow(this IWebDriver webDriver, Size? windowSize)
        {
            if (windowSize != null)
            {
                webDriver.Manage().Window.Size = windowSize.Value;
            }
            else
            {
                webDriver.Manage().Window.Maximize();
            }
        }

        public static void SmartClick(this IWebDriver webDriver, Selector selector)
        {
            var element = webDriver.UniqueElement(selector);
            if (element.Enabled)
            {
                element.Click();
            }
        }

        public static void SetText(this IWebDriver webDriver, Selector selector, string value)
        {
            var element = webDriver.UniqueElement(selector);
            element.SendKeys(value);
        }

        public static void AssertTextEqual(this IWebDriver webDriver, Selector selector, string value)
        {
            var element = webDriver.UniqueElement(selector);

            if (!element.Text.Equals(value))
            {
                throw new Exception($"{selector.Name} Text equal to {element.Text} is not like excpeted {value}");
            }
        }

        public static void TakeScreenshot(this IWebDriver webDriver, string fileName, string outputDirectory)
        {
            //var projectOutputDirectory = 
                //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var sessionId = ((RemoteWebDriver)webDriver).SessionId;
            var screenshotFullPath = Path.Combine(outputDirectory, $"{sessionId}");
            var screenshot = ((ITakesScreenshot)webDriver).GetScreenshot();
            var filePath = Path.Combine(screenshotFullPath, fileName);
            var directoryName = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (File.Exists(filePath))
            {
                throw new Exception($"Save screenshot : the file {filePath}  already exist");
            }
            screenshot.SaveAsFile(filePath);
        }

        public static IWebElement UniqueElement(this IWebDriver webDriver, Selector selector)
        {
            IWebElement element = null;
            Exception exception  = null;
            int retry = 0;
            while (element == null && retry < FetchRetries)
            {
                var by = ByHelper.Construct(selector);
                try
                {
                    var elements = webDriver.FindElements(by);
                    var displayedElements = elements.Where(e => e.Displayed);
                    var webElements = displayedElements.ToList();
                    if (webElements.Count > 1)
                    {
                        exception = new InvalidSelectorException(
                            $"{selector.Name} return more than 1 displayed element");
                    }

                    element = webElements.FirstOrDefault();
                    System.Threading.Thread.Sleep(RetrySleepInterval);
                }
                catch (Exception ex)
                {
                    //Silently error
                }
                retry++;
            }
            if (element == null)
            {
                if (exception == null)
                {
                    throw new InvalidSelectorException($"{selector.Name} no element to return with this selector");
                }
                throw exception;
            }
           
            return element;
        }

        public static async Task EnsureChromeDriverAsync()
        {            
            if (true)
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(SeleniumConfig.LastChromeDriverVersionUrl);
                    var chromeLastVersion = await response.Content.ReadAsStringAsync();
                    var zipFile = $"chromedriver_{chromeLastVersion}_win32.zip";

                    if (
                        !File.Exists(Path.Combine(zipFile)) &&
                        !File.Exists(Path.Combine("chrome", "chromedriver.exe"))
                        )
                    {
                        using (var cli = new WebClient())
                        {
                            var bytes = await Task.Run(() => cli.DownloadData($"https://chromedriver.storage.googleapis.com/{chromeLastVersion}/chromedriver_win32.zip"));

                            using (var stream = File.Create(zipFile))
                            {
                                await stream.WriteAsync(bytes, 0, bytes.Length);
                            }

                            FileInfo zipFileInfo = new FileInfo( zipFile);
                            var driverDirFullPath = Path.Combine(zipFileInfo.DirectoryName, SeleniumConfig.ChromeDriverLocation);
                            if (Directory.Exists(driverDirFullPath))
                            {
                                Directory.Delete(driverDirFullPath, true);
                            }

                            ZipFile.ExtractToDirectory(zipFileInfo.FullName, Path.Combine(zipFileInfo.DirectoryName, "chrome"), System.Text.Encoding.ASCII);
                        }
                    }
                }
            }
        }
    }
}
