namespace Spider_cli
{
    using Fclp;
    using NLog;
    using Spider.Common.Enums;
    using Spider.Common.Model;
    using Spider.SeleniumClient;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using Spider.Reporting;
    using System.Diagnostics;
    using System.IO;

    class Program
    {
        private static readonly Logger _log_ = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            //var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            //config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Rules for mapping loggers to targets        
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);

            // Apply config           
            LogManager.Configuration = config;


            var fluentParser = SetupArguments();
            ICommandLineParserResult result;
            result = fluentParser.Parse(args);



            if (result.HasErrors == false)
            {
                List<string> tests = new List<string>();

                for (int index = 0; index < LogManager.Configuration.LoggingRules.Count; index++)
                {
                    LogManager.Configuration.LoggingRules[index].SetLoggingLevels(fluentParser.Object.LogLevel, LogLevel.Fatal);
                }

                if (fluentParser.Object.Tests != null)
                {
                    tests.AddRange(fluentParser.Object.Tests);
                }

                if (fluentParser.Object.TestsLocation != null)
                {
                    var testfiles = Directory.GetFiles(fluentParser.Object.TestsLocation, "*.json", SearchOption.TopDirectoryOnly);
                    foreach(var testfile in testfiles)
                    {
                        tests.Add(Path.Combine(fluentParser.Object.TestsLocation, testfile));
                    }                   
                }

                if (tests.Count > 0)
                {
                    if (fluentParser.Object.ParallelScope == ParallelScope.None)
                    {
                        _log_.Info("Executing tests in series");
                        List<ITest> testExecutions = new List<ITest>();
                        foreach (var test in tests)
                        {
                            testExecutions.Add(await ExecuteTestAsync(test, fluentParser.Object));
                        }
                        WriteTestResults(testExecutions);
                        ReportingHelper.GenerateHtmlReport(fluentParser.Object);
                    }

                    if (fluentParser.Object.ParallelScope == ParallelScope.All)
                    {
                        _log_.Info("Executing tests in parallel");
                        List<Task<ITest>> tasks = new List<Task<ITest>>();
                        foreach (var test in tests)
                        {
                            tasks.Add(ExecuteTestAsync(test, fluentParser.Object));
                        }

                        Task.WaitAll(tasks.ToArray());

                        var testExecutions = tasks.Select(ts => ts.Result);

                        WriteTestResults(testExecutions);
                        ReportingHelper.GenerateHtmlReport(fluentParser.Object);
                    }
                }
                else
                {
                    _log_.Error("Spider-cli: ");
                    _log_.Error(result.ErrorText);
                    _log_.Error("Try `Spider-cli.exe --help' for more information.");
                    return;
                }
            }
        }

        public static void WriteTestResults(IEnumerable<ITest> tests)
        {
            Console.WriteLine($"{tests.Where(t => !(t.Failed.HasValue && t.Failed.Value)).Count()} successful tests");
            Console.WriteLine($"{tests.Where(t => (t.Failed.HasValue && t.Failed.Value)).Count()} failed tests");

            foreach (var test in tests)
            {
                Console.ForegroundColor = (test.Failed.HasValue && test.Failed.Value) ? ConsoleColor.Red : ConsoleColor.Green;
                var status = (test.Failed.HasValue && test.Failed.Value) ? "Failed" : "Success";
                Console.WriteLine($"{test.Name} - {status} - [{test.Measure.StartDate} - {test.Measure.EndDate} *****************");
                if (test.Failed.HasValue && test.Failed.Value)
                {
                    Console.WriteLine($"{test.StackTrace}");
                }
                Console.ResetColor();
            }
        }

        private static FluentCommandLineParser<ExecutionEnvironment> SetupArguments()
        {
            var fluentParser = new FluentCommandLineParser<ExecutionEnvironment>();

            fluentParser.Setup(arg => arg.Title)
                .As('e', "title")
                .WithDescription("\nSelected tests to be ran");

            fluentParser.Setup(arg => arg.Tests)
                .As('t', "tests")
                .WithDescription("\nSelected tests to be ran");

            fluentParser.Setup(arg => arg.TestsLocation)
                .As('d', "tests-location")
                .WithDescription("\nSelected tests location to be ran");

            fluentParser.Setup(arg => arg.ParallelScopeString)
                .As('p', "parallel-scope")
                .WithDescription("\nDefine the running Parallel scope (None/All) in series or in parallel");

            fluentParser.Setup(arg => arg.BrowserTypeString)
                .As('b', "browser-type").SetDefault("chrome")
                .WithDescription("\nDefine the running browser type");

            fluentParser.Setup(arg => arg.OutputDirectoryLocation)
                .As('o', "output-directory")
                .WithDescription("\nDefine the output directory");

            fluentParser.Setup(arg => arg.ReportTemplate)
                .As('r', "report-template")
                .WithDescription("\nDefine the report template");            

            fluentParser.Setup(arg => arg.ContextDirectoryLocation)
                .As('c', "contexts-directory")
                .WithDescription("\nDefine the context directory");

            fluentParser.Setup(arg => arg.ScenarioDirectoryLocation)
                .As('s', "scenario-directory")
                .WithDescription("\nDefine the scenario directory");

            fluentParser.Setup(arg => arg.SiteMapDirectoryLocation)
                .As('m', "sitemap-directory")
                .WithDescription("\nDefine the sitemap directory");

            fluentParser.Setup(arg => arg.GridEnabled)
                 .As('g', "grid-enabled")
              .WithDescription("\nExecute test on remote selenium grid");

            fluentParser.Setup(arg => arg.InteractiveMode)
              .As('i', "interactive-mode")
               .WithDescription("\nEnable interactive mode (display report after execution)");

            fluentParser.Setup(arg => arg.BinaryLocation)
               .As('x', "binary-location")
               .WithDescription("\nLocation to the browser exe");

            fluentParser.Setup(arg => arg.LogLevelString)
               .As('l', "log-level")
               .WithDescription("\nChoose the log level");

            fluentParser.Setup(arg => arg.SeleniumHubAddress)
            .As('h', "selenium-hub")
            .WithDescription("\nChoose the selenium hub");

            fluentParser.SetupHelp("?", "help")
                .Callback(text => _log_.Info(text));

            return fluentParser;

        }

        private static async Task<ITest> ExecuteTestAsync(string test, ExecutionEnvironment executionEnvironment)
        {
            _log_.Info("Begin Executing test");
            var executionTest = await SeleniumTestLauncher.ExecuteTestFromJsonAsync(test, executionEnvironment);
            _log_.Info($"End Executing {executionTest.Name}");
            return executionTest;
        }
    }
}
