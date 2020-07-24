using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using ServiceCatalog.BL.Web.Offnet;
using ServiceCatalog.BL.Models.Offnet;

namespace ServiceCatalog.BL.Offnet
{
    [DataContract]
    [Serializable]
    #region PartnerServPkg
    public class PartnerServPkg
    {
        #region Fields
        [DataMember] public PartnerKeyWeb Partner { get; set; }
        #endregion
        #region EffectiveDate
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
        [DataMember] public OffnetServKeyPair[] Services { get; set; }
        [DataMember] public bool IsValid { get; set; }
        [DataMember] public string ErrorString { get; set; }
        public PartnerServPkg ()
        {
            IsValid = true;
        }

        
        public PartnerServPkg(PartnerServicePackage pkg)
        {
            Partner = pkg.Partner;
            IsValid = true;
            if (pkg.Services != null)
            {
                Services = new OffnetServKeyPair[pkg.Services.Count];
                var i = 0;
                foreach (var pair in pkg.Services)
                {
                    Services[i++] = new OffnetServKeyPair(pair.Key, pair.Value.ToWeb());
                }
            }
        }

        public PartnerServicePackage GetPartnerServicePackage()
        {
            var pkg = new PartnerServicePackage();
            pkg.Partner = Partner;
            if (Services != null)
            {
                foreach (var pair in Services)
                {
                    if (Services != null)
                    {
                        var key = new OffnetServiceKey(pair.Service);
                        pkg.AddService(pair.Name, key);
                    }
                }
            }
            return pkg;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("PartnerServPkg: ");
            sb.Append("Partner = [");
            sb.Append(Partner);
            sb.Append("]");

            sb.Append(",Services = [");
            if (Services != null)
            {
                foreach (var key in Services)
                {
                    sb.Append("{");
                    sb.Append(key);
                    sb.Append("},");
                }
                if (sb[sb.Length - 1] == ',') sb.Replace(",", "]", sb.Length - 1, 1);
                else sb.Append("]");

            }
            else sb.Append("]");

            sb.Append(" ,IsValid = "); sb.Append(IsValid);
            sb.Append(" ,ErrorString = "); sb.Append(ErrorString);

            return sb.ToString();
        }

        public static PartnerServPkg FromString(string prodServPkgString)
        {
            PartnerServPkg partServPkg = null;
            int index, index2;

            if (prodServPkgString.IndexOf("ParentProduct = [") > 0)
                return FromStringNew(prodServPkgString);

            if (prodServPkgString.StartsWith("PartnerServPkg:"))
            {
                partServPkg = new PartnerServPkg();
                index = prodServPkgString.IndexOf("PartnerServPkg:");
                partServPkg.Partner = PartnerKeyWeb.FromString(prodServPkgString.Substring(29, index - 29));

                prodServPkgString = prodServPkgString.Substring(index + 18);                
                // todo: write code for Services here

                index = prodServPkgString.IndexOf(",IsValid =");
                index2 = prodServPkgString.IndexOf(",ErrorString =");
                partServPkg.IsValid = bool.Parse(prodServPkgString.Substring(index + 11, index2 - index - 11));
                partServPkg.ErrorString = prodServPkgString.Substring(index2 + 15);
            }

            return partServPkg;
        }

