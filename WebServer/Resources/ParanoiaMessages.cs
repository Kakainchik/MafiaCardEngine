using System.Globalization;
using System.Resources;

namespace WebServer.Resources
{
    public static class ParanoiaMessages
    {
        private static readonly ResourceManager Manager =
            new("WebServer.Resources.ParanoiaMessages", typeof(ParanoiaMessages).Assembly);

        public static string Get(string key, bool russian)
        {
            return Manager.GetString(
                key,
                russian ? CultureInfo.GetCultureInfo("ru-RU") : CultureInfo.GetCultureInfo("en-US"))
                ?? key;
        }
    }
}
