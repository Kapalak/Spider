using Spider.Common.Enums;

namespace Spider.Common.Model
{
    public interface IStep
    {
        string Description { get; set; }
        bool? Failed { get; set; }
        Measure Measure { get; }
        string Name { get; set; }
        string Page { get; set; }
        string Param { get; set; }
        Selector Selector { get; set; }
        string SessionId { get; set; }
        string StackTrace { get; set; }
        bool TakeScreenshotAfter { get; set; }
        bool TakeScreenshotBefore { get; set; }
        StepType Type { get; }
        string Value { get; set; }
    }
}