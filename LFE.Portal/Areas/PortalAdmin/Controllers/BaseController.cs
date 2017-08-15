using System.Web.Mvc;
using LFE.Application.Services.Interfaces;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    [Authorize(Roles = "System,Admin")]
    public class BaseController : Portal.Controllers.BaseController
    {
        private readonly Portal.Controllers.AccountController _mainAccountController = new Portal.Controllers.AccountController();
        public IUserAccountServices UserAccountServices { get; set; }

        #region properties
        #endregion

        public BaseController()
        {
            if (!IsAdminRequestAuthorized())
            {
                _mainAccountController.SignUserOut();
            }

            UserAccountServices = DependencyResolver.Current.GetService<IUserAccountServices>();
        }        
    }
}
