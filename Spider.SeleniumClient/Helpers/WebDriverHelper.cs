namespace Spider.SeleniumClient.Helpers
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Remote;
    using Spider.Common.Enums;
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

        public static IWebDriver CreateSession(ExecutionEnvironment executionEnvironment)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument(string.Format("--lang={0}", CultureInfo.CurrentCulture));

            FirefoxOptions firefoxOptions = new FirefoxOptions();
            firefoxOptions.BrowserExecutableLocation = @"C:\Program Files\Mozilla Firefox\firefox.exe";

            var projectOutputDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var webDriverPath = Path.Combine(projectOutputDirectory, SeleniumConfig.WebDriverLocation);

            IWebDriver webDriver = executionEnvironment.GridEnabled ?
                new RemoteWebDriver(new Uri(SeleniumConfig.SeleniumHubAddress), executionEnvironment.BrowserType == BrowserType.CHROME ? (DriverOptions) chromeOptions : firefoxOptions) :
                    executionEnvironment.BrowserType == BrowserType.CHROME ?
                    (IWebDriver)new ChromeDriver(webDriverPath, chromeOptions, TimeSpan.FromSeconds(60)) :
                    (IWebDriver)new FirefoxDriver(webDriverPath, firefoxOptions, TimeSpan.FromSeconds(60));
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

        public static void AssertExists(this IWebDriver webDriver, Selector selector)
        {
            var element = webDriver.UniqueElement(selector);

            if (element == null)
            {
                throw new Exception($"{element.Text} doesn't exist on the DOM.");
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
            Exception exception = null;
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

        public static async Task EnsureDriverAsync(ExecutionEnvironment executionEnvironment)
        {
            if (executionEnvironment.BrowserType == BrowserType.CHROME)
            {
                await EnsureChromeDriverAsync();
            }
            if (executionEnvironment.BrowserType == BrowserType.FIREFOX)
            {
                await EnsureFirefoxDriverAsync();
            }
        }

        private static async Task EnsureChromeDriverAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(SeleniumConfig.LastChromeDriverVersionUrl);
                var chromeLastVersion = await response.Content.ReadAsStringAsync();

                var path = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
                //Directory.GetCurrentDirectory();
                Console.WriteLine($"Spider Workdirectory is : {path}");
                //Environment.CurrentDirectory;
                // Path.GetTempPath();

                Console.WriteLine($"The last chrome driver: chromeLastVersion / Source {SeleniumConfig.LastChromeDriverVersionUrl}");
                var zipFile = $"chromedriver_{chromeLastVersion}_win32.zip";

                Console.WriteLine($"check if {Path.Combine(path, zipFile)} exist");
                Console.WriteLine($"check if {Path.Combine(path, "chrome", "chromedriver.exe")} exist");
                if (
                    !File.Exists(Path.Combine(path, zipFile)) ||
                    !File.Exists(Path.Combine(path, SeleniumConfig.WebDriverLocation, "chromedriver.exe"))
                    )
                {
                    using (var cli = new WebClient())
                    {
                        var zipUrl = $"https://chromedriver.storage.googleapis.com/{chromeLastVersion}/chromedriver_win32.zip";
                        Console.WriteLine($"Download driver zip file from {zipUrl}");
                        var bytes = await Task.Run(() => cli.DownloadData(zipUrl));

                        using (var stream = File.Create(Path.Combine(path, zipFile)))
                        {
                            await stream.WriteAsync(bytes, 0, bytes.Length);
                        }
                    }

                    FileInfo zipFileInfo = new FileInfo(Path.Combine(path, zipFile));
                    var driverDirFullPath = Path.Combine(zipFileInfo.DirectoryName, SeleniumConfig.WebDriverLocation);

                    if (Directory.Exists(Path.Combine(driverDirFullPath, SeleniumConfig.WebDriverLocation)))
                    {
                        File.Delete(Path.Combine(driverDirFullPath, SeleniumConfig.WebDriverLocation));
                    }
                    Console.WriteLine($"Unzipping ino {zipFileInfo.FullName}");
                    ZipFile.ExtractToDirectory(zipFileInfo.FullName, Path.Combine(zipFileInfo.DirectoryName, SeleniumConfig.WebDriverLocation), System.Text.Encoding.ASCII);
                }
            }
        }

        private static async Task EnsureFirefoxDriverAsync()
        {
            var zipUrl = SeleniumConfig.LastFirefoxDriverVersionUrl;
            var path = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
            var zipFile = $"geckodriver-v0.26.0-win32.zip";
            if (
                    !File.Exists(Path.Combine(path, zipFile)) ||
                    !File.Exists(Path.Combine(path, SeleniumConfig.WebDriverLocation, "geckodriver.exe"))
                    )
            {

                using (var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }))
                {
                    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "fr-FR,fr;q=0.9,en-US;q=0.8,en;q=0.7");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                    var response = await client.GetAsync(SeleniumConfig.LastFirefoxDriverVersionUrl);
                    zipUrl = response.Headers.Location.AbsoluteUri;
                }

                using (var client = new WebClient())
                {
                    client.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    client.Headers.Add("Accept-Language", "fr-FR,fr;q=0.9,en-US;q=0.8,en;q=0.7");
                    client.Headers.Add("Cache-Control", "no-cache");
                    Console.WriteLine($"Downloading driver zip file from {zipUrl} ... ");
                    client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DownloadFile(new Uri(zipUrl), zipFile);
                    Console.WriteLine($"{zipUrl} donwloaded. ");
                }

                FileInfo zipFileInfo = new FileInfo(Path.Combine(path, zipFile));
                var driverDirFullPath = Path.Combine(zipFileInfo.DirectoryName, SeleniumConfig.WebDriverLocation);
                if (Directory.Exists(Path.Combine(driverDirFullPath, SeleniumConfig.WebDriverLocation)))
                {
                    File.Delete(Path.Combine(driverDirFullPath, SeleniumConfig.WebDriverLocation));
                }
                Console.WriteLine($"Unzipping ino {zipFileInfo.FullName}");
                ZipFile.ExtractToDirectory(zipFileInfo.FullName, Path.Combine(zipFileInfo.DirectoryName, SeleniumConfig.WebDriverLocation), System.Text.Encoding.ASCII);
                await Task.Delay(1000);
            }
        }
    }
}
