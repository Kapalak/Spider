﻿//using EO.Pdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Nustache.Core;
using Spider.Common.Model;
using Spider.Reporting.Models;
using System.Collections.Generic;
using System.IO;

namespace Spider.Reporting
{
    public static class ReportingHelper
    {
        private static readonly Logger _log_ = LogManager.GetCurrentClassLogger();
        public static void GenerateHtmlReport(ExecutionEnvironment executionEnvironment)
        {
            var path = new DirectoryInfo(Path.Combine(".", executionEnvironment.OutputDirectoryLocation)).FullName;
            var files = Directory.GetFiles(path, "*-result.json", SearchOption.AllDirectories);
            List<TestReport> listTestReport = new List<TestReport>();

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var testText = File.ReadAllText(file);
                var testReport = JsonConvert.DeserializeObject<TestReport>(testText);
                var content = JsonConvert.DeserializeObject<JObject>(testText);
                var steps = JsonConvert.DeserializeObject<List<StepReport>>(content["Steps"].ToString());
                testReport.Steps.Clear();
                foreach(var step in steps)
                {
                    testReport.Steps.Add(step);
                }

                testReport.OutputDirectory = fileInfo.DirectoryName;
                listTestReport.Add(testReport);
            }

            foreach (var testreport in listTestReport)
            {
                foreach (var step in testreport.Steps)
                {
                    ((StepReport)step).TestParent = testreport;
                }
            }

            var renderer = Render.FileToString(Path.Combine(".", executionEnvironment.ReportTemplate), new
            {
                Reports = listTestReport
            });

            var reportHtmlFileInfo = new FileInfo(Path.Combine(path, "index.html"));
            File.WriteAllText(reportHtmlFileInfo.FullName, renderer);

            /*
            var reportPdfFileInfo = new FileInfo(Path.Combine(path, "report.pdf"));
            PdfDocument pdf = new PdfDocument();
            HtmlToPdf.ConvertHtml(renderer, pdf);
            pdf.Save(reportPdfFileInfo.FullName);
            */

            _log_.Trace($"{reportHtmlFileInfo.FullName} generated");
        }
    }
}
