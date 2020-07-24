using Dapper;
using Oracle.ManagedDataAccess.Client;
using PCAT.Common.Biz;
using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Service;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;
using PCAT.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCatalog.BL
{
    public static class ServiceAttributes
    {

        #region Get Attributes
        public static ICollection<AttributeInfo> Get(ServiceKey key, bool populateLists)
        {
            var config = ServiceConfiguration.Get(key.Id, key.EffectiveDate);

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


            var ruleSet = new ServiceRuleSet();
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
        #endregion
        #region Get Attribute definitions
        public static Dictionary<string, AttributeDefinition> Get(long id, DateTime effectiveDate)
        {
            var definitions = new Dictionary<string, AttributeDefinition>();

            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();

                var attributes = connection.Query(
                    "select sa.* " +
                    "   from service_attributes sa " +
                    "      inner join service_configuration sc on sa.service_id = sc.service_id and sa.version = sc.version" +
                    "   where sc.service_id = :id and (sc.from_eff_date <= :effectiveDate and (sc.to_eff_date is null or sc.to_eff_date > :effectiveDate)) " +
                    "union " +
                    "select sa.* " +
                    "   from service_attributes sa " +
                    "      inner join service_inheritance si on sa.service_id = si.inherited_service_id" +
                    "      inner join service_configuration sc on sc.service_id = si.inherited_service_id and sa.version = sc.version" +
                    "   where si.service_id = :id and (sc.from_eff_date <= :effectiveDate and (sc.to_eff_date is null or sc.to_eff_date > :effectiveDate)) ", new { id, effectiveDate });

                foreach (var attribute in attributes)
                {
                    try
                    {
                        definitions.Add(attribute.NAME, new AttributeDefinition()
                        {
                            Key = Utility.ToString(attribute.NAME),
                            Name = Utility.ToString(attribute.NAME),
                            HelpText = Utility.ToString(attribute.HELP_TEXT),
                            ApplicableRule = SimpleRuleParser.GetRule(Utility.ToString(attribute.IS_APPLICABLE)) ?? new SimpleConstantRule(true),
                            RequiredRule = SimpleRuleParser.GetRule(Utility.ToString(attribute.IS_REQUIRED)) ?? new SimpleConstantRule(false),
                            Type = new AttributeType(Utility.ToString(attribute.ATTRIBUTE_TYPE)),
                            RequiredElements = new List<string>(Parser.Split(Utility.ToString(attribute.REQUIRED_ELEMENTS), ',')),
                            DefaultValue = Utility.ToString(attribute.DEFAULT_VALUE),
                            Sequence = Utility.ToInt(attribute.SEQUENCE),
                            Label = Utility.ToString(attribute.LABEL),
                            MaxRepeats = Utility.ToInt(attribute.MAX_REPEATS),
                            ReadOnlyRule = SimpleRuleParser.GetRule(Utility.ToString(attribute.READ_ONLY_RULE)),
                            HiddenRule = SimpleRuleParser.GetRule(Utility.ToChar(attribute.HIDDEN) == 'Y' ? "true" : "false"),
                            ComplexType = Utility.ToInt(attribute.REFERENCE_TYPE_ID),
                            Mask = Utility.ToString(attribute.MASK),
                            DataConstraint = DataConstraintParser.GetConstraint(Utility.ToString(attribute.DATA_CONSTRAINT)),
                            RequiresRefresh = !("N".Equals(Utility.ToString(attribute.REQUIRES_REFRESH), StringComparison.CurrentCultureIgnoreCase)),
                            TechRole = Utility.ToString(attribute.TECH_ROLE),
                            ApplicableForChange = ("Y".Equals(Utility.ToString(attribute.APPLICABLE_FOR_CHANGE), StringComparison.CurrentCultureIgnoreCase)),
                            AffectsChildren = ("Y".Equals(Utility.ToString(attribute.AFFECTS_CHILDREN), StringComparison.CurrentCultureIgnoreCase)),
                            AffectsRelation = ("Y".Equals(Utility.ToString(attribute.AFFECTS_RELATION), StringComparison.CurrentCultureIgnoreCase)),
                            DesignImpact = SimpleRuleParser.GetRule(Utility.ToString(attribute.DESIGN_IMPACT)),
                            ProvisioningImpact= SimpleRuleParser.GetRule(Utility.ToString(attribute.PROVISIONING_IMPACT))
                        });
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"Error loading attribute {attribute.NAME} for service {id}: {e.Message}", e);
                    }
                }
            }

            return definitions;
        }
        #endregion
        #region ImpactAttributes
        public static ImpactAttributes GetImpactAttributes(ServiceKey key, List<string> changedAttributes)
        {
            var attributes = Get(key, false);
            List<string> impacts = new List<string>();
            var serviceId = key.Id;
            var serviceInstanceId = key.ServiceInstanceId;

            foreach (var attribute in changedAttributes)
            {
                bool isDesignImpact = attributes.Where(a => a.Name == attribute).Select(a => a.DesignImpact).FirstOrDefault();
                bool isProvisioningImpact = attributes.Where(a => a.Name == attribute).Select(a => a.ProvisioningImpact).FirstOrDefault();

                if(isDesignImpact&&isProvisioningImpact)
                {
                    impacts.Add(attribute + " (DESIGN IMPACT, POVISIONING IMPACT)");
                }
                else if(isDesignImpact&&!isProvisioningImpact)
                {
                    impacts.Add(attribute + " (DESIGN IMPACT)");
                }
                else if(!isDesignImpact && isProvisioningImpact)
                {
                    impacts.Add(attribute + " (POVISIONING IMPACT)");
                }
            }
            return new ImpactAttributes(serviceId, serviceInstanceId,impacts);
        }
        #endregion
    }
}
