using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAT.Common.Models.Debugging;
using PCAT.Common.Web.Models.Attribute;
using PCAT.Common.Web.Models.Service;
using ServiceCatalog.BL;
using ServiceCatalog.BL.Models;
using ServiceCatalog.BL.Models.OffnetService;
using ServiceCatalog.BL.Offnet;
using ServiceCatalog.BL.Utilities;
using ServiceCatalog.BL.Web.Offnet;
using ServiceCatalog.WepApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static ServiceCatalog.WepApi.Models.PingModels;

#region summary
/*
 * This test method is used to test the medioation links for fsc mediation
 * All the end points for pom and other regular services are tested based on the assumptions
   made in the methods
*/
#endregion
namespace ServiceCatalogMsTest.Mediation
{
    
    [TestClass]
    public class MediationEndpointTest
    {
        [TestMethod]
        public void Ping()
        {
            Startup.LoadSettings();
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.ping;
            var ping = ApiClient.Get<PingResponse>(HttpTargetType.FSCMediation, "FSC Mediation Ping", url, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(ping);
            Assert.AreEqual<string>(ping.Ping.Status, "OK");
        }
        [TestMethod]
        public void PingConfig()
        {
            Startup.LoadSettings();
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.pingConfig;
            var ping = ApiClient.Get<PingConfig>(HttpTargetType.FSCMediation, "FSC Mediation Config", url, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(ping);
            Assert.AreEqual<string>(ping.Ping.Status, "OK");
        }
     #region POM   
        [TestMethod]
        public void PomResolveServices()
        {
            //Making a call to get service for vendor at&t and vendor id 1
            Startup.LoadSettings();
            var Package = new PartnerServPkg();
            var partner = new PartnerKeyWeb();
            Package.EffectiveDate = DateTime.Now;
            Package.IsValid = true;
            partner.VendorId = 1;
            partner.VendorName = "AT&T";
            partner.PartnerOrderId = 0;
            partner.Source = "CPO,SWIFT";
            Package.Partner = partner;
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.pomResolveServices;
            var pkg = ApiClient.Post<PartnerServPkg, PartnerServPkg>(HttpTargetType.FSCMediation, "FSC Mediation Pom Resolve", url, Package,ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(pkg);
            Assert.IsNull(pkg.ErrorString);
        }
        [TestMethod]
        public void PomDefinition()
        {
            //Making a call to get service def id 1
            Startup.LoadSettings();
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.pomServiceDef+"/1";
            var def = ApiClient.Get<OffnetServiceDefinitionWeb>(HttpTargetType.FSCMediation, "FSC Mediation Pom Service Def", url, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(def);
            Assert.AreEqual(def.ServiceId,1);
        }
        [TestMethod]
        public void PomAttributes()
        {
            Startup.LoadSettings();
            var key = new OffnetServiceAttributesKeyWeb();
            key.PopulateLists = true;
            key.ServiceID = 1;
            key.Attributes = new OffnetAttrInstanceWeb[0];
            key.ChildServices = new OffnetServiceCollectionWeb[0];
            key.Date = DateTime.Now;
            key.IsValid = true;
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.pomServiceAttrs;
            var attrs = ApiClient.Post<OffnetAttributeSourcesInfoWeb,OffnetServiceAttributesKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Pom Serv Attributes", url,key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(attrs);
            Assert.IsTrue(attrs.Attributes.Length > 0);
            Assert.IsNull(attrs.ErrorString);
        }
        [TestMethod]
        public void PomBuildChild()
        {
            //Assumes that service Id 3 has children
            Startup.LoadSettings();
            var key = new BuildOffnetChildServiceRequestWeb();
            var parent = new OffnetServiceKeyWeb();
            parent.ServiceID = 3;
            parent.Date = DateTime.Now;
            parent.Attributes = new OffnetAttrInstanceWeb[0];
            parent.ChildServices = new OffnetServiceCollectionWeb[0];
            parent.IsValid = true;
            key.ParentService = parent;
            key.Name = "ASOG_UNI";
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.pomBuildChild;
            var hierarchy = ApiClient.Post<OffnetHierarchyWeb, BuildOffnetChildServiceRequestWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Pom Serv BuildChild", url, key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(hierarchy);
            Assert.IsTrue(hierarchy.Services.Length > 0);
        }
        [TestMethod]
        public void PomChildren()
        {
            Startup.LoadSettings();
            var key = new OffnetServiceKeyWeb();
            key.ServiceID = 3;
            key.Date = DateTime.Now;
            key.Attributes = new OffnetAttrInstanceWeb[0];
            key.ChildServices = new OffnetServiceCollectionWeb[0];
            key.IsValid = true;
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.pomChildren;
            var children = ApiClient.Post<List<OffnetServiceChildWeb>,OffnetServiceKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Pom Serv Children", url, key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(children);
            Assert.IsTrue(children.Count > 0);
        }
        #endregion
     #region Service
        [TestMethod]
        public void ServiceDefinition()
        {
            //Making a call to get service def id 212
            Startup.LoadSettings();
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceDef + "/212";
            var def = ApiClient.Get<ServiceDefinitionWeb>(HttpTargetType.FSCMediation, "FSC Mediation Service Def", url, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(def);
            Assert.AreEqual(def.ServiceId, 212);
        }
        [TestMethod]
        public void ServiceAttributes()
        {
            Startup.LoadSettings();
            var key = new ServiceAttributesKeyWeb();
            key.PopulateLists = true;
            key.ServiceID = 215;
            key.Attributes = new AttributeInstanceWeb[0];
            key.ChildServices = new ServiceNamedCollectionWeb[0];
            key.Date = DateTime.Now;
            key.IsValid = true;
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceAttrs;
            var attrs = ApiClient.Post<AttributeInfoWeb[], ServiceAttributesKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Serv Attributes", url, key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(attrs);
            Assert.IsTrue(attrs.Length > 0);
        }
        [TestMethod]
        public void ServiceBuildChild()
        {
            //Assumes that service Id 227 has children tha are available when product id is 570
            Startup.LoadSettings();
            var key = new BuildChildServiceRequestWeb();
            var parent = new ServiceKeyWeb();
            parent.ServiceID = 227;
            parent.Date = DateTime.Now;
            parent.Attributes = new AttributeInstanceWeb[1];            
            var attr = new AttributeInstanceWeb();
            attr.Name = "PCATProductID";
            attr.Type = "Text";
            var value = new AttributeChoiceWeb();
            value.Value = "570";
            attr.Value = value;
            parent.Attributes[0] = attr;
            parent.ChildServices = new ServiceNamedCollectionWeb[0];
            parent.IsValid = true;
            key.ParentService = parent;
            key.Name = "LAN PORT SERVICE";
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceBuildChild;
            var hierarchy = ApiClient.Post<ServiceKeyWeb,BuildChildServiceRequestWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Serv Build child", url, key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(hierarchy);
            Assert.IsTrue(hierarchy.Attributes.Length > 0);
        }
        [TestMethod]
        public void ServiceChildren()
        {
            //Assumes that service 217 has children
            Startup.LoadSettings();
            var key = new ServiceKeyWeb();
            key.ServiceID = 217;
            key.Date = DateTime.Now;
            key.Attributes = new AttributeInstanceWeb[0];
            key.ChildServices = new ServiceNamedCollectionWeb[0];
            key.IsValid = true;
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceChildren;
            var children = ApiClient.Post<List<ServiceChildWeb>, ServiceKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Service children", url, key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(children);
            Assert.IsTrue(children.Count > 0);
        }

        [TestMethod]
        public void ServiceRelationships()
        {
            //Assumes that service 217 has an no active relationships
            Startup.LoadSettings();
            var key = new ServiceKeyWeb();
            key.ServiceID = 217;
            key.Date = DateTime.Now;
            key.Attributes = new AttributeInstanceWeb[0];
            key.ChildServices = new ServiceNamedCollectionWeb[0];
            key.IsValid = true;
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceRelationships;
            var relations = ApiClient.Post<ServiceRelationshipWeb[], ServiceKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Service Realtionships", url, key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(relations);
            Assert.IsTrue(relations.Length == 0);
        }

        [TestMethod]
        public void ServiceImpactedChildren()
        {
            //Assumes that service 217 has an active relationship
            Startup.LoadSettings();
            var Impact = new ImpactedServiceKeyWeb();
            var key = new ServiceKeyWeb();
            key.ServiceID = 217;
            key.Date = DateTime.Now;
            key.Attributes = new AttributeInstanceWeb[1];
            key.ChildServices = new ServiceNamedCollectionWeb[0];
            key.IsValid = true;
            var attr = new AttributeInstanceWeb();
            attr.Name = "CoS Allocation Type";
            attr.Type = "Text";
            var value = new AttributeChoiceWeb();
            value.Value = "Multiple COS";
            attr.Value = value;
            key.Attributes[0] = attr;
            Impact.Key = key;
            Impact.ChangedAttributes = new List<string> { "Peak Information Rate", "eBGP Remote Peer ASN" };
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceImpactedChildren;
            var Imp = ApiClient.Post<List<long>, ImpactedServiceKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Service Impacted children", url, Impact, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(Imp);
            Assert.IsTrue(Imp.Count > 0);
        }

        [TestMethod]
        public void ServiceImpactedRelationships()
        {
            //Assumes that service 217 has no impacted relationships
            Startup.LoadSettings();
            var Impact = new ImpactedServiceKeyWeb();
            var key = new ServiceKeyWeb();
            key.ServiceID = 217;
            key.Date = DateTime.Now;
            key.Attributes = new AttributeInstanceWeb[0];
            key.ChildServices = new ServiceNamedCollectionWeb[0];
            key.IsValid = true;
            Impact.Key = key;
            Impact.ChangedAttributes = new List<string> { "Peak Information Rate", "eBGP Remote Peer ASN" };
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceImpactedRelationships;
            var Imp = ApiClient.Post<ServiceRelationshipWeb[], ImpactedServiceKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Service Impacted Relationships", url, Impact, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(Imp);
            Assert.IsTrue(Imp.Length == 0);
        }
        [TestMethod]
        public void ServiceImpacts()
        {
            //Assumes that service 217 has no impacts
            Startup.LoadSettings();
            var Impact = new ImpactedServiceKeyWeb();
            var key = new ServiceKeyWeb();
            key.ServiceID = 217;
            key.Date = DateTime.Now;
            key.Attributes = new AttributeInstanceWeb[0];
            key.ChildServices = new ServiceNamedCollectionWeb[0];
            key.IsValid = true;
            Impact.Key = key;
            Impact.ChangedAttributes = new List<string> { "Peak Information Rate", "eBGP Remote Peer ASN" };
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceImpacts;
            var Imp = ApiClient.Post<ImpactAttributes, ImpactedServiceKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Service Impacts", url, Impact, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(Imp);
            Assert.IsTrue(Imp.Impacts.Count == 0);
        }

        [TestMethod]
        public void ServiceValidate()
        {
            //Assumes that service 217 has no impacts
            Startup.LoadSettings();
            var key = new ServiceKeyWeb();
            key.ServiceID = 185;
            key.Date = DateTime.Now;
            key.Attributes = new AttributeInstanceWeb[5];
            key.Attributes[0] = new AttributeInstanceWeb
            {
                Name = "activity",
                Value = new AttributeChoiceWeb { Value = "None" }
            };
            key.Attributes[1] = new AttributeInstanceWeb
            {
                Name = "PCATProductID",
                Type = "Text",
                Value = new AttributeChoiceWeb { Value = "471", DisplayValue= "471" },
                
            };
            key.Attributes[2] = new AttributeInstanceWeb
            {
                Name = "GPID",
                Type = "Text",
                Value = new AttributeChoiceWeb { Value = "220152096", DisplayValue = "220152096" }
            };
            key.Attributes[2] = new AttributeInstanceWeb
            {
                Name = "name",
                Type = "Text",
                Value = new AttributeChoiceWeb { Value = "ANS Shared VPN", DisplayValue = "ANS Shared VPN" }
            };
            key.ChildServices = new ServiceNamedCollectionWeb[0];
            key.IsValid = true;
    
            var url = FscApplication.Current.Settings.FscUrls.FSCMediation + MediationEndPoints.serviceValidate;
            var val = ApiClient.Post<ValidateServiceResponseWeb, ServiceKeyWeb>
                (HttpTargetType.FSCMediation, "FSC Mediation Service Impacts", url, key, ApiClient.ContentType.Json, ApiClient.HeaderType.FSCMediation);
            Assert.IsNotNull(val);
            Assert.IsFalse(val.IsValid);
        }
        #endregion

    }
}
