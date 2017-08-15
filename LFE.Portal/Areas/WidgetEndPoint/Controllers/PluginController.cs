using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;

namespace LFE.Portal.Areas.WidgetEndPoint.Controllers
{
    public class PluginController : BaseController
    {
        //
        
        private readonly IGeoServices _geoServices;
        private readonly IPluginStoreServices _pluginStoreServices;
        
        public PluginController()
        {
            
            _pluginStoreServices    = DependencyResolver.Current.GetService<IPluginStoreServices>();
            _geoServices            = DependencyResolver.Current.GetService<IGeoServices>();
        }
        
        public HttpResponseMessage Install(string uid, byte? type, string pluginVersion, string domain)
        {
            Logger.Debug(String.Format("PLUGIN INSTALL MSG::uid = {0}::type = {1}::domain {2}",uid,type,domain));

            if(type == null || String.IsNullOrEmpty(uid)) return new HttpResponseMessage(HttpStatusCode.BadRequest);

            var pluginType = Utils.ParseEnum<PluginEnums.ePluginType>(type.ToString());

            var token = new PluginInstallationDTO
            {
                Uid    =  uid,
                Type   = pluginType,
                Domain = domain
            };
            string error;
            var result = WidgetEndpointServices.SavePluginInstallaltion(token, out error);
            if (result) SaveUserEvent(CommonEnums.eUserEvents.PLUGIN_INSTALLATION, Utils.GetEnumDescription(pluginType) + " plugin installed::"  + uid, uid);
            Logger.Debug(String.Format("PLUGIN INSTALL MSG::installed{3}::uid = {0}::type = {1}::domain {2}", uid, type, domain,result));

            return new HttpResponseMessage(result ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        }

        public HttpResponseMessage UnInstall(string uid)
        {
            Logger.Debug(String.Format("PLUGIN UNINSTALL MSG::uid = {0}", uid));

            if (String.IsNullOrEmpty(uid)) return new HttpResponseMessage(HttpStatusCode.BadRequest);

            string error;
            var result = WidgetEndpointServices.UninstallPlugin(uid, out error);

            Logger.Debug(String.Format("PLUGIN UNINSTALL MSG::installed{0}::uid = {1}",result, uid));

            return new HttpResponseMessage(result ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        }

        public ActionResult UserScript(string callback, string uid)
        {
            ViewBag.linkMessage = "null";
            ViewBag.linkUrl     = "null";
            var currencies      = JsSerializer.Serialize(_geoServices.ActiveCurrenciesList);
            dynamic stores      = new BaseWebStoreDTO[0];
            var userAuthorized  = false;
            string error;
            var installToken = WidgetEndpointServices.GetPluginInstallationDto(uid, out error);

            //Logger.Debug("Call plugin user script for " + uid);

            if (installToken == null)
            {
                ViewBag.linkMessage = "'Installation not registered with " + error + ". Please contact support team.'";
                ViewBag.linkUrl = "'#'";
            }
            else
            {
                if (CurrentUserId < 0)
                {
                    ViewBag.linkMessage = installToken.UserId == null ? "'Oooooppss… No LFE account is connected to this plugin. Click here to connect an LFE account with this plugin'" : "'Oooooppss… You are not logged in. Click here to login with the LFE account associated with this plugin'";
                    ViewBag.linkUrl = "'#login'";
                }
                else
                {
                    if (installToken.UserId != null)
                    {
                        if (installToken.UserId == CurrentUserId)
                        {
                            stores = WidgetEndpointServices.GetOwnerStores(CurrentUserId).OrderBy(x => x.Name).ToArray();
                            userAuthorized = true;
                            //show panel
                        }
                        else
                        {
                            ViewBag.linkMessage = "'Oooooppss… You are attempting to login with an LFE account that is not connected to this application. Click here to connect with the LFE account associated with this plugin'";
                            ViewBag.linkUrl = "'#'";
                        }
                    }
                    else
                    {
                        // Show message"Connect account"
                        ViewBag.linkMessage = "'Oooooppss… No LFE account is connected to this application. Click here to connect your LFE account'";
                        ViewBag.linkUrl = "'#'";
                    }
                    
                }
            }

            var newStore = new BaseWebStoreDTO
            {
                 StoreId = -1
                ,TrackingID = Guid.NewGuid().ToString()
                ,DefaultCurrencyId = Constants.DEFAULT_CURRENCY_ID
            }; 

            

            ViewBag.currencies   = string.IsNullOrEmpty(currencies) ? "[]" : currencies;
            ViewBag.stores       = JsSerializer.Serialize(stores);
            ViewBag.user         = CurrentUserId;
            ViewBag.newStore     = JsSerializer.Serialize(newStore);
            ViewBag.callback     = callback;
            ViewBag.uid          = uid;
            ViewBag.status       = userAuthorized ? string.Empty : HttpStatusCode.Forbidden.ToString();
            Response.ContentType = "text/javascript";
            return View(); 
        }

        public ActionResult SaveNewStore(BaseWebStoreDTO token, string uid, int? srcId, string callback)
        {
            string error;
            token.StoreId = -1;

            //check source
            var plugin = WidgetEndpointServices.GetPluginInstallationDto(uid, out error);

            if (plugin != null) token.RegistrationSource = plugin.Type.PluginType2RegistrationSource();

            var saved = _pluginStoreServices.SaveStore(ref token, CurrentUserId,srcId, out error);

            ViewBag.result = JsSerializer.Serialize(new JsonResponseToken { success = saved, error = error });
            ViewBag.callback = callback;
            Response.ContentType = "text/javascript";
            return View();
        }


       
    }
}
