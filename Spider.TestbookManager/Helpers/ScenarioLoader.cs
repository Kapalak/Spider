using System.Linq;

namespace Spider.TestbookManager.Models
{
    using Spider.Common.Helper;
    using Spider.Common.Model;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class ScenarioLoader
    {
        public static string _scenarioFolder => ConfigHelper.GetStringValue("Spider.Scenario.Folder");

        private static volatile ScenarioLoader instance;
        private static object syncRoot = new Object();
        private static object syncDic = new Object();

        private ScenarioLoader() { }

        public static ScenarioLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ScenarioLoader();
                    }
                }

                return instance;
            }
        }

        public Dictionary<string, Scenario> DicoScenario = new Dictionary<string, Scenario>();

        public Scenario GetScenario(string scenarioKey, string scenarioFolder)
        {
            lock(syncDic)
            {
                if (!DicoScenario.ContainsKey(scenarioKey))
                {
                    DicoScenario.Add(scenarioKey, this.LoadScenarioFromJson(scenarioFolder, $"{scenarioKey}.json"));
                }
            }

            lock (syncDic)
            {
                return DicoScenario[scenarioKey];
            }
        }

        private Scenario LoadScenarioFromJson(string scenarioFolder, string jsonFile)
        {
            var fileFullPath = Directory.GetFiles(scenarioFolder ?? throw new InvalidOperationException()
                , $"*{jsonFile}",
                SearchOption.AllDirectories).
                FirstOrDefault();
            Scenario page = JsonHelper.DeserializeObject<Scenario>(fileFullPath);
            return page;
        }
    }
}
