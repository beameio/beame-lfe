using System;
using System.Web.Mvc;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Application.Services.Helper;
using LFE.Core.Enums;


namespace LFE.Portal.Areas.WixEndPoint.Controllers
{
    public class AccountController : BaseController
    {
        private readonly Portal.Controllers.AccountController _mainAccountController = new Portal.Controllers.AccountController();

        
        [AllowAnonymous]
        public ActionResult WixLoginResultHandler(bool success, string instanceToken, Guid? uid, Guid instanceId, string origCompIdToken, string compIdToken, string error = null)
        {
            Response.Headers.Set("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            //NB! IMPORTANT: in case of force login it ALWAYS return true  , otherwise it could create endless loop on trying to login
            if (success)
            {
                TempData["WixLoginError"] = "";
                return RedirectToAction("AppSettingsNew", "Home", new { instance = instanceToken, origCompId = origCompIdToken, compId = compIdToken });
            }

            ModelState.AddModelError("", error ?? "Unexpected login error");
            TempData["WixLoginError"] = error ?? "Unexpected login error";

            return RedirectToAction("Index", "Home", new { area = "WixEndPoint", instance = instanceToken });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult WixFbLogin(string token)
        {
            string error;

            var loginResult = _mainAccountController.CreateOrUpdateFbAccountAndLoginWixUser(token, out error);
            Response.Headers.Set("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
         
            return Json(new JsonResponseToken { success = loginResult, error = error }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogoutWix(string instance, string compId, string origCompId)
        {
            string error;
            var instanceDto = instance.DecodeInstance2WixInstanceDTO(out error);

            UserServices.DisconnectWixUser(CurrentUserId, instanceDto.instanceId);

            return RedirectToAction("SignOff", "Account", new { area = "", returnUrl = @Url.Action("Index", "Home", new { area = "WixEndPoint", instance = instance, compId = compId, origCompId = origCompId }) });
        }

        public JsonResult LogAppPublishEvent(string instaceId, string wixSiteUrl)
        {
            var success = false;
            try
            {
                var additionalData = "instance Id = " + instaceId + " ; Wix Site Url = " + wixSiteUrl;

                if (!String.IsNullOrEmpty(instaceId))
                {
                    string error;
                    
                }

                success = EventLoggerService.Report(new ReportToken
                {
                    UserId             = CurrentUserId,
                    EventType          = CommonEnums.eUserEvents.WIX_APP_PUBLISHED,
                    NetSessionId       = Session.SessionID,
                    AdditionalMiscData = additionalData,
                    HostName           = GetReferrer()
                });
                return Json(new JsonResponseToken { success = success }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                 return Json(new JsonResponseToken { success = success, error = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LogAppDeletedEvent(string instaceId, string wixSiteUrl)
        {
            var success = false;
            try
            {
                var additionalData = "instance Id = " + instaceId + " ; Wix Site Url = " + wixSiteUrl;
                success = EventLoggerService.Report(new ReportToken
                {
                    UserId             = CurrentUserId,
                    EventType          = CommonEnums.eUserEvents.WIX_APP_DELETED,
                    NetSessionId       = Session.SessionID,
                    AdditionalMiscData = additionalData,
                    HostName           = GetReferrer()
                });

                string error;
                WidgetEndpointServices.UninstallPlugin(instaceId, out error);

                return Json(new JsonResponseToken { success = success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new JsonResponseToken { success = success, error = Utils.FormatError(ex)}, JsonRequestBehavior.AllowGet);
            }
        }
       
    }
}
