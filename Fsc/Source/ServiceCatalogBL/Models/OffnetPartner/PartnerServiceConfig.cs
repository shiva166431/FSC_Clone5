using PCAT.Common.Caching;
using ServiceCatalog.BL.Web.Offnet;
using ServiceCatalog.BL.Offnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL.Models.Offnet
{
    public class PartnerServiceConfig
    {
        private static readonly Cache<PartnerServiceConfig> _Cache = new Cache<PartnerServiceConfig>("PartnerServiceConfig Cache", CacheSizeType.Default, CacheDurationType.Default, CacheStorageType.LocalAndCouchbase);

        public int PartnerId { get; set; }
        public int Version { get; set; }
        public string ServiceRule {get;set;}
        
        public PartnerServiceConfig Get(PartnerKeyWeb Key)
        {
            PartnerServiceConfig config;
            config = _Cache.CheckCache(Key.PartnerOrderId.ToString());
            if(config == null)
            {
                try
                {
                    config = OffnetDataAccess.GetPartnerServiceConfig(Key.PartnerOrderId);
                }
                catch(Exception e)
                {
                    throw new ApplicationException("Error Fetching the PartnerServiceConfig :"+ e.Message);

                }
            }
            _Cache.StoreCache(Key.PartnerOrderId.ToString(), config);
            return config;
        }
    }
}
