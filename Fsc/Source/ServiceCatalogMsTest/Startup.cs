using Microsoft.Extensions.Configuration;
using PCAT.Common.Caching;
using ServiceCatalog.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalogMsTest
{
    class Startup
    {
        public static void LoadSettings()
        {
            var config = GetConfiguration();
            FscApplication.LoadSettings(config);

            PCAT.Common.Caching.CacheManager.ScopedCache = new DictionaryCache();
        }

        private static IConfiguration GetConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            return config;
        }
    }
}
