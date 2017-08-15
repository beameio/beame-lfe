using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Portal.Areas.AuthorAdmin.Models;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    public class DiscussionController : BaseController
    {
        private readonly IAuthorAdminDiscussionServices _discussionServices;

        public DiscussionController()
        {
            _discussionServices = DependencyResolver.Current.GetService<IAuthorAdminDiscussionServices>();
        }

        #region views
        public ActionResult AuthorClassRooms()
        {
            return View();
        }

        public ActionResult ClassRoomEditPartial(int id)
        {
            var token = new RoomManagePageToken
            {
                RoomDto  = _discussionServices.GetClassRoomDTO(id)
                ,Courses = id >= 0 ? _discussionServices.GetRoomsCourses(id).OrderBy(x=>x.name).ToList() : new List<BaseListDTO>()
            };

            return PartialView("Discussion/_EditClassRoom", token);
        }

        public ActionResult RoomCoursesTooltip(int? id)
        {
            var list = id!=null ? _discussionServices.GetRoomsCourses((int) id) : new List<BaseListDTO>();

            return PartialView("Discussion/_RoomCoursesTooltipList", list);
        }
        #endregion
        
        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAuthorClassRooms([DataSourceRequest] DataSourceRequest request)
        {
            if (CurrentUserId < 0) return RedirectToAction("NonAuthorized", "Error");

            var list = _discussionServices.GetAuthorClassRooms(CurrentUserId).OrderBy(x => x.Name).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region post
        [HttpPost]
        public ActionResult SaveClassRoom(DiscussionClassRoomDTO RoomDto)
        {
            if (CurrentUserId < 0) return RedirectToAction("NonAuthorized", "Error");

            if (RoomDto != null && ModelState.IsValid)
            {
                string error;

                var isNew = RoomDto.RoomId == -1;
                
                var result = _discussionServices.SaveClassRoom(ref RoomDto,CurrentUserId,CurrentUserId, out error);

                if (RoomDto.RoomId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

                if (isNew)
                {
                    SaveUserEvent(CommonEnums.eUserEvents.ROOM_CREATED, String.Format("Class Room \"{0}\" created for author \"{1}\")", RoomDto.Name,User.Identity.Name));
                }

                return Json(new JsonResponseToken
                {
                    success = result
                   ,result = new
                       {
                           id    = RoomDto.RoomId
                           ,name = RoomDto.Name
                       }
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyRoom([DataSourceRequest] DataSourceRequest request, AuthorRoomListDTO dto)
        {
            if (dto != null)
            {
                string error;
                _discussionServices.DeleteRoom(dto.RoomId, out error);
            }

            return Json(new[] { dto }.ToDataSourceResult(request, ModelState));
        }
        #endregion
    }
}
