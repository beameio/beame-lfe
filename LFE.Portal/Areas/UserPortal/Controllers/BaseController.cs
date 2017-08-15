using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Portal.Areas.UserPortal.Helpers;
using LFE.Portal.Areas.UserPortal.Models;

namespace LFE.Portal.Areas.UserPortal.Controllers
{
    public class BaseController : Portal.Controllers.BaseController
    {
        private readonly IUserPortalServices _userPortalServices;

        #region properties
         #endregion

        public BaseController()
        {
             _userPortalServices = DependencyResolver.Current.GetService<IUserPortalServices>();
        }

        #region user
        public UserProfilePageToken GetUserProfileDto(int id,int pageSize)
        {
            try
            {
                var dto = new UserProfilePageToken
                    {
                        LearnerCourses = _userPortalServices.GetLearnerCourses(id, CurrentUserId).Select(x => x.SetCoursePageUrl(null)).OrderByDescending(x => x.LearnerCount).ThenBy(x => x.Name).ToList(),
                        AuthorCourses  = _userPortalServices.GetAuthorCourses(id, CurrentUserId).Select(x => x.SetCoursePageUrl(null)).OrderByDescending(x => x.LearnerCount).ThenBy(x => x.Name).ToList(),
                        PageSize       = pageSize
                    };



                var token = _userPortalServices.GetUserProfileDto(id);
                var profile = new UserProfileCartToken
                {
                    Profile = token
                    ,TotalLearn = dto.LearnerCourses.Count
                    ,TotalTeach = dto.AuthorCourses.Count
                };

                dto.ProfileCart = profile;

                dto.IsValid = true;

                return dto;
            }
            catch (Exception ex)
            {
                Logger.Error("Get User profile Dto",ex,id,CommonEnums.LoggerObjectTypes.UserAccount);

                return new UserProfilePageToken
                {
                    IsValid = false
                    ,Message = Utils.FormatError(ex)
                };
            }
        }
        #endregion


        #region common controller services      
        #endregion

        public string RenderPartialToString(Controller controller, string viewName, object model)
        {

            try
            {
                controller.ViewData.Model = model;

                using (var sw = new StringWriter())
                {
                    var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                    var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                    viewResult.View.Render(viewContext, sw);

                    return sw.GetStringBuilder().ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
