using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCatalog.WepApi.Models
{
    public class WebResponse
    {
        public WebException Exception { get; set; }

        public WebResponse(WebException webException)
        {
            Exception = webException;
        }
    }
}
