using Spider.Steps.Abstractions;

namespace Spider.Steps.Steps
{
    public class ClickButton : IStep
    {
        public string Selector { get; set; }
    }
}