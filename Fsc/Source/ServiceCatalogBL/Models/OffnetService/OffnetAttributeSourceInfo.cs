using ServiceCatalog.BL.Offnet;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL.Models.OffnetService
{
    [DataContract]
    [Serializable]
    public class OffnetAttributeSourceInfo
    {
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public long OffnetServiceId { get; set; }
        [DataMember]
        public string PartnerOrderId { get; set; }
        [DataMember]
        public string RequestSource { get; set; }
       
        [DataMember]
        public string SolutionTarget { get; set; }
        [DataMember]
        public Dictionary<string,string> Attributes { get; set; }
        public OffnetAttributeSourceInfo()
        {

        }
        public OffnetAttributeSourceInfo(OffnetAttributeSource source, List<string> attributesList)
        {

        }
    }
}
