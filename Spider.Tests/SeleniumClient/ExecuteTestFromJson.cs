namespace Spider.Test.SeleniumClient
{
    using Spider.SeleniumClient;
    using NUnit.Framework;
    using Spider.Common.Model;
    using System.Threading.Tasks;
    using Spider.SeleniumClient.Helpers;

    [Parallelizable(ParallelScope.All)]
    public class ExecuteTestFromJson
    {
        [SetUp]
        public async Task GlobalSetup()
        {
            await WebDriverHelper.EnsureChromeDriverAsync();
        }

        [Category("INT-TEST")]
        [Test]
        //[TestCase("Testbooks/Simple/HelloTalentConnectAndSearch.json")]
        [TestCase("Testbooks/Simple/CreateChromeSession.json")]
        //[TestCase("Testbooks/Simple/HelloTalentSignIn.json")]
        [TestCase("Testbooks/Simple/SearchWithGoogle.json")]
        //[TestCase("Testbooks/Simple/HelloTalentConnectAndSearchWrong.json")]
        public void ExecuteTestFromFile(string jsonFile)
        {
            var envExecution = new ExecutionEnvironment() { 
                OutputDirectoryLocation = ".",
                ContextDirectoryLocation = ".",
                SiteMapDirectoryLocation = ".", 
                ScenarioDirectoryLocation = "."
            };
            SeleniumTestLauncher.ExecuteTestFromJson(jsonFile, envExecution);
        }

        [Category("UNIT-TEST")]
        [Test]
        public void MyUnitTest()
        {
            Assert.IsTrue(true);
        }
    }
}
