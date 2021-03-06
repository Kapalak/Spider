﻿namespace Spider.SeleniumClient
{
    using System.IO;
    using OpenQA.Selenium.Support.Extensions;
    using System;
    using OpenQA.Selenium;
    using Spider.Common.Enums;
    using Spider.SeleniumClient.Helpers;
    using Spider.Common.Helper;
    using Spider.Common.Model;
    using Spider.TestbookManager.Helper;
    using System.Threading.Tasks;
    using NLog;

    public static class SeleniumTestLauncher
    {
        private static readonly Logger _log_ = LogManager.GetCurrentClassLogger();
        public static string CreateJsonSampleTest()
        {
            Test newTest = new Test() { Name = "My First Test", Description = "This Test ..." };
            Step createSession = new Step() { Name = "CREATE_SESSION", Description = "This step open the browser" };
            newTest.Steps.Add(createSession);
            string jsonTest = JsonHelper.SerializeObject(newTest);
            return jsonTest;
        }

        public static async Task<string> ExecuteAsync(this Test test, ExecutionEnvironment executionEnvironment)
        {
            test.Title = executionEnvironment.Title;
            test.Measure.StartDate = DateTime.Now;
            _log_.Trace($"Begin Executing test {test.Name} | {test.Measure.StartDate}");
            string sessionId = string.Empty;
            IWebDriver testWebDriver = null;
            try
            {
                foreach (Step step in test.Steps)
                {
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        step.SessionId = sessionId;
                    } 
                    else
                    {
                        if (executionEnvironment.GridEnabled == false)
                        {
                            await WebDriverHelper.EnsureDriverAsync(executionEnvironment);
                        }
                    }

                    step.Execute(ref testWebDriver, executionEnvironment);
                    if (sessionId != step.SessionId)
                    {
                        sessionId = step.SessionId;
                    }                   
                }
                test.Failed = false;
            }
            catch (Exception ex)
            {
                test.Failed = true;
                test.StackTrace = $"{test.Name} : {ex.Message}";
            }
            finally
            {
                test.Measure.EndDate = DateTime.Now;
                if (testWebDriver != null)
                {
                    testWebDriver.Close();
                    testWebDriver.Quit();
                }
            }

            _log_.Trace($"End Executing test {test.Name} | {test.Measure.StartDate} - {test.Measure.EndDate}");
            return sessionId;
        }

        public static void Execute(this Step step, ref IWebDriver webDriver, ExecutionEnvironment executionEnvironment)
        {
            try
            {
                step.Measure.StartDate = DateTime.Now;
                _log_.Trace($"Begin Executing Step {step.Name} | {step.Param}| {step.Value} | {step.Measure.StartDate}");
                switch (step.Type)
                {
                    case (StepType.CREATE_SESSION):
                        {
                            webDriver = WebDriverHelper.CreateSession(executionEnvironment);
                            var sessionId = ((OpenQA.Selenium.Remote.RemoteWebDriver)webDriver).SessionId;
                            var windowSize = ConvertStringToSize(step.Param);
                            webDriver.ResizeWindow(windowSize);
                            _log_.Trace($"Session created : {sessionId.ToString()}");
                            step.SessionId = sessionId.ToString();
                            break;
                        }
                    case (StepType.NAVIGATE_URL):
                        var url = UrlHelper.Combine(webDriver.Url, step.Param);
                        webDriver.Navigate().GoToUrl(url);
                        break;
                    case (StepType.CLICK_BUTTON):
                        webDriver.SmartClick(step.Selector);
                        break;
                    case (StepType.SET_TEXT):
                        webDriver.SetText(step.Selector, step.Value);
                        break;
                    case (StepType.MOUSE_HOVER):
                        webDriver.MouseHover(step.Selector);
                        break;
                    case (StepType.ASSERT_TEXT):
                        webDriver.AssertTextEqual(step.Selector, step.Value);
                        break;
                    case (StepType.TAKE_SCREENSHOT):
                        webDriver.TakeScreenshot(step.Value, Path.Combine(executionEnvironment.OutputDirectoryLocation));
                        break;
                    case (StepType.RESIZE_WINDOW):
                        {
                            var windowSize = ConvertStringToSize(step.Param);
                            webDriver.ResizeWindow(windowSize);
                            break;
                        }
                    case (StepType.EXECUTE_JAVASCRIPT):
                        var result = webDriver.ExecuteJavaScript<string>(step.Param);
                        if (result != step.Value)
                        {
                            throw new Exception($"{step.Param} return {result} and not {step.Value} as expected.");
                        }
                        break;
                    case (StepType.ASSERT_EXISTS):
                        webDriver.AssertExists(step.Selector);
                        break;
                    case StepType.EXECUTE_SCENARIO:
                        throw new InvalidOperationException("Selenium launcher execute only elementary step");
                    default:
                        throw new NotImplementedException();

                }
                step.Measure.EndDate = DateTime.Now;
                step.Failed = false;
                _log_.Trace($"End Executing Step {step.Name} | {step.Measure.StartDate} - {step.Measure.EndDate}");
            }
            catch (Exception ex)
            {
                _log_.Trace($"End Executing Step with failure {step.Name} | {ex.Message}");
                step.Failed = true;
                step.StackTrace = ex.Message;
                throw ex;
            }
        }

        public static async Task<ITest> ExecuteTestFromJsonAsync(string jsonFile, ExecutionEnvironment executionEnvironment)
        {
            var test = TestBookHelper.ReadTestFromJson(jsonFile);
            test.ConvertScenarioToElementarySteps(executionEnvironment);
            test.InsertScreenshotSteps();
            test.ConvertFromPageObject(executionEnvironment);
            //to use on debug mode only
            //TestBookHelper.SaveTestToJson(test, $"{test.FilePath.Replace(".json", "-conv.json")}");
            var sessionId = await test.ExecuteAsync(executionEnvironment);
            //TestBookHelper.SaveTestToJson(test, $"{test.FilePath.Replace(".json", "-result.json")}");
            var outputFile = Path.Combine(executionEnvironment.OutputDirectoryLocation, sessionId, test.FileName.Replace(".json", "-result.json"));
            TestBookHelper.SaveTestToJson(test, outputFile);
            return test;
        }

        private static System.Drawing.Size? ConvertStringToSize(string size)
        {
            int width = 0;
            int heigth = 0;
            var parseSucceded = int.TryParse(size.Contains("*") ? size.Split('*')[0] : string.Empty, out width);
            parseSucceded = parseSucceded && int.TryParse(size != null && size.Contains("*") ? size.Split('*')[1] : string.Empty, out heigth);
            var windowsSize = (parseSucceded) ? new System.Drawing.Size(width, heigth) : (System.Drawing.Size?)null;
            return windowsSize;
        }
    }
}
