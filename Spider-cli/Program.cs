namespace Spider_cli
{
    using Fclp;
    using NLog;
    using Spider.Common.Enums;
    using Spider.Common.Model;
    using Spider.SeleniumClient;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
                        foreach (var test in tests)
                        {
                            await ExecuteTestAsync(test, fluentParser.Object);
                        }
                    }

                    if (fluentParser.Object.ParallelScope == ParallelScope.All)
                    {
                        _log_.Info("Executing tests in parallel");
                        List<Task> tasks = new List<Task>();
                        foreach (var test in tests)
                        {
                            tasks.Add(ExecuteTestAsync(test, fluentParser.Object));
                        }

                        Task.WaitAll(tasks.ToArray());
                    }
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

            fluentParser.SetupHelp("?", "help")
                .Callback(text => _log_.Info(text));

            return fluentParser;

        }

        private static async Task ExecuteTestAsync(string test, ExecutionEnvironment execEnv)
        {
            _log_.Info("Begin Executing test");

            var executionTest = await SeleniumTestLauncher.ExecuteTestFromJsonAsync(test, execEnv);
            _log_.Info($"End Executing {executionTest.Name}");
            var status = executionTest.Failed ? "Failed" : "Success";
            Console.ForegroundColor = executionTest.Failed ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine($"{test} {status}");
            Console.ResetColor();
            Console.WriteLine($"{executionTest.StackTrace}");
            Console.WriteLine($"{executionTest.Measure.StartDate}");
            Console.WriteLine($"{executionTest.Measure.EndDate}");
           
        }
    }
}
