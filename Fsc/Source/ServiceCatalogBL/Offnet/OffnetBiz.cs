using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PCAT.Common.Caching;
using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using ServiceCatalog.BL.Web.Offnet;
using ServiceCatalog.BL.Models.Offnet;
using ServiceCatalog.BL.Rules;

namespace ServiceCatalog.BL.Offnet
{
    public class OffnetBiz
    {
        #region ResolveService
        public static PartnerServicePackage ResolveOffnet(PartnerServicePackage oldPackage, out string error)
        {
            var NewPackage = new PartnerServicePackage();
            var partnerAttributes = new PartnerAtrributes();

            var _e = new Exception();
            error = null;
            var validInput = oldPackage.Partner.IsValidInput();
            if (oldPackage.Partner == null)
            {
                _e.Data.Add("Custom Msg", "Partner key is cannot be null");
                throw (_e);
            }
            try
            {
                //Fetch and populate Partner key if Partner does not exist 
                if (oldPackage.Partner.PartnerOrderId == 0 || String.IsNullOrEmpty(oldPackage.Partner.PartnerOrderName))
                {
                    if (!validInput.valid)
                    {
                        _e.Data.Add("Custom Msg", validInput.message);
                        throw (_e);
                    }
                    partnerAttributes.FromWeb(oldPackage.Partner);
                    NewPackage.Partner = partnerAttributes.Get(partnerAttributes.VendorId, partnerAttributes.Name, partnerAttributes.Source);
                }
                //Verify if all the partner attributes values are set correctly
                else if (!oldPackage.Partner.HasAllAttributes().valid)                       
                {
                    partnerAttributes.FromWeb(oldPackage.Partner);
                    NewPackage.Partner = partnerAttributes.Get(partnerAttributes.VendorId, partnerAttributes.Name, partnerAttributes.Source);
                }
                else
                {
                    NewPackage.Partner = oldPackage.Partner;
                }

                //Parse rules and get services and Populate the service key
                if (oldPackage.Services != null)
                {
                    {
                        //We need to make sure that each service attribute has its type assigned.
                        foreach (var sk in oldPackage.Services.Values)
                        {
                            var sc = GetServiceConfiguration(sk);
                            foreach (var name in sk.Values.Keys)
                            {
                                var a = sk.Values[name];
                                if (a.Type == null) a.Type = sc.GetAttributeType(name);
                            }
                        }

                        List<string> removedAttributes = new List<string>();
                        //It is possible that some service attributes have invalid values.  We need to remove their value if they are not valid.
                        foreach (var sk in oldPackage.Services.Values)
                        {
                            var listAttributes = OffnetServiceAttributes.Get(sk, true);
                            // GetAttributes(sk, ValidationStage.Design).Where(a => a.Type.Equals(AttributeType.List)).ToList();
                            foreach (var la in listAttributes)
                            {
                                var currentValue = sk.GetAttributeValue("la", SearchOptions.ALL_FALSE);
                                if (currentValue != null && !la.Values.Contains(new AttributeValue(currentValue)))
                                {
                                    sk.RemoveAttribute(la.Name);
                                    removedAttributes.Add(la.Name);
                                }
                            }

                            var names = listAttributes.Select(la => la.Name).ToList();
                            var currentListAttributes = sk.Values.Where(v => AttributeType.List.Equals(v.Value.Type)).Select(v => v.Key).ToList();
                            foreach (var name in currentListAttributes)
                            {
                                if (!names.Contains(name))
                                {
                                    sk.RemoveAttribute(name);
                                    removedAttributes.Add(name);
                                }
                            }

                            if (removedAttributes.Count > 0)
                            {
                                // add removed attributes back with default values
                                listAttributes = OffnetServiceAttributes.Get(sk, true);
                                listAttributes = listAttributes.Where(a => a.Type.Equals(AttributeType.List)).ToList();
                                //GetAttributes(sk, ValidationStage.Design).Where(a => a.Type.Equals(AttributeType.List)).ToList();
                                foreach (string name in removedAttributes)
                                {
                                    var la = (ListAttribute)listAttributes.FirstOrDefault(a => a.Name.Equals(name) && a.Type.Equals(AttributeType.List) && ((ListAttribute)a).GetList().Count == 1);
                                    if (la != null)
                                        sk.AddValue(name, la.GetAttributeValue());
                                }
                            }
                        }
                    }
                }
                //When No services in the initial call
                var set = new OffnetServiceRuleSet();
                set.ApplyRules(NewPackage);

                if (NewPackage.Services != null)
                {
                    foreach (var service in NewPackage.Services.Values)
                    {
                        var attributes = OffnetServiceAttributes.Get(service, false);
                        foreach (var la in from attribute in attributes
                                           where
                                               service.GetAttributeValue(attribute.Name, SearchOptions.ALL_FALSE) == null &&
                                               attribute.Type.Equals(AttributeType.List)
                                           select (ListAttribute)attribute
                            into la
                                           where la.GetList().Count == 1
                                           select la)
                        {
                            service.AddValue(la.Name, la.GetAttributeValue());
                        }
                    }
                }

                if (NewPackage.Services != null)
                {
                    //It is possible that the service keys have picked up some extra fields.  We need to remove them as they could cause problems with downstream applications
                    foreach (var sk in NewPackage.Services.Values)
                    {
                        var c = GetServiceConfiguration(sk);
                        var names = sk.Values.Keys.Where(a => !c.IsConfigurableAttribute(a, sk)).ToList();
                        foreach (var n in names) sk.RemoveAttribute(n);
                        // Make sure all the attributes have a type associated with them...
                        foreach (var name in sk.Values.Keys)
                        {
                            if (c.IsConfigurableAttribute(name, sk) && sk.Values[name].Type == null)
                                sk.Values[name].Type = c.GetAttributeType(name);
                        }
                    }

                    //It is possible that some service attributes have invalid values.  We need to remove their value if they are not valid.
                    foreach (var sk in oldPackage.Services.Values)
                    {
                        var listAttributes = OffnetServiceAttributes.Get(sk, false).Where(a => a.Type.Equals(AttributeType.List)).ToList();
                        //GetAttributes(sk, ValidationStage.Design).Where(a => a.Type.Equals(AttributeType.List)).ToList();
                        foreach (var la in listAttributes)
                        {
                            var currentValue = sk.GetAttributeValue(la.Name, SearchOptions.ALL_FALSE);
                            if (currentValue != null && !la.Values.Contains(new AttributeValue(currentValue)))
                                sk.RemoveAttribute(la.Name);
                        }
                    }
                }


            }
            catch (Exception e)
            {
                throw (e);
            }
            return NewPackage;
        }
        #endregion
        #region Partner 
        public static PartnerServiceConfig GetPartnerServiceConfig(PartnerKeyWeb key)
        {
            var config = new PartnerServiceConfig();
            return config.Get(key);
        }
        #endregion
        #region Service
        public static OffnetServiceConfiguration GetServiceConfiguration(long id, DateTime effectiveDate)
        {
            return OffnetServiceConfiguration.Get(id, effectiveDate);
        }
        public static OffnetServiceConfiguration GetServiceConfiguration(OffnetServiceKey key)
        {
            OffnetServiceConfiguration sc;
            sc = GetServiceConfiguration(key.Id, key.EffectiveDate);
            return sc;
        }
        #endregion
    }
}
