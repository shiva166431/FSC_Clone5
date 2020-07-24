using PCAT.Common.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using PCAT.Common.Utilities;
using PCAT.Common.Parsers;
using System.Linq;
using PCAT.Common.Models.Service;
using PCAT.Common.Biz;
using PCAT.Common.Models;
using PCAT.Common.Models.Validation;
using PCAT.Common.Web.Models.Service;

namespace ServiceCatalog.BL
{
    public class ServiceRelationships
    {

        public string Name;
        public SimpleRule ApplicableRule;
        public List<long> Services;
        public string Minimum;
        public string Maximum;
        public ReverseNameDescription ReverseName;
        public string FilterRule;
        public class ReverseNameDescription
        {
            public string ReverseName;

            public ReverseNameDescription() { }

            public ReverseNameDescription(string name)
            {
                ReverseName = (name ?? "Not Set...  Fix in database.");
            }
        }
        #region helpers
        private static List<long> AllowedServicesToList(string svcIds)
        {
            return new List<long>(svcIds
                                               .Split(',')
                                               .Where(re => !string.IsNullOrEmpty(re))
                                               .Select(s => Convert.ToInt64(s)));

        }
        private static List<ServiceRelationships> QueryToServiceRelationships(List<dynamic> In)
        {
            var Out = new List<ServiceRelationships>();
            foreach (var Releation in In)
            {
                try
                {
                    Out.Add(new ServiceRelationships()
                    {
                        Name = Utility.ToString(Releation.NAME),
                        ApplicableRule = SimpleRuleParser.GetRule(Utility.ToString(Releation.APPLICABLE_RULE)),
                        Services = AllowedServicesToList(Releation.ALLOWED_SERVICES),
                        Minimum = Utility.ToString(Releation.MIN_QUANTITY),
                        Maximum = Utility.ToString(Releation.MAX_QUANTITY),
                        ReverseName = new ReverseNameDescription(Utility.ToString(Releation.REVERSE_NAME)),
                        FilterRule = Utility.ToString(Releation.FILTER_RULE)
                    });

                }
                catch (Exception e)
                {
                    // throw new ApplicationException();
                }
            }
            return Out;
        }
        #endregion
        public static List<ServiceRelationships> GetServiceRelationships(long serviceId, int version)
        {
            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                var serviceRelationship = new List<ServiceRelationships>();
                connection.Open();


                var qry = connection.Query("select * " +
                               "from service_relationship sr " +
                               "where service_id = :serviceId " +
                               "and version = :version ", new { serviceId, version }).AsList();

                serviceRelationship = QueryToServiceRelationships(qry);
                return serviceRelationship;
            }  
        }

        public static ICollection<ServiceRelationships> Get(ServiceKey key)
        {
            var config = ServiceConfiguration.Get(key.Id, key.EffectiveDate);
            var svcRel = GetServiceRelationships(key.Id, config.Version);

            return svcRel;
        }

        public ServiceRelationships GetDefinition(ServiceKey key)
        {
            if (ApplicableRule.Evaluate(key).Result)
            {
                var r = new ServiceRelationships
                {
                    Name = Name,
                    Services = new List<long>(),
                    Minimum = GetRuletValue(Minimum, key),
                    Maximum = GetRuletValue(Maximum, key),
                    ReverseName = ReverseName,
                    FilterRule = FilterRule
                };
                r.Services.AddRange(Services);

                return r;
            }
            return null;
        }
        public string GetRuletValue(string name, ServiceKey sk)
        {

            //See if it is a rule first...  
            try
            {
                var rule = ValueRuleParser.ParseRule(name);
                if (rule != null && !(rule is RuleValue))
                {
                    RuleValue value;
                    string error;
                    var rs = sk.ToValueHolder();
                    if (rule.TryGetValue(rs, SearchOptions.ALL_TRUE, out value, out error)) return value?.ToString();
                }
            }
            catch (Exception e)
            {
                return name;
            }
            return name;

        }

