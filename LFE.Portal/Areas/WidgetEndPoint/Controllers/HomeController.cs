using System;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Portal.Areas.WidgetEndPoint.Models;


namespace LFE.Portal.Areas.WidgetEndPoint.Controllers
{
    
    public class HomeController : BaseController
    {
        private readonly Portal.Controllers.AccountController _mainAccountController = new Portal.Controllers.AccountController();

        public ActionResult Index(string id)
        {
            var token = new PluginIndexToken ();

            if (String.IsNullOrEmpty(id))
            {
                token.IsValid = false;
                token.Message = "InstallationId required";

                return View(token);
            }

            string error;
            var plugin = WidgetEndpointServices.GetPluginInstallationDto(id, out error);

            if (plugin == null)
            {
                token.IsValid = false;
                token.Message = error;

                return View(token);
            }

            var registrationType = Utils.ParseEnum<CommonEnums.eRegistrationSources>(plugin.Type.ToString());

            token.RegistrationSource = registrationType;
            token.Uid              = id;
            token.IsValid          = true;

            if (!User.Identity.IsAuthenticated) return View(token);

            if (plugin.UserId != null && plugin.UserId == CurrentUserId)
            {

                return View("Welcome",new PluginIndexToken{IsValid = true,Uid = id});
            }
            
            _mainAccountController.SignUserOut();
            return View(token);
        }

        [PlaginAuthorize]
        public ActionResult Welcome(string id)
        {
            return View(new PluginIndexToken { IsValid = true, Uid = id });
        }

        [PlaginAuthorize]
        public ActionResult PluginContent(string id)
        {
            return View(new PluginIndexToken { IsValid = true, Uid = id });
        }

        [PlaginAuthorize]
        public ActionResult SavePluginOwner(string id)
        {
            var error = String.IsNullOrEmpty(id) ? "UID Required" : null;

            var token = new PluginIndexToken
            {
                Uid = id                
            };

            if (error == null)
            {
                var result = WidgetEndpointServices.VerifyPluginOwner(id, out error);
                if (result)
                {
                    token.IsValid = true;
                    return View("Welcome", token);
                }
            }

            _mainAccountController.SignUserOut();

            token.IsValid = false;
            token.Message = error;

            return View("Index", token);
        }
    }

   
}

