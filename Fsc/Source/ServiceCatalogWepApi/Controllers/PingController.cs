using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCatalog.WepApi.Models;
using ServiceCatalog.WepApi.Utilities;
using static ServiceCatalog.WepApi.Models.PingModels;

namespace ServiceCatalogWepApi.Controllers
{
    [Produces("application/json")]
    [Route("Application/v1/FSC")]
    [ApiController]
    public class PingController : ControllerBase
    {
        [HttpGet]
        [Route("Ping")]
        [ProducesResponseType(typeof(PingResponse),200)]
        [ProducesResponseType(typeof(WebException),400)]
        public ObjectResult Ping()
        {
            return WebFunction.Execute<PingResponse>(this,()=>
            {
                return new WebResult<PingResponse>(new PingResponse());
            });
        }

        [HttpGet]
        [Route("Config")]
        [ProducesResponseType(typeof(PingConfig), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Config()
        {
            return WebFunction.Execute<PingConfig>(this, () =>
            {
                var config = new PingConfig();
                config.WebHost = HttpContext.Request.Host.Value.ToString();
                return new WebResult<PingConfig>(config);
            });
        }
    }
}