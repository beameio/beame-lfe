using System;
using System.Web.Mvc;
using System.Web.Routing;
using LFE.Application.Services.Interfaces;
using LFE.DataTokens;

namespace LFE.Portal.Controllers
{
//    [System.Web.Http.RoutePrefix("api/wix/webstore")]
    public class WixStoreController : ApiBaseController
    {
        private readonly IWixApiWebStoreServices _webStoreServices;

        public WixStoreController()
        {
            _webStoreServices = DependencyResolver.Current.GetService<IWixApiWebStoreServices>();
        }


        [ActionName("update")]
        [HttpPost]
        public JsonResponseToken UpdateWebStoreChangeLog(int? id=null)
        {
            if (id == null)
            {
                return ErrorResponse("storeId required");
            }
            string error;
            _webStoreServices.UpdateWebStoreLog((int)id,out error);

            return new JsonResponseToken
            {
                success = string.IsNullOrEmpty(error)
                ,error  = error
            };
        }

        [ActionName("updatecourse")]
        [HttpPost]
        public JsonResponseToken UpdateCourseWebStoresChangeLog(int? id=null)
        {
            if (id == null)
            {
                return ErrorResponse("storeId required");
            }
            string error;
            _webStoreServices.UpdateCourseWebStoresLog((int)id, out error);

            return new JsonResponseToken
            {
                success = string.IsNullOrEmpty(error)
                ,error = error
            };
        }

        [ActionName("get")]
        [HttpGet]
        public JsonResponseToken GetLastWebStoreChange(Guid? id=null)
        {
            if (id == null)
            {
                return ErrorResponse("storeId required");
            }
            string error;
            var date = _webStoreServices.GetWebStoreLastUpdate((Guid)id, out error);

            return new JsonResponseToken
            {
                success = string.IsNullOrEmpty(error)
                ,result = date
                ,error = error
            };
        }

        [ActionName("getByTrack")]
        [HttpGet]
        public JsonResponseToken GetLastStoreChangeByTrackingId(string id=null)
        {
            if (id == null)
            {
                return ErrorResponse("storeId required");
            }
            string error;
            var date = _webStoreServices.GetWebStoreLastUpdate(id, out error);

            return new JsonResponseToken
            {
                success = string.IsNullOrEmpty(error)
                ,result = date
                ,error = error
            };
        }
      
    }
}
