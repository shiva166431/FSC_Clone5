using Dapper;
using Oracle.ManagedDataAccess.Client;
using PCAT.Common.Biz;
using PCAT.Common.Caching;
using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Linked;
using PCAT.Common.Models.Service;
using PCAT.Common.Models.Validation;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;
using PCAT.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServiceCatalog.BL
{
    public class ServiceConfiguration
    {
        private static readonly Cache<List<ServiceConfiguration>> _Cache = new Cache<List<ServiceConfiguration>>("Service Configuration Cache", CacheSizeType.Default, CacheDurationType.Default, CacheStorageType.LocalAndCouchbase);

        public long Id { get; set; }
        public int Version { get; set; }
        public DateTime FromEffectiveDate { get; set; }
        public DateTime ToEffectiveDate { get; set; }
        public string DisplayPattern { get; set; }
        public string ServiceLayerRule { get; set; }
        public string AllowExisting { get; set; }
        public char? AllowSharedStandalone { get; set; }
        public string ValidValueRule { get; set; }

        public Dictionary<string, AttributeDefinition> Attributes;

        #region Get
        public static ServiceConfiguration Get(long id, DateTime effectiveDate)
        {
            ServiceConfiguration config;

            var configs = _Cache.CheckCache(id.ToString());

            if (configs == null)
            {
                config = GetFromDb(id, effectiveDate);
                _Cache.StoreCache(id.ToString(), new List<ServiceConfiguration>() { config });
            }
            else
            {
                config = configs.FirstOrDefault(c => c.IsEffective(effectiveDate));
                if (config == null)
                {
                    config = GetFromDb(id, effectiveDate);
                    configs.Add(config);
                }
            }

            return config;
        }
        private static ServiceConfiguration GetFromDb(long id, DateTime effectiveDate)
        {
            ServiceConfiguration config;

            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();

                config = connection.QueryFirst<ServiceConfiguration>(
                              $"select SERVICE_ID Id, Version, FROM_EFF_DATE FromEffectiveDate, TO_EFF_DATE ToEffectiveDate," +
                              $"   DISPLAY_PATTERN DisplayPattern, SERVICE_LAYER_RULE ServiceLayerRule, ALLOW_EXISTING AllowExisting," +
                              $"   ALLOW_SHARED_STANDALONE AllowSharedStandalone, VALID_VALUE_RULE ValidValueRule" +
                               " from service_configuration where service_id = :id " +
                               "   and from_eff_date <= :effectiveDate " +
                               "   and (to_eff_date is null or to_eff_date > :effectiveDate) ", new { id, effectiveDate });
            }
            config.Attributes = ServiceAttributes.Get(id, effectiveDate);

            return config;
        }
        #endregion

        #region AffectsRelation
        public bool AffectsRelation(string name)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && (ad.AffectsRelation);
        }
        #endregion
        #region AffectsChildren
        public bool AffectsChildren(string name)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && (ad.AffectsChildren);
        }
        #endregion
        #region GetApplicableForChange
        public bool GetApplicableForChange(string name)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && (ad.ApplicableForChange);
        }
        #endregion
        #region GetDefaultValue
        public string GetDefaultValue(string name, ServiceKey sk)
        {
            Attributes.TryGetValue(name, out var ad);
            if (ad != null && !string.IsNullOrEmpty(ad.DefaultValue))
            {
                try
                {
                    if (ad.Type == AttributeType.Related)
                        return sk.GetAttributeValue(ad.DefaultValue, SearchOptions.ALL_TRUE);

                    var rule = ValueRuleParser.ParseRule(ad.DefaultValue);
                    if (rule != null && !(rule is RuleValue))
                    {
                        RuleValue value;
                        string error;
                        var rs = sk.ToValueHolder();
                        if (rule.TryGetValue(rs, SearchOptions.ALL_TRUE, out value, out error)) 
                            return value?.ToString();
                    }
                }
                catch (Exception e)
                {
                    return "Error getting default";
                }

                if (ad.DefaultValue != null && ad.DefaultValue.StartsWith("'") && ad.DefaultValue.EndsWith("'"))
                    return ad.DefaultValue.Substring(1, ad.DefaultValue.Length - 2);
                else if (ad.DefaultValue.Contains("."))
                    return sk.GetAttributeValue(ad.DefaultValue, SearchOptions.ALL_TRUE);
                else
                    return ad.DefaultValue;
            }
            return null;
        }
        #endregion
        #region GetLabel
        public string GetLabel(string name)
        {
            AttributeDefinition ad;

            Attributes.TryGetValue(name, out ad);
            if (ad != null)
            {
                return (String.IsNullOrEmpty(ad.Label)) ? name : ad.Label;
            }
            return name;
        }
        #endregion
        #region GetMaxRepeats
        public int GetMaxRepeats(string name)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) ? ad.MaxRepeats : 0;
        }
        #endregion
        #region GetRequiresRefresh
        public bool GetRequiresRefresh(string name)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && (ad.RequiresRefresh);
        }
        #endregion
        #region HasDefault
        public bool HasDefault(string name)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            if (ad == null) return false;
            return !string.IsNullOrEmpty(ad.DefaultValue);
        }
        #endregion
        #region IsConfigurableAttribute
        public bool IsConfigurableAttribute(string name, IValueHolder vh)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            if (vh == null) return ad != null;
            return (ad != null) && ad.ApplicableRule.Evaluate(vh).Result;
        }
        #endregion
        #region IsEffective
        public bool IsEffective(DateTime date)
        {
            date = Utility.ConvertToMountain(date);
            if (FromEffectiveDate <= date &&
                 (ToEffectiveDate == null || ToEffectiveDate >= date || ToEffectiveDate == DateTime.MinValue))
                return true;
            return false;
        }
        #endregion
        #region IsHidden
        public bool IsHidden(string name, IValueHolder vh)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && ad.IsHidden(vh);
        }
        #endregion
        #region IsOptional  
        public bool IsOptional(string name, IValueHolder vh)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            if (ad != null)
            {
                return (!(ad.RequiredRule.Evaluate(vh)).Result);
            }
            return true;
        }
        #endregion
        #region IsReadOnly
        public bool IsReadOnly(string name, IValueHolder vh)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && ad.IsReadOnly(vh);
        }
        #endregion
        #region IsDesignImpact
        public bool IsDesignImpact(string name, IValueHolder vh)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && ad.IsDesignImpact(vh);
        }
        #endregion
        #region IsProvisioningImpact
        public bool IsProvisioningImpact(string name, IValueHolder vh)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return (ad != null) && ad.IsProvisioningImpact(vh);
        }
        #endregion
        #region SortList
        public ICollection<AttributeInfo> SortList(ICollection<AttributeInfo> list)
        {
            var sl = new SortedList<int, AttributeInfo>();

            foreach (var att in list)
            {
                Attributes.TryGetValue(att.Name, out var ad);
                if (ad != null)
                {
                    var seq = ad.Sequence;
                    while (sl.ContainsKey(seq)) seq++;
                    sl.Add(seq, att);
                }
                else sl.Add(10000, att);
            }

            return sl.Values;
        }
        #endregion
        #region IsValid
        public ValidateResponse IsValid(string name, string value, IValueHolder vh)
        {
            AttributeDefinition ad;
            Attributes.TryGetValue(name, out ad);
            return ad?.DataConstraint != null ? ad?.DataConstraint.IsValid(name, value, vh) : new ValidateResponse(true);

        }
        #endregion

        #region Validate
        public ValidateServiceResponse Validate(ServiceKey key)
        { 
            var response = new ValidateServiceResponse(true, key) { Errors = new List<IValidationError>() };
            var vr = new ValidateResponse(true,key) { Errors = new List<IValidationError>() };

            // let's make sure all required attributes are there
            vr.Errors.AddRange(from ad in Attributes.Values
                                     where !IsOptional(ad.Name, key)
                                     //checking if the attribute is supposed to have defaults
                                     where !HasDefault(ad.Name)
                                     where !ad.Type.Equals(AttributeType.Complex)
                                     let value = key.GetAttributeValue(ad.Name, SearchOptions.ALL_TRUE)
                                     where string.IsNullOrEmpty(value)
                                     select new ValidationServiceError(string.Format("{0}: {1} is required.", key.ServiceInstanceId, ad.Label), ValidationError.SERVICE, ad.Name, ValidationError.MISSING, ad.Label, null, key.ServiceInstanceId));

            // if attributes are missing, let's stop there
            if (vr.Errors.Count == 0)
            {
                // valid value check
                var validValueRules = ValidValueRuleParser.GetRules(ValidValueRule);

                foreach (IValidValueRule rule in validValueRules)
                {
                    var v = key.GetAttributeValue(rule.GetAttributeName(), SearchOptions.NO_DEFAULTS);
                    //checking if attribute is applicable and is required
                    if (IsConfigurableAttribute(rule.GetAttributeName(), key)  && !IsOptional(rule.GetAttributeName(),key))
                    {                                            
                        vr.AddResponse(rule.ValidateAttributes(key));
                    }
                }

                // DataConstraints
                foreach (var a in key.Values)
                    vr.AddResponse(IsValid(a.Key, a.Value.Value, key));
                

                // service relationships
                vr.AddResponse(ServiceRelationships.ValidateRelationships(key));
                

                // children
                vr.AddResponse(ServiceHierarchy.Validate(key));
               


                //Since we have all of the needed values, we can now make sure that it meets all of the business rules.
                var attributes = ServiceAttributes.Get(key, false);
                AttributeInfo attributeInfo;
                foreach (var attributeName in Attributes.Keys)
                {
                    attributeInfo = attributes.FirstOrDefault(a => a.Name.Equals(attributeName));
                    if (attributeInfo != null)
                    {
                        if (attributeInfo.GetValue() == null && !IsOptional(attributeName, key) && !HasDefault(attributeName))
                        {
                            vr.AddError(attributeInfo.Label + " does not have a valid value.", ValidationError.ATTRIBUTE, key.GetIdentifier(null), ValidationError.INVALID_VALUE, attributeName);
                        }
                        //If the value returned by the GetAttributes doesn't match the one returned by the key, we are not valid.
                        if (attributeInfo.Type.Equals(AttributeType.List))
                        {
                            string value = null;
                            if (!string.IsNullOrEmpty(attributeInfo.GetValue()))
                                value = attributeInfo.GetValue();
                            else if (!string.IsNullOrEmpty(attributeInfo.DefaultValue))
                                value = attributeInfo.DefaultValue;

                            var keyValue = key.GetAttributeValue(attributeName, SearchOptions.ALL_TRUE);
                            if (value != null && keyValue != null && !value.Equals(keyValue))
                            {
                                vr.AddError(string.Format("{0} ({1}) does not have a valid value. Should be ({2}).",
                                                                attributeInfo.Label, keyValue, value), ValidationError.ATTRIBUTE, key.GetIdentifier(null), ValidationError.INVALID_VALUE, attributeName);
                            }
                        }
                    }
                    else if (!HasDefault(attributeName) && !IsOptional(attributeName, key))
                    {
                        vr.AddError(
                                string.Format("{0} is required, does not have a default value, and '{1}' is not returned by GetAttributes.",
                                    attributeInfo.Label, key.GetAttributeValue(attributeName, SearchOptions.ALL_TRUE)),
                                ValidationError.ATTRIBUTE, key.GetIdentifier(null), ValidationError.INVALID_VALUE, attributeName);

                    }
                }
            }
            response.AddResponse(response.ToServiceResponse(FixAttributeNameToLabel(key,vr)));

            if (response.Errors.Count > 0)
            {
                response.IsValid = false;
                List<ValidationServiceError> vErrors = new List<ValidationServiceError>();

                foreach (var error in response.Errors)
                {
                    if (error is ValidationError)
                        vErrors.Add(new ValidationServiceError(error.Description, error.Category,
                            error.Entity, error.Type, error.Reason, error.ErrorCode, key.ServiceInstanceId));
                    else {
                        if((error as ValidationServiceError)?.InstanceId == null || (error as ValidationServiceError)?.InstanceId == 0)
                                vErrors.Add(new ValidationServiceError(error.Description, error.Category,
                            error.Entity, error.Type, error.Reason, error.ErrorCode, key.ServiceInstanceId));
                        else vErrors.Add(error as ValidationServiceError); }


                }
                if (vErrors.Count > 0)
                {
                    response.Errors = null;
                    response.AddErrors(new List<ValidationServiceError>(vErrors));
                }

            }

            return response;
        }
        #endregion
       public ValidateResponse FixAttributeNameToLabel(ServiceKey sk,ValidateResponse vr)
        {
            ValidateResponse resp = new ValidateResponse(true);
            var atts = ServiceAttributes.Get(sk, false);
            var errors = vr.GetErrors();
            if (errors?.Count() > 0)
            {
                resp.IsValid = false;
                foreach (var err in errors)
                {
                    if (err.Description.Contains("[") && err.Description.Contains("]"))
                    {
                        var attName = Regex.Match(err.Description, @"\[([^)]*)\]")?.Groups[1]?.Value;
                        var info = atts.FirstOrDefault(x => x.Name == attName);
                        var description = err.Description.Replace("["+attName+"]", info.Label);
                        resp.AddError(description, err.Category, err.Entity, err.Type, err.Reason, err.ErrorCode);
                    }
                    else resp.AddError(err.Description, err.Category, err.Entity, err.Type, err.Reason, err.ErrorCode);
                }
            }
            return resp;
        }
    }
}
