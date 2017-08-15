using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using LFE.Application.Services.ExternalProviders;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Infrastructure.NLogger;
using LFE.Portal.Helpers;
using WebMatrix.WebData;

namespace LFE.Portal.Controllers
{
    public class BaseController : Controller
    {
        private readonly IGeoServices _geoServices;

        #region properties
        public static JavaScriptSerializer JsSerializer { get; private set; }
        public NLogLogger Logger { get; private set; }

        //services
        public IUserEventLoggerServices EventLoggerService { get; private set; }
        public IEmailServices EmailService { get; private set; }
        public FacebookServices FbServices { get; private set; }
        public string BaseUrl
        {
            get { return Utils.GetKeyValue("baseUrl"); }
        }
        #endregion

        #region .ctor
        public BaseController()
        {
            Logger = new NLogLogger();
            JsSerializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            
            if(EventLoggerService == null)  EventLoggerService  = DependencyResolver.Current.GetService<IUserEventLoggerServices>();
            if(EmailService       == null ) EmailService        = DependencyResolver.Current.GetService<IEmailServices>();
            if(FbServices        == null)   FbServices          = DependencyResolver.Current.GetService<FacebookServices>();
            if (_geoServices     == null)   _geoServices        = DependencyResolver.Current.GetService<IGeoServices>();

        } 
        #endregion

        #region services
        #region user
        public int CurrentUserId
        {
            get
            {
                if (!WebSecurity.IsAuthenticated) return -1;

                var user = this.CurrentUser();

                return user != null ? user.UserId : -1;
            }
        }

        public int? NullableCurrentUserId
        {
            get
            {     
                return CurrentUserId < 0 ? (int?) null : CurrentUserId;
            }
        }

        public bool IsAdminRequestAuthorized()
        {
            if (!WebSecurity.IsAuthenticated) return false;

            return Roles.IsUserInRole(WebSecurity.CurrentUserName, CommonEnums.UserRoles.Admin.ToString()) || Roles.IsUserInRole(WebSecurity.CurrentUserName, CommonEnums.UserRoles.System.ToString());
        }


        #endregion

        #region actions
        public ActionResult ErrorResponse(string error)
        {
            return Json(new JsonResponseToken
            {
                result = false
               ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("UserEndPoint", "Account", new { area = "" });
        }

        public ActionResult LogoutRedirect(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Login", "Account", new { area = "" });
        }

        public ActionResult ReturnErrorPage()
        {
            return RedirectToAction("Error", "Home",new{area=""});
        }

        public string FormatError(Exception ex)
        {
            return Utils.FormatError(ex);
        }
        #endregion

        #region errors
        public static string GetModelStateError(List<ModelState> list)
        {
            return list.Aggregate(String.Empty, (current1, state) => state.Errors.Where(error => !String.IsNullOrEmpty(error.ErrorMessage) || error.Exception != null).Aggregate(current1, (current, error) => current + ( " " + String.Format("{0} / {1}", error.ErrorMessage, Utils.FormatError(error.Exception)) + ";" )));
        }
        #endregion

        #region events
        public void SaveUserEvent(CommonEnums.eUserEvents userEvent, string additionalData = null, string trackingID = null, int? courseId = null, int? bundleId = null,long? bcId = null)
        {
            var user = this.CurrentUser();
            if (user == null) return;
            EventLoggerService.Report(new ReportToken
            {
                UserId             = CurrentUserId,
                EventType          = userEvent,
                NetSessionId       = Session.SessionID,
                AdditionalMiscData = additionalData,
                TrackingID         = trackingID,
                CourseId           = courseId,
                BundleId           = bundleId,
                HostName           = GetReferrer()
            });
        }

        public void SaveUserRegistrationEvent(int? userId, string additionalData = null, string trackingID = null)
        {
            EventLoggerService.Report(new ReportToken
            {
                UserId             = userId,
                EventType          = CommonEnums.eUserEvents.REGISTRATION_SUCCESS,
                NetSessionId       = Session.SessionID,
                AdditionalMiscData = additionalData,
                TrackingID         = trackingID,
                HostName           = GetReferrer()
            });
        }
        #endregion 

        #region common
        public string TodayFileString()
        {
            var today = DateTime.Now;

            return today.Month + "_" + today.Day + "_" + today.Year;
        }
        #endregion
        #endregion

        public string GetReferrer()
        {
            try
            {
                if (System.Web.HttpContext.Current == null) return "out of http context";
                var cookie   = System.Web.HttpContext.Current.Request.Cookies["_lfeReferrer"];
                var referrer = cookie != null ? HttpUtility.UrlDecode(cookie.Value) : "unknown";
                return referrer;
            } 
            catch(Exception )
            {
                return null;
            }
        }
        
        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public static string RenderRazorViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        } 
    }

    public class BinaryContentResult : ActionResult
    {
        private readonly string contentType;
        private readonly byte[] contentBytes;
        private readonly string fileName;
        //private const bool showDisposition = true;

        public BinaryContentResult(byte[] contentBytes, string contentType, string fileName = null)
        {
            this.contentBytes = contentBytes;
            this.contentType = contentType;
            this.fileName = fileName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.ContentType = contentType;
            //if (showDisposition) 
                response.AddHeader("content-disposition", "inline; filename=" + fileName);
            response.Charset = Encoding.UTF8.ToString();
            using (var stream = new MemoryStream(contentBytes))
            {
                stream.WriteTo(response.OutputStream);
                stream.Flush();
            }
        }
    }
}
