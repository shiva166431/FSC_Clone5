using PCAT.Common.Biz;
using PCAT.Common.Models.Service;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL
{
    public class ServiceRuleSet: CommonRuleSet
    {
        public ServiceRuleSet() : base()
        { }

        #region AddDefaults
        public void AddDefaults(ServiceKey key)
        {
            var config = ServiceConfiguration.Get(key.Id, key.EffectiveDate);

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
        public void AddServiceKey(ServiceKey key)
        {
            _key = key;

            if (key.Values != null)
                foreach (var value in key.Values)
                    AddValue(value.Key, new RuleValue(value.Value));
        }
        #endregion
    }
}
