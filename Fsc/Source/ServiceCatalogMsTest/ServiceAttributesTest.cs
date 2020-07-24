using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Service;
using ServiceCatalog.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceCatalogMsTest
{
    [TestClass]
    public class ServiceAttributesTest
    {
        [TestMethod]
        public void RetrieveTheCorrectAttributesTest()
        {
            Startup.LoadSettings();

            var config = ServiceConfiguration.Get(31, new DateTime(2012, 2, 24));
            var attributes = config.Attributes;

            Assert.IsTrue(attributes.Count < 30 && attributes.Count > 20);
        }

        [TestMethod]
        public void RelatedAttributeTest()
        {
            Startup.LoadSettings();
            // test requirements 
            //      service 215 
            //          has EBGP Remote Peer ASN attribute of type Relationship, with default rule on Relationship[IPVPN]
            //          has IPVPN relationship

            var key = new ServiceKey();
            key.Id = 215;
            key.EffectiveDate = DateTime.Now;
            key.AddValue("ValueTest", "8", PCAT.Common.Models.Attribute.AttributeType.Integer);
            key.Relationships = new Dictionary<string, List<ServiceKey>>();

            var ipvpn = new ServiceKey()
            {
                Id = 217,
                EffectiveDate = DateTime.Now
            };
            ipvpn.AddValue("eBGP Remote Peer ASN", "65123", AttributeType.Integer);
            key.Relationships.Add("IPVPN", new List<ServiceKey>() { ipvpn });

            var results = ServiceAttributes.Get(key, true);
            //int x = 1;
        }

        [TestMethod]
        public void ParentAttributeTest()
        {
            Startup.LoadSettings();
            // test requirements 
            //      service 215 
            //          inherits from 300 with parent attributes GPID and PCATProductID

            var key = new ServiceKey();
            key.Id = 215;
            key.EffectiveDate = DateTime.Now;
            key.AddValue("ValueTest", "8", PCAT.Common.Models.Attribute.AttributeType.Integer);
            key.Relationships = new Dictionary<string, List<ServiceKey>>();

            var parent = new ServiceKey()
            {
                Id = 212,
                EffectiveDate = DateTime.Now
            };
            parent.AddValue("GPID", "12345", AttributeType.Integer);
            parent.AddValue("PCATProductID", "444", AttributeType.Integer);
            key.ParentServiceKey = parent;

            var results = ServiceAttributes.Get(key, true);
        }

        [TestMethod]
        public void ChildAttributeTest()
        {
            Startup.LoadSettings();
            // test requirements 
            //      service 219 
            //         has Attribute with default rule on Child[MULTIPLE COS SERVICE].
            //          has MULTIPLE COS SERVICE child relationship in Service Hierarchy

            var key = new ServiceKey();
            key.Id = 219;
            key.EffectiveDate = DateTime.Now;
            key.AddValue("CoS Allocation Type", "Multiple COS", PCAT.Common.Models.Attribute.AttributeType.List);
            key.Children = new Dictionary<string, List<ServiceKey>>();

            var child = new ServiceKey()
            {
                Id = 228,
                EffectiveDate = DateTime.Now
            };
            child.AddValue("Test Attribute", "6", AttributeType.SimpleText);
            key.Children.Add("MULTIPLE COS SERVICE", new List<ServiceKey>() { child });

            var results = ServiceAttributes.Get(key, true);
        }

        [TestMethod]
        public void ReadOnlyRuleAttributeTest()
        {
            Startup.LoadSettings();
            // test requirements 
            //      service 215 
            //          inherits from 300 with parent attributes GPID and PCATProductID

            var key = new ServiceKey();
            key.Id = 234;
            key.EffectiveDate = DateTime.Now;

            var parent = new ServiceKey()
            {
                Id = 217,
                EffectiveDate = DateTime.Now
            };
            parent.AddValue("GPID", "12345", AttributeType.Integer);
            parent.AddValue("PCATProductID", "551", AttributeType.Integer);
            parent.AddValue("Netflow Data Collection to Customer", "Yes", AttributeType.List);
            key.ParentServiceKey = parent;                      
            var results = ServiceAttributes.Get(key, true);
            Assert.IsFalse(results.First(a => a.Name.Equals("Port")).ReadOnly);

            parent.AddValue("Netflow Data Collection to Customer", "No", AttributeType.List);
            results = ServiceAttributes.Get(key, true);
            Assert.IsTrue(results.First(a => a.Name.Equals("Netflow Data Collection to Customer")).ReadOnly);
        }
    }
}
