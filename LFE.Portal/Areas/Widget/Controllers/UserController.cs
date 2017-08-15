using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Utils;
using LFE.Portal.Areas.UserPortal.Models;
using LFE.Portal.Helpers;
using LFE.Portal.Models;
using WebMatrix.WebData;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class UserController : BaseController
    {
        private readonly IWidgetUserServices _widgetUserServices;
        private readonly IWidgetServices _widgetServices;
        private readonly IStudentSertificateCervices _studentSertificateCervices;

        public UserController()
        {
            _studentSertificateCervices = DependencyResolver.Current.GetService<IStudentSertificateCervices>();
            _widgetUserServices = DependencyResolver.Current.GetService<IWidgetUserServices>();
            _widgetServices = DependencyResolver.Current.GetService<IWidgetServices>();           
        }

        private UserIndicatorViewModel GetUserView()
        {
            var model = new UserIndicatorViewModel {IsLoggedIn = false};

            if (!WebSecurity.IsAuthenticated) return model;

            var user = this.CurrentUser();

            if (user == null) return model;

            model.IsLoggedIn  = true;
            model.Id          = user.UserId;
            model.Email       = user.Email;
            model.DisplayName = user.FullName;
            model.LastLogin   = user.LastLogin;

            return model;
        }

        public ActionResult Certificate(string key)
        {
            var token = _studentSertificateCervices.GetStudentCertificate(key);

            return View("~/Areas/Widget/Views/StudentCertificate.cshtml", token);
        }

        //[ChildActionOnly]
        public ActionResult _UserIndicator()
        {
            return PartialView("~/Areas/Widget/Views/Shared/Account/_UserLoginLinks.cshtml", GetUserView());
        }

        //[ChildActionOnly]
        public ActionResult _UserIndicatorMainSite()
        {  
            return PartialView("~/Areas/Widget/Views/Shared/Account/_UserLoginLinksMainSite.cshtml", GetUserView());
        }
        public ActionResult _AuthorProfile(int? id)
        {

            var token = id == null ? new AuthorProfilePageToken
                                                                {
                                                                    IsValid = false
                                                                    ,Message = "authorId required"
                                                                } 
                                    : GetAuthorProfilePageToken((int)id);
            
            return PartialView("User/_AuhtorProfile", token);
        }
    }
}
