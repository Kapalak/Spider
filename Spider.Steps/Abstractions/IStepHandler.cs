using OpenQA.Selenium;

namespace Spider.Steps.Abstractions
{
    public interface IStepHandler<T> where T : IStep
    {
        void Handle(T step, ref IWebDriver webDriver);
    }
}