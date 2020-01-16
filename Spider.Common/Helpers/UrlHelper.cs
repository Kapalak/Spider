namespace Spider.Common.Helper
{
    using System;
    using System.IO;

    public static class UrlHelper
    {
        public static Uri Combine(params string[] paths)
        {
            string url = string.Empty;
            foreach(var path in paths)
            {
                if (path.StartsWith("http") || string.IsNullOrEmpty(url))
                {
                    url = path;
                }
                else
                {
                    var newUri = new Uri(url);
                    var baseUrl = newUri.AbsoluteUri.Substring(0, newUri.AbsoluteUri.Length - newUri.LocalPath.Length);                    
                    url = baseUrl + path;
                }
            }
            var build = new UriBuilder(url);
            return build.Uri;
        }
    }
}