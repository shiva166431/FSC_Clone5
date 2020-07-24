using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceCatalog.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalogMsTest
{
    [TestClass]
    public class ServiceRelationshipTest
    {
        [TestMethod]
        public void ServiceRelationship_Get()
        {
            Startup.LoadSettings();

            var sd = ServiceRelationships.GetServiceRelationships(217, 0);

            Assert.IsNotNull(sd);
            Assert.AreEqual<string>(sd[0].Name, "IPVPN ENDPOINT CPE");
        }
    }
}
