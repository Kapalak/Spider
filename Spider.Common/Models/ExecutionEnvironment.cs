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
        public List<string> Tests { get; set; }

        /// <summary>
        /// Specify a test directory to run
        /// </summary>
        public string TestsLocation { get; set; }
        public BrowserType BrowserType => (BrowserType) Enum.Parse(typeof(BrowserType), value: BrowserTypeString, true);
        public string BrowserTypeString { get; set; }

        public ParallelScope ParallelScope { get; set; }

        public string OutputDirectoryLocation { get; set; }

        public string ScenarioDirectoryLocation { get; set; }
        public string ContextDirectoryLocation { get; set; }
        public string SiteMapDirectoryLocation { get; set; }
        public bool GridEnabled { get; set; }
    }
}
