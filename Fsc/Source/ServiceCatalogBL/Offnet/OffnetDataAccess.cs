using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using PCAT.Common.Models.Attribute;
using ServiceCatalog.BL.Models.Offnet;
using System.Data;
using PCAT.Common.Utilities;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;

namespace ServiceCatalog.BL.Offnet
{
    public class OffnetDataAccess
    {
        #region Partner
        public static PartnerAtrributes GetPartnerAttribuesByVendor(int vendorId, string vendorName, string source)
        {
            var result = new PartnerAtrributes();
            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();
                result = connection.QueryFirst<PartnerAtrributes>($"select vendor_id VendorId, vendor_name Name, request_type Request, partner_order_format PartnerFormat, order_action Action, product_family Family, partner_order_id PartnerId, partner_order_name Partner, ordering_source Source, PREORDER_VALIDATION PreOrderValidation from " +
                    $"fsc_dba.offnet_partner_definition where vendor_id = :vendorId and Vendor_name = :vendorName and ordering_source = :source", new { vendorId, vendorName, source });
                connection.Close();
            }
            return result;
        }

        public static PartnerServiceConfig GetPartnerServiceConfig(int PartnerId)
        {
            var result = new PartnerServiceConfig();
            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();
                result = connection.QueryFirst<PartnerServiceConfig>($"select partner_order_id PartnerId, version Version, service_rule ServiceRule from " +
                    $"fsc_dba.offnet_service where partner_order_id = :PartnerId", new { PartnerId });
                connection.Close();
            }
            return result;
        }
        #endregion
        #region Offnet Service Definition
        public static OffnetServiceDefinition GetOffnetServiceDefinition(string id)
        {
            OffnetServiceDefinition def;
            
                using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
                {
                    connection.Open();

                    def = connection.QueryFirst<OffnetServiceDefinition>($"select offnet_Service_Id Id, Name, Category, Description," +
                        $" (select case when exists (select 1  from FSC_DBA.OFFNET_HIERARCHY where OFFNET_SERVICE_ID = :id) then 'true' else 'false' end from dual) as HasChildren" +
                        " from fsc_dba.offnet_Service_Definition where offnet_service_id = :id", new { id });
                }
            return def;
        }
        #endregion
        #region OffnetServiceConfig
        public static OffnetServiceConfiguration GetServiceConfig(long id, DateTime effectiveDate)
        {
            var result = new OffnetServiceConfiguration();
            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();
                result = connection.QueryFirst<OffnetServiceConfiguration>($"select OFFNET_SERVICE_ID Id, Version, FROM_EFF_DATE FromEffectiveDate, TO_EFF_DATE ToEffectiveDate," +
                              $"   DISPLAY_PATTERN DisplayPattern, SERVICE_LAYER_RULE ServiceLayerRule," +
                              $"   VALID_VALUE_RULE ValidValueRule" +
                               " from fsc_dba.offnet_service_configuration where OFFNET_SERVICE_ID = :id " +
                               "   and from_eff_date <= :effectiveDate " +
                               "   and (to_eff_date is null or to_eff_date > :effectiveDate) ", new { id, effectiveDate });
                connection.Close();
            }
            result.Attributes = GetServiceAttributes(id, effectiveDate);
            return result;
        }
        #endregion
        #region OffnetServiceAttributes
        public static Dictionary<string, AttributeDefinition> GetServiceAttributes(long id, DateTime effectiveDate)
        {
            var definitions = new Dictionary<string, AttributeDefinition>();

            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();

                var attributes = connection.Query(
                    "select sa.* " +
                    "   from fsc_dba.offnet_service_attributes sa " +
                    "      inner join fsc_dba.offnet_service_configuration sc on sa.offnet_service_id = sc.offnet_service_id and sa.version = sc.version" +
                    "   where sc.offnet_service_id = :id and (sc.from_eff_date <= :effectiveDate and (sc.to_eff_date is null or sc.to_eff_date > :effectiveDate)) " +
                    "union " +
                    "select sa.* " +
                    "   from fsc_dba.offnet_service_attributes sa " +
                    "      inner join fsc_dba.offnet_inheritance si on sa.offnet_service_id = si.inherited_service_id" +
                    "      inner join fsc_dba.offnet_service_configuration sc on sc.offnet_service_id = si.inherited_service_id and sa.version = sc.version" +
                    "   where si.offnet_service_id = :id and (sc.from_eff_date <= :effectiveDate and (sc.to_eff_date is null or sc.to_eff_date > :effectiveDate)) ", new { id, effectiveDate });

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
                            ProvisioningImpact = SimpleRuleParser.GetRule(Utility.ToString(attribute.PROVISIONING_IMPACT))
                        });
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"Error loading attribute {attribute.NAME} for service {id}: {e.Message}", e);
                    }
                    connection.Close();
                }
            }
            return definitions;
        }
        #endregion
        #region OffnetServiceHierarchy
        public static List<OffnetServiceHierarchy> GetOffnetChildren(OffnetServiceKey key, string name = null)
        {
            List<OffnetServiceHierarchy> children = new List<OffnetServiceHierarchy>();

            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();

                children = connection.Query<OffnetServiceHierarchy>(@"
                    select Child_Service_Id Id, o.Name, Min_Quantity MinQuantityRule, Max_Quantity MaxQuantityRule
                    from fsc_dba.Offnet_Hierarchy o
                    inner join fsc_dba.Offnet_Service_Configuration sc on o.Child_Service_Id = sc.Offnet_Service_Id and (sc.from_eff_date <= :effectiveDate and (sc.to_eff_date is null or sc.to_eff_date > :effectiveDate))
                    where o.Offnet_Service_Id = :id", new
                {
                    id = key.Id,
                    effectiveDate = key.EffectiveDate
                }).AsList();
            }
            return children;
        }
        #endregion
        #region OffnetAttributeSource
        public static List<OffnetAttributeSource> GetOffnetAttributeSource(OffnetServiceKey key)
        {
            List<OffnetAttributeSource> sources = new List<OffnetAttributeSource>();

            using (var connection = new OracleConnection(FscApplication.Current.Settings.FscConnectionString))
            {
                connection.Open();

                sources = connection.Query<OffnetAttributeSource>(@"
                    select s.OFFNET_SERVICE_ID OffnetServiceId, s.VERSION Version, s.PARTNER_ORDER_ID PartnerOrderId, s.ATTRIBUTE_NAME AttributeName, s.REQUEST_SOURCE RequestSource, s.ATTRIBUTE_SOURCE AttributeSource, s.SOLUTION_TARGET SolutionTarget
                    from fsc_dba.OFFNET_ATTRIBUTE_SOURCE s
                    inner join fsc_dba.Offnet_Service_Configuration sc on s.OFFNET_SERVICE_ID = sc.Offnet_Service_Id and (sc.from_eff_date <= :effectiveDate and (sc.to_eff_date is null or sc.to_eff_date > :effectiveDate))
                    where s.OFFNET_SERVICE_ID = :id", new
                {
                    id = key.Id,
                    effectiveDate = key.EffectiveDate
                }).AsList();
            }
            return sources;
        }
        #endregion
    }
}
