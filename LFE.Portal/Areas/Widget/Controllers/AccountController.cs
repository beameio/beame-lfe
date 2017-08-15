using System;
using System.Web;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Portal.Helpers;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class AccountController : BaseController
    {
        private readonly Portal.Controllers.AccountController _mainAccountController = new Portal.Controllers.AccountController();
        private readonly IWebStoreFacebookServices _webStoreFacebookServices;
        private readonly IUserAccountServices _userAccountServices;

        public AccountController()
        {
            _webStoreFacebookServices = DependencyResolver.Current.GetService<IWebStoreFacebookServices>();
            _userAccountServices = DependencyResolver.Current.GetService<IUserAccountServices>();
        }

        /// <summary>
        /// handle widget login result, called from Main Account controller
        /// </summary>
        /// <param name="email"></param>
        /// <param name="success"></param>
        /// <param name="isWidget"></param>
        /// <param name="trackingId"></param>
        /// <param name="parentUrl"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        //[HttpPost]
        [AllowAnonymous]
        public ActionResult WidgetLoginResultHandler(string email,bool success,bool isWidget, string trackingId , string parentUrl = null,string error = null)
        {
            if (!success)
            {
                ModelState.AddModelError("",string.IsNullOrEmpty(error) ? "unexpected error. please try again" : error);
            }

            var model =  new LoginDTO
                                    {
                                        IsLoggedIn       = success
                                        ,Email           = email
                                        ,IsWidget        = isWidget
                                        ,ParentWindowURL = parentUrl
                                        ,LoginError      = error
                                        ,TrackingID      = trackingId
                                    };

            return PartialView(isWidget ? "~/Areas/Widget/Views/Shared/Account/_Login.cshtml" : "~/Areas/Widget/Views/Shared/Account/_LoginMainSite.cshtml", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult FbLogin(string token, string returnUrl, string trackingId, byte? typeId = 0)
        {
            if (!String.IsNullOrEmpty(returnUrl)) returnUrl = returnUrl.DecodeUrl();

            var type = Utils.ParseEnum<CommonEnums.eRegistrationSources>(typeId.ToString());

            string error;
            string email;

            var loginResult = _mainAccountController.CreateOrUpdateFbAccountAndLoginWidgetUser(token, type, out email, out error, trackingId);
            var url = String.IsNullOrEmpty(returnUrl) ? Url.Action("UserEndPoint", "Account", new { area = "" }) : returnUrl;

            // ReSharper disable once RedundantAnonymousTypePropertyName
            return Json(new { isSuccess = loginResult, error = error, responseUrl =url});
        }

        /// <summary>
        /// fb login window handle on client, when access token received call to main account controller to create or update account
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trackingId"></param>        
        /// <param name="typeId"></param>
        /// <returns></returns>
        //[HttpPost]
        [AllowAnonymous]
        public JsonResult WidgetFbLogin(string token, string trackingId, byte? typeId = 0)
        {
            string error;
            string email;
            var type = Utils.ParseEnum<CommonEnums.eRegistrationSources>(typeId.ToString());
            var loginResult = _mainAccountController.CreateOrUpdateFbAccountAndLoginWidgetUser(token, type, out email, out error, trackingId);

            // ReSharper disable once RedundantAnonymousTypePropertyName
            return Json(new { isSuccess = loginResult, error = error, responseUrl = Request.UrlReferrer !=null ? Request.UrlReferrer.AbsoluteUri : string.Empty });
        }

        //[HttpPost]
        [AllowAnonymous]
        public JsonResult VerifyAndConnect(string token, int? userId, string trackingId)
        {
            string error;

            try
            {
                bool isSuccess;
                string returnUrl = null;
                if (userId == null)
                {
                    isSuccess = false;
                    error = "UserId required";
                }
                else
                {
                    var loginResult = _mainAccountController.VerifyFbUserAccessToken(token);

                    if (loginResult.IsSuccessful)
                    {
                        isSuccess = _mainAccountController.ConnectSocialLoginToLfeAccount((int)userId, loginResult.ProviderUserId, CommonEnums.SocialProviders.Facebook, out error);

                        if (isSuccess) isSuccess = _webStoreFacebookServices.CreateOrValidateUserFbStore((int)userId, trackingId, out error);
                        if (isSuccess) returnUrl = Url.Action("AppSettings", "Facebook", new { area = "Widget", trackingID = trackingId });
                    }
                    else
                    {
                        error = loginResult.Error.Message;
                        isSuccess = false;
                    }
                }
                
                return Json(new JsonResponseToken { success = isSuccess, error = error , result = new{returnUrl }});
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                return Json(new JsonResponseToken { success = false, error = error });
            }
        }


       // [HttpPost]
        [AllowAnonymous]
        public JsonResult ConnectFbAndLfeAccount(string uid, int? userId, string trackingId)
        {
            string error;

            bool loginResult;
            if (userId != null && !String.IsNullOrEmpty(uid))
            {
                loginResult = _mainAccountController.ConnectSocialLoginToLfeAccount((int)userId, uid, CommonEnums.SocialProviders.Facebook, out error);

                _webStoreFacebookServices.CreateOrValidateUserFbStore((int)userId, trackingId, out error);
            }
            else
            {
                error = "UserId and/or Uid required. Please contact support team";
                loginResult = false;
            }


            // ReSharper disable once RedundantAnonymousTypePropertyName
            return Json(new { isSuccess = loginResult, error = error });
        }


        //[HttpPost]
        [AllowAnonymous]
        public JsonResult FbAppAdminCreate(string token, string trackingId)
        {
            string error;
            string email;
            var loginResult = _mainAccountController.CreateOrUpdateFbAccountAndLoginWidgetUser(token, CommonEnums.eRegistrationSources.FB, out email, out error, trackingId);
            
            if (!loginResult) return Json(new JsonResponseToken {success = false, error = error});
            
            var user = _userAccountServices.FindUserByEmail(email);

            if (user == null) return Json(new JsonResponseToken{success = false,error = "Created User not found. Please contact support team"});

            var storeSaved  = _webStoreFacebookServices.CreateOrValidateUserFbStore(user.UserId, trackingId, out error);

            var returnUrl = Url.Action("AppSettings", "Facebook", new { area = "Widget", trackingID = trackingId });

            return Json(new JsonResponseToken { success = storeSaved, error = error, result = new{returnUrl }});
        }
        //public ActionResult Logoff(string trackingID)
        //{
        //    _mainAccountController.SignUserOut();

        //    if (MainLayoutViewModel != null && MainLayoutViewModel.WebStore != null && !string.IsNullOrEmpty(MainLayoutViewModel.WebStore.TrackingID))
        //    {
        //        return Redirect("~/widget/" + MainLayoutViewModel.WebStore.TrackingID);
        //    }

        //    ViewBag.StartupScript = "resizeIframe('35'); top.location.reload();";

        //    return Redirect("~/Widget/MainSiteToolBar");
        //}

        [AllowAnonymous]
        [HttpPost]
        //[RequireHTTPSNonLocal]
        public ActionResult WidgetRegisterUser(RegisterDTO model)
        {
            string error;
            var userRestired = _mainAccountController.RegisterWidgetUser(model, out error);

            if (!userRestired)
            {
                ModelState.AddModelError("",error ?? " something went wrong. please try again");
            }
            else
            {
                if (model.RequiredConfirmation) return RedirectToAction("RegistrationActivationNotification", "Account",new{name=model.RegisterToken2FullName()});
            }

            return PartialView("~/Areas/Widget/Views/Shared/Account/_Register.cshtml", model);
        }

        public ActionResult RegistrationActivationNotification(string name)
        {
            return PartialView("~/Areas/Widget/Views/Shared/Account/_RegistrationActivationNotification.cshtml", name);
        }

        public ActionResult SafariLoginSession()
        {
            var cookie = new HttpCookie("TestCookie", "1");
            Response.Cookies.Add(cookie);

            return View("~/Areas/Widget/Views/Account/SafariLoginSession.cshtml");
        }
    }
}
