namespace Spider.Test.SeleniumClient
{
    using NUnit.Framework;
    using Spider.Common.Model;
    using Spider.Reporting;
    using System.IO;
    using System.Reflection;

    [Parallelizable(ParallelScope.All)]
    public class GenerateReportingTest
    {

        [Test]     
        public void GenerateReport_CreateIndexFile()
        {
            var resultFolder = @"Reporting\Test1";
            ReportingHelper.GenerateHtmlReport(new ExecutionEnvironment() { OutputDirectoryLocation = "Reporting\\Test1" });
            Assert.IsTrue(File.Exists(Path.Combine(resultFolder, "index.html")));
        }        
    }
}
