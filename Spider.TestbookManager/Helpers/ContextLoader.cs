

namespace Spider.TestbookManager.Models
{
    using System.Linq;
    using Spider.Common.Helper;
    using Spider.Common.Model;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class ContextLoader
    {
        public static string ContextFolder => ConfigHelper.GetStringValue("Spider.Context.Folder");

        private static volatile ContextLoader instance;
        private static object syncRoot = new Object();
        private static object syncDic = new Object();

        private ContextLoader() { }

        public static ContextLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ContextLoader();
                    }
                }

                return instance;
            }
        }

        public Dictionary<string, Context> DicoContext = new Dictionary<string, Context>();

        public Context GetContext(string contextKey, string contextFolder)
        {
            lock (syncDic)
            {
                if (!DicoContext.ContainsKey(contextKey))
                {
                    DicoContext.Add(contextKey, this.LoadContextFromJson(contextFolder, $"{contextKey}.json"));
                }
            }
            return DicoContext[contextKey];
        }

        private Context LoadContextFromJson(string contextFolder, string jsonFile)
        {
            var fileFullPath = Directory.GetFiles(
                contextFolder ?? throw new Exception($"Context Folder is not defined in this execution: Cannot fetch {jsonFile}")
                , $"*{jsonFile}"
                ,SearchOption.AllDirectories).FirstOrDefault();
            Context context = JsonHelper.DeserializeObject<Context>(fileFullPath);
            return context;
        }
    }
}
