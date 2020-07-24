using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using PCAT.Common.Models.Service;
using PCAT.Common.Parsers;
using PCAT.Common.Models;
using PCAT.Common.Rules;
using PCAT.Common.Models.Validation;
using PCAT.Common.Caching;
using System.Linq;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Web.Models.Service;

namespace ServiceCatalog.BL
{
    public class ServiceHierarchy
    {
        private static readonly Cache<List<ServiceHierarchy>> _Cache = new Cache<List<ServiceHierarchy>>("Service Hierarchy Cache", CacheSizeType.Default, CacheDurationType.Default, CacheStorageType.LocalAndCouchbase);

        public long Id { get; set; }
        public string Name { get; set; }
        public string MaxQuantityRule { get; set; }
        public string MinQuantityRule { get; set; }

        #region BuildChild
        public static ServiceKey BuildChild(ServiceKey parent, string childName)
        {
            var children = Get(parent, childName);
            ServiceKey childService = null;

            if (children.Count == 0)
                throw new ApplicationException($"{childName} is not a child of service {parent.Id}");
            foreach (var child in children)
            {
                if (child.MaxQuantity == -1
                    || ((parent.Children == null || !parent.Children.ContainsKey(childName)) && child.MaxQuantity > 0)
                    || (parent.Children != null && parent.Children.ContainsKey(childName) && child.MaxQuantity > parent.Children[childName].Count)
                    )
                {
                    childService = new ServiceKey(child.Id)
                    {
                        EffectiveDate = parent.EffectiveDate,
                        ParentServiceKey = parent
                    };
                    childService.AddMissingAttributes();

                    break;
                }
            }

            if (childService == null)
                throw new ApplicationException($"{childName} can't be created on service {parent.Id}");

            return childService;
        }
        #endregion
        #region Get
        public static List<ServiceChild> Get(ServiceKey key, string name = null)
        {
            List<ServiceHierarchy> children = _Cache.CheckCache(key.Id.ToString());
            var kids = new List<ServiceChild>();

            if (children == null)
            {
                using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
                {
                    connection.Open();

                    children = connection.Query<ServiceHierarchy>(@"
select Child_Service_Id Id, sh.Name, Max_Quantity MaxQuantityRule, Min_Quantity MinQuantityRule
from Service_Hierarchy sh
  inner join Service_Configuration sc on sh.Child_Service_Id = sc.Service_Id and (sc.from_eff_date <= :effectiveDate and (sc.to_eff_date is null or sc.to_eff_date > :effectiveDate))
where sh.Service_Id = :id", new
                    {
                        id = key.Id,
                        effectiveDate = key.EffectiveDate
                    }).AsList();

                    _Cache.StoreCache(key.Id.ToString(), children);
                }
            }

            if (children != null && children.Count > 0)
            {
                var searchOptions = SearchOptions.DEFAULTS_ONLY;
                string error;
                RuleValue quantity;
                ServiceChild kid;

                var ruleSet = new ServiceRuleSet();
                ruleSet.AddDefaults(key);
                ruleSet.AddServiceKey(key);

                foreach (var child in children)
                {
                    if (name == null || name.Equals(child.Name))
                    {
                        kid = new ServiceChild()
                        {
                            Id = child.Id,
                            Name = child.Name
                        };

                        if (ValueRuleParser.ParseRule(child.MaxQuantityRule).TryGetValue(ruleSet, searchOptions, out quantity, out error))
                            kid.MaxQuantity = quantity.ToInteger() ?? 0;
                        if (ValueRuleParser.ParseRule(child.MinQuantityRule).TryGetValue(ruleSet, searchOptions, out quantity, out error))
                            kid.MinQuantity = quantity.ToInteger() ?? 0;

                        if (kid.MaxQuantity > 0 || kid.MaxQuantity == -1)
                            kids.Add(kid);
                    }
                }
            }

            return kids;
        }
        #endregion
        #region Validate
        public static ValidateResponse Validate(ServiceKey key)
        {
            var response = new ValidateResponse(true, key);
            var kids = Get(key);

            // validate min/max
            foreach (var kid in kids)
            {
                if (kid.MinQuantity > 0
                    && (key.Children == null || !key.Children.ContainsKey(kid.Name) || key.Children[kid.Name].Count < kid.MinQuantity))
                    response.AddError($"Minimum quanity not met for child '{kid.Name}'", ValidationError.SERVICE, key.GetIdentifier(null), ValidationError.MINIMUM, "Minimum quantity not met");
                else if (kid.MaxQuantity >= 0
                    && key.Children != null
                    && key.Children.ContainsKey(kid.Name)
                    && key.Children[kid.Name].Count > kid.MaxQuantity)
                    response.AddError($"Too many instances of child '{kid.Name}'", ValidationError.SERVICE, key.GetIdentifier(null), ValidationError.MAXIMUM, "Maximum quantity exceeded");
            }
            // make sure we don't have any extras
            if (key.Children != null)
                foreach (var child in key.Children)
                {
                    var allowedIds = kids.Where(k => k.Name.Equals(child.Key)).Select(k => k.Id);
                    if (allowedIds.Count() == 0)
                        response.AddError($"Child '{child.Key}' is not allowed on this service", ValidationError.SERVICE, key.GetIdentifier(null), ValidationError.MAXIMUM, $"Child '{child.Key}' is not allowed");
                    else
                    {
                        foreach (var v in child.Value)
                            if (!allowedIds.Contains(v.Id))
                                response.AddError($"Service #{v.Id} is not allowed for child '{child.Key}'", ValidationError.SERVICE, key.GetIdentifier(null), ValidationError.MAXIMUM, $"Service #{v.Id} is not allowed");
                    }
                }

            return response;
        }
        #endregion
        #region ImpactedChildren
        public static List<long> GetImpactedChildren(ServiceKey key, List<string> changedAttributes)
        {
            var children = Get(key, null);
            var impactedChildren = new List<long>();
            foreach (var child in children)
            {
                var config = ServiceConfiguration.Get(child.Id, DateTime.Now);

                impactedChildren.AddRange(from a in config.Attributes
                                          where (a.Value.Type.Equals(AttributeType.Parent) 
                                                    && changedAttributes.Contains(a.Value.Name)
                                                    )
                                                || (a.Value.DefaultValue != null 
                                                    && a.Value.DefaultValue.StartsWith("Parent.") 
                                                    && a.Value.DefaultValue.Length > 7 
                                                    && changedAttributes.Contains(a.Value.DefaultValue.Substring(7)
                                                    )
                                               )
                                          select child.Id);
                var childKey = BuildChild(key, child.Name);
                impactedChildren.AddRange(GetImpactedChildren(childKey, changedAttributes));
            }
            return impactedChildren.Distinct().ToList();
        }
        #endregion
    }
}
