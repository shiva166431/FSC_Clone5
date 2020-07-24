using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PCAT.Common.Models;
using PCAT.Common;
using PCAT.Common.Utilities;
using PCAT.Common.Rules;
using PCAT.Common.Parsers;
using PCAT.Common.Biz;
using PCAT.Common.Models.Service;
using ServiceCatalog.BL.Web.Offnet;
using ServiceCatalog.BL.Models.Offnet;

namespace ServiceCatalog.BL.Offnet
{
    [Serializable]
    [DataContract]
    public class PartnerServicePackage
    {
        public PartnerServicePackage() { }
      
        [DataMember] public PartnerKeyWeb Partner;
        [DataMember] public Dictionary<string, OffnetServiceKey> Services = new Dictionary<string, OffnetServiceKey>();
    
        public void AddService(string name, OffnetServiceKey key)
        {
            if (Services == null) Services = new Dictionary<string, OffnetServiceKey>();
            if (name == null) return;
            if (key == null && Services.ContainsKey(name)) Services.Remove(name);
            if (Services.ContainsKey(name)) throw new ApplicationException(name + " already exists.  Service names need to be unique in the PSP.");
            if (key != null) Services.Add(name, key);
        }
        public OffnetServiceKey GetService(string name)
        {
            OffnetServiceKey key;
            if (Services == null) return null;
            Services.TryGetValue(name, out key);
            return key;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Partner / Service Package\n");
            sb.Append("Partner = ");
            sb.Append(Partner);
            sb.Append("\n");
            if (Services != null && Services.Count > 0)
            {
                sb.Append("Services\n");
                foreach (var pair in Services)
                {
                    sb.Append("\t");
                    sb.Append(pair.Key);
                    sb.Append(" = ");
                    sb.Append(pair.Value);
                    sb.Append("\n");
                }
            }

            return sb.ToString();
        }

        public double? GetNumericValue(string name, SearchOptions searchOptions)
        {
            throw new ApplicationException(name + " is an unknown attribute of a PartnerServicePackage.");
        }

        public PartnerKeyWeb GetPartnerKey()
        {
            return Partner;
        }

    }
}
