using System;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.DataTokens;
using LFE.Portal.Helpers;
using WebMatrix.WebData;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    [Authorize]
    public class BaseController : Portal.Controllers.BaseController
    {
        #region properties
        
        public IAuthorAdminServices BaseAuthorServices { get; private set; } 
        #endregion

        public BaseController()
        {
          BaseAuthorServices = DependencyResolver.Current.GetService<IAuthorAdminServices>();         
        }

        #region user
        public UserViewDto GetCurrentUser()
        {
            try
            {
                if (!WebSecurity.IsAuthenticated) return null;

                var user = this.CurrentUser();

                return BaseAuthorServices.FindUsers(user.UserId, null, null).FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }           
        }    
        #endregion


        #region common controller services       
        #endregion

       
    }
}
