using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Debugging;
using PCAT.Common.Rules;
using PCAT.Common.Utilities;
using ServiceCatalog.BL.Web.Offnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCatalog.BL.Rules
{
    public class OffnetRuleSet
    {
       //RuleSet
        private PartnerKeyWeb _partnerKey;
        private long? _serviceId;

        private List<IValidValueRule> _validValueRules = new List<IValidValueRule>();
        private List<IVerifyRule> _verifyRules = new List<IVerifyRule>();
        private IDictionary<string, DefaultRuleValue> _defaults = new Dictionary<string, DefaultRuleValue>();
        private readonly IDictionary<string, RuleValue> _values = new Dictionary<string, RuleValue>();
        //This is for passing around other ruleSets that might be used for looking things up.
        //We want to cache them to speed them up.
        public readonly IDictionary<long, OffnetRuleSet> RuleSets = new Dictionary<long, OffnetRuleSet>();

        public OffnetRuleSet()
        {
        }


        public void AddRule(string label, IRule rule)
        {
            var item = rule as IValidValueRule;
            if (item != null)
                _validValueRules.Add(item);
            var verifyRule = rule as IVerifyRule;
            if (verifyRule != null)
                _verifyRules.Add(verifyRule);
        }
        public void AddRules(OffnetRuleSet rs)
        {
            _validValueRules = rs._validValueRules;
            _verifyRules = rs._verifyRules;
            _defaults = rs._defaults;
        }

        private void RemoveRule(string label)
        {
            if (_values.ContainsKey(label)) _values.Remove(label);
        }

        public void AddDefault(string label, DefaultRuleValue drv)
        {
            _defaults[label] = drv;
            _validValueRules.Add(drv);
        }
        public void AddDefault(string label, string value)
        {
            AddDefault(label, new DefaultRuleValue(new PriceRuleValue(value, false)));
        }
        public bool ContainsDefaultValue(string label)
        {
            return _defaults.ContainsKey(label);
        }

        public PartnerKeyWeb GetProductKey()
        {
            return _partnerKey;
        }


        public OffnetRuleSet Clone()
        {
            var newSet = new OffnetRuleSet();
            {
                foreach (var x in _defaults) newSet._defaults.Add(x.Key, (DefaultRuleValue)x.Value.Clone());
                foreach (var x in _validValueRules) newSet._validValueRules.Add((IValidValueRule)x.Clone());
                foreach (var x in _verifyRules) newSet._verifyRules.Add((IVerifyRule)x.Clone());
                foreach (var x in _values) newSet._values.Add(x);
            }
            return newSet;
        }


        public long Id // IKey support
        {
            get { return GetDebugId(); }
            set { _serviceId = value; }
        }
        private long GetDebugId()
        {
            if (_serviceId.HasValue)
                return _serviceId.Value + 50000;
            else
                return _partnerKey?.PartnerOrderId ?? 0;
        }
    }
}
