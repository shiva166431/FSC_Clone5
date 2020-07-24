using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ServiceCatalog.WepApi.Models
{
   public class WebException
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }

        public string Detail { get; set; }

        public WebException()
        { }

        public WebException(string internalErrorDetail)
        {
            HttpStatusCode = HttpStatusCode.InternalServerError;
            Code = 500001;
            Message = "Internal Server Error";
            Detail = internalErrorDetail;
        }
    }
}
