using NLog;
using Spider.Common.Enums;
using System;
using System.Collections.Generic;

namespace Spider.Common.Model
{
    public class ExecutionEnvironment
    {
        /// <summary>
        /// Specify the selected test file path.
        /// </summary>
        public string Title { get; set; }
        public List<string> Tests { get; set; }

        /// <summary>
        /// Specify a test directory to run
        /// </summary>
        public string TestsLocation { get; set; }
        public BrowserType BrowserType => (BrowserType) Enum.Parse(typeof(BrowserType), value: BrowserTypeString, true);
        public string BrowserTypeString { get; set; } = "chrome";
        public string ParallelScopeString { get; set; } = "None";
        public ParallelScope ParallelScope => (ParallelScope)Enum.Parse(typeof(ParallelScope), value: ParallelScopeString, true);

        public string OutputDirectoryLocation { get; set; } = "results";
        public string ReportTemplate { get; set; } = @"Templates\DefaultTemplate.html";
        public string ScenarioDirectoryLocation { get; set; }
        public string ContextDirectoryLocation { get; set; }
        public string SiteMapDirectoryLocation { get; set; }
        public bool GridEnabled { get; set; }
        public bool InteractiveMode { get; set; }
        public string BinaryLocation { get; set; }

        public string LogLevelString { get; set; } = "Error";
        public LogLevel LogLevel => LogLevel.FromString(LogLevelString);

        public string SeleniumHubAddress { get; set; } = "http://localhost:4444/wd/hub";
    }
}