        public static PartnerServPkg FromStringNew(string prodServPkgString)
        {
            PartnerServPkg partServPkg = null;
            int index1, index2;

            if (prodServPkgString.StartsWith("PartnerServPkg:"))
            {
                partServPkg = new PartnerServPkg();
                
                    PartnerKeyWeb key = null;
                    index1 = prodServPkgString.IndexOf("VendorId");
                    key.VendorId =  Convert.ToInt32(prodServPkgString.Substring(index1 + 13, index1 - 13));

                    index2 = prodServPkgString.IndexOf("VendorName =", index1);
                    index1 = prodServPkgString.IndexOf(",", index2 + 2);
                    key.VendorName = prodServPkgString.Substring(index2 + 8, index1 - index2 - 8);

                    index2 = prodServPkgString.IndexOf("RequestType =", index1);
                    index1 = prodServPkgString.IndexOf(",", index2 + 2);
                    key.VendorName = prodServPkgString.Substring(index2 + 8, index1 - index2 - 8);

                    index2 = prodServPkgString.IndexOf("PartnerOrderFormat =", index1);
                    index1 = prodServPkgString.IndexOf(",", index2 + 2);
                    key.VendorName = prodServPkgString.Substring(index2 + 8, index1 - index2 - 8);

                    index2 = prodServPkgString.IndexOf("OrderAction =", index1);
                    index1 = prodServPkgString.IndexOf(",", index2 + 2);
                    key.VendorName = prodServPkgString.Substring(index2 + 8, index1 - index2 - 8);

                    index2 = prodServPkgString.IndexOf("ProductFamily =", index1);
                    index1 = prodServPkgString.IndexOf(",", index2 + 2);
                    key.VendorName = prodServPkgString.Substring(index2 + 8, index1 - index2 - 8);

                    index2 = prodServPkgString.IndexOf("PartnerOrderId =", index1);
                    index1 = prodServPkgString.IndexOf(",", index2 + 2);
                    key.VendorName = prodServPkgString.Substring(index2 + 8, index1 - index2 - 8);

                    index2 = prodServPkgString.IndexOf("PartnerOrderName =", index1);
                    index1 = prodServPkgString.IndexOf(",", index2 + 2);
                    key.VendorName = prodServPkgString.Substring(index2 + 8, index1 - index2 - 8);

                    partServPkg.Partner = key;
        //Need to add the logic for extracting services from string 
                OffnetServKeyPair[] ServkeyPair = null;
                OffnetServiceKeyWeb ServKey = null;
                int servIndex;
                servIndex = prodServPkgString.IndexOf("ServKey:");
                string servKeyString = prodServPkgString.Substring(servIndex, servIndex + prodServPkgString.IndexOf("]"));

                    index1 = prodServPkgString.IndexOf("Service ID =");
                ServKey.ServiceID = long.Parse(prodServPkgString.Substring(index1 + 13, index1 - 13));

                    index2 = prodServPkgString.IndexOf(",Date =", index1);
                    index1 = prodServPkgString.IndexOf(',', index2 + 2);
                ServKey.Date = DateTime.Parse(prodServPkgString.Substring(index2 + 8, index1 - index2 - 8));

                    index1 = prodServPkgString.IndexOf(",Attributes = [", index1);
                    if (index1 > 0)
                    {
                        index2 = prodServPkgString.IndexOf(']', index1);
                        string[] instances = prodServPkgString.Substring(index1 + 15, index2 - index1 - 16).Split(',');
                        List<OffnetAttrInstanceWeb> instanceList = new List<OffnetAttrInstanceWeb>();
                        foreach (string instance in instances)
                            instanceList.Add(OffnetAttrInstanceWeb.FromString(instance));
                    ServKey.Attributes = instanceList.ToArray();
                    }

                    index1 = prodServPkgString.IndexOf(",ErrorString = ", index2);
                ServKey.IsValid = bool.Parse(prodServPkgString.Substring(index2 + 11, index1 - index2 - 11));

                ServKey.ErrorString = prodServPkgString.Substring(index1 + 15);

                partServPkg.Services = ServkeyPair;
            }

            return partServPkg;
        }

        public PartnerServicePackageWeb ToWeb()
        {
            var web = new PartnerServicePackageWeb();
            return web;
        }
    }
    #endregion
    #region offnetServKeyPair
    [DataContract]
    [Serializable]
    public class OffnetServKeyPair
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public OffnetServiceKeyWeb Service { get; set; }
        public OffnetServKeyPair() { }
        public OffnetServKeyPair(string name, OffnetServiceKeyWeb key)
        {
            Name = name;
            Service = key;
        }
        public override string ToString()
        {
            return Name + "=" + Service;
        }
        public static OffnetServKeyPair FromString(string text)
        {
            int index = text.IndexOf('=');
            var sk = new OffnetServKeyPair()
            {
                Name = text.Substring(0, index - 1),
                Service = OffnetServiceKeyWeb.FromString(text.Substring(index + 1))
            };
            return sk;
        }
    }
    #endregion
}
