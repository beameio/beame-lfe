using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace LFE.Portal.Helpers
{
    /// <summary>
    /// Api helpers and extensions
    /// </summary>
    public static class ApiExtension
    {
        /// <summary>
        /// Generic create response service
        /// </summary>
        /// <param name="obj">generic caller</param>
        /// <param name="actionContext">context</param>
        /// <param name="code">status code</param>
        /// <param name="message">optional message</param>
        public static void CreateResponse(this object obj,HttpActionContext actionContext, HttpStatusCode code, string message)
        {
            var response = actionContext.Request.CreateResponse(code);
            if (!string.IsNullOrEmpty(message)) response.Content = new StringContent(message);
            actionContext.Response = response;
        }
    }
}
