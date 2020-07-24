using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using PCAT.Common.Biz;
using PCAT.Common.Models;
using PCAT.Common.Rules;
using System.Linq;
using PCAT.Common;

namespace ServiceCatalog.BL.Web.Offnet
{
    public class PartnerKeyWeb : IValueHolder
    {
        public PartnerKeyWeb() { }
        [DataMember] public int VendorId { get; set; }
        [DataMember] public string VendorName { get; set; }
        [DataMember] public string RequestType { get; set; }
        [DataMember] public string PartnerOrderFormat { get; set; }
        [DataMember] public string OrderAction { get; set; }
        [DataMember] public string ProductFamily { get; set; }
        [DataMember] public int PartnerOrderId { get; set; }
        [DataMember] public string PartnerOrderName { get; set; }
        [DataMember] public string Source { get; set; }
        [DataMember] public string PreOrderValidation { get; set; }
        #region Effdate
        private DateTime _effDate;
        [DataMember]
        public DateTime EffectiveDate
        {
            get
            {
                return _effDate == DateTime.MinValue ? DateTime.Now : _effDate;
            }
            set { _effDate = value; }
        }
        #endregion
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("PartnerKey: ");
            sb.Append("VendorId = "); sb.Append(VendorId);
            sb.Append(",VendorName = "); sb.Append(VendorName);
            sb.Append(",RequestType = "); sb.Append(RequestType);
            sb.Append(",PartnerOrderFormat = "); sb.Append(PartnerOrderFormat);
            sb.Append(",OrderAction = "); sb.Append(OrderAction);
            sb.Append(",ProductFamily = "); sb.Append(ProductFamily);
            sb.Append(",PartnerOrderId = "); sb.Append(PartnerOrderId);
            sb.Append(",PartnerOrderName = "); sb.Append(PartnerOrderName);
            sb.Append(",Source = "); sb.Append(Source);
            return sb.ToString();
        }

        public static PartnerKeyWeb FromString(string partnerKeyString)
        {
            PartnerKeyWeb key = null;
            int index1, index2;

            if (partnerKeyString.StartsWith("PartnerKey:"))
            {
                index1 = partnerKeyString.IndexOf("VendorId");
                key.VendorId = Convert.ToInt32(partnerKeyString.Substring(index1 + 13, index1 - 13));

                index2 = partnerKeyString.IndexOf("VendorName =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.VendorName = partnerKeyString.Substring(index2 + 8, index1 - index2 - 8);

                index2 = partnerKeyString.IndexOf("RequestType =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.RequestType = partnerKeyString.Substring(index2 + 8, index1 - index2 - 8);

                index2 = partnerKeyString.IndexOf("PartnerOrderFormat =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.PartnerOrderFormat = partnerKeyString.Substring(index2 + 8, index1 - index2 - 8);

                index2 = partnerKeyString.IndexOf("OrderAction =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.OrderAction = partnerKeyString.Substring(index2 + 8, index1 - index2 - 8);

                index2 = partnerKeyString.IndexOf("ProductFamily =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.ProductFamily = partnerKeyString.Substring(index2 + 8, index1 - index2 - 8);

                index2 = partnerKeyString.IndexOf("PartnerOrderId =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.PartnerOrderId = Convert.ToInt32(partnerKeyString.Substring(index2 + 8, index1 - index2 - 8));

                index2 = partnerKeyString.IndexOf("PartnerOrderName =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.PartnerOrderName = partnerKeyString.Substring(index2 + 8, index1 - index2 - 8);

                index2 = partnerKeyString.IndexOf("Source =", index1);
                index1 = partnerKeyString.IndexOf(",", index2 + 2);
                key.Source = partnerKeyString.Substring(index2 + 8, index1 - index2 - 8);
            }
            return key;
        }

        #region input Valid
        public ValidMessage IsValidInput()
        {
            var v = new ValidMessage();
            if ((this.VendorId == 0)
                || (String.IsNullOrEmpty(this.VendorName)) 
                || (String.IsNullOrEmpty(this.Source)))
                {
                v.message = "VendorId & VendorName,Source are Required Partner attributes";
                v.valid = false;
            }
            else
            {
                v.message = "";
                v.valid = true;
            }
            return v;
        }

        public ValidMessage HasAllAttributes()
        {
            var v = new ValidMessage();
            if ((String.IsNullOrEmpty(this.PartnerOrderFormat))
                        || (String.IsNullOrEmpty(this.OrderAction))
                        || (String.IsNullOrEmpty(this.ProductFamily))
                        || (String.IsNullOrEmpty(this.PartnerOrderName))
                        || (String.IsNullOrEmpty(this.PreOrderValidation)))
            {
                v.message = "Partner key does not have all the attribues";
                v.valid = false;
            }
            else
            {
                v.message = "";
                v.valid = true;
            }
            return v;
        }

        public  class ValidMessage
        {
            public  bool valid { get; set; }
            public string message { get; set; }
        }
        #endregion
        #region IValueHolder Inheritance
        public void AddAttribute(string name, string value)
        {
            throw new NotImplementedException();
        }

        public string GetAttributeValue(string name, SearchOptions searchOptions)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string name, SearchOptions searchOptions, out RuleValue value, out string error)
        {
            throw new NotImplementedException();
        }

        public ICollection<string> GetAttributesFor(string att)
        {
            throw new NotImplementedException();
        }

        public double? GetNumericValue(string name, SearchOptions searchOptions)
        {
            throw new NotImplementedException();
        }

        public void AddAddress(string name, GlmAddress address)
        {
            throw new NotImplementedException();
        }

        public GlmAddress GetAddress(string addressName)
        {
            throw new NotImplementedException();
        }

        public List<IValueHolder> GetValueHolderList(string collectionName)
        {
            throw new NotImplementedException();
        }

        public IKey GetKey(string entityName)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, GlmAddress> GetAddresses()
        {
            throw new NotImplementedException();
        }

        public DateTime GetEffectiveDate()
        {
            throw new NotImplementedException();
        }

        public string GetIdentifier(string[] names)
        {
            string name = null;

            name = names != null ? names.FirstOrDefault().ToString() : null;
            return name;
        }

        public void RemoveAttribute(string name)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
