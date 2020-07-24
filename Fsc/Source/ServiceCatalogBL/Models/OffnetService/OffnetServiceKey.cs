using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PCAT.Common.Biz;
using PCAT.Common.Rules;
using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using ServiceCatalog.BL.Web.Offnet;
using System.Linq;
using PCAT.Common;

namespace ServiceCatalog.BL.Models.Offnet
{
    [Serializable]
    [DataContract]
    public class OffnetServiceKey : IValueHolder, IKey
    {
        #region Fields
        [DataMember] public long Id { get; set; }
        [DataMember] public Dictionary<string, AttributeValue> Values;
        [DataMember] public Dictionary<string, string> OtherProperties;
        [DataMember] public string TemporaryIdentifier;
        [DataMember] public long ServiceInstanceId;
        [DataMember] public Dictionary<string, List<OffnetServiceKey>> Children;
        [DataMember] public long TempChildInstance { get; set; }
        #endregion
        #region EffectiveDate
        private DateTime _effDate;
        [DataMember]
        public DateTime EffectiveDate
        {
            get
            {
                return _effDate == DateTime.MinValue ? DateTime.Now : _effDate;
            }
            set { _effDate = value; }
        }
        #endregion
        #region ParentServiceKey
        private OffnetServiceKey _parentServiceKey;
        [DataMember]
        public OffnetServiceKey ParentServiceKey
        {
            get { return _parentServiceKey; }
            set
            {
                _parentServiceKey = value;
                if (value != null)
                    AddAttribute("ParentID", value.Id.ToString());
            }
        }
        #endregion

        #region Constructors
        public OffnetServiceKey() { }
        public OffnetServiceKey(long serviceId)
        {
            Id = serviceId;
            Values = new Dictionary<string, AttributeValue>();
            Children = new Dictionary<string, List<OffnetServiceKey>>();
        }

        public OffnetServiceKey(OffnetServiceKeyWeb web)
            : this(web.ServiceID)
        {

            if (web.Attributes != null)
            {
                foreach (var ai in web.Attributes)
                {
                    if (ai.Value != null)
                    {
                        var a = new AttributeValue(ai.Value.Value, ai.Value.DisplayValue, new AttributeType(ai.Type));
                        AddValue(ai.Name, a);
                    }
                }

            }

            if (web.Date != null) EffectiveDate = (DateTime)web.Date;
            if (web.ParentServiceKey != null) ParentServiceKey = new OffnetServiceKey(web.ParentServiceKey);

            if (web.ChildServices != null || web.ChildServices.Count() > 0)
            {
                foreach (var child in web.ChildServices)
                {
                    Children.Add(child.Name, child.Services.Select(s => new OffnetServiceKey(s)).ToList());
                }
            }
        }
        #endregion


