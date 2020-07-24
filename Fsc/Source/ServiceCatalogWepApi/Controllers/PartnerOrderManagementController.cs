using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCatalog.WepApi.Models;
using ServiceCatalog.BL;
using ServiceCatalog.BL.Offnet;
using ServiceCatalog.BL.Web.Offnet;
using ServiceCatalog.WepApi.Utilities;
using ServiceCatalog.BL.Models.Offnet;
using ServiceCatalog.BL.Models.OffnetService;

namespace ServiceCatalogWepApi.Controllers
{
    [Produces("application/json")]
    [Route("Application/v1/POM")]
    [ApiController]
    public class PartnerOrderManagementController : ControllerBase
    {
        #region ResolveOffnetServices
        [HttpPost]
        [Route("ResolveServices")]
        [ProducesResponseType(typeof(PartnerServPkg), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult ResolveOffnetServices(PartnerServPkg postData)
        {
            return WebFunction.Execute<PartnerServPkg, PartnerServPkg>(this, postData, (osp) =>
            {
                string error;
                PartnerServPkg pkg;

                try
                {
                    var NewOsp = OffnetBiz.ResolveOffnet(osp.GetPartnerServicePackage(), out error);
                    pkg = new PartnerServPkg(NewOsp);
                }
                catch (Exception e)
                {

                    if (e.Data.Contains("Custom Msg"))
                    {
                        pkg = new PartnerServPkg { ErrorString = e.Data["Custom Msg"].ToString() + e.Message, IsValid = false };
                    }
                    else
                    {
                        pkg = new PartnerServPkg { ErrorString = e.Message, IsValid = false };
                    }
                }
                return new WebResult<PartnerServPkg>(pkg);
            });
        }
        #endregion
        #region OffnetServiceDef
        [HttpGet]
        [Route("Service/Definition/{id}")]
        [ProducesResponseType(typeof(OffnetServiceDefinitionWeb), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult ById(long id)
        {
            return WebFunction.Execute<long, OffnetServiceDefinitionWeb>(this, id, (serviceId) =>
            {
                return new WebResult<OffnetServiceDefinitionWeb>(OffnetServiceDefinition.Get(serviceId).ToWeb());
            });
        }
        #endregion
        #region Attributes
        [HttpPost]
        [Route("Service/Attributes")]
        [ProducesResponseType(typeof(OffnetAttributeSourcesInfoWeb), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Attributes(OffnetServiceAttributesKeyWeb postData)
        {
            return WebFunction.Execute<OffnetServiceAttributesKeyWeb, OffnetAttributeSourcesInfoWeb>(this, postData, (sa) =>
            {
                OffnetAttributeSourcesInfoWeb info;
                var sKey = new OffnetServiceKey(sa);
                info = OffnetServiceAttributes.GetAttributes(sKey, sa.PopulateLists);
                return new WebResult<OffnetAttributeSourcesInfoWeb>(info);
            });
        }
        #endregion
        #region Hierarchy
        [HttpPost]
        [Route("Service/BuildChild")]
        [ProducesResponseType(typeof(OffnetHierarchyWeb), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult BuildChild(BuildOffnetChildServiceRequestWeb postData)
        {
            return WebFunction.Execute<BuildOffnetChildServiceRequestWeb, OffnetHierarchyWeb>(this, postData, (req) =>
            {
                var key = new OffnetServiceKey(req.ParentService);
                var child = OffnetServiceHierarchy.BuildChild(key, req.Name);            
                return new WebResult<OffnetHierarchyWeb>(child);
            });
        }

        [HttpPost]
        [Route("Service/Children")]
        [ProducesResponseType(typeof(List<OffnetServiceChildWeb>), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Children(OffnetServiceKeyWeb postData)
        {
            return WebFunction.Execute<OffnetServiceKeyWeb, List<OffnetServiceChildWeb>>(this, postData, (sv) =>
            {
                var key = new OffnetServiceKey(sv);
                var children = OffnetServiceHierarchy.Get(key);

                return new WebResult<List<OffnetServiceChildWeb>>(children.Select(c => c.ToWeb()).ToList());
            });
        }
        #endregion
    }
}