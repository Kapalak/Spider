using System.Collections.Generic;

namespace Spider.Common.Model
{
    public interface ITest
    {
        string Description { get; set; }
        bool? Failed { get; set; }
        string FileName { get; set; }
        string FilePath { get; set; }
        IMeasure Measure { get; }
        string Name { get; set; }
        string StackTrace { get; set; }
        //List<IStep> Steps { get; set; }
    }
}