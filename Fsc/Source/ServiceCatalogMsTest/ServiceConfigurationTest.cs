using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Service;
using ServiceCatalog.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalogMsTest
{
    [TestClass]
    public class ServiceConfigurationTest
    {
        [TestMethod]
        public void ServiceConfig_Get()
        {
            Startup.LoadSettings();

            var sc = ServiceConfiguration.Get(55, DateTime.Today);

            Assert.IsNotNull(sc);
            Assert.AreEqual<long>(sc.Id, 55);
        }

        [TestMethod]
        public void ServiceConfig_ValidValueValidation()
        {
            Startup.LoadSettings();

            var key = new ServiceKey()
            {
                Id = 215,
                EffectiveDate = DateTime.Now
            };
            key.AddValue("GPID", "12345", AttributeType.Integer);
            key.AddValue("PCATProductID", "551", AttributeType.Integer);
            key.AddValue("IPv4 eBGP Remote Peer ASN", "1234", AttributeType.Integer);
            key.AddValue("Ipv4 eBGP Remote Peer ASN", "blah", AttributeType.List);
            key.AddValue("Public ASN Validation", "blah", AttributeType.SimpleText);
            key.AddValue("Authentication Required", "Yes", AttributeType.List);
            key.AddValue("IPv4 Authentication Key", "Yes", AttributeType.SimpleText);

            var sc = ServiceConfiguration.Get(key.Id, key.EffectiveDate);
            var response = sc.Validate(key);

            // eBGP Remote Peer ASN Type can't be blah
            Assert.IsFalse(response.IsValid);
        }

        [TestMethod]
        public void ServiceConfig_ValidValueValidation2()
        {
            Startup.LoadSettings();

            var key = new ServiceKey()
            {
                Id = 195,
                EffectiveDate = DateTime.Now
            };
            key.AddValue("GPID", "12345", AttributeType.Integer);
            key.AddValue("PCATProductID", "551", AttributeType.Integer);

            key.AddValue("sslWinHostChkTiering", "Yes", AttributeType.List);
            key.AddValue("SSLClntQty", "0", AttributeType.Integer);

            var sc = ServiceConfiguration.Get(key.Id, key.EffectiveDate);
            var response = sc.Validate(key);

            // sslWinHostChkTiering should be no error
            Assert.IsFalse(response.IsValid);
        }
    }
}
