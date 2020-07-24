using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PCAT.Common.Models.Service;
using PCAT.Common.Models.Validation;
using PCAT.Common.Web.Models.Attribute;
using PCAT.Common.Web.Models.Service;
using ServiceCatalog.BL;
using ServiceCatalog.WepApi.Models;
using ServiceCatalog.WepApi.Utilities;


namespace ServiceCatalog.WepApi.Controllers
{
    [Produces("application/json")]
    [Route("Application/v1/Service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        [HttpPost]
        [Route("Attributes")]
        [ProducesResponseType(typeof(AttributeInfoWeb), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Attributes(ServiceAttributesKeyWeb postData)
        {
            return WebFunction.Execute<ServiceAttributesKeyWeb, AttributeInfoWeb[]>(this, postData, (sa) =>
            {
                AttributeInfoWeb[] list;

                if (sa == null)
                {
                    list = new AttributeInfoWeb[1];
                    list[0] = new AttributeInfoWeb { ErrorString = "Key cannot be null", IsValid = false };

                    return new WebResult<AttributeInfoWeb[]>(list);
                }
                var sKey = new ServiceKey(sa);
                try
                {
                    var atts = ServiceAttributes.Get(sKey, sa.PopulateLists);
                    list = new AttributeInfoWeb[atts.Count];
                    var i = 0;
                    foreach (var att in atts)
                        list[i++] = new AttributeInfoWeb(att);
                }
                catch (Exception e)
                {
                    list = new AttributeInfoWeb[1];
                    list[0] = new AttributeInfoWeb { ErrorString = e.Message, IsValid = false };
                }

                return new WebResult<AttributeInfoWeb[]>(list);
            });
        }

        [HttpPost]
        [Route("BuildChild")]
        [ProducesResponseType(typeof(ServiceKeyWeb), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult BuildChild(BuildChildServiceRequestWeb postData)
        {
            return WebFunction.Execute<BuildChildServiceRequestWeb, ServiceKeyWeb>(this, postData, (req) =>
            {
                var key = new ServiceKey(req.ParentService);
                var child = ServiceHierarchy.BuildChild(key, req.Name);

                return new WebResult<ServiceKeyWeb>(child.ToWeb());
            });
        }

        [HttpGet]
        [Route("Definition/{id}")]
        [ProducesResponseType(typeof(ServiceDefinitionWeb), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult ById(long id)
        {
            return WebFunction.Execute<long, ServiceDefinitionWeb>(this, id, (serviceId) =>
            {
                return new WebResult<ServiceDefinitionWeb>(ServiceDefinition.Get(serviceId).ToWeb());
            });
        }

        [HttpPost]
        [Route("Children")]
        [ProducesResponseType(typeof(List<ServiceChildWeb>), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Children(ServiceKeyWeb postData)
        {
            return WebFunction.Execute<ServiceKeyWeb, List<ServiceChildWeb>>(this, postData, (sv) =>
            {
                var key = new ServiceKey(sv);
                var children = ServiceHierarchy.Get(key);

                return new WebResult<List<ServiceChildWeb>>(children.Select(c => c.ToWeb()).ToList());
            });
        }

        [HttpPost]
        [Route("Validate")]
        [ProducesResponseType(typeof(ValidateServiceResponseWeb), 200)]
        [ProducesResponseType(typeof(ValidateServiceResponseWeb), 400)]
        public ObjectResult Validate(ServiceKeyWeb postData)
        {
            return WebFunction.Execute<ServiceKeyWeb, ValidateServiceResponseWeb>(this, postData, (sv) =>
            {
                var key = new ServiceKey(sv);
                var config = ServiceConfiguration.Get(key.Id, key.EffectiveDate);

                ValidateServiceResponse answer = config.Validate(key);

                return new WebResult<ValidateServiceResponseWeb>(new ValidateServiceResponseWeb(answer));
            }, (ex) =>
            {
                return new WebResult<ValidateServiceResponseWeb>(new ValidateServiceResponseWeb(ex));
            });
        }
        [HttpPost]
        [Route("Relationships")]
        [ProducesResponseType(typeof(ServiceRelationshipWeb[]), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Relationships(ServiceKeyWeb postData)
        {
            return WebFunction.Execute<ServiceKeyWeb, ServiceRelationshipWeb[]>(this, postData, (sv) =>
            {
                var key = new ServiceKey(sv);
                var config = ServiceRelationships.Get(key);

                var svcRel = config.Select(def => def.GetDefinition(key)).Where(pd => pd != null).ToList();
                var answer = svcRel.Select(rel => rel.ToWeb()).ToArray();

                return new WebResult<ServiceRelationshipWeb[]>(answer);
            });
        }
        [HttpPost]
        [Route("ImpactedChildren")]
        [ProducesResponseType(typeof(List<long>), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult ImpactedChildren(ImpactedServiceKeyWeb postData)
        {
            return WebFunction.Execute<ImpactedServiceKeyWeb, List<long>>(this, postData, (k) =>
            {
                var key = new ServiceKey(k.Key);
                var children = ServiceHierarchy.GetImpactedChildren(key, k.ChangedAttributes);
                return new WebResult<List<long>>(children);
            });
        }
        [HttpPost]
        [Route("ImpactedRelationships")]
        [ProducesResponseType(typeof(ServiceRelationshipWeb[]), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult ImpactedRelatoinships(ImpactedServiceKeyWeb postData)
        {
            return WebFunction.Execute<ImpactedServiceKeyWeb, ServiceRelationshipWeb[]>(this, postData, (si) =>
             {
                 var key = new ServiceKey(si.Key);
                 var rel = ServiceRelationships.GetImpactedRelationhips(key, si.ChangedAttributes).ToList().ToArray();
                 return new WebResult<ServiceRelationshipWeb[]>(rel.Select(i => i.ToWeb()).ToArray());
             });
        }
        [HttpPost]
        [Route("GetImpacts")]
        [ProducesResponseType(typeof(ImpactAttributes), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult ImpactAttributes(ImpactedServiceKeyWeb postData)
        {
            return WebFunction.Execute<ImpactedServiceKeyWeb, ImpactAttributes>(this, postData, (k) =>
            {
                var key = new ServiceKey(k.Key);
                var impactAttributes = ServiceAttributes.GetImpactAttributes(key, k.ChangedAttributes);
                return new WebResult<ImpactAttributes>(impactAttributes);
            });
        }
    }
}