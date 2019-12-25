using Newtonsoft.Json;
using NUnit.Framework;
using Spider.Steps.Helpers;
using Spider.Steps.Steps;

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

        [Category("UNIT-TEST")]
        [Test]
        public void ReturnAListOfStepsWhenJsonIsCorrect()
        {
            var steps = StepsDeserializer.Deserialize(CORRECT_STEPS);

            Assert.IsNotEmpty(steps);
            Assert.IsInstanceOf<NavigateToUrl>(steps[0]);
            Assert.AreEqual("Google.com", (steps[0] as NavigateToUrl).Url);
            Assert.IsInstanceOf<ClickButton>(steps[1]);
            Assert.AreEqual("#BUTTON_ID", (steps[1] as ClickButton).Selector);
        }

        [Category("UNIT-TEST")]
        [Test]
        public void ThrowAnExceptionWhenAStepIsNotSupported()
        {
            Assert.Throws(typeof(JsonSerializationException), () => StepsDeserializer.Deserialize(INCORRECT_STEPS));
        }
    }
}