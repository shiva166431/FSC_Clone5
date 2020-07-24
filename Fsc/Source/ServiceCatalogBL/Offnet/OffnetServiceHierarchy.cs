using Dapper;
using Oracle.ManagedDataAccess.Client;
using PCAT.Common.Caching;
using PCAT.Common.Models;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;
using ServiceCatalog.BL.Models.Offnet;
using ServiceCatalog.BL.Rules;
using ServiceCatalog.BL.Web.Offnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL.Offnet
{
    public class OffnetServiceHierarchy
    {
        private static readonly Cache<List<OffnetServiceHierarchy>> _Cache = new Cache<List<OffnetServiceHierarchy>>("Offnet Service Hierarchy Cache", CacheSizeType.Default, CacheDurationType.Default, CacheStorageType.LocalAndCouchbase);

        public long Id { get; set; }
        public string Name { get; set; }
        public string MaxQuantityRule { get; set; }
        public string MinQuantityRule { get; set; }

        #region Get
        public static List<OffnetServiceChild> Get(OffnetServiceKey key, string name = null)
        {
            //List<OffnetServiceHierarchy> children = _Cache.CheckCache(key.Id.ToString());
            List<OffnetServiceHierarchy> children = new List<OffnetServiceHierarchy>();
            var kids = new List<OffnetServiceChild>();

            children = OffnetDataAccess.GetOffnetChildren(key);

            // _Cache.StoreCache(key.Id.ToString(), children);

            if (children != null && children.Count > 0)
            {
                var searchOptions = SearchOptions.DEFAULTS_ONLY;
                string error;
                RuleValue quantity;
                OffnetServiceChild kid;

                var ruleSet = new OffnetServiceRuleSet();
                ruleSet.AddDefaults(key);
                ruleSet.AddServiceKey(key);

                foreach (var child in children)
                {

                    if (name == null || name.Equals(child.Name))
                    {
                        kid = new OffnetServiceChild()
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
        #region BuildChild
        public static OffnetHierarchyWeb BuildChild(OffnetServiceKey parent, string childName)
        {
            var children = Get(parent, childName);
            OffnetServiceKey childService = null;
            OffnetServiceKey[] childServices;
            OffnetHierarchyWeb offnetHierarchy = null;

            if (children.Count == 0)
                throw new ApplicationException($"{childName} is not a child of service {parent.Id}");
            int i;
            foreach (var child in children)
            {
                childServices = new OffnetServiceKey[child.MaxQuantity];
                if (child.MinQuantity > 1 && child.MaxQuantity > 1)
                {
                    long tempInstance = 1;

                    if (((parent.Children == null || !parent.Children.ContainsKey(childName)) && child.MaxQuantity > 0)
                        || (parent.Children != null && parent.Children.ContainsKey(childName) && child.MaxQuantity > parent.Children[childName].Count)
                        )
                    {
                        for (i = 0; i <= child.MaxQuantity - 1; i++)
                        {
                            childService = new OffnetServiceKey(child.Id)
                            {
                                EffectiveDate = parent.EffectiveDate,
                                ParentServiceKey = parent,
                            };
                            childService.AddMissingAttributes();
                            childService.SetAttributeValue(childService, tempInstance);
                            tempInstance++;
                            childServices[i] = childService;
                        }
                        offnetHierarchy = new OffnetHierarchyWeb(childServices);
                        break;
                    }
                }
                else
                {
                    if ((child.MaxQuantity == -1 || (parent.Children == null || !parent.Children.ContainsKey(childName)) && child.MaxQuantity > 0)
                      || (parent.Children != null && parent.Children.ContainsKey(childName) && child.MaxQuantity > parent.Children[childName].Count))
                    {
                        childService = new OffnetServiceKey(child.Id)
                        {
                            EffectiveDate = parent.EffectiveDate,
                            ParentServiceKey = parent,
                        };
                        childService.AddMissingAttributes();
                        childServices[0] = childService;
                    }
                    offnetHierarchy = new OffnetHierarchyWeb(childServices);
                    break;
                }
            }
            if (childService == null)
                throw new ApplicationException($"{childName} can't be created on service {parent.Id}");
            return offnetHierarchy;
        }
        #endregion
        //ImpactedChildren
    }
}
