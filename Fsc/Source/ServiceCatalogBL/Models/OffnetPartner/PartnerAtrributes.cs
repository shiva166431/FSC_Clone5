using System;
using System.Collections.Generic;
using System.Text;
using PCAT.Common.Caching;
using ServiceCatalog.BL.Web.Offnet;
using System.Runtime.Serialization;
using ServiceCatalog.BL.Offnet;
using System.Reflection;

namespace ServiceCatalog.BL.Models.Offnet
{
    [DataContract]
    [Serializable]
    public class PartnerAtrributes
    {
        private static readonly Cache<PartnerKeyWeb> _Cache = new Cache<PartnerKeyWeb>("PartnerAtrributes Cache", CacheSizeType.Default, CacheDurationType.Default, CacheStorageType.LocalAndCouchbase);

        //Add any logic related to partner key if needed 
        public int VendorId { get; set; }
        public string Name { get; set; }
        public string Request { get; set; }
        public string PartnerFormat { get; set; }
        public string Action { get; set; }
        public string Family { get; set; }
        public int PartnerId { get; set; }
        public string Partner { get; set; }
        public string Source { get; set; }
        public string PreOrderValidation { get; set; }
        #region Get
        public PartnerKeyWeb Get(int vendorId, string vendorName, string source)
        {
            PartnerKeyWeb key;            
            key = _Cache.CheckCache(vendorId + vendorName + source);
            if (key == null)
            {
                try
                {
                    var queryRes = OffnetDataAccess.GetPartnerAttribuesByVendor(vendorId, vendorName, source);
                    key = queryRes.ToWeb();
                }
                catch(Exception e)
                {
                    e.Data.Add("Custom Msg","Error while fetching Partner attributes;");
                    throw (e);
                }

            }
            _Cache.StoreCache(vendorId + vendorName + source,key);
            return key;
        }
        #endregion
        public string GetAttributeValue(string attrName)
        {
            return this[attrName].ToString();
        }
        public void FromWeb(PartnerKeyWeb Key)
        {
            VendorId = Key.VendorId;
            Name = Key.VendorName;
            Request = Key.RequestType ;
            PartnerFormat = Key.PartnerOrderFormat;
            Action = Key.OrderAction;
            Family = Key.ProductFamily;
            PartnerId = Key.PartnerOrderId;
            Partner = Key.PartnerOrderName;
            Source = Key.Source;
            PreOrderValidation = Key.PreOrderValidation;
        }

        public PartnerKeyWeb ToWeb()
        {
            return new PartnerKeyWeb
            {
                VendorId = this.VendorId,
                VendorName = this.Name,
                RequestType = this.Request,
                PartnerOrderFormat = this.PartnerFormat,
                OrderAction = this.Action,
                ProductFamily = this.Family,
                PartnerOrderId = this.PartnerId,
                PartnerOrderName = this.Partner,
                Source = this.Source,
                PreOrderValidation = this.PreOrderValidation

            };
        }
        public object this[string propertyName]
        {
            //Get class properties by Name
            get
            {
                Type myType = typeof(PartnerAtrributes);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(PartnerAtrributes);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }

        }

    }
}
