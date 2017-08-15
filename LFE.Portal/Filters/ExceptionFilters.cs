using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using LFE.Core.Utils;
using LFE.Portal.Helpers;
using LFE.Portal.Interfaces;
using LFE.Portal.Models;

namespace LFE.Portal.Filters
{
    class FilterExtensions
    {
        public Guid? GetRouteValue(HttpActionContext actionContext,string routeName,out string error)
        {
            error = string.Empty;
            try
            {
                var routeData = actionContext.ControllerContext.RouteData.Values.Where(x => x.Key == routeName).ToList();

                if (routeData.Count() != 1)
                {
                    error = "Not found";
                    return null;
                }

                var id = routeData[0].Value.ToString();
                Guid uid;

                if (Guid.TryParse(id, out uid)) return uid;

                error = "Not valid";
                return null;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return null;
            }
        }

        public IApiInterfaces GetController(HttpActionContext actionContext)
        {
            var cntlr = actionContext.ControllerContext.Controller as IApiInterfaces;

            return cntlr;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// override base
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(HttpActionExecutedContext context)
        {
            var status = HttpStatusCode.InternalServerError;

            var exType = context.Exception.GetType();

            if (exType == typeof(UnauthorizedAccessException))
                status = HttpStatusCode.Unauthorized;
            else if (exType == typeof(ArgumentException))
                status = HttpStatusCode.NotFound;

            var apiError = new ApiModels.ApiMessageError 
            { 
               message = context.Exception.Message 
            };

            // create a new response and attach our ApiError object
            // which now gets returned on ANY exception result
            var errorResponse = context.Request.CreateResponse(status, apiError);
            context.Response = errorResponse;

            base.OnException(context);
        }
    }

    /// <summary>
    /// Used for validation app_id in url route data
    /// </summary>
//    public class ValidateAppAttribute : ActionFilterAttribute
//    {
//        /// <summary>
//        /// OnActionExecuting override validate app_id
//        /// </summary>
//        /// <param name="actionContext"></param>
//        public override void OnActionExecuting(HttpActionContext actionContext)
//        {
//            var helper = new FilterExtensions();
//            string error;
//
//            var uid = helper.GetRouteValue(actionContext, "app_id", out error);
//
//            if (uid == null)
//            {
//                this.CreateResponse(actionContext, HttpStatusCode.BadRequest, "ApplicationId error:" + error);
//                return;
//            }
//
//            var ctlr = helper.GetController(actionContext);
//
//            if (ctlr == null)
//            {
//                this.CreateResponse(actionContext, HttpStatusCode.BadRequest, "Invalid path");
//                return;
//            }
//
//            ctlr.AppId = (Guid)uid;
//        }
//    }
//
//    /// <summary>
//    /// Validate requested record Uid route value, name: uid
//    /// </summary>
//    public class ValidateUidAttribute : ActionFilterAttribute
//    {
//        /// <summary>
//        /// OnActionExecuting override validate uid
//        /// </summary>
//        /// <param name="actionContext"></param>
//        public override void OnActionExecuting(HttpActionContext actionContext)
//        {
//            var helper = new FilterExtensions();
//            string error;
//
//            var uid = helper.GetRouteValue(actionContext, "uid", out error);
//
//            if (uid == null)
//            {
//                this.CreateResponse(actionContext, HttpStatusCode.BadRequest, "RecordId error:" + error);
//                return;
//            }
//
//            var ctlr = helper.GetController(actionContext);
//
//            if (ctlr == null)
//            {
//                this.CreateResponse(actionContext, HttpStatusCode.BadRequest, "Invalid path");
//                return;
//            }
//
//            ctlr.Uid = (Guid)uid;
//        }
//    }

    /// <summary>
    /// common api exception handler
    /// </summary>
    public class ExceptionAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public override void OnException(HttpActionExecutedContext context)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(Utils.FormatError(context.Exception)),
                ReasonPhrase = "Deadly Exception"
            });
        }
    }
}