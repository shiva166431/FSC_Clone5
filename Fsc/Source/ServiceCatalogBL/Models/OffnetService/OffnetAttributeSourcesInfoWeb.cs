using ServiceCatalog.BL.Offnet;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL.Models.OffnetService
{
    [DataContract]
    [Serializable]
    public class OffnetAttributeSourcesInfoWeb
    {
        [DataMember]
        public OffnetAttributeSourceInfo[] Sources { get; set; }
        [DataMember]
        public OffnetAttributeInfoWeb[] Attributes { get; set; }
        [DataMember]
        public string ErrorString { get; set; }
        public OffnetAttributeSourcesInfoWeb()
        {
        }
        public OffnetAttributeSourcesInfoWeb(OffnetAttributeSourceInfo[] sources, OffnetAttributeInfoWeb[] attributeInfo)
        {
            Sources = sources;
            Attributes = attributeInfo;
        }
    }
}
