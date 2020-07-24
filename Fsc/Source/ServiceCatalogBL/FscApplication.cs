using Microsoft.Extensions.Configuration;
using PCAT.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using static ServiceCatalog.BL.FscSettings;

namespace ServiceCatalog.BL
{
    public class FscApplication: CatalogApplication
    {
        private static FscApplication _current;
        public new static FscApplication Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new FscApplication();
                    CatalogApplication.Current = _current;
                }

                return _current;
            }
        }

        public new FscSettings Settings
        {
            get
            {
                return base.Settings as FscSettings;
            }
            set
            {
                base.Settings = value;
            }
        }

        public static void LoadSettings(IConfiguration config)
        {
            Current.Settings = new FscSettings();
            config.GetSection("FscSettings").Bind(Current.Settings);

            //Decryption of encrypted text
            #region DecryptConnection
            var strigsDict = config.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
            
            foreach (var item in strigsDict)
            {
                CryptoAlgorithm settingsDecryptor = new CryptoAlgorithm("ENCRYPT", "FSCS", new AesManaged());
                var value = settingsDecryptor.Decrypt(item.Value);
                var key = settingsDecryptor.Decrypt(item.Key);
                FscApplication.Current.Settings.FscConnectionString = value;
            }
            #endregion
            #region Urls
            FSCUrl Urls = new FSCUrl();
            var urls = config.GetSection("FscSettings").GetSection("Urls").Get<Dictionary<string, string>>();
            foreach(var url in urls)
            {
                var value = url.Value;
                var key = url.Key;
                Urls[key] = value;                
            }
            FscApplication.Current.Settings.FscUrls = Urls;
            #endregion
            // Todo, add logging and event log support
            //Current.HttpServiceCalled += dao.InsertEventLog;
            //Current.AddLogEntry += PcatUtility.AddLogEntry;
        }
    }
}
