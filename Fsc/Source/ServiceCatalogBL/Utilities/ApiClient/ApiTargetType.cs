using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ServiceCatalog.BL.Utilities
{
    public enum TargetType
    {
        FSCMediation
    }
 #region EndPoints
    public static class MediationEndPoints
    {
       
        public static string ping { get { return "/fsc/Ping"; } }
        
        public static string pingConfig { get { return "/fsc/Config"; } }
        #region POM
        public static string pomResolveServices { get { return "/pom/ResolveServices"; } }
        public static string pomServiceDef { get { return "/pom/Service/Definition"; } }
        public static string pomServiceAttrs { get { return "/pom/Service/Attributes"; } }
        public static string pomBuildChild { get { return "/pom/Service/BuildChild"; } }
        public static string pomChildren { get { return "/pom/Service/Children"; } }
        #endregion
        #region service
        public static string serviceDef { get { return "/service/Definition"; } }
        public static string serviceAttrs { get { return "/service/Attributes"; } }
        public static string serviceBuildChild { get { return "/service/BuildChild"; } }
        public static string serviceChildren { get { return "/service/Children"; } }
        public static string serviceValidate { get { return "/service/Validate"; } }
        public static string serviceRelationships { get { return "/service/Relationships"; } }
        public static string serviceImpactedChildren { get { return "/service/ImpactedChildren"; } }
        public static string serviceImpactedRelationships { get { return "/service/ImpactedRelationships"; } }
        public static string serviceImpacts { get { return "/service/GetImpacts"; } }
        #endregion   
    }
#endregion
}
