using PCAT.Common.Biz;
using PCAT.Common.Caching;
using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;
using PCAT.Common.Utilities;
using PCAT.Common.Web.Models.Attribute;
using ServiceCatalog.BL.Models.Offnet;
using ServiceCatalog.BL.Models.OffnetService;
using ServiceCatalog.BL.Offnet;
using ServiceCatalog.BL.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL
{
    [DataContract]
    [Serializable]
    public class OffnetAttributes
    {
        [DataMember]
        public Dictionary<string, AttributeDefinition> ListAttributes = new Dictionary<string, AttributeDefinition>();
    }
    public static class OffnetServiceAttributes
    {
        private static readonly Cache<OffnetAttributes> _Cache = new Cache<OffnetAttributes>("Offnet Attributes", CacheSizeType.Default, CacheDurationType.Price, CacheStorageType.LocalAndCouchbase);


        #region Get Attributes
        public static ICollection<AttributeInfo> Get(OffnetServiceKey key, bool populateLists)
        {
            var config = OffnetServiceConfiguration.Get(key.Id, key.EffectiveDate);

            var list = ValidValueRuleParser.GetRules(config.ValidValueRule);

            var set = new ValidValueRuleSet();
            set.AddRules(list);

            var attributeSet = set.GetAttributes(key, SearchOptions.ALL_FALSE);
            //If we have an attribute with  no valid options, clear the value and try again...
            var emptyAttributes = (from a in attributeSet where a.Values == null || a.Values.Count == 0 select a.Name).ToList();
            if (emptyAttributes.Count > 0)
            {
                foreach (var a in emptyAttributes)
                    key.RemoveAttribute(a);
                attributeSet = set.GetAttributes(key, SearchOptions.ALL_FALSE);
            }

            IDictionary<string, AttributeInfo> tempList = new Dictionary<string, AttributeInfo>();
            foreach (var a in attributeSet)
                tempList[a.Name] = a;

            key.AddMissingAttributes();
            //Next we need to look if there are non-list items that need to be collected.
            foreach (var pair in config.Attributes)
            {
                AttributeInfo a = null;
                if (tempList.ContainsKey(pair.Key))
                    tempList.TryGetValue(pair.Key, out a);

                tempList[pair.Key] = AttributeFactory.CreateAttribute(pair.Value, pair.Key, a, key);
                if ((pair.Value.Type == AttributeType.Parent || pair.Value.Type == AttributeType.Related)
                    && key.HasAttribute(pair.Key))
                    tempList[pair.Key].SetValue(key.GetAttributeValue(pair.Key, SearchOptions.ALL_TRUE));
            }


            var ruleSet = new OffnetServiceRuleSet();
            ruleSet.AddDefaults(key);  // add defaults so rules such as IsApplicable can use them

            //key doesn't have all the data we have generated so to use the latest we will build a ValueHolder that has what we need...
            string aValue;
            foreach (var a in tempList.Values)
            {
                aValue = a.GetValue();
                if (!string.IsNullOrEmpty(aValue)) // don't add empty values
                    ruleSet.AddValue(a.Name, new RuleValue(aValue));
            }
            //Determine which attributes we don't need
            var finalList = tempList.Values.Where(a => config.IsConfigurableAttribute(a.Name, ruleSet)).ToDictionary(a => a.Name);


            //Last we need to try and add a description to attributes that haven't had them added yet...
            //and flag them as optional or not.  We will also set the default value if there is one.
            foreach (var a in finalList.Values)
            {
                a.Optional = config.IsOptional(a.Name, ruleSet).ToString();

                a.Label = config.GetLabel(a.Name);
                if (config.HasDefault(a.Name))
                {
                    try
                    {
                        a.DefaultValue = config.GetDefaultValue(a.Name, key);
                    }
                    catch (Exception) { }
                }
                a.Hidden = config.IsHidden(a.Name, key);
                a.MaxRepeats = config.GetMaxRepeats(a.Name);
                a.RequiresRefresh = config.GetRequiresRefresh(a.Name);
                a.ReadOnly = config.IsReadOnly(a.Name, key);
                a.ApplicableForChange = config.GetApplicableForChange(a.Name);
                a.AffectsChildren = config.AffectsChildren(a.Name);
                a.DesignImpact = config.IsDesignImpact(a.Name, key);
                a.ProvisioningImpact = config.IsProvisioningImpact(a.Name, key);

                var attribute = a as ListAttribute;
                if (attribute != null)
                {
                    var la = attribute;
                    if (populateLists && la.GetValue() != null && !la.ReadOnly && !la.Hidden)
                    {
                        //Since the value has been set, the list of options is empty.  If it is asked for, we will determine 
                        //the list of options if this was not set.
                        var myKey = key.Clone(false);

                        myKey.AddValue(la.Name, null);
                        var myAtts = set.GetAttributes(myKey, SearchOptions.ALL_FALSE);

                        foreach (var av in
                            from myAtt in myAtts
                            where myAtt.Name.Equals(la.Name)
                            from av in ((ListAttribute)myAtt).GetList()
                            select av)
                        {
                            la.AddValue(av);
                        }
                    }
                }
            }

            return config.SortList(finalList.Values);
        }
        public static OffnetAttributeSourcesInfoWeb GetAttributes(OffnetServiceKey key, bool populateLists)
        {
            AttributeInfoWeb list;
            OffnetAttributeInfoWeb[] info;
            OffnetAttributeSourceInfo source;
            OffnetAttributeSourceInfo[] sources;
            OffnetAttributeSourcesInfoWeb infoWeb;           
            if (key == null)
            {           
                infoWeb = new OffnetAttributeSourcesInfoWeb { ErrorString = "Key cannot be null" };
                return infoWeb;
            }
            try
            {
                var atts = Get(key, populateLists);
                var srcs = OffnetDataAccess.GetOffnetAttributeSource(key);
                string name = null;
                string value = null;
                string partnerOrderId = null;
                info = new OffnetAttributeInfoWeb[atts.Count];
                var i = 0;
                foreach (var att in atts)
                {
                    name = att.Name;
                    value = att.GetValue();
                    list = new AttributeInfoWeb(att);
                    info[i++] = new OffnetAttributeInfoWeb(name, value, list);
                    if (name == "PartnerOrderId")
                        partnerOrderId = value;                
                }

               //var attributes= srcs.Where(x => x.PartnerOrderId.ToString() == partnerOrderId && x.OffnetServiceId == key.Id).Select(x => x.AttributeName).ToList();
                List<string> filteredSrcs = new List<string>();
                filteredSrcs= srcs.Where(x => x.PartnerOrderId.ToString() == partnerOrderId && x.OffnetServiceId == key.Id).Select(x => x.AttributeSource).Distinct().ToList();
                sources = new OffnetAttributeSourceInfo[filteredSrcs.Count];
                var j = 0;
               
                foreach (var s in filteredSrcs)
                {
                    var attributes = new List<string>(srcs.Where(x => x.PartnerOrderId.ToString() == partnerOrderId && x.OffnetServiceId == key.Id && x.AttributeSource == s)
                        .Select(x => x.AttributeName).ToList());
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    foreach (var a in attributes)
                    {
                        var attrList = info.ToList();
                        var v = attrList.Where(x => x.AttributeProperties.Name.Equals(a)).Select(x => x.AttributeProperties.Value).FirstOrDefault();
                        keyValuePairs.Add(a,v);
                    }
                    source = new OffnetAttributeSourceInfo
                    {
                        Source = s,
                        OffnetServiceId = key.Id,
                        PartnerOrderId = partnerOrderId,
                        Attributes = keyValuePairs
                    };
                    sources[j++] = source;
                }             
                infoWeb = new OffnetAttributeSourcesInfoWeb(sources,info);
            }
            catch (Exception e)
            {               
                infoWeb = new OffnetAttributeSourcesInfoWeb { ErrorString = e.Message };
            }

            return infoWeb;
        }

        #endregion
        #region Get Attribute definitions
        public static OffnetAttributes Get(long id, DateTime effectiveDate)
        {
            OffnetAttributes atts;
            atts = _Cache.CheckCache(id.ToString());
            if (atts == null)
            {
                atts.ListAttributes = OffnetDataAccess.GetServiceAttributes(id, effectiveDate);
            }
            _Cache.StoreCache(id.ToString(), atts);
            return atts;
        }
        #endregion
        #region ImpactAttributes
        public static ImpactAttributes GetImpactAttributes(OffnetServiceKey key, List<string> changedAttributes)
        {
            var attributes = Get(key, false);
            List<string> impacts = new List<string>();
            var serviceId = key.Id;
            var serviceInstanceId = key.ServiceInstanceId;

            foreach (var attribute in changedAttributes)
            {
                bool isDesignImpact = attributes.Where(a => a.Name == attribute).Select(a => a.DesignImpact).FirstOrDefault();
                bool isProvisioningImpact = attributes.Where(a => a.Name == attribute).Select(a => a.ProvisioningImpact).FirstOrDefault();

                if (isDesignImpact && isProvisioningImpact)
                {
                    impacts.Add(attribute + " (DESIGN IMPACT, POVISIONING IMPACT)");
                }
                else if (isDesignImpact && !isProvisioningImpact)
                {
                    impacts.Add(attribute + " (DESIGN IMPACT)");
                }
                else if (!isDesignImpact && isProvisioningImpact)
                {
                    impacts.Add(attribute + " (POVISIONING IMPACT)");
                }
            }
            return new ImpactAttributes(serviceId, serviceInstanceId, impacts);
        }
        #endregion
    }
}
