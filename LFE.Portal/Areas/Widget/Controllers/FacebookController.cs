using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.Widget.Helpers;
using LFE.Portal.Areas.Widget.Models;
using LFE.Portal.Areas.WixEndPoint.Models;
using LFE.Portal.Helpers;
using LFE.Portal.Models;
using Microsoft.AspNet.Mvc.Facebook;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace LFE.Portal.Areas.Widget.Controllers
{

    public class FacebookController : FacebookBaseController
    {
        private readonly IWidgetServices _widgetServices;
        private readonly IWebStoreFacebookServices _webStoreFacebookServices;
        private readonly IAuthorAdminCourseServices _authorAdminCourseServices;
        private readonly IWidgetWebStoreServices _webStorePortalServices;
        private readonly IWidgetEndpointServices _widgetEndpointServices;
        private readonly Portal.Controllers.AccountController _mainAccountController = new Portal.Controllers.AccountController();

        public FacebookController()
        {
            _webStoreFacebookServices  = DependencyResolver.Current.GetService<IWebStoreFacebookServices>();
            _widgetServices            = DependencyResolver.Current.GetService<IWidgetServices>();
            _authorAdminCourseServices = DependencyResolver.Current.GetService<IAuthorAdminCourseServices>();
            _webStorePortalServices    = DependencyResolver.Current.GetService<IWidgetWebStoreServices>();
            _widgetEndpointServices    = DependencyResolver.Current.GetService<IWidgetEndpointServices>();
        }

        public ActionResult AppLandPage()
        {
            HttpContext.Response.Headers.Add(new NameValueCollection
            {
              {"Access-Control-Allow-Origin",Utils.GetKeyValue("homeUrl") }
            });
            return View();
        }

        public ActionResult ConnectApp()
        {
            ViewBag.IsTabAdded = false;
            
            var query = Request.QueryString.ToString().ToLower();
            if (!query.Contains("tabs_added")) return View();

            ViewBag.IsTabAdded = true;

            var firstIndex = query.IndexOf("tabs_added%5b", StringComparison.Ordinal) + 13;
            var lastIndex = query.IndexOf("%5d=", firstIndex, StringComparison.Ordinal);
            var pageID = query.Substring(firstIndex, lastIndex - firstIndex);
            var facebookPage = "https://facebook.com/" + pageID;

            ViewBag.PageUrl = facebookPage;

            #region save plugin
            var token = new PluginInstallationDTO
            {
                Uid  = pageID,
                Type = PluginEnums.ePluginType.FB
            };
            string error;
            if (_widgetEndpointServices.SavePluginInstallaltion(token, out error)) SaveUserEvent(CommonEnums.eUserEvents.PLUGIN_INSTALLATION, "Facebook plugin installed::" + pageID,pageID);
            #endregion

            return Redirect(facebookPage);

        }


        public ActionResult LoadAdminPanel(FbAdminAuthenticationResult token)
        {
            switch (token.state)
            {
                case FbPageAppAdminMatchResults.FoundAndProviderdApproved:

                    string error;
                    if (token.fbUserId != null && _mainAccountController.ForceLoginUser(token.fbUserId, token.trackingId, out error))
                    {
                        var storeCreated = _webStoreFacebookServices.CreateOrValidateUserFbStore((int) token.fbUserId, token.trackingId, out error);
                        if (storeCreated)
                        {
                            return RedirectToAction("AppSettings", new {trackingID = token.trackingId});
                        }
                        token.IsValid = false;
                        token.Message = error;
                    }
                    else
                    {
                        token.Message = "FB Uid required";
                        token.IsValid = false;
                    }                   
                    break;
                case FbPageAppAdminMatchResults.FoundAndMatchedByEmail:
                    var currentUser = this.CurrentUser();
                    if(currentUser != null && currentUser.Email != token.fbUserEmail) _mainAccountController.SignUserOut();
                    token.IsValid = true;
                    break;
                case FbPageAppAdminMatchResults.NotFoundAuthenticated:
                case FbPageAppAdminMatchResults.NotFoundNotAuthenticated:
                    token.IsValid = true;
                    break;
                default:
                    token.IsValid = false;
                    token.Message = "Unknown authentication state. Please try again or contact support team";
                    break;
            }

            return View("AdminPanel",token);
        }

        public ActionResult SettingsLogin(string trackingID, FacebookContext context)
        {

            var user = this.CurrentUser();
            if (user != null)
            {
                return RedirectToAction("AppSettings", "Facebook", new { area = "Widget", trackingId = trackingID });
            }
            return View();
        }
        public ActionResult AppSettings(string trackingID)
        {
            var error = "";
            var user = this.CurrentUser();
            if (user == null)
            {
                Response.Write("User wasn't found, please contact " + Constants.APP_OFFICIAL_NAME + " support team. " + error);
                return null;
            }

            var webStore = _widgetServices.GetWidgetStoreDto(trackingID);

            var settingToken = trackingID.ToDefaultSettingsToken();

            settingToken.Instance        = null;
            settingToken.UserId          = user.UserId;
            settingToken.UserCoursesList = _authorAdminCourseServices.GetAuthorCoursesList(Constants.DEFAULT_CURRENCY_ID, user.UserId);

            if (!String.IsNullOrEmpty(trackingID)) // for FB app , tracking ID equal to pageId
            {
                //update plugin user
                _widgetEndpointServices.VerifyPluginOwner(trackingID, out error);
            }

            if (webStore == null)
            {
                //settingToken.storeId = -1;
                //create a new web store
                var jsonToken = new FacebookSettingsJsonToken
                {
                    cbIsShowBorder    = false,
                    cbIsShowTitleBar  = true,
                    cbIsTransparent   = false,
                    cpBackgroundColor = "#FFFFFF",
                    cpFontColor       = "#000000",
                    cpTabsFontColor   = "#006699",
                    StoreId           = null,
                    txtStoreName      = "",
                    UniqueId          = "",
                    TrackingId        = trackingID
                };
                
                _webStoreFacebookServices.UpdateFacebookSettings(ref jsonToken, this.CurrentUser(), out error);
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

            // return View("~/Areas/Widget/Views/facebook/FacebookSettings.cshtml", settingToken);
            return View("FacebookSettings", settingToken);
        }

        
        private SettingsViewToken GetSettingsViewToken(string trackingId)
        {
            var error = "";
            var user = this.CurrentUser();
            if (user == null)
            {
                Response.Write("User wasn't found, please contact " + Constants.APP_OFFICIAL_NAME + " support team. " + error);
                return null;
            }

            var webStore = _widgetServices.GetWidgetStoreDto(trackingId);

            var settingToken = new SettingsViewToken
            {
                Instance        = null,
                UserId          = user.UserId,
                UserCoursesList = _authorAdminCourseServices.GetAuthorCoursesList(Constants.DEFAULT_CURRENCY_ID, user.UserId),
                TrackingID      = trackingId
            };

            if (webStore == null)
            {
                //settingToken.storeId = -1;
                //create a new web store
                var jsonToken = new FacebookSettingsJsonToken
                {
                    cbIsShowBorder    = false,
                    cbIsShowTitleBar  = true,
                    cbIsTransparent   = false,
                    cpBackgroundColor = "#FFFFFF",
                    cpFontColor       = "#000000",
                    cpTabsFontColor   = "#006699",
                    StoreId           = null,
                    txtStoreName      = "",
                    UniqueId          = "",
                    TrackingId        = trackingId
                };


                _webStoreFacebookServices.UpdateFacebookSettings(ref jsonToken, this.CurrentUser(), out error);

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

            return settingToken;
        }

        

        public ActionResult FacebookTab(string trackingId, int? width, int? height)
        {
            if (MainLayoutViewModel.IsSingleCourseStore)
            {
                ItemProductPageToken token;
                if (MainLayoutViewModel.NumCourses > 0 && MainLayoutViewModel.CategoriesList != null && MainLayoutViewModel.CategoriesList.Any())
                {
                    token = _widgetServices.GetWixDefaultItem( MainLayoutViewModel.CategoriesList.Select(x => x.WebStoreCategoryID).ToList(), CurrentUserId);
                }
                else
                {
                    token = _widgetServices.GetPlaceHolderItemInfoToken();  
                }
                return View("FacebookPlaceHolder", token);
            }
            
            var pagesize = _widgetServices.NumItemsInPage(width, height);
            const int page = 1;

            var model = _widgetServices.GetIndexModelView(_webStorePortalServices.GetStoreCurrencyByTrackingId(trackingId), trackingId, page, "", pagesize, null, "", CurrentUserId, MainLayoutViewModel.WixViewMode);

            return model == null ? View("~/Areas/Widget/Views/Shared/Error.cshtml") : View("~/Areas/Widget/Views/Widget/Index.cshtml", model);
        }

        public ActionResult Index(FacebookContext context,bool? debug = false)
        {

            //var fs = new FacebookServices();

            //fs.GetCurrentUser("CAAEWkqzCCHYBACFe04a09eZAUvDjBmk1CDIe4aZBTJB8tVWea8EmZCN24ETgEnrqAWoTYLpezdL0FrYJbyE0tODcrqcj57gpMqJQdCxyOZAARCcVKocVPI5lBFDtzEf0qOxBQ2sCgzSQg3VX6YBlg8BtxUuxZBZCjW03ObrHKm2NwNNKVTN6atkhH9JnTNJdIcONBFuYUmp1LQYwMPr0dD");

            FacebookLayoutToken facebookPageToken = null;
            FbSignedRequestToken signedRequest;
            if (debug == null || !(bool)debug)
            {
                if (!ModelState.IsValid) return View("Error");

                dynamic signedRequestJson = context.Client.ParseSignedRequest(Utils.GetKeyValue("Facebook:AppSecret"), Request.Params["signed_request"]);
                signedRequest = JsSerializer.Deserialize<FbSignedRequestToken>(signedRequestJson.ToString());    
            }
            else
            {
                #region debug mode
                const string json = "{\"algorithm\": \"HMAC-SHA256\",\"issued_at\": 1404917218,\"page\": {\"id\": \"384502031623764\",\"liked\": false,\"admin\": true},\"user\": {\"country\": \"il\",\"locale\": \"en_US\",\"age\": {\"min\": 21}}}";
                signedRequest = JsSerializer.Deserialize<FbSignedRequestToken>(json);
                #endregion
            }
            Logger.Debug("signed request::" + JsSerializer.Serialize(signedRequest));
          

            if (signedRequest.page != null)
            {
                facebookPageToken = new FacebookLayoutToken
                {
                    Liked        = signedRequest.page.liked
                    ,Admin       = signedRequest.page.admin
                    ,TrackingId  = signedRequest.page.id
                    ,AccessToken = context != null ? context.AccessToken : string.Empty
                };

                if (facebookPageToken.Admin)
                {
                    facebookPageToken.Settings = User.Identity.IsAuthenticated ? GetSettingsViewToken(facebookPageToken.TrackingId) : facebookPageToken.TrackingId.ToDefaultSettingsToken();
                }
            
            }

            return facebookPageToken == null ? View("Error") : View("FacebookFrames", facebookPageToken);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SettingsUpdate(FacebookSettingsJsonToken settings)
        {
            if (this.CurrentUser() == null) return Json(new { success = "false", storeID = "-1", error = "Please Login" });

            string error;
            var updated = _webStoreFacebookServices.UpdateFacebookSettings(ref settings, this.CurrentUser(), out error);

            return Json(new
                            {
                                success    = updated.ToString().ToLower()
                                ,storeID   = settings.StoreId
                                ,storeName = settings.txtStoreName
                                ,settings.UniqueId
                                ,error
                            });
        }


        #region actions
        #endregion

        // This action will handle the redirects from FacebookAuthorizeFilter when
        // the app doesn't have all the required permissions specified in the FacebookAuthorizeAttribute.
        // The path to this action is defined under appSettings (in Web.config) with the key 'Facebook:AuthorizationRedirectPath'.
        //public ActionResult Permsissions(FacebookRedirectContext context)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        return View(context);
        //    }

        //    return View("Error");
        //}
    }
}
