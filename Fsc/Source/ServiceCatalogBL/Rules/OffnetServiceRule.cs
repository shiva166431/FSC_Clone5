using System;
using System.Collections.Generic;
using System.Text;
using PCAT.Common.Rules;
using ServiceCatalog.BL.Offnet;
using ServiceCatalog.BL.Models.Offnet;
using PCAT.Common.Utilities;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Debugging;
using ServiceCatalog.BL.Web.Offnet;
using PCAT.Common.Models;

namespace ServiceCatalog.BL.Rules
{
    public abstract class OffnetServiceMappingRule : IRule
    {
        public abstract string CompareString();
        //public abstract int CompareOrder();
        public void Apply(PartnerServicePackage package, ResolveOffnetServiceSet rss)
        {
            Apply(package, "0", rss);
        }
        public abstract void Apply(PartnerServicePackage package, string interation, ResolveOffnetServiceSet rss);
        protected string Concat(string name, string iteration)
        {
            if (iteration == null || iteration.Equals("0")) return name;
            return name + "-" + iteration;
        }

        public IRule Clone()
        {
            return this;
        }

    }
    #region PartnerTagServiceRule
    public class PartnerTagServiceRule : OffnetServiceMappingRule
    {
        public PartnerTagServiceRule()
        {

        }

        public PartnerTagServiceRule(string name)
        {
            TagName = name;
        }

        public override string CompareString()
        {
            return "PartnerTag:" + TagName;
        }
        public string TagName
        {
            get;
            set;
        }
        public override void Apply(PartnerServicePackage package, string interation, ResolveOffnetServiceSet rss)
        {
            // This is basically applied when the rule is added to the rule set.
            return;
        }
        public override string ToString()
        {
            return "Partner Tag: [" + TagName + "]";
        }
    }
    #endregion
    #region AddOffNetServiceRule
    public class AddOffnetServiceRule : OffnetServiceMappingRule
    {

        private readonly string _tagName;
        private readonly OffnetServiceKey _key;

        public AddOffnetServiceRule(long serviceId, DateTime effDate, string tagName, bool allowReplacement, bool requireReplacement)
        {
            _tagName = tagName;
            _key = new OffnetServiceKey(serviceId) { EffectiveDate = effDate };
            var sc = OffnetBiz.GetServiceConfiguration(serviceId, effDate);
            //_key.AllowSharedStandalone = sc.AllowSharedStandalone;
        }
        public override void Apply(PartnerServicePackage package, string iteration, ResolveOffnetServiceSet rss)
        {
            //See if tag already exists...
            var sk = rss.FetchAndRemoveExistingService(rss.GetTagName(_tagName));
            if (sk == null) sk = rss.FetchAndRemoveExistingService(_tagName);
            if (sk == null) sk = _key;
            if (!sk.Id.Equals(_key.Id))
            {
                sk.Id = _key.Id;
                // sk.Activity = ActivityType.Modify;
                sk.EffectiveDate = DateTime.Now;
            }
            package.AddService(rss.GetTagName(_tagName), sk);
        }
        public override string CompareString()
        {
            return string.Format("AddService: {0}", _tagName);
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Add OffnnetService [");
            sb.Append(_tagName);
            sb.Append("] of type [");
            sb.Append(_key);

            return sb.ToString();
        }
    }
    #endregion
    #region OffnetValues
    public class OffnetValuesRule : OffnetServiceMappingRule
    {
        public const string CONSTANT = "constant";
        protected string _pTag;
        protected string _sTag;
        protected string _pAtt;
        protected string _sAttName;
        protected bool _strip;

        public OffnetValuesRule()
        {
        }
        public OffnetValuesRule(string partnerTag, string partnerAttribute, string serviceTag, string serviceAttribute, bool strip)
        {
            _pTag = partnerTag;
            _pAtt = partnerAttribute;
            _sTag = serviceTag;
            _sAttName = serviceAttribute;
            _strip = strip;
        }
        public override void Apply(PartnerServicePackage package, string iteration, ResolveOffnetServiceSet rss)
        {
            var sk = package.GetService(rss.GetTagName(Concat(_sTag, iteration)));
            if (sk == null)
            {
                Debugger.Log(new PcatExceptionInfo($"Service {_sTag} not found!") { StackTrace = this.ToString() });
                return;
            }

            AttributeValue value;
            PartnerKeyWeb key;
            var patts = new PartnerAtrributes();
            patts.FromWeb(package.Partner);
            StringBuilder debugDetail = new StringBuilder();
            if (_pTag == CONSTANT)
            {
                value = CreateAttribute(sk, _pAtt);
                key = package.Partner;

                debugDetail.Append("Constant");
            }
            else
            {
                key = rss.GetPartner(_pTag);
                var av = patts.GetAttributeValue(_pAtt);
                if (av == null)
                {
                    // if the product doesn't have a value, let's remove it from the service in case an old value is sitting there
                    sk.RemoveAttribute(_sAttName);
                    Debugger.Log(new DebugAttributeInfo(sk?.Id * -1, _sAttName, null, DebugAttributeInfoSourceType.Rule, DebugAttributeInfoFunctionType.GetValue, null)
                    { RuleText = $"Removed because {_pTag}.{_pAtt} doesn't exist or is null" });
                    return;
                }
                value = CreateAttribute(sk, av);

                debugDetail.Append($"Copy from {_pTag}.{_pTag}");
            }
            if (value != null)
            {
                var oldValue = sk.GetValue(_sAttName, SearchOptions.ALL_TRUE);
                var changed = (oldValue != null) ? false : AttributeChanged(oldValue, value);
                if (_strip)
                {
                    //We need to remove everything but the number from the value before we use it...
                    value.Value = ParseNumber(value.Value);
                    debugDetail.Append(", value stripped to just a number");
                }
            }
            else
                debugDetail.Append($", {_sTag}.{_sAttName} is not applicable");

            sk.AddValue(_sAttName, value);
            Debugger.Log(new DebugAttributeInfo(sk?.Id + 50000, _sAttName, value?.Value, DebugAttributeInfoSourceType.Rule, DebugAttributeInfoFunctionType.GetValue, null)
            { RuleText = debugDetail.ToString() });
        }

