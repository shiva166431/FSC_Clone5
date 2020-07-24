using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace ServiceCatalog.BL.Web.Offnet
{
    [DataContract]
    [Serializable]
    public class OffnetServiceCollectionWeb
    {

        [DataMember] public string Name { get; set; }
        [DataMember] public OffnetServiceKeyWeb[] Services { get; set; }

        public OffnetServiceCollectionWeb()
        { }
        public OffnetServiceCollectionWeb(string name, OffnetServiceKeyWeb[] service)
        {
            Name = name;
            Services = service;
        }
    }
}
