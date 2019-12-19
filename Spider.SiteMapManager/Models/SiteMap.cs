namespace SiteMapManager.Models
{
    using SiteMapManager.Helpers;
    using Spider.Common;
    using Spider.Common.Model;
    using System;
    using System.Collections.Generic;

    public class SiteMap
    {
        private static volatile SiteMap _instance;
        private static readonly object syncRoot = new Object();
        private static readonly object syncDic = new Object();

        private SiteMap() { }

        public static SiteMap Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SiteMap();
                    }
                }

                return _instance;
            }
        }

        public Dictionary<string, Page> Pages = new Dictionary<string, Page>();

        public Page GetPage(string pageKey, ExecutionEnvironment execEnv)
        {
            lock (syncDic)
            {
                if (!Pages.ContainsKey(pageKey))
                {
                    Pages.Add(pageKey, SiteMapHelper.LoadPageFromJson(execEnv.SiteMapDirectoryLocation, $"{pageKey}.json"));
                }
            }

            lock (syncDic)
            {
                return Pages[pageKey];
            }
        }
    }
}
