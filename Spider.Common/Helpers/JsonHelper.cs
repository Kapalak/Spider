namespace Spider.Common.Helper
{
    using Newtonsoft.Json;
    using System.IO;

    public static class JsonHelper
    {
        public static string GetFileContent(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{file} not founded");
            }
            using (StreamReader r = new StreamReader(file))
            {
                
                string json = r.ReadToEnd();
                return json;
            }
        }
        public static T DeserializeObject<T>(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{file} not founded");
            }

            using (StreamReader r = new StreamReader(file))
            {
                string json = r.ReadToEnd();
                T page = JsonConvert.DeserializeObject<T>(json);
                return page;
            }
        }

        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static void SaveObjectIntoFile(object value, string fileName)
        {
            var content = JsonConvert.SerializeObject(value);

            File.WriteAllText(fileName, content);
        }
    }
}
