using PCAT.Common.Web.Models.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceCatalog.WepApi.Models
{
    [DataContract]
    public class ServiceAttributesKeyWeb : ServiceKeyWeb
    {
        [DataMember]
        public bool PopulateLists { get; set; }
    }
}
