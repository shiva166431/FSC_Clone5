using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PCAT.Common.Parsers;
using PCAT.Common.Rules;
using ServiceCatalog.BL.Rules;

namespace ServiceCatalog.BL.Parsers
{
    class OffnetServiceRuleParser : ParserBase, IParser
    {
        public DateTime EffectiveDate { get; set; }
        public OffnetServiceRuleParser()
        {
            List<IRuleParser> rules = new List<IRuleParser>();
            rules.Add(new PartnerTagServiceRuleRP());
            rules.Add(new AddOffnetServiceRP());
            rules.Add(new OffnetValuesRuleRP());
            rules.Add(new OffnetConditionServiceRuleRP());//if 
           // rules.Add(new OffnetConditionalIncludeRuleRP());  // ConditionalIncludeRuleRP / includeif

            Initialize("Offnet Service", "Rules seperated by ;", rules, "-->", true);
        }

        private static OffnetServiceRuleParser _instance;
        public static List<OffnetServiceMappingRule> GetRules(string rules, DateTime effDate)
        {
            if (_instance == null)
                _instance = new OffnetServiceRuleParser();
            var ruleCollection = _instance.Parse(rules, effDate);
            return ruleCollection != null ? ruleCollection.Select(r => (OffnetServiceMappingRule)r).ToList() : new List<OffnetServiceMappingRule>();
        }

        public List<IRule> Parse(string ruleText, DateTime effectiveDate)
        {
            EffectiveDate = effectiveDate;
            return base.Parse(ruleText);
        }
        public override List<IRule> Parse(string ruleText)
        {
            EffectiveDate = DateTime.Today;
            return base.Parse(ruleText);
        }
    }

    #region PartnerTagServiceRuleParser
    public class PartnerTagServiceRuleRP : IRuleParser
    {
        public IRule TryParse(string ruleText, IParser parser)
        {
            if (ruleText.StartsWith("PartnerTag"))
            {
                var parameters = Parser.GetParameters(ruleText);
                if (parameters.Length != 1) throw new ApplicationException("GetPartnerTagRule expects 1 parameter as a list of rules.");
                return new PartnerTagServiceRule(parameters[0].Trim());
            }

            return null;
        }

        public RuleInfo GetRuleInfo()
        {
            var info = new RuleInfo()
            {
                Name = "partnerTag",
                KeyWord = "partnerTag"
            };

            info.Parameters.Add(new RuleParameterInfo() { Syntax = "partnerTag(<name>)" });

            return info;
        }
    }
    #endregion
    #region AddOffnetServiceRuleParse
    public class AddOffnetServiceRP : IRuleParser
    {
        public IRule TryParse(string ruleText, IParser parser)
        {
            if (ruleText.StartsWith("OffnetService"))
            {
                var parameters = Parser.GetParameters(ruleText);
                if (parameters.Length != 2 && parameters.Length != 4) throw new ApplicationException("GetServiceRule expects 2 or 4 parameters.");

                var serviceId = Convert.ToInt64(parameters[1]);
                var requireReplacement = false;
                var allowReplacement = false;
                if (parameters.Length == 4)
                {
                    allowReplacement = "true".Equals(parameters[2].Trim(), StringComparison.CurrentCultureIgnoreCase);
                    requireReplacement = "true".Equals(parameters[3].Trim(), StringComparison.CurrentCultureIgnoreCase);
                }
                //Need to fix the parameters 
                return new AddOffnetServiceRule(serviceId, ((OffnetServiceRuleParser)parser).EffectiveDate, parameters[0].Trim(), allowReplacement, requireReplacement);
            }

            return null;
        }

        public RuleInfo GetRuleInfo()
        {
            var info = new RuleInfo()
            {
                Name = "Service",
                KeyWord = "Service"
            };

            info.Parameters.Add(new RuleParameterInfo() { Syntax = "OffnetService(<tagName>,<serviceId>)" });
            info.Parameters.Add(new RuleParameterInfo() { Syntax = "OffnetService(<tagName>,<serviceId>,<allowReplacement>,<requireReplacement>)", Description = "allowReplacement and requireReplacement should be 'true' or 'false'" });

            return info;
        }
    }
    #endregion
    #region OffNetValuesRuleRP
    public class OffnetValuesRuleRP : IRuleParser
    {
        public IRule TryParse(string ruleText, IParser parser)
        {
            if (ruleText.StartsWith("values"))
            {
                var parameters = Parser.GetParameters(ruleText);
                if (parameters.Length < 4) throw new ApplicationException("ValuesRule expects 4 or 5 parameters.");
                if (parameters.Length > 5) throw new ApplicationException("ValuesRule expects 4 or 5 parameters.");

                var strip = false;
                if (parameters.Length == 5) strip = "true".Equals(parameters[4].Trim(), StringComparison.CurrentCultureIgnoreCase);

                return new OffnetValuesRule(parameters[0].Trim(), parameters[1].Trim(), parameters[2].Trim(), parameters[3].Trim(), strip);
            }

            return null;
        }

        public RuleInfo GetRuleInfo()
        {
            var info = new RuleInfo()
            {
                Name = "values",
                KeyWord = "values"
            };

            info.Parameters.Add(new RuleParameterInfo() { Syntax = "values(<partnerTag>,<partnerAttribute>,<serviceTag>,<serviceAttribute>)" });
            info.Parameters.Add(new RuleParameterInfo() { Syntax = "values(<partnerTag>,<partnerAttribute>,<serviceTag>,<serviceAttribute>,<strip>)", Description = "strip should be 'true' or 'false'" });

            return info;
        }
    }
    #endregion
    #region OffnetConditionServiceRuleRP
    public class OffnetConditionServiceRuleRP:IRuleParser
    {
        public IRule TryParse(string ruleText, IParser parser)
        {
            if (ruleText.StartsWith("if"))
            {
                var parameters = Parser.GetParameters(ruleText);
                if (parameters.Length != 3) throw new ApplicationException("GetConditionalRule expects 3 parameters.");

                SimpleRule cond = SimpleRuleParser.GetRule(parameters[1]);
                OffnetServiceMappingRule sr = (OffnetServiceMappingRule)parser.ParseLine(parameters[2]);
                return new OffnetConditionalServiceRule(parameters[0].Trim(), cond, sr);
            }

            return null;
        }

        public RuleInfo GetRuleInfo()
        {
            var info = new RuleInfo()
            {
                Name = "if",
                KeyWord = "if"
            };

            info.Parameters.Add(new RuleParameterInfo() { Syntax = "if(<partnerTag>,<simpleRule>,<serviceRule>)" });

            return info;
        }
    }
    #endregion
}
