namespace SiteMapManager.Helpers
{
    using System;
    using System.IO;
    using Models;
    using Spider.Common.Helper;
    using System.Linq;

    public static class SiteMapHelper
    {
        public static Page LoadPageFromJson(string siteMapFolder, string jsonFile)
        {
            var fileFullPath = Directory.GetFiles(siteMapFolder ?? throw new InvalidOperationException()
                    , $"*{jsonFile}",
                    SearchOption.AllDirectories).
                FirstOrDefault();
            Page context = JsonHelper.DeserializeObject<Page>(fileFullPath);
            return context;
        }
    }
}
