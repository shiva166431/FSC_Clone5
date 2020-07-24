using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAT.Common.Models.Service;
using ServiceCatalog.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalogMsTest
{
    [TestClass]
    public class ServiceHierarchyTest
    {
        [TestMethod]
        public void Get()
        {
            Startup.LoadSettings();
            // test assumes service 212 has child of 213 named "test"

            var key = new ServiceKey();
            key.Id = 212;
            key.EffectiveDate = DateTime.Now;
            key.AddValue("ValueTest", "8", PCAT.Common.Models.Attribute.AttributeType.Integer);

            var sh = ServiceHierarchy.Get(key);
            Assert.IsTrue(sh.Count > 0);
            Assert.AreEqual(sh[0].Name, "test");
        }

        [TestMethod]
        public void Build_MinMaxTest()
        {
            Startup.LoadSettings();
            // test assumes service 212 has child of 213 named "test" with max rule of 2

            var key = new ServiceKey()
            {
                Id = 212,
                EffectiveDate = DateTime.Now
            };

            var sh = ServiceHierarchy.BuildChild(key, "test");
            Assert.IsNotNull(sh);

            try
            {
                sh = ServiceHierarchy.BuildChild(key, "blah");
                Assert.IsNull(sh);  // exception should be thrown
            }
            catch(Exception)
            {}

            key.Children = new Dictionary<string, List<ServiceKey>>();
            key.Children.Add("test", new List<ServiceKey>());
            key.Children["test"].Add(new ServiceKey()
            {
                Id = 213,
                EffectiveDate = DateTime.Now
            });
            sh = ServiceHierarchy.BuildChild(key, "test");
            Assert.IsNotNull(sh);  // max is two

            key.Children["test"].Add(new ServiceKey()
            {
                Id = 213,
                EffectiveDate = DateTime.Now
            });
            try
            {
                sh = ServiceHierarchy.BuildChild(key, "test");
                Assert.IsNull(sh);  // max is two
            }
            catch (Exception)
            { }
        }

        [TestMethod]
        public void Build_ParentDefaultTest()
        {
            Startup.LoadSettings();
            // test assumes service 227 has child of 236 named "LAN PORT SERVICE"

            var key = new ServiceKey()
            {
                Id = 227,
                EffectiveDate = DateTime.Now
            };
            key.AddAttribute("Number of LAN Ports", "2");
            key.AddAttribute("GPID", "HelloWorld");
            key.AddAttribute("IsSourceOfProductsServiceAlias", "Yes");

            var sh = ServiceHierarchy.BuildChild(key, "LAN PORT SERVICE");

            Assert.IsTrue(sh.ParentServiceKey.GetAttributeValue("GPID", PCAT.Common.Models.SearchOptions.ALL_FALSE).Equals("HelloWorld"));
            Assert.IsTrue(sh.GetAttributeValue("GPID", PCAT.Common.Models.SearchOptions.ALL_FALSE).Equals("HelloWorld"));

            Assert.IsTrue(sh.ParentServiceKey.GetAttributeValue("IsSourceOfProductsServiceAlias", PCAT.Common.Models.SearchOptions.ALL_FALSE).Equals("Yes"));
            Assert.IsTrue(sh.GetAttributeValue("IsSourceOfProductsServiceAlias", PCAT.Common.Models.SearchOptions.ALL_FALSE).Equals("No"));
        }

        [TestMethod]
        public void Validate()
        {
            Startup.LoadSettings();
            // test assumes service 227 has child of 236 named "LAN PORT SERVICE"

            var key = new ServiceKey();
            key.Id = 227;
            key.EffectiveDate = DateTime.Now;
            key.AddAttribute("Number of LAN Ports", "2");
            key.AddAttribute("GPID", "HelloWorld");
            key.AddAttribute("IsSourceOfProductsServiceAlias", "Yes");

            var result = ServiceHierarchy.Validate(key);
            Assert.IsFalse(result.IsValid);

            key.Children = new Dictionary<string, List<ServiceKey>>();
            key.Children.Add("blah", new List<ServiceKey>());
            result = ServiceHierarchy.Validate(key);
            Assert.IsFalse(result.IsValid); // child name violation

            key.Children.Remove("blah");
            key.Children.Add("LAN PORT SERVICE", new List<ServiceKey>());
            key.Children["LAN PORT SERVICE"].Add(new ServiceKey()
            {
                Id = 236
            });
            result = ServiceHierarchy.Validate(key);
            Assert.IsTrue(result.IsValid); // we good

            key.AddAttribute("Number of LAN Ports", "0");
            key.Children["LAN PORT SERVICE"].Add(new ServiceKey()
            {
                Id = 236
            });
            result = ServiceHierarchy.Validate(key);
            Assert.IsFalse(result.IsValid); // max violation

            key.Children["LAN PORT SERVICE"].Clear();
            key.Children["LAN PORT SERVICE"].Add(new ServiceKey()
            {
                Id = 216
            });
            result = ServiceHierarchy.Validate(key);
            Assert.IsFalse(result.IsValid); // wrong service id for child name

        }
    }
}
