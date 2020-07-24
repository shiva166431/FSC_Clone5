using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PCAT.Common.Caching;
using ServiceCatalog.WepApi.Models;
using ServiceCatalog.WepApi.Utilities;

namespace ServiceCatalogWepApi.Controllers
{
    [Produces("application/json")]
    [Route("Application/v1/Cache")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        [HttpPost]
        [Route("Clear")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Clear()
        {
            return WebFunction.Execute<string, string>(this, "blah", (sv) =>
            {
                CacheManager.ClearCaches();
                
                return new WebResult<string>("All caches will be cleared within the next minute.");
            });
        }

        [HttpGet]
        [Route("Number")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(WebException), 400)]
        public ObjectResult Number()
        {
            return WebFunction.Execute<string, string>(this, "blah", (sv) =>
            {
                return new WebResult<string>(CacheManager.GetCacheNumber().ToString());
            });
        }
    }
}