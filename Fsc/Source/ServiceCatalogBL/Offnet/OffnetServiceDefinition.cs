using PCAT.Common.Caching;
using ServiceCatalog.BL.Web.Offnet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL.Offnet
{
    public class OffnetServiceDefinition
    {
        private static readonly Cache<OffnetServiceDefinition> _Cache = new Cache<OffnetServiceDefinition>("Offnet Service Defintion Cache", CacheSizeType.Default, CacheDurationType.Default, CacheStorageType.LocalAndCouchbase);

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool HasChildren { get; set; }


        public static OffnetServiceDefinition Get(long id)
        {
            OffnetServiceDefinition def;
            def = _Cache.CheckCache(id.ToString());
            if(def == null){
                try
                {
                    def = OffnetDataAccess.GetOffnetServiceDefinition(id.ToString());
                }
                catch(Exception e)
                {
                    string message;
                    switch(e.Message)
                    {
                        case "Sequence contains no elements":
                            message = "Definition does not exist for " + id.ToString() + ",try again with a valid Id.";
                            break;
                        default:
                            message = "Error fetching the service deinition.";
                            break;
                    }
                    throw new ApplicationException(message);
                }
            }
            _Cache.StoreCache(id.ToString(), def);
            return def;
        }

        public OffnetServiceDefinitionWeb ToWeb()
        {
            return new OffnetServiceDefinitionWeb()
            {
                ServiceId = Id,
                ServiceName = Name,
                Category = Category,
                Description = Description,
                HasChildren = HasChildren
            };
        }
    }

}
