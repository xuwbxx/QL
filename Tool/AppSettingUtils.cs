using Microsoft.Extensions.Configuration;

namespace Tool
{
    public static class AppSettingUtils
    {
        private static readonly IConfiguration Configuration;

        static AppSettingUtils()
        {
            Configuration = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();
        }

        public static string GetSetting(string key)
        {
            return Configuration[key];
        }

        public static void Test()
        {
            //"AppSettings": {
            //    "SiteTitle": "我的网站",
            //    "ApiKey": "abc123xyz",
            //    "Login": {
            //                    "CookieKey": "CCSHJTechUserInfo",
            //      "Key": "8rd2f07d0f5e74bb84cf6ae4",
            //      "IV": "ShjCloud"
            //    }
            //}
            //AppSettingUtils.GetSetting("AppSettings:Login:CookieKey");
        }

    }


}
