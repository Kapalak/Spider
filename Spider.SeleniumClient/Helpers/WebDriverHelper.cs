namespace Spider.SeleniumClient.Helpers
{
    using NLog;
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
        private static readonly Logger _log_ = LogManager.GetCurrentClassLogger();

        public static int FetchRetries => ConfigHelper.GetIntValue("SeleniumClient.Fetch.MaxRetries", 1);
        public static int RetrySleepInterval => ConfigHelper.GetIntValue("SeleniumClient.Fetch.RetrySleepInterval", 500);

        public static IWebDriver CreateSession(ExecutionEnvironment executionEnvironment)
        {
            _log_.Trace($"Create Session : browser type {executionEnvironment.BrowserType} / GridEnabled {executionEnvironment.GridEnabled} ");

            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument(string.Format("--lang={0}", CultureInfo.CurrentCulture));

            FirefoxOptions firefoxOptions = new FirefoxOptions
            {
                BrowserExecutableLocation = @"C:\Program Files\Mozilla Firefox\firefox.exe"
            };

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
            _log_.Trace($"Resize Window {windowSize.Value} ");
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
            _log_.Trace($"SmartClick {selector.SelectorType.ToString()} {selector.Text}");
            var element = webDriver.UniqueElement(selector);
            if (element.Enabled)
            {
                element.Click();
            }
        }

        public static void SetText(this IWebDriver webDriver, Selector selector, string value)
        {
            _log_.Trace($"SetText {selector.SelectorType.ToString()} {selector.Text} {value}");
            var element = webDriver.UniqueElement(selector);
            element.SendKeys(value);
        }

        public static void AssertTextEqual(this IWebDriver webDriver, Selector selector, string value)
        {
            _log_.Trace($"AssertTextEqual {selector.SelectorType.ToString()} {selector.Text} {value}");
            var element = webDriver.UniqueElement(selector);

            if (!element.Text.Equals(value))
            {
                throw new Exception($"{selector.Name} Text equal to {element.Text} is not like excpeted {value}");
            }
        }

        public static void AssertExists(this IWebDriver webDriver, Selector selector)
        {
            _log_.Trace($"AssertExists {selector.SelectorType.ToString()} {selector.Text}");
            var element = webDriver.UniqueElement(selector);

            if (element == null)
            {
                throw new Exception($"{element.Text} doesn't exist on the DOM.");
            }
        }

        public static void TakeScreenshot(this IWebDriver webDriver, string fileName, string outputDirectory)
        {
            _log_.Trace($"TakeScreenshot {outputDirectory} {fileName}");           
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
            _log_.Trace($"Get UniqueElement {selector.SelectorType.ToString()} {selector.Text}");
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
                    _log_.Warn($"{ex.StackTrace} {ex.Message}");
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
                _log_.Info($"Spider Workdirectory is : {path}");
                _log_.Info($"Spider Workdirectory is : {path}");
                //Environment.CurrentDirectory;
                // Path.GetTempPath();

                _log_.Info($"The last chrome driver: chromeLastVersion / Source {SeleniumConfig.LastChromeDriverVersionUrl}");
                var zipFile = $"chromedriver_{chromeLastVersion}_win32.zip";

                _log_.Info($"check if {Path.Combine(path, zipFile)} exist");
                _log_.Info($"check if {Path.Combine(path, "chrome", "chromedriver.exe")} exist");
                if (
                    !File.Exists(Path.Combine(path, zipFile)) ||
                    !File.Exists(Path.Combine(path, SeleniumConfig.WebDriverLocation, "chromedriver.exe"))
                    )
                {
                    using (var cli = new WebClient())
                    {
                        var zipUrl = $"https://chromedriver.storage.googleapis.com/{chromeLastVersion}/chromedriver_win32.zip";
                        _log_.Info($"Download driver zip file from {zipUrl}");
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
                    _log_.Info($"Unzipping ino {zipFileInfo.FullName}");
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
                    //client.Headers.Add("Connection", "keep-alive");
                    client.Headers.Add("Sec-Fetch-Site", "cross-site");
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36");
                    var fileFullPath = Path.Combine(path, zipFile);
                    _log_.Info($"Downloading driver zip file from {fileFullPath} ... ");
                    client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DownloadFile(new Uri(zipUrl), fileFullPath);
                    //File.Move(zipFile, fileFullPath);
                    _log_.Info($"{Path.Combine(path, zipFile)} donwloaded. ");
                }

                FileInfo zipFileInfo = new FileInfo(Path.Combine(path, zipFile));
                var driverDirFullPath = Path.Combine(zipFileInfo.DirectoryName, SeleniumConfig.WebDriverLocation);
                if (Directory.Exists(Path.Combine(driverDirFullPath, SeleniumConfig.WebDriverLocation)))
                {
                    File.Delete(Path.Combine(driverDirFullPath, SeleniumConfig.WebDriverLocation));
                }
                _log_.Info($"Unzipping ino {zipFileInfo.FullName}");
                ZipFile.ExtractToDirectory(zipFileInfo.FullName, Path.Combine(zipFileInfo.DirectoryName, SeleniumConfig.WebDriverLocation), System.Text.Encoding.ASCII);
                await Task.Delay(1000);
            }
        }
    }
}
