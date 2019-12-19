namespace Spider.Test.SeleniumClient
{
    using Spider.SeleniumClient;
    using NUnit.Framework;
    using Spider.Common.Model;

    [Parallelizable(ParallelScope.Children)]
    public class ExecuteTestFromJson
    {
        [Test]
        [TestCase("Testbooks/Simple/HelloTalentConnectAndSearch.json")]
        [TestCase("Testbooks/Simple/CreateChromeSession.json")]
        [TestCase("Testbooks/Simple/HelloTalentSignIn.json")]
        [TestCase("Testbooks/Simple/SearchWithGoogle.json")]
        [TestCase("Testbooks/Simple/HelloTalentConnectAndSearchWrong.json")]
        public void ExecuteTestFromFile(string jsonFile)
        {
            var envExecution = new ExecutionEnvironment() { 
                OutputDirectoryLocation = ".",
                ContextDirectoryLocation = ".",
                SiteMapDirectoryLocation = "."
            };
            SeleniumTestLauncher.ExecuteTestFromJson(jsonFile, envExecution);
        }
    }
}
