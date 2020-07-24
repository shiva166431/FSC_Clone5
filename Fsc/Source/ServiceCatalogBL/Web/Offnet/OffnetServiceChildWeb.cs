using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL.Web.Offnet
{
    [DataContract]
    [Serializable]
    public class OffnetServiceChildWeb
    {
        [DataMember] public long Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public int MaxQuantity { get; set; }
        [DataMember] public int MinQuantity { get; set; }
    }
}
