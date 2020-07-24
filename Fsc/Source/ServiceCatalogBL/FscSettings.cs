using PCAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceCatalog.BL
{
    public class FscSettings: CatalogSettings
    {
        public string FscConnectionString { get; set; }
        public FSCUrl FscUrls { get; set; }

        [Serializable]
        [DataContract]
        public class FSCUrl
        {
            // commmon urls:
            [DataMember] public string GlmHost { get; set; }
            [DataMember] public string FSCMediation { get; set; }
            [DataMember] public string PriceConversionUrl { get; set; }

            public FSCUrl() { }
            public object this[string propertyName]
            {
                //Get class properties by Name
                get
                {
                    Type myType = typeof(FSCUrl);
                    PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                    return myPropInfo.GetValue(this, null);
                }
                set
                {
                    Type myType = typeof(FSCUrl);
                    PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                    myPropInfo.SetValue(this, value, null);
                }
            }
        }
    }
}