        #region Add Attribute/Value
        public void AddAttribute(string name, string value)
        {
            if (name == null || value == null) return;
            AddValue(name, new AttributeValue(value, AttributeType.Unknown));
        }
        public void AddValue(string name, string value, AttributeType type)
        {
            if (Values == null) Values = new Dictionary<string, AttributeValue>();
            if (value == null && Values.ContainsKey(name)) Values.Remove(name);
            else Values[name] = new AttributeValue(value, type);
        }
        public void AddValue(string name, AttributeValue value)
        {
            if (Values == null) Values = new Dictionary<string, AttributeValue>();
            if ((value == null || value.Value == null) && Values.ContainsKey(name)) Values.Remove(name);
            else if (value != null && value.Value != null) Values[name] = value;
        }
        #endregion
        #region Get Values
        public string GetAttributeValue(string name, SearchOptions searchOptions)
        {
            try
            {
                AttributeValue v = GetValue(name, searchOptions);
                return (v == null) ? null : v.Value;
            }
            catch (Exception)
            {
                throw new ApplicationException("Unable to get attribute value for " + name);
            }
        }
        public AttributeValue GetValue(string name, SearchOptions searchOptions)
        {
            if (Values == null) Values = new Dictionary<string, AttributeValue>();
            AttributeValue value;

            if (name.StartsWith("Child["))
            {
                var nameSplit = name.Split('.');
                var childName = nameSplit[0].Substring(6, nameSplit[0].Length - 7);
                Children.TryGetValue(childName, out var childKeys);
                if (childKeys == null || childKeys.Count == 0)
                    value = new AttributeValue(null, AttributeType.Unknown);
                else if (childKeys.Count == 1)
                {
                    if (nameSplit.Length == 1)
                        value = new AttributeValue(childKeys[0].TemporaryIdentifier, AttributeType.SimpleText);
                    else
                    {
                        var attName = nameSplit[1];
                        value = new AttributeValue(childKeys[0].GetAttributeValue(attName, searchOptions), AttributeType.Unknown);
                    }
                }
                else
                    value = new AttributeValue(null, AttributeType.Unknown);
            }
            else if (name.StartsWith("Parent."))
            {
                if (ParentServiceKey != null)
                    value = new AttributeValue(ParentServiceKey.GetAttributeValue(name.Substring(7), searchOptions), AttributeType.Unknown);
                else
                    value = new AttributeValue(null, AttributeType.Unknown);
            }
            else
                Values.TryGetValue(name.Trim(), out value);

            return value;
        }
        #endregion
        #region GetNumericValue
        public double? GetNumericValue(string name, SearchOptions searchOptions)
        {
            var v = new RuleValue(GetValue(name, searchOptions));
            double? nv;
            v.TryGetNumericValue(out nv);
            return nv;
        }
        #endregion
        #region HasAttribute
        public bool HasAttribute(string name, bool checkParent = true)
        {
            if (Values.ContainsKey(name))
                return true;

            return checkParent && ParentServiceKey != null && ParentServiceKey.HasAttribute(name);
        }
        #endregion
        #region Remove Attribute
        public void RemoveAttribute(string name)
        {
            if (Values.ContainsKey(name)) Values.Remove(name);
        }
        #endregion

        #region TryGetValue
        public bool TryGetValue(string name, SearchOptions searchOptions, out RuleValue value, out string error)
        {
            error = "";
            try
            {
                value = new RuleValue(GetAttributeValue(name, searchOptions));
            }
            catch (Exception e)
            {
                value = null;
                error = e.Message;
                return false;
            }
            return true;
        }
        #endregion


        #region Clone
        public OffnetServiceKey Clone(bool downTree)
        {
            var key = new OffnetServiceKey(Id);
            foreach (var pair in Values)
                key.AddValue(pair.Key, pair.Value);
            key.EffectiveDate = EffectiveDate;
            key.ParentServiceKey = ((ParentServiceKey != null && !downTree) ? ParentServiceKey.Clone(false) : ParentServiceKey);

            if (downTree)
            {
                foreach (var child in Children)
                    key.Children.Add(child.Key, child.Value.Select(v => v.Clone(true)).ToList());
            }

            return key;
        }
        #endregion

        public DateTime GetEffectiveDate()
        {
            return _effDate;
        }

        public List<IValueHolder> GetValueHolderList(string collectionName)
        {
            throw new ApplicationException("This value holder type does not support this method. Cannot use these kind of rules with this value holder.");
        }
        public IKey GetKey(string entityName)
        {
            return null;
        }
        public ICollection<string> GetAttributesFor(string label)
        {
            return new List<string>();
        }

