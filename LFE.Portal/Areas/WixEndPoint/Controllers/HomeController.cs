using System;
using System.Web.Mvc;
using LFE.Application.Services.Helper;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.WixEndPoint.Helpers;
using LFE.Portal.Areas.WixEndPoint.Models;
using LFE.Portal.Helpers;

namespace LFE.Portal.Areas.WixEndPoint.Controllers
{
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {

            ViewBag.Message = "Welcome LFE Wix!";

            var instance   = GetInstanceDto(Request.QueryString["instance"]);
            var compId     = !string.IsNullOrEmpty(Request.QueryString["compId"]) ? Request.QueryString["compId"] : "";
            var origCompId = !string.IsNullOrEmpty(Request.QueryString["origCompId"]) ? Request.QueryString["origCompId"] : "";

            var token = new IndexViewToken();

            if (instance != null)
            {
                token.Instance   = instance;
                token.compId     = compId;
                token.origCompId = origCompId;
            }
            else
            {
                return View(token);
            }


            //check if there is a store with current instance ID, find the store's owner user
            var wixUser = UserServices.FindUserDtoByWixInstanceId(instance.instanceId);
            if (wixUser == null)
            {
                var user = this.CurrentUser();
                if (user == null) //display login screen
                {
                    return View(token);
                }
                //if LFE user is already login send him to app settings screen
                return RedirectToAction("AppSettingsNew", "Home", new { instance = Request.QueryString["instance"], origCompId = token.origCompId, compId = token.compId });
            }

            //if the current instance has already attached store, force login to store's owner and redirect to app settings
            var returnUrl = Url.ToWixLoginHandlerUrl(instance.instanceToken, compId, origCompId);

            return RedirectToAction("ForceWixUserSignIn", "Account", new { area = "", returnUrl, email = wixUser.Email, userProfileId = wixUser.UserProfileId, trackingId = instance.instanceId.ToString() });
        }

