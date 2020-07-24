using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceCatalog.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalogMsTest
{
    [TestClass]
    public class ServiceDefinitionTest
    {
        [TestMethod]
        public void ServiceDefinition_Get()
        {
            Startup.LoadSettings();

            var sd = ServiceDefinition.Get(1);

            Assert.IsNotNull(sd);
            Assert.AreEqual<long>(sd.Id, 1);
        }
    }
}
