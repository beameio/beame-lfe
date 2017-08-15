using System;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Portal.Areas.UserPortal.Models;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class DiscussionController : UserPortal.Controllers.BaseController
    {
        private IUserPortalDiscussionServices _discussionServices { get; set; }
        private IUserNotificationServices _userNotificationServices { get; set; }
      
        public DiscussionController()
        {
            _discussionServices       = DependencyResolver.Current.GetService<IUserPortalDiscussionServices>();
            _userNotificationServices = DependencyResolver.Current.GetService<IUserNotificationServices>();
        }

        #region views
        public ActionResult GetCourseDiscussionPartial(int id,int courseId)
        {
            var token = new CourseDiscussionToken
            {
                RoomId = id
                ,CourseId = courseId
            };

            return PartialView("Discussion/_CourseDiscussion",token);
        }
        
        public ActionResult GetRoomFeedPartial(int id,int courseId)
        {
            var token = new CourseDiscussionToken
            {
                RoomId    = id
                ,CourseId = courseId
                ,Messages = _discussionServices.GetRoomMessages(id,DiscussionSortFields.AddOn,CommonEnums.SortDirections.desc).ToArray()
            };

            return PartialView("Discussion/_RoomFeed",token);
        }

        public ActionResult GetRoomFeedTreePartial(int roomId,int courseId,DiscussionSortFields? field,CommonEnums.SortDirections? dir)
        {
            var f = field ?? DiscussionSortFields.AddOn;
            var d = dir ?? CommonEnums.SortDirections.desc;

            var token = new CourseDiscussionToken
            {
                RoomId    = roomId
                ,CourseId = courseId
                ,Messages = _discussionServices.GetRoomMessages(roomId,f,d).ToArray()
            };

            return PartialView("Discussion/_RoomFeedTree",token);
        }

        public ActionResult MessageInputPartial(int roomId, int courseId, long? parentId)
        {
            var token = new DiscussionMessageInputDTO
            {
                RoomId           = roomId
                ,CourseId        = courseId
                ,ParentMessageId = parentId
            };

            return PartialView("Discussion/_MessageInputForm", token);
        }

        public ActionResult UserFeed()
        {
            return View();
        }

        public ActionResult Message(Guid id)
        {
            var dto = _discussionServices.GetMessageDTO(id);            

            return View(dto);
        }     

        public ActionResult Feed(string id)
        {
            var token = getHashtagFeedToken(id, DiscussionSortFields.AddOn, CommonEnums.SortDirections.desc,4,true,false);
            return View(token);
        }

        public ActionResult IFrameFeed(string id)
        {
            var token = getHashtagFeedToken(id, DiscussionSortFields.AddOn, CommonEnums.SortDirections.desc, 3,false,true);            
            return View(token);
        }

        public ActionResult IFrameMessage(Guid id)
        {
            var message = _discussionServices.GetMessageFeed(id).ToArray();
            var token = new HashtagFeedToken
            {
                Title               = string.Empty,
                Hashtag             = string.Empty,
                Messages            = message,
                HashtagId           = -1,
                Uid                 = id,
                UserCoursesPageSize = 3,
                ShowTitle           = false,
                ShowCloseBtn        = true
            };
            return View("IFrameFeed",token);
        }

        private HashtagFeedToken getHashtagFeedToken(string hashtag, DiscussionSortFields? field, CommonEnums.SortDirections? dir, int pageSize, bool showTitle, bool showCloseBtn, Guid? uid = null)
        {
            var data = new HashtagFeedToken
            {
                Title               = hashtag + " feed",
                Hashtag             = hashtag,
                HashtagId           = -1,
                UserCoursesPageSize = pageSize,
                ShowTitle           = showTitle,
                ShowCloseBtn        = showCloseBtn
            };

            if (!String.IsNullOrEmpty(hashtag))
            {
                long? hashtagId;
                string error;
                data.Messages = _discussionServices.GetHashtagFeed(hashtag.Replace("#", string.Empty), field ?? DiscussionSortFields.AddOn, dir ?? CommonEnums.SortDirections.desc, out hashtagId, out error).ToArray();
                data.HashtagId = hashtagId;
            }
            else if (uid != null)
            {
                data.Messages = _discussionServices.GetMessageFeed((Guid)uid).ToArray();
            }
            else
            {
                data.Messages = new MessageViewDTO[0];
            }

            return data;
        }

        public ActionResult GetHashFeedTreePartial(string q, DiscussionSortFields? field, CommonEnums.SortDirections? dir, int pageSize, bool showTitle, bool showCloseBtn,Guid? uid = null)
        {
            var token = getHashtagFeedToken(q, field, dir,pageSize,showTitle,showCloseBtn,uid);
            return PartialView("Discussion/_HashFeedTree",token);
        }

        public ActionResult GetHashFeedListPartial(string q, DiscussionSortFields? field, CommonEnums.SortDirections? dir, int pageSize, bool showTitle, bool showCloseBtn)
        {
            var token = getHashtagFeedToken(q, field, dir, pageSize, showTitle, showCloseBtn);
            return PartialView("Discussion/_HashFeedList", token);
        }
        #endregion

        #region api
        public JsonResult FindUsers(string q)
        {
            var list = _discussionServices.FindUsers(q).OrderBy(x => x.label).ToArray();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region posts
        [HttpPost]
        public JsonResult SaveMessage(DiscussionMessageInputDTO token)
        {
            string error;
            var saved = _discussionServices.SaveUserMessage(ref token,CurrentUserId, out error);
            MessageViewDTO dto = null;
            
            if (saved && token.MessageId >= 0 && token.Uid != null)
            {
                dto = _discussionServices.GetMessageDTO((Guid) token.Uid);

                //var newMessage = _userNotificationServices.GetNotificationToken((int)dto.MessageId);

                //if (newMessage != null)
                //{
                //    var hubContext = GlobalHost.ConnectionManager.GetHubContext<DiscussionNotificationHub>();

                //    hubContext.Clients.All.create(newMessage);
                //}

            }

            return Json(new JsonResponseToken
            {
                success = saved
                ,error  = error
                ,result = dto
            }, JsonRequestBehavior.AllowGet);

        }

        #endregion

    }
}
