using System;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.DataTokens;


namespace LFE.Portal.Controllers
{
//    [System.Web.Http.RoutePrefix("api/wix/course")]
    public class WixCourseController : ApiBaseController
    {
        private readonly IWixApiCourseServices _courseServices;

        public WixCourseController()
        {
            _courseServices = DependencyResolver.Current.GetService<IWixApiCourseServices>();
        }

         //[System.Web.Http.Authorize]
        [HttpPost, ActionName("updatelog")]
        public JsonResponseToken UpdateCourseChangeLog(int? id=null)
        {
            if (id == null)
            {
                return ErrorResponse("courseId required");
            }
            string error;
            _courseServices.UpdateCourseChangeLog((int)id,out error);

            return new JsonResponseToken
            {
                success = string.IsNullOrEmpty(error)
                ,error  = error
            };
        }     

        [HttpGet, ActionName("get")]
        public JsonResponseToken GetLastCourseChange(Guid? id=null)
        {
            if (id == null)
            {
                return ErrorResponse("course uid required");
            }
            string error;
            var date = _courseServices.GetCourseLastUpdate((Guid)id, out error);

            return new JsonResponseToken
            {
                success = string.IsNullOrEmpty(error)
                ,result = date
                ,error = error
            };
        }

    }
}
