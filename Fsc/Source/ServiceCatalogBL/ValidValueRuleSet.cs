using PCAT.Common.Biz;
using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Service;
using PCAT.Common.Rules;
using ServiceCatalog.BL.Models.Offnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCatalog.BL
{
    public class ValidValueRuleSet
    {
        private readonly List<IValidValueRule> _vvRules = new List<IValidValueRule>();
        private readonly List<IVerifyRule> _vfyRules = new List<IVerifyRule>();

        public void AddRules(ICollection<IRule> rules)
        {
            foreach (var r in rules)
            {
                var rule = r as IValidValueRule;
                if (rule != null) _vvRules.Add(rule);
                var item = r as IVerifyRule;
                if (item != null) _vfyRules.Add(item);
            }
        }

        public ICollection<AttributeInfo> GetAttributes(ServiceKey key, SearchOptions searchOptions)
        {
            //Use the rule set to GetAttributes...
            var myRuleSet = new CommonRuleSet();

            foreach (var r in _vvRules) 
                myRuleSet.AddRule(null, r);
            foreach (var r in _vfyRules) 
                myRuleSet.AddRule(null, r);

            foreach (var pair in key.Values.Where(pair => pair.Value != null))
            {
                myRuleSet.AddRule(pair.Key, new RuleValue(pair.Value));
                var list = new string[1];
                list[0] = pair.Value.Value;
                myRuleSet.AddRule(pair.Key, new ValidValueSet(pair.Key, list));
            }
            myRuleSet.AddAddresses(key);

            var atts = myRuleSet.GetAttributes(searchOptions, null, null);

            return atts;
        }

        public ICollection<AttributeInfo> GetAttributes(OffnetServiceKey key, SearchOptions searchOptions)
        {
            //Offnet service attributes Use the rule set to GetAttributes...
            var myRuleSet = new CommonRuleSet();

            foreach (var r in _vvRules)
                myRuleSet.AddRule(null, r);
            foreach (var r in _vfyRules)
                myRuleSet.AddRule(null, r);

            foreach (var pair in key.Values.Where(pair => pair.Value != null))
            {
                myRuleSet.AddRule(pair.Key, new RuleValue(pair.Value));
                var list = new string[1];
                list[0] = pair.Value.Value;
                myRuleSet.AddRule(pair.Key, new ValidValueSet(pair.Key, list));
            }
            myRuleSet.AddAddresses(key);

            var atts = myRuleSet.GetAttributes(searchOptions, null, null);

            return atts;
        }
    }
}
