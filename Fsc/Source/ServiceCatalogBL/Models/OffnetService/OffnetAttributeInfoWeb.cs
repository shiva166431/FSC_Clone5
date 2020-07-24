using PCAT.Common.Web.Models.Attribute;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL.Models.OffnetService
{
    [DataContract]
    [Serializable]
    public class OffnetAttributeInfoWeb
    {
        [DataMember]
        public Dictionary<string,string> Attribute { get; set; }
        [DataMember]
        public AttributeInfoWeb AttributeProperties { get; set; }
        [DataMember]
        public string ErrorString { get; set; }
        public OffnetAttributeInfoWeb()
        {         
        }
        public OffnetAttributeInfoWeb(string name, string value, AttributeInfoWeb attributeInfo)
        {
            Attribute = new Dictionary<string, string> { { name, value } };
            AttributeProperties = attributeInfo;
        }   
    }
}