        public override string CompareString()
        {
            return string.Format("SetAttribute: {0}.{1}", _sTag, _sAttName);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Set Service Attribute [");
            sb.Append(_sAttName);
            sb.Append(" on service = [");
            sb.Append(_sTag);
            if (_pTag.Equals(CONSTANT))
            {
                sb.Append(" with [");
                sb.Append(_pAtt);
            }
            else
            {
                sb.Append("] using Product Attribute [");
                sb.Append(_pAtt);
                sb.Append("] and product key = [");
                sb.Append(_pTag);
            }
            sb.Append("]");

            return sb.ToString();
        }
        protected string ParseNumber(string value)
        {
            double? numericValue;
            try
            {
                numericValue = Convert.ToDouble(value);
            }
            catch (FormatException)
            {
                if (value.EndsWith("%"))
                {
                    //Percentage value
                    var valStr = value.Substring(0, value.Length - 1);
                    numericValue = Convert.ToDouble(Double.Parse(valStr) / 100.0);
                }
                else if (value.StartsWith("$"))
                {
                    //Dollar value...  Drop the dollar sign
                    var valStr = value.Substring(1);
                    numericValue = Double.Parse(valStr);
                }
                else if (value.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    numericValue = 1;
                }
                else if (value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                {
                    numericValue = 0;
                }
                else if (value.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
                {
                    numericValue = 1;
                }
                else if (value.Equals("no", StringComparison.CurrentCultureIgnoreCase))
                {
                    numericValue = 0;
                }
                else if (value.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    numericValue = 1;
                }
                else if (value.Equals("n", StringComparison.CurrentCultureIgnoreCase))
                {
                    numericValue = 0;
                }
                else
                {
                    //Generic value
                    var sb = new StringBuilder();
                    var chars = value.ToCharArray();
                    foreach (var c in chars)
                    {
                        if (Char.IsDigit(c) || c == '.') sb.Append(c);
                        else break;
                    }
                    numericValue = Convert.ToDouble(sb.ToString());

                }
            }
            return numericValue.ToString();
        }

        protected bool AttributeChanged(AttributeValue oldValue, AttributeValue newValue)
        {
            return oldValue == null || !oldValue.Equals(newValue);
        }
        protected AttributeValue CreateAttribute(OffnetServiceKey sk, string value)
        {
            //Need to determine type to use...
            try
            {
                var sc = OffnetBiz.GetServiceConfiguration(sk.Id, sk.EffectiveDate);
                if (!sc.IsConfigurableAttribute(_sAttName, sk)) return null;
                return new AttributeValue(value,
                                          value,
                                          AttributeType.SimpleText);
                //Alaw simple text type for partner attributes
            }
            catch (Exception e)
            {
                Debugger.Log(new PcatExceptionInfo(this.ToString(), e));

                throw new ApplicationException("Resolve Services failed.  Unable to get ServiceAttribute type for " + _sAttName + " on Service " + sk?.Id);
            }

        }
    }
    #endregion
    #region OffnetConditionalServiceRule
    public class OffnetConditionalServiceRule : OffnetServiceMappingRule
    {
        private readonly OffnetServiceMappingRule _rule;
        private readonly string _partnerTag;
        private readonly SimpleRule _condition;

        public OffnetConditionalServiceRule(string partnerTag, SimpleRule condition, OffnetServiceMappingRule r)
        {
            _partnerTag = partnerTag;
            _condition = condition;
            _rule = r;
        }

        public override void Apply(PartnerServicePackage package, string iteration, ResolveOffnetServiceSet rss)
        {
           _rule.Apply(package, iteration, rss);
        }
        public override string CompareString()
        {
            return string.Format("Condition({0}): {1}", _condition, _rule.CompareString());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Conditional Service Rule: if (");
            sb.Append(_condition); sb.Append(") then (");
            sb.Append(_rule);
            sb.Append(") on PartnerTag(");
            sb.Append(_partnerTag);
            sb.Append(")");

            return sb.ToString();
        }
    }
    #endregion
}