        #region ToString
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ServiceKey(");
            sb.Append(Id);
            sb.Append(")\n\r");
            if (Values != null && Values.Count > 0)
            {
                sb.Append("Values");
                foreach (var pair in Values)
                {
                    sb.Append("\t");
                    sb.Append(pair.Key); sb.Append("="); sb.Append(pair.Value);
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }
        #endregion
        #region ToValueHolder
        public ValueHolder ToValueHolder()
        {
            var vh = new ValueHolder();

            foreach (var value in Values)
                vh.AddAttribute(value.Key, value.Value.Value);

            return vh;
        }
        #endregion
        #region ToWeb
        public OffnetServiceKeyWeb ToWeb()
        {
            var web = new OffnetServiceKeyWeb();
            int i;

            web.IsValid = true;
            web.ServiceID = Id;
            web.Date = EffectiveDate;

            if (ParentServiceKey != null)
                web.ParentServiceKey = ParentServiceKey.ToWeb();
            if (Values != null)
            {
                web.AttributesList = new Dictionary<string, string>();
                web.Attributes = new OffnetAttrInstanceWeb[Values.Count];          
                i = 0;
                foreach (var pair in Values)
                {
                    var a = pair.Value;
                    web.AttributesList.Add(pair.Key, a.Value);
                    web.Attributes[i++] = new OffnetAttrInstanceWeb(pair.Key, a.Value, a.DisplayValue, a.Type);
                   
                }
            }
            web.ChildServices = new OffnetServiceCollectionWeb[Children.Count];
            i = 0;
            foreach (var child in Children)
                web.ChildServices[i++] = new OffnetServiceCollectionWeb(child.Key, child.Value.Select(c => c.ToWeb()).ToArray());
            i = 0;
            return web;
        }
        #endregion

        #region Add/Get Address
        public void AddAddress(String name, GlmAddress address)
        {

        }
        public GlmAddress GetAddress(string name)
        {
            return null;
        }

        public IDictionary<string, GlmAddress> GetAddresses()
        {
            return new Dictionary<string, GlmAddress>();
        }
        #endregion

        public string GetIdentifier(string[] names)
        {
            return TemporaryIdentifier ?? ServiceInstanceId.ToString();
        }

        #region Missing Attributes
        //Extensions
        public void AddMissingAttributes()
        {
            AddMissingParentAttributes(this);
            AddMissingRelatedAttributes(this);
            AddMissingDefaultAttributes(this);
        }
        public static void AddMissingDefaultAttributes(OffnetServiceKey key)
        {
            var childConfig = OffnetServiceConfiguration.Get(key.Id, key.EffectiveDate);
            string value;
            foreach (var attribute in childConfig.Attributes)
            {
                //if (attribute.Value.Type != AttributeType.List
                //    && key.GetAttributeValue(attribute.Value.Name, SearchOptions.ALL_FALSE) == null
                //    && !childConfig.IsOptional(attribute.Value.Name, key))
                //{
                value = childConfig.GetDefaultValue(attribute.Value.Name, key);
                if (value != null)
                    key.AddAttribute(attribute.Value.Name, value);
                //}
            }
        }

        public static void AddMissingParentAttributes(OffnetServiceKey key)
        {
            if (key.ParentServiceKey != null)
            {
                key.ParentServiceKey.AddMissingAttributes();

                var childConfig = OffnetServiceConfiguration.Get(key.Id, key.EffectiveDate);
                var parentConfig = OffnetServiceConfiguration.Get(key.ParentServiceKey.Id, key.ParentServiceKey.EffectiveDate);
                string value;
                foreach (var attribute in childConfig.Attributes)
                {
                    if (attribute.Value.Type == AttributeType.Parent)
                    {
                        value = key.ParentServiceKey.GetAttributeValue(attribute.Value.Name, SearchOptions.ALL_TRUE);
                        if (value == null)
                            value = parentConfig.GetDefaultValue(attribute.Value.Name, key.ParentServiceKey);
                        if (value != null)
                            key.AddAttribute(attribute.Value.Name, value);
                    }
                }
            }
        }

        public static void AddMissingRelatedAttributes(OffnetServiceKey key)
        {
            //Relationships if needed
        }

        public void SetAttributeValue(OffnetServiceKey key, long id)
        {
            var childConfig = OffnetServiceConfiguration.Get(key.Id, key.EffectiveDate);
            string value;
            foreach (var attribute in childConfig.Attributes)
            {
                if (attribute.Value.Name.Equals("UNI Reference ID"))
                {
                    value = id.ToString();
                    key.AddAttribute(attribute.Value.Name, value);
                }
            }

        }

        #endregion
    }
}
