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
                for (int index = 0; index < LogManager.Configuration.LoggingRules.Count; index++)
                {
                    LogManager.Configuration.LoggingRules[index].SetLoggingLevels(fluentParser.Object.LogLevel, LogLevel.Fatal);
                }

                if (fluentParser.Object.Tests != null)
                {
                    var tests = fluentParser.Object.Tests;

                    if (fluentParser.Object.ParallelScope == ParallelScope.None)
                    {
                        _log_.Info("Executing tests in series");
                        List<ITest> testExecutions = new List<ITest>();
                        foreach (var test in tests)
                        {
                            testExecutions.Add(await ExecuteTestAsync(test, fluentParser.Object));
                        }
                        WriteTestResults(testExecutions);
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
            Console.WriteLine($"{tests.Where(t => !t.Failed).Count()} successful tests");
            Console.WriteLine($"{tests.Where(t => t.Failed).Count()} failed tests");

            foreach (var test in tests)
            {
                Console.ForegroundColor = test.Failed ? ConsoleColor.Red : ConsoleColor.Green;
                var status = test.Failed ? "Failed" : "Success";
                Console.WriteLine($"{test.Name} - {status} - [{test.Measure.StartDate} - {test.Measure.EndDate} *****************");
                if (test.Failed)
                {
                    Console.WriteLine($"{test.StackTrace}");
                }
                Console.ResetColor();
            }
        }

        private static FluentCommandLineParser<ExecutionEnvironment> SetupArguments()
        {
            var fluentParser = new FluentCommandLineParser<ExecutionEnvironment>();

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
