using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCatalog.WepApi.Models
{
    public class WebResult<T>
    {
        public HttpStatusCode Status { get; set; }
        public WebException Exception { get; set; }
        public T Result { get; set; }

        public WebResult(T result)
        {
            Status = HttpStatusCode.OK;
            Result = result;
        }

        public WebResult(HttpStatusCode status, int level3Code, string genericMessage, string errorDetail)
        {
            Status = status;

            Exception = new WebException()
            {
                Code = level3Code,
                HttpStatusCode = status,
                Message = genericMessage,
                Detail = errorDetail
            };
        }

        public WebResult(Exception e)
        {
            Status = HttpStatusCode.InternalServerError;

            StringBuilder messageBuilder = new StringBuilder();
            Exception exception = e;
            while (exception != null)
            {
                messageBuilder.Append(exception.Message);
                exception = exception.InnerException;
            }

            Exception = new WebException(messageBuilder.ToString());
        }
    }
}
