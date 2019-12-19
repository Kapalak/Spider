using OpenQA.Selenium;
using Spider.Steps.Abstractions;

namespace Spider.Steps.Steps
{
    public class NavigateToUrlHandler : IStepHandler<NavigateToUrl>
    {
        public void Handle(NavigateToUrl step, ref IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl(step.Url);
        }
    }
}