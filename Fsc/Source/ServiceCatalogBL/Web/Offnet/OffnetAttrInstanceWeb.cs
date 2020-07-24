using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using PCAT.Common.Web.Models.Attribute;
using PCAT.Common.Models.Attribute;
using System.Dynamic;
using Newtonsoft.Json;
using System.Web;
namespace ServiceCatalog.BL.Web.Offnet
{
    [DataContract]
    [Serializable]
    public class OffnetAttrInstanceWeb
    {    
        [DataMember] public string Name { get; set; }
        [DataMember] public string Type { get; set; }
        [DataMember] public AttributeChoiceWeb Value { get; set; }
      
        #region Constructors
        public OffnetAttrInstanceWeb() { }
        public OffnetAttrInstanceWeb(string name, string value, string display, AttributeType type)
        {         
            Name = name;
            Type = (type == null) ? "Unknown" : type.ToString();
            Value = new AttributeChoiceWeb(value, display);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("AttributeInstance: ");
            sb.Append(Name);
            if (Type != null && !Type.Equals(AttributeType.Unknown.ToString()))
            {
                sb.Append("@#(");
                sb.Append(Type);
                sb.Append(")");
            }
            sb.Append(" = ");
            sb.Append(Value != null ? Value.ToString() : "null");
            return sb.ToString();
        }

        public static OffnetAttrInstanceWeb FromString(string attributeInstanceString)
        {
            OffnetAttrInstanceWeb attributeInstance = null;

            if (attributeInstanceString.StartsWith("AttributeInstance: "))
            {
                attributeInstance = new OffnetAttrInstanceWeb();

                // remove (address) from glm id
                int open = attributeInstanceString.IndexOf("@#(");
                int equal = attributeInstanceString.IndexOf('=');
                if (open > 0 && open < equal)
                {
                    attributeInstance.Name = attributeInstanceString.Substring(19, open - 19);
                    attributeInstance.Type = attributeInstanceString.Substring(open + 3, attributeInstanceString.IndexOf(')', open) - open - 3);
                }
                else
                {
                    attributeInstance.Name = attributeInstanceString.Substring(19, equal - 20);
                }

                string value = "null";

                if (attributeInstanceString.Length > equal + 2)
                    value = attributeInstanceString.Substring(equal + 2);
                if (value.Equals("null"))
                    attributeInstance.Value = null;
                else
                    attributeInstance.Value = AttributeChoiceWeb.FromString(value);
            }

            return attributeInstance;
        }
        #endregion
    }
}
