namespace Spider_cli
{
    using Fclp;
    using Spider.Common.Enums;
    using Spider.Common.Model;
    using Spider.SeleniumClient;
    using Spider.SeleniumClient.Helpers;
    using System;
    using System.Threading.Tasks;

    class Program
    {
        public static async Task Main(string[] args)
        {

            var fluentParser = SetupArguments();

            ICommandLineParserResult result;
            result = fluentParser.Parse(args);

            if (result.HasErrors == false)
            {
                if (fluentParser.Object.Tests != null)
                {
                    var tests = fluentParser.Object.Tests;
                    if (fluentParser.Object.ParallelScope == ParallelScope.None)
                    {
                        Console.WriteLine("Executing tests in series");
                        foreach (var test in tests)
                        {
                            await ExecuteTestAsync(test, fluentParser.Object);
                        }
                    }

                    if (fluentParser.Object.ParallelScope == ParallelScope.All)
                    {
                        Console.WriteLine("Executing tests in parallel");
                        Parallel.ForEach(tests, async test =>
                        {
                            await ExecuteTestAsync(test, fluentParser.Object);
                        });
                    }
                }
            }
            else
            {
                Console.Write("Spider-cli: ");
                Console.WriteLine(result.ErrorText);
                Console.WriteLine("Try `Spider-cli.exe --help' for more information.");
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
                .As('l', "tests-location")
                .WithDescription("\nSelected tests location to be ran");

            fluentParser.Setup(arg => arg.ParallelScope)
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

            fluentParser.SetupHelp("?", "help")
                .Callback(text => Console.WriteLine(text));

            return fluentParser;

        }

        private static async Task ExecuteTestAsync(string test, ExecutionEnvironment execEnv)
        {
            Console.WriteLine("Begin Executing test");
            var executionTest = await SeleniumTestLauncher.ExecuteTestFromJsonAsync(test, execEnv);
            Console.WriteLine($"End Executing {executionTest.Name}");
            var status = executionTest.Failed ? "Failed" : "Success";
            Console.WriteLine($"{status}");
            Console.WriteLine($"{executionTest.StackTrace}");
            Console.WriteLine($"{executionTest.Measure.StartDate}");
            Console.WriteLine($"{executionTest.Measure.EndDate}");
        }
    }
}
