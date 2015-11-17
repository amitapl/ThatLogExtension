using System.Configuration;

namespace ThatLogExtension.Controllers
{
    public static class Utils
    {
        public static string GetSetting(string environmentVariableKey)
        {
            var setting = ConfigurationManager.AppSettings[environmentVariableKey];
            if (setting == null)
            {
                var connectionString = ConfigurationManager.ConnectionStrings[environmentVariableKey];
                if (connectionString != null)
                {
                    setting = connectionString.ConnectionString;
                }
            }
            return setting;
        }
    }
}