using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace ServiceCatalog.BL.Web.Offnet
{
        [DataContract]
        [Serializable]
        [XmlType(TypeName = "OffnetServiceDef")]
        public class OffnetServiceDefinitionWeb
        {
            [DataMember]
            public long ServiceId { get; set; }
            [DataMember]
            public string ServiceName { get; set; }
            [DataMember]
            public string Category { get; set; }
            [DataMember]
            public string Description { get; set; }
            
            [DataMember]
            public bool HasChildren { get; set; }


            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("ServiceDef: Id=");
                sb.Append(ServiceId);
                sb.Append(", Name=");
                sb.Append(ServiceName);
                sb.Append(", Description=");
                sb.Append(Description);
                sb.Append(", HasChildren=");
                sb.Append(HasChildren);
                return sb.ToString();
            }
        }
    }