        public static ValidateResponse ValidateRelationships(ServiceKey key)
        {
            var response = new ValidateResponse(true, key) { Errors = new List<IValidationError>() };
            var serviceName = ServiceDefinition.Get(key.Id).Name;
            try
            {
                var svcRelationships = Get(key);
                //got applicable relations
                var svcRel = svcRelationships.Select(def => def.GetDefinition(key)).Where(pd => pd != null).ToList();
                if (svcRel?.Count > 0)
                {
                    response.Errors.AddRange(from rel in svcRel
                                             where Convert.ToInt32(rel.Minimum) > 0
                                             where !(key.Relationships.Any(x => x.Key == rel.Name))
                                             select new ValidationError(string.Format("Required Related {0} is missing on service {1}", rel.Name, serviceName), ValidationError.RELATIONSHIP, key.GetIdentifier(null), ValidationError.MISSING, rel.Name + " is missing.", rel.Name));
                }
                if (key.Relationships != null)
                {
                    foreach (var pair in key.Relationships)
                    {
                        var relExists = svcRel.Any(x => x.Name == pair.Key);
                        if (!relExists)
                        {
                            response.AddError(string.Format("{0} is an unknown relationship for service {1}.", pair.Key,
                                                            serviceName), ValidationError.RELATIONSHIP, key.GetIdentifier(null), ValidationError.UNKNOWN, pair.Key + " is unknown for this service.", null);
                        }
                        else
                        {
                            var rel = svcRel.Find(x => x.Name == pair.Key);

                            if (key.Relationships[pair.Key].Count() < Convert.ToInt32(rel.Minimum))
                            {
                                response.AddError(
                                   string.Format(
                                        "Service relationship {0} for Service {1} does not meet the minimum of {2}.",
                                        pair.Key,
                                        serviceName, rel.Minimum), ValidationError.RELATIONSHIP, key.GetIdentifier(null), ValidationError.MINIMUM, pair.Value.Count + "<" + rel.Minimum);
                            }
                            if (key.Relationships[pair.Key].Count() > Convert.ToInt32(rel.Maximum) && Convert.ToInt32(rel.Maximum) != -1)
                            {
                                response.AddError(
                                    string.Format("Service relationship {0} for Service {1} exceeds the maximum of {2}.",
                                                  pair.Key, serviceName, rel.Maximum), ValidationError.RELATIONSHIP, key.GetIdentifier(null), ValidationError.MAXIMUM, pair.Value.Count + ">" + rel.Maximum, rel.Name);
                            }
                            foreach (var k in pair.Value.Where(k => !rel.Services.Contains(k.Id)))
                            {
                                response.AddError(
                                    string.Format(
                                        "Service relationship {0} for Service {1} does not support service of type {2}.",
                                        pair.Key, serviceName, ServiceDefinition.Get(k.Id).Name), ValidationError.RELATIONSHIP, k.GetIdentifier(null), ValidationError.INVALID_VALUE, k.Id.ToString(), rel.Name);
                            }
                        }
                    }
                }
                if (response.Errors.Count() > 0) response.IsValid = false;
            }
            catch (Exception e)
            {
                response.AddError(e.Message, ValidationError.SERVICE, key.GetIdentifier(null), ValidationError.MISSING_CONFIG, e.Message);
                response.IsValid = false;
                return response;
            }

            return response;
        }


        public ServiceRelationshipWeb ToWeb()
        {
            return new ServiceRelationshipWeb()
            {
                Name = Name,
                Services = Services.ToArray(),
                Minimum = Minimum != null ? Convert.ToInt64(Minimum) : Convert.ToInt64(0),
                Maximum = Maximum != null ? Convert.ToInt64(Maximum) : Convert.ToInt64(1),
                ReverseName = ReverseName.ReverseName,
                FilterRule = FilterRule
            };
        }

        public static List<ServiceRelationships> GetImpactedRelationhips(ServiceKey Key, List<string> changedAttributes)
        {
            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                var impactedServiceRelations = new List<ServiceRelationships>();
                List<ServiceRelationships> result = new List<ServiceRelationships>();
                var config = ServiceConfiguration.Get(Key.Id, Key.EffectiveDate);
                
                var qryStr = new StringBuilder();
                qryStr.Append(@"select distinct sr.* 
                        from FSC_DBA.service_attributes sa, FSC_DBA.service_relationship sr, FSC_DBA.service_configuration sc
                        where sa.attribute_type = 'Relation'                        
                        and :serviceId in (select trim(regexp_substr(allowed_services, '[^,]+', 1, LEVEL)) Sids from dual CONNECT BY regexp_substr(allowed_services, '[^,]+', 1, LEVEL) IS NOT NULL)
                        and sc.version = :scVersion
                        and sa.service_id = sc.service_id
                        and sa.version = sc.version
                        and sr.service_id = sc.service_id 
                        and sr.version = sc.version and substr(sa.default_value, instr(sa.default_value, '.') +1) in (");

                int i = 0;
                if (changedAttributes != null && changedAttributes.Count > 0)
                {
                    if (i >= 0)
                        foreach (var changedAttr in changedAttributes)
                        {
                            qryStr.Append("'" + changedAttr + "'" + ",");
                            i++;
                        }
                    qryStr.Remove(qryStr.Length - 1, 1);
                    qryStr.Append(")");
                    connection.Open();
                    var qry = connection.Query(qryStr.ToString(), new
                    {
                        serviceId = Key.Id,
                        scVersion = config.Version
                    }).AsList();
                    connection.Close();
                    if (qry != null && qry.Count > 0)
                    {
                        impactedServiceRelations = QueryToServiceRelationships(qry);
                    }
                }
                
                return impactedServiceRelations;
            }
        }
    }
}
