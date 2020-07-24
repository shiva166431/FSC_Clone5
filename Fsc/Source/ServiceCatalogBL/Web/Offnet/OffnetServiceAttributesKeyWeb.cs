using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL.Web.Offnet
{
    [DataContract]
    public class OffnetServiceAttributesKeyWeb : OffnetServiceKeyWeb
    {
        [DataMember]
        public bool PopulateLists { get; set; }
    }
}