        [AllowAnonymous]
        public ActionResult AppSettingsNew(string instance, string origCompId, string compId)
        {
            var user = this.CurrentUser();

            if (user == null)
            {
                return RedirectToAction("Index", "Home", new { area = "WixEndPoint", instance = instance, compId = compId, origCompId = origCompId });
            }

            
            var wixInstance = GetInstanceDto(instance);

            if (instance != null)
            {
                #region save plugin
                var token = new PluginInstallationDTO
                {
                    Uid = wixInstance.instanceId.ToString(),
                    Type = PluginEnums.ePluginType.WIX
                };
                string error;
                if (WidgetEndpointServices.SavePluginInstallaltion(token, out error)) SaveUserEvent(CommonEnums.eUserEvents.PLUGIN_INSTALLATION, Utils.GetEnumDescription(PluginEnums.ePluginType.WIX) + " plugin installed::" + wixInstance.instanceId, wixInstance.instanceId.ToString());
                #endregion

                var wixUser = UserServices.FindUserDtoByWixInstanceId(wixInstance.instanceId);

                if (wixUser != null && user.UserId != wixUser.UserId)
                {
                    var returnUrl = Url.ToWixLoginHandlerUrl(wixInstance.instanceToken, compId, origCompId);
                    return RedirectToAction("ForceWixUserSignIn", "Account", new { area = "", returnUrl, email = wixUser.Email, userProfileId = wixUser.UserProfileId, trackingId = wixInstance.instanceId.ToString() });
                }

                #region update plugin owner
                WidgetEndpointServices.VerifyPluginOwner(wixInstance.instanceId.ToString(),out error);
                #endregion 
            }


            var webStore = WidgetServices.GetWidgetStoreDto(wixInstance.instanceId.ToString()) ?? UserServices.GetAndUpdateZombieStore(wixInstance.instanceId, user.UserId);
            //check for zombie store (if user had store and clicked disconnect and than reconnected)

            
            var settingToken = new SettingsViewToken
            {
                Instance        = wixInstance,
                UserId          = user.UserId,
                StoreId         = webStore != null ? webStore.WebStoreID : (int?)null,
                UserCoursesList = CoursesServices.GetAuthorCoursesList(Constants.DEFAULT_CURRENCY_ID, user.UserId)
            };

            if (webStore == null)//create a new store
            {
                var jsonToken = new WixSettingsJsonToken
                {
                    cbIsShowBorder    = false,
                    cbIsShowTitleBar  = true,
                    cbIsTransparent   = false,
                    cpBackgroundColor = "#FFFFFF",
                    cpFontColor       = "#000000",
                    cpTabsFontColor   = "#006699",
                    InstanceId        = wixInstance.instanceId.ToString(),
                    StoreId           = null,
                    txtStoreName      = "",
                    UniqueId          = "",
                    WixSiteUrl        = ""
                };

                string error;
                WebstoreWixService.UpdateWixSettings(ref jsonToken, this.CurrentUser(), out error);

                settingToken.StoreName       = jsonToken.txtStoreName;
                settingToken.FontColor       = jsonToken.cpFontColor;
                settingToken.TabsFontColor   = jsonToken.cpTabsFontColor;
                settingToken.BackgroundColor = jsonToken.cpBackgroundColor;
                settingToken.IsTransparent   = jsonToken.cbIsTransparent;
                settingToken.IsShowBorder    = jsonToken.cbIsShowBorder;
                settingToken.IsShowTitleBar  = jsonToken.cbIsShowBorder;
                settingToken.StoreId         = jsonToken.StoreId;
                settingToken.UniqueId        = jsonToken.UniqueId;
            }
            else
            {
                settingToken.StoreName       = webStore.WebStoreName;
                settingToken.FontColor       = webStore.FontColor;
                settingToken.TabsFontColor   = webStore.TabsFontColor;
                settingToken.BackgroundColor = webStore.BackgroundColor;
                settingToken.IsTransparent   = webStore.IsTransParent;
                settingToken.IsShowBorder    = webStore.IsShowBorder;
                settingToken.IsShowTitleBar  = webStore.IsShowTitleBar;
                settingToken.StoreId         = webStore.WebStoreID;
                settingToken.UniqueId        = webStore.UniqueId;
                // settingToken.StoreCoursesIds = WidgetServices.GetAllStoreCourseIds(webStore.WebStoreID);             
            }

            return View(settingToken);
        }

        public ActionResult Error(string message = null)
        {
            return View(message ?? "Unexpected Error");
        }

        public WixInstanceDTO GetInstanceDto(string instance)
        {
            try
            {
                string error;
                return instance.DecodeInstance2WixInstanceDTO(out error);
            }
            catch (Exception)
            {
                return null;
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult AddItemToStore(Guid? itemId,int? storeId)
        {
            string error;

            if (itemId == null || storeId == null)
            {
                return Json(new JsonResponseToken{success = false,error = "required params missing"},JsonRequestBehavior.AllowGet);
            }

            var saved = WebstoreWixService.AddItemToStore((Guid)itemId,(int)storeId, out error);

            return Json(new JsonResponseToken { success = saved, error = error},JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateWixStoreUrl(WixStoreUrlToken settings)
        {
            string error;
            
            var updated = WebstoreWixService.UpdateWixSiteUrl(ref settings, out error);

            return Json(
                new {
                    success = updated
                    ,error
                });
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult SettingsUpdate(WixSettingsJsonToken settings)
        {
            if (this.CurrentUser() == null) return Json(new {success = "false", storeID = "-1"});

            string error;
            
            var updated = WebstoreWixService.UpdateWixSettings(ref settings, this.CurrentUser(), out error);

            return Json(new
            {
                success   = updated
                ,storeID   = settings.StoreId
                ,storeName = settings.txtStoreName
                ,settings.UniqueId
                ,error
            });
        }
    }

}
