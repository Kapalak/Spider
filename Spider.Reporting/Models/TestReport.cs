﻿namespace Spider.Reporting.Models
{
    using Spider.Common.Model;

    public class TestReport : Test
    {
        public string OutputDirectory { get; set; }
        public string Status
        {
            get
            {
                return Failed ? "Failed" : "Success";
            }
        }
    }
}
