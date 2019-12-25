using FluentAssertions;
using Newtonsoft.Json;
using Spider.Steps.Helpers;
using Spider.Steps.Steps;
using System;
using Xunit;

namespace Spider.Steps.UnitTests
{
    public class StepsDeserializerShould
    {
        private const string CORRECT_STEPS = @"[
                                                  {
                                                    ""$type"" : ""NavigateToUrl"",
                                                    ""Url"":""Google.com""
                                                  },
                                                  {
                                                    ""$type"" : ""ClickButton"",
                                                    ""Selector"":""#BUTTON_ID""
                                                  }
                                               ]";

        private const string INCORRECT_STEPS = @"[
                                                  {
                                                    ""$type"" : ""NavigateToUrl"",
                                                    ""Url"":""Google.com""
                                                  },
                                                  {
                                                    ""$type"" : ""Houlloulou"",
                                                    ""Selector"":""#BUTTON_ID""
                                                  }
                                               ]";

        [Fact]
        public void ReturnAListOfStepsWhenJsonIsCorrect()
        {
            var steps = StepsDeserializer.Deserialize(CORRECT_STEPS);

            steps.Should().NotBeEmpty();
            steps[0].Should().BeOfType<NavigateToUrl>();
            var rediretionStep = steps[0] as NavigateToUrl;
            rediretionStep.Url.Should().Be("Google.com");

            steps[1].Should().BeOfType<ClickButton>();
            var clickButtonStep = steps[1] as ClickButton;
            clickButtonStep.Selector.Should().Be("#BUTTON_ID");
        }

        [Fact]
        public void ThrowAnExceptionWhenAStepIsNotSupported()
        {
            Action act = () => StepsDeserializer.Deserialize(INCORRECT_STEPS);

            act.Should().ThrowExactly<JsonSerializationException>();
        }
    }
}