using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PCAT.Common.Biz;
using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;
using PCAT.Common.Utilities;
using ServiceCatalog.BL.Web.Offnet;
using ServiceCatalog.BL.Models.Offnet;
using ServiceCatalog.BL.Offnet;
using ServiceCatalog.BL.Parsers;

namespace ServiceCatalog.BL.Rules
{
    public class OffnetServiceRuleSet : ResolveOffnetServiceSet
    {
        public const string PARENT = "Parent";

        private readonly Dictionary<string, OffnetServiceKey> _existingServices = new Dictionary<string, OffnetServiceKey>();
        private readonly List<IValidValueRule> _vvRules = new List<IValidValueRule>();
        private readonly List<IVerifyRule> _vfyRules = new List<IVerifyRule>();

        public void AddValidValueRules(ICollection<IRule> rules)
        {
            foreach (var r in rules)
            {
                var rule = r as IValidValueRule;
                if (rule != null) _vvRules.Add(rule);
                var item = r as IVerifyRule;
                if (item != null) _vfyRules.Add(item);
            }
        }

        public PartnerServicePackage ApplyRules(PartnerServicePackage package)
        {
            //Validate partner

            var key = package.Partner;
            if (key == null) throw new ApplicationException("Missing Partner!");
            if ((key.PartnerOrderId == 0 || String.IsNullOrEmpty(key.PartnerOrderName)) || string.IsNullOrEmpty(OffnetBiz.GetPartnerServiceConfig(key).ServiceRule)) return package; //If there are no rules, there is nothing to do.


            AddRules(key);

            // For all the existing services, move them into the rule set so they can be used by the rules...
            if (package.Services != null)
            {
                foreach (var pair in package.Services)
                {
                    _existingServices.Add(pair.Key, pair.Value);
                }
            }
            //Run through the rules...
            _serviceRules = _serviceRules.ToList();
            foreach (var rule in _serviceRules)
            {
                try
                {
                    rule.Apply(package, this);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + ", Partner " + package?.Partner?.PartnerOrderName + ", Rule: " + rule, e);
                }
            }

            //Any left over services need to be removed from package.
            foreach (var pair in _existingServices)
            {
                var sKey = pair.Value;
                package.AddService(pair.Key, sKey);
            }

            return package;
        }

        public override OffnetServiceKey FetchAndRemoveExistingService(string name)
        {
            OffnetServiceKey key = null;
            if (_existingServices.ContainsKey(name)) _existingServices.TryGetValue(name, out key);
            if (key != null) _existingServices.Remove(name);
            return key;
        }

    }

    public class ResolveOffnetServiceSet : CommonRuleSet
    {
        protected List<OffnetServiceMappingRule> _serviceRules = new List<OffnetServiceMappingRule>();
        protected readonly Dictionary<string, PartnerKeyWeb> _partnerTags = new Dictionary<string, PartnerKeyWeb>();
        private readonly string _tagSuffix;
        private readonly ResolveOffnetServiceSet _parentResolveServiceSet;

        public ResolveOffnetServiceSet() { }

        public ResolveOffnetServiceSet(string tagSuffix, ResolveOffnetServiceSet rss)
        {
            _tagSuffix = tagSuffix;
            _parentResolveServiceSet = rss;
        }
        public virtual OffnetServiceKey FetchAndRemoveExistingService(string name)
        {
            return _parentResolveServiceSet.FetchAndRemoveExistingService(name);
        }

        public string GetTagName(string name)
        {
            return _tagSuffix == null ? name : string.Format("{0}:{1}", name, GetTagSuffix());
        }

        public string GetTagSuffix()
        {
            if (_parentResolveServiceSet == null) return _tagSuffix;
            var parentTagSuffix = _parentResolveServiceSet.GetTagSuffix();
            return parentTagSuffix != null ? string.Format("{0}:{1}", parentTagSuffix, _tagSuffix) : _tagSuffix;
        }
        public PartnerKeyWeb GetPartner(string name)
        {
            var tagName = GetTagName(name);
            if (_partnerTags.ContainsKey(tagName))
                return _partnerTags[tagName];
            if (_parentResolveServiceSet != null)
            {
                var partner = _parentResolveServiceSet.GetPartner(name);
                if (partner != null)
                    return partner;
            }
            throw new ApplicationException("Partner Tag [" + tagName + "] not found.");
        }
        public void AddRule(OffnetServiceMappingRule sr, PartnerKeyWeb key, ResolveOffnetServiceSet rss)
        {
            var rule = sr as PartnerTagServiceRule;
            if (rule != null)
            {
                var ptsr = rule;
                _partnerTags.Add(rss.GetTagName(ptsr.TagName), key);
                return;
            }
            else if (sr != null)
            {
                //If the "key" is in there already, replace it.
                _serviceRules.RemoveAll(r => r.CompareString().Equals(sr.CompareString()));
                _serviceRules.Add(sr);
            }
        }

        public void AddRules(PartnerKeyWeb key)
        {
            var rules = OffnetServiceRuleParser.GetRules(OffnetBiz.GetPartnerServiceConfig(key).ServiceRule, Utility.ConvertToMountain(key.EffectiveDate));
            if (rules != null)
            {
                foreach (var r in rules)
                {
                     AddRule(r, key, this);
                }
            }
        }

        public List<OffnetServiceMappingRule> GetRules()
        {
            return _serviceRules;
        }
        public void SortRules()
        {
            _serviceRules = _serviceRules.ToList();
        }
        //Servicecatalogbl ServiceRuleset
        #region AddDefaults
        public void AddDefaults(OffnetServiceKey key)
        {
            var config = OffnetServiceConfiguration.Get(key.Id, key.EffectiveDate);

            IPriceRule pricingRule;
            foreach (var attribute in config.Attributes)
                if (!string.IsNullOrEmpty(attribute.Value.DefaultValue))
                {
                    pricingRule = ValueRuleParser.ParseRule(attribute.Value.DefaultValue);
                    if (pricingRule == null || pricingRule is RuleValue)
                        pricingRule = new PriceRuleValue(attribute.Value.DefaultValue, false);
                    AddDefault(attribute.Key, new DefaultRuleValue(pricingRule));
                }
        }
        #endregion
        #region AddServiceKey
        public void AddServiceKey(OffnetServiceKey key)
        {
            _key = key;

            if (key.Values != null)
                foreach (var value in key.Values)
                    AddValue(value.Key, new RuleValue(value.Value));
        }
        #endregion
    }
}
