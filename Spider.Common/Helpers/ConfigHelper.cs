namespace Spider.Common.Helper
{
    using System.Configuration;
    using System.Drawing;

    public static class ConfigHelper
    {
        public static string GetStringValue(string key, string defaultValue = null)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            else
            {
                return value;
            }
        }

        public static bool GetBoolValue(string key)
        {
            bool value = bool.Parse(GetStringValue(key));
            return value;
        }

        public static int GetIntValue(string key, int defaultValue = 0)
        {
            var stringValue = GetStringValue(key);
            if (string.IsNullOrEmpty(stringValue))
            {
                return defaultValue;
            }
            else
            {
                int value = int.Parse(stringValue);
                return value;
            }
        }


        public static Size? GetSizeValue(string key)
        {
            var values = GetStringValue(key).Split('*');
            if (values.Length == 2)
            {
                Size value = new Size(int.Parse(values[0]), int.Parse(values[1]));
                return value;
            }
            return null;
        }
    }
}
