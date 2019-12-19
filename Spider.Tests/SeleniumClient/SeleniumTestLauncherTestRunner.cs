namespace Spider.Test.SeleniumClient
{
    using NUnit.Framework;
    using Spider.Common.Model;
    using Spider.SeleniumClient;
    using System;

    [Parallelizable(ParallelScope.All)]
    public class SeleniumTestLauncherTestRunner
    {
        public SeleniumTestLauncherTestRunner()
        {            
        }

        [Test]
        [TestCase("TestBooks/Simple/HelloTalentConnectAndSearch.json")]
        [TestCase("TestBooks/Simple/CreateChromeSession.json")]
        [TestCase("TestBooks/Simple/HelloTalentSignIn.json")]
        [TestCase("TestBooks/Simple/SearchWithGoogle.json")]
        [TestCase("TestBooks/Simple/HelloTalentConnectAndSearchWrong.json")]
        public void ExecuteJsonFile(string jsonFile)
        {
            var envExecution = new ExecutionEnvironment()
            {
                OutputDirectoryLocation = ".",
                ContextDirectoryLocation = ".",
                ScenarioDirectoryLocation = ".",
                SiteMapDirectoryLocation = "."
            };
            var test = SeleniumTestLauncher.ExecuteTestFromJson(jsonFile, envExecution);
            if (test.Failed)
            {
                throw new Exception(test.StackTrace);
            }
        }

        public void TestRemoteGrid()
        {
            SeleniumTestLauncher.ExecuteSampleTest();
        }
    }
}
