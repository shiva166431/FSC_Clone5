using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceCatalog.BL;
using ServiceCatalog.WepApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceCatalog.WepApi.Utilities
{
    public class WebFunction
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FscApplication));

        /// <summary>
        /// Execute the passed in function while logging the output and execution time.  (Helper for Execute with TIn)
        /// </summary>
        public static ObjectResult Execute<TOut>(ControllerBase controller,
            Func<WebResult<TOut>> function, // code to execute the web function
            Func<Exception, WebResult<TOut>> exceptionHandler = null)  // code to handle any exceptions thrown by function
            where TOut : class
        {
            return Execute<string, TOut>(controller, null, (x) => function(), exceptionHandler);
        }

        /// <summary>
        /// Execute the passed in function while logging the input, output, and execution time.
        /// </summary>
        /// <typeparam name="TIn">Type of the input</typeparam>
        /// <typeparam name="TOut">Returned type</typeparam>
        /// <param name="input">input to the function (such as post data)</param>
        /// <param name="function">Function to execute</param>
        /// <param name="exceptionHandler">Optional. If not specified and TOut is a WebResult<>, then any exceptions will be returned in WebResult.ErrorMessage.</param>
        /// <returns></returns>
        public static ObjectResult Execute<TIn, TOut>(ControllerBase controller,
            TIn input,
            Func<TIn, WebResult<TOut>> function, // code to execute the web function
            Func<Exception, WebResult<TOut>> exceptionHandler = null)  // code to handle any exceptions thrown by function
            where TOut : class
        {
            WebResult<TOut> result;
            DateTime startTime = DateTime.Now;
            string route = controller.RouteData.Values["controller"] as string;
            string action = controller.RouteData.Values["action"] as string;

            route = route + "/" + action; // to match pcat so we can use the same rest log parser
            logger.Info(string.Format("{0} {1} #{2} input: {3}", route, action, startTime.Ticks, input == null ? "<null>" : JsonConvert.SerializeObject(input, Newtonsoft.Json.Formatting.None)));

            try
            {
                result = function(input);

                logger.Info(string.Format("{0} {1} #{2} in {3} returned: {4}", route, action, startTime.Ticks, DateTime.Now - startTime, result == null ? "<null>" : JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.None)));
            }
            catch (Exception exception)
            {
                logger.Error(string.Format("{0} {1} #{2} in {3} exception: {4}", route, action, startTime.Ticks, DateTime.Now - startTime,
                    exception.Message + Environment.NewLine + "StackTrace: " + exception.StackTrace));

                // is there a custom exception handler?
                if (exceptionHandler != null)
                    result = exceptionHandler(exception);
                else
                    result = new WebResult<TOut>(exception);
            }
            
            if (result.Exception == null)
                return controller.StatusCode((int)result.Status, result.Result);
            else
                return controller.StatusCode((int)result.Status, new WebResponse(result.Exception));
        }
    }
}
