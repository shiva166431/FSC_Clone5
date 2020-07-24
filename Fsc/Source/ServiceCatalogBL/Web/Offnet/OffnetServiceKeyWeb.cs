using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ServiceCatalog.BL.Web.Offnet
{
    [Serializable]
    [DataContract]
    [XmlType(TypeName = "OffnetServKey")]
    public class OffnetServiceKeyWeb
    {
        [DataMember] public long ServiceID { get; set; }
        [DataMember] public DateTime? Date { get; set; }
        [DataMember] public Dictionary<string,string> AttributesList { get; set; }
        [DataMember] public OffnetAttrInstanceWeb[] Attributes { get; set; }
        [DataMember] public OffnetServiceCollectionWeb[] ChildServices { get; set; }
        [DataMember] public OffnetServiceKeyWeb ParentServiceKey { get; set; }
        [DataMember] public bool IsValid { get; set; }
        [DataMember] public string ErrorString { get; set; }

        public OffnetServiceKeyWeb()
        {
            IsValid = true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ServKey: ");
            sb.Append("Service ID = "); sb.Append(ServiceID);
            sb.Append(",Date = "); sb.Append(Date);
            if (Attributes != null)
            {
                sb.Append(",Attributes = [");
                foreach (var ai in Attributes)
                {
                    sb.Append(ai);
                    sb.Append(",");
                }
                if (sb[sb.Length - 1] == ',') sb.Replace(",", "]", sb.Length - 1, 1);
                else sb.Append("]");

            }
            sb.Append(",IsValid = "); sb.Append(IsValid);
            sb.Append(",ErrorString = "); sb.Append(ErrorString);

            return sb.ToString();
        }

        public static OffnetServiceKeyWeb FromString(string servKeyString)
        {
            OffnetServiceKeyWeb key = null;
            int index1, index2;

            if (servKeyString.StartsWith("ServKey:"))
            {
                key = new OffnetServiceKeyWeb();
                index1 = servKeyString.IndexOf("Service ID =");
                key.ServiceID = long.Parse(servKeyString.Substring(index1 + 13,index1 - 13));

                index2 = servKeyString.IndexOf(",Date =", index1);
                index1 = servKeyString.IndexOf(',', index2 + 2);
                key.Date = DateTime.Parse(servKeyString.Substring(index2 + 8, index1 - index2 - 8));

                index1 = servKeyString.IndexOf(",Attributes = [", index1);
                if (index1 > 0)
                {
                    index2 = servKeyString.IndexOf(']', index1);
                    string[] instances = servKeyString.Substring(index1 + 15, index2 - index1 - 16).Split(',');
                    List<OffnetAttrInstanceWeb> instanceList = new List<OffnetAttrInstanceWeb>();
                    foreach (string instance in instances)
                        instanceList.Add(OffnetAttrInstanceWeb.FromString(instance));
                    key.Attributes = instanceList.ToArray();
                }

                index1 = servKeyString.IndexOf(",ErrorString = ", index2);
                key.IsValid = bool.Parse(servKeyString.Substring(index2 + 11, index1 - index2 - 11));

                key.ErrorString = servKeyString.Substring(index1 + 15);
            }

            return key;
        }
    }
}
