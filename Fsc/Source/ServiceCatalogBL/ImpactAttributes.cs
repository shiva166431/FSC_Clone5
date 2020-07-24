using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL
{
    [DataContract]
    public class ImpactAttributes
    {
        public ImpactAttributes(long serviceId,long serviceInstanceId, List<string> impacts)
        {
            ServiceId = serviceId;
            ServiceInstanceId = serviceInstanceId;
            Impacts = impacts;
        }
        [DataMember]
        public long ServiceId { get; set; }
        [DataMember]
        public long ServiceInstanceId { get; set; }
        [DataMember]
        public List<string> Impacts{ get; set; }   
    }
}

