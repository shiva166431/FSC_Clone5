using Dapper;
using Oracle.ManagedDataAccess.Client;
using PCAT.Common.Caching;
using PCAT.Common.Web.Models.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL
{
    public class ServiceDefinition
    {
        private static readonly Cache<ServiceDefinition> _Cache = new Cache<ServiceDefinition>("Service Defintion Cache", CacheSizeType.Default, CacheDurationType.Default, CacheStorageType.LocalAndCouchbase);


        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool HasRelation  { get; set; }
        public bool HasChildren { get; set; }


        public static ServiceDefinition Get(long id)
        {
            ServiceDefinition def;

            def = _Cache.CheckCache(id.ToString());
            if (def == null)
            {
                using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
                {
                    connection.Open();

                    def = connection.QueryFirst<ServiceDefinition>($"select Service_Id Id, Name, Category, Description," +
                        $" (select case when exists (select 1  from SERVICE_RELATIONSHIP  where SERVICE_ID = :id) then 'true' else 'false'  end from dual) as HasRelation," +
                        $" (select case when exists (select 1  from SERVICE_HIERARCHY where SERVICE_ID = :id) then 'true' else 'false' end from dual) as HasChildren" +
                        " from Service_Definition where service_id = :id", new { id });
                }

                _Cache.StoreCache(id.ToString(), def);
            }
            return def;
        }

        public ServiceDefinitionWeb ToWeb()
        {
            return new ServiceDefinitionWeb()
            {
                ServiceId = Id,
                ServiceName = Name,
                Category = Category,
                Description = Description,
                HasRelation = HasRelation,
                HasChildren = HasChildren
            };
        }
    }
}
