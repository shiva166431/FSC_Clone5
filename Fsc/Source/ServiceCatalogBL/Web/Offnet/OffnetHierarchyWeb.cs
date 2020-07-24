using ServiceCatalog.BL.Models.Offnet;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL.Web.Offnet
{
    public class OffnetHierarchyWeb
    {
        [DataMember] public OffnetServiceKey[] Services { get; set; }

        public OffnetHierarchyWeb()
        { }
        public OffnetHierarchyWeb(OffnetServiceKey[] service)
        {
            Services = service;
        }
    }
}
