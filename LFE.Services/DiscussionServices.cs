using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LFE.Application.Services.Base;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;

namespace LFE.Application.Services
{
	public class DiscussionServices : ServiceBase, IAuthorAdminDiscussionServices, IUserPortalDiscussionServices
	{
		#region private params
		private readonly IFacebookServices _facebookServices;
		private readonly IEmailServices _emailServices;
		private const int CURRENT_MSG_HTML_VERSION = 2;
		//private const string FEED_LINK_V1 = "<span class=\"btn\" title=\"click to view\" onclick=\"onMessageClickEvent('{0}',{1},'{2}')\">{3}{4}</span>";
		private const string FEED_LINK_V2 = "<span data-val=\"{1}\" data-kind=\"{2}\" class=\"msg-btn\" title=\"click to view\" onclick=\"onMessageClickEvent('{0}',{1},'{2}')\">{3}{4}</span>";
		#endregion

		public DiscussionServices()
		{           
		   _facebookServices = DependencyResolver.Current.GetService<IFacebookServices>();
		   _emailServices = DependencyResolver.Current.GetService<IEmailServices>();
		}

		#region IAuthorAdminDiscussionServices implementation
		
		public List<BaseListDTO> AuthorRoomsLOV(int authorId)
		{
			return DiscussionClassRoomRepository.GetMany(x => x.AuthorId == authorId).Select(x => x.RoomEntity2BaseListDto()).ToList();
		}

		public List<AuthorRoomListDTO> GetAuthorClassRooms(int authorId)
		{
			return DiscussionClassRoomRepository.GetAuthorRooms(authorId).Select(x => x.Entity2AuthorRoomListDto()).ToList();
		}

		public DiscussionClassRoomDTO GetClassRoomDTO(int roomId)
		{
			return roomId < 0 ? new DiscussionClassRoomDTO() : DiscussionClassRoomRepository.GetById(roomId).Entity2ClassRoomDto();
		}

		public bool SaveClassRoom(ref DiscussionClassRoomDTO dto, int authorId,int userId, out string error)
		{
			return _SaveClassRoom(ref dto, authorId, userId, out error);
		}
	  
		public bool DeleteRoom(int roomId, out string error)
		{
			error = string.Empty;

			try
			{
				var entity = DiscussionClassRoomRepository.GetById(roomId);

				if (entity == null)
				{
					error = "Room entity not found";
					return false;
				}

				DiscussionClassRoomRepository.Delete(entity);

				DiscussionClassRoomRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("delete room", roomId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return false;
			}
		}
		
		public List<BaseListDTO> GetRoomsCourses(int roomId)
		{
			return CourseRepository.GetMany(x => x.ClassRoomId == roomId).Select(x => x.CourseEntity2BaseListDto()).ToList();
		}
		#endregion

		#region IUserPortalDiscussionServices implementation
		
		#region private helpers
		private bool SaveMessageHtml(long messageId,string template, out string error)
		{
			error = string.Empty;
			try
			{
				var messageEntity = DiscussionMessageRepository.GetById(messageId);

				if (messageEntity == null)
				{
					error = "message entity not found";
					return false;
				}

				var original = messageEntity.Text;
				var html = original;
				var emailHtml = original;
				//tags
				var tags = DiscussionMessageHashtagViewRepository.GetMany(x => x.MessageId == messageId);

				foreach (var tag in tags)
				{
					var link = String.Format(template, messageEntity.Uid, tag.HashtagId, eFeedFilterKinds.Hashtag, "#", tag.HashTag);
					html     =  html.Replace("#" + tag.HashTag, link);

					link      = tag.HashTag.String2Html(HASHTAG_URL_TEMPLATE, HASHTAG_HTML_STYLE_UNDERLINE, tag.HashTag.Replace("#",""));
					emailHtml = emailHtml.Replace(tag.HashTag, link);
				}

				//users
				var users = DiscussionMessageUserRepository.GetMany(x => x.MessageId == messageId);

				foreach (var row in users)
				{
					var user = UserRepository.GetById(row.UserId).Entity2TagItemDto();
					var link = String.Format(template, messageEntity.Uid, user.value, eFeedFilterKinds.User, "@", user.label);
					html     = html.Replace("@" + user.label, link);

					link      = user.label.String2Html(USER_PROFILE_URL_TEMPLATE, NAME_HTML_STYLE_UNDERLINE, user.value);
					emailHtml = emailHtml.Replace("@" + user.label, link);
				}

				messageEntity.HtmlMessage      = html;
				messageEntity.HtmlEmailMessage = emailHtml;
				messageEntity.HtmlVersion      = CURRENT_MSG_HTML_VERSION;

				DiscussionMessageRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("SaveMessageHtml", messageId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return false;
			}
		}

		private bool SaveMessageHashtags(IEnumerable<string> tags, long messageId, out string error)
		{
			error = string.Empty;
			try
			{
				
				foreach (var tag in tags.Distinct())
				{
					if (String.IsNullOrEmpty(tag) || tag.Length == 1) continue;

					var name = tag.Substring(1);

					var entity = DiscussionHashtagRepository.Get(x => x.HashTag.ToLower() == name.ToLower());

					if (entity == null) continue;

					if (DiscussionMessageHashtagRepository.IsAny(x => x.MessageId == messageId && x.HashtagId == entity.HashtagId)) continue;

					DiscussionMessageHashtagRepository.Add(new DSC_MessageHashtags
					{
						MessageId = messageId
					   ,HashtagId = entity.HashtagId
					});

				}

				DiscussionMessageHashtagRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				DiscussionMessageHashtagRepository.UnitOfWork.RollbackChanges();
				error = Utils.FormatError(ex);
				Logger.Error("SaveMessageHashtags", messageId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return false;
			}
		}

		private bool SaveMessageUsers(IEnumerable<string> names, long messageId, out string error)
		{
			error = string.Empty;
			try
			{
			  
				foreach (var name in names.Distinct())
				{
					if (String.IsNullOrEmpty(name) || name.Length == 1) continue;

					var userIdStr = name.Substring(1);
					int userId;

					var parsed = int.TryParse(userIdStr, out userId);

					if (!parsed) continue;

					var entity = UserRepository.GetById(userId);

					if (entity == null) continue;

					if (DiscussionMessageUserRepository.IsAny(x => x.MessageId == messageId && x.UserId == userId)) continue;

					DiscussionMessageUserRepository.Add(new DSC_MessageUsers
					{
						MessageId = messageId
						,UserId = userId
					});

				}

				DiscussionMessageUserRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				DiscussionMessageUserRepository.UnitOfWork.RollbackChanges();
				error = Utils.FormatError(ex);
				Logger.Error("SaveMessageUsers", messageId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return false;
			}
		}

		private bool SaveHashtags(IEnumerable<string> tags, int userId,out string error)
		{
			error = string.Empty;
			try
			{
			  
				foreach (var tag in tags.Distinct())
				{
					if (String.IsNullOrEmpty(tag) || tag.Length == 1) continue;

					var name = tag.Substring(1);

					var exists = DiscussionHashtagRepository.IsAny(x => x.HashTag.ToLower() == name.ToLower());

					if (exists) continue;

					DiscussionHashtagRepository.Add(new DSC_Hashtags
					{
						HashTag    = name
						,AddOn     = DateTime.Now
						,CreatedBy = userId
					});                   
				}

				DiscussionHashtagRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				DiscussionHashtagRepository.UnitOfWork.RollbackChanges();
				error = Utils.FormatError(ex);
				Logger.Error("save message hash tags", userId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return false;
			}
		}

		private bool SaveRoomFollower(int userId,int roomId,out string error)
		{
			error = string.Empty;
			try
			{
				if (DiscussionFollowersRepository.IsAny(x => x.FollowerId == userId && x.ClassRoomId == roomId)) return true;

				DiscussionFollowersRepository.Add(new DSC_Followers
				{
					ClassRoomId = roomId
					,FollowerId = userId
					,AddOn = DateTime.Now
					,CreatedBy = userId
				});

				DiscussionFollowersRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save follower", userId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return false;
			}
		}

		private void SaveMessageNotifications(long messageId,int userId, out string error)
		{
			error = string.Empty;
			try
			{
				//followers notification
				//var followers = FollowerRepository.GetMany(x => x.ClassRoomId == roomId && x.FollowerId != userId).ToList();

				//foreach (var follower in followers)
				//{
				//    _notificationServices.SaveNotification(follower.FollowerId, messageId, out error);
				//}
				
				//message users notifications
				var users = DiscussionMessageUserRepository.GetMany(x => x.MessageId == messageId).ToList();

				foreach (var user in users.Where(user => user.UserId != userId))
				{
					_SaveNotification(user.UserId, messageId, out error);
				}
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save message notification", messageId, ex, CommonEnums.LoggerObjectTypes.Discussion);
			}
		}

		private void SaveReplayMessageNotifications(long messageId,long parentId,out string error)
		{
			try
			{
				var userId = DiscussionMessageRepository.GetById(parentId).CreatedBy;

				_SaveNotification(userId, messageId, out error);
			   
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save replay message notification", messageId, ex, CommonEnums.LoggerObjectTypes.Discussion);
			}
		}

		private void SaveFbPostMessages(long messageId,int courseId,int writerId)
		{
			try
			{
				var notifications = DiscussionMessageRepository.GetMessageNotifications4FB(messageId).ToList();

				//foreach (var notification in notifications)
				//{
				//    var token = notification.Entity2PostMessageDto();

				//    if (token == null) continue;

				//    string error;
				//    _facebookServices.SavePostMessage(token, out error, notification.NotificationId);
				//}

				//save writer story
				_facebookServices.CreateUserFbStory(writerId, courseId, FbEnums.eFbActions.comment);
			}
			catch (Exception ex)
			{
				Logger.Error("post message notification to FB", messageId, ex, CommonEnums.LoggerObjectTypes.Discussion);
			}
		}

		private void SaveMessageEmails(long messageId)
		{
			var messages = _emailServices.GetMessageNotifications(messageId).ToList();

			foreach (var token in messages.ToList())
			{
				_emailServices.SaveEmailNotificationRecord(token);
			}
		}

		private long? FindHashtagByName(string q)
		{
			var entity = DiscussionHashtagRepository.Get(x => x.HashTag.ToLower() == q.ToLower());
			return entity == null ? (long?) null : entity.HashtagId;
		}
		#endregion

		#region interface implementation
		
		public List<UserTagItemDTO> FindUsers(string q)
		{
			const string cacheKey = "USER_LOV";

			var result = GetCachedListByKey<UserTagItemDTO>(cacheKey, CacheProxy);

			if (result != null) return result.Where(x => x.label.ToLower().Contains(q.ToLower())).ToList();

			result = UserRepository.GetAll().Select(x => x.Entity2TagItemDto()).ToList();

			CacheProxy.Add(cacheKey, result, DateTimeOffset.Now.AddMinutes(10));

			return result.Where(x => x.label.ToLower().Contains(q.ToLower())).ToList();
		}

		public bool SaveUserMessage(ref DiscussionMessageInputDTO token, int userId, out string error)
		{
			try
			{
				//var oid = FbServices.GetCourseFbObjectId(token.CourseId);

				token.HTMLVersion = CURRENT_MSG_HTML_VERSION;

				var entity = token.Dto2MessageEntity(userId);

				DiscussionMessageRepository.Add(entity);

				DiscussionMessageRepository.UnitOfWork.CommitAndRefreshChanges();

				var messageId = entity.MessageId;

				var names = String.IsNullOrEmpty(token.NamesArrayStr) ? new string[0] : JsSerializer.Deserialize<string[]>(token.NamesArrayStr);

				var tags = String.IsNullOrEmpty(token.TagsArrayStr) ? new string[0] : JsSerializer.Deserialize<string[]>(token.TagsArrayStr);

				if (tags.Length > 0)
				{
					if (!SaveHashtags(tags, userId, out error)) return false;

					if (!SaveMessageHashtags(tags, messageId, out error)) return false;
				}

				if (names.Length > 0)
				{
					if (!SaveMessageUsers(names, messageId, out error)) return false;
				}

				if (!SaveMessageHtml(messageId,FEED_LINK_V2, out error)) return false;

				if(!SaveRoomFollower(userId,token.RoomId,out error)) return false;

				SaveMessageNotifications(messageId, userId, out error);//token.RoomId,

				if (token.ParentMessageId != null)
				{
					SaveReplayMessageNotifications(messageId,(long)token.ParentMessageId,out error);
				}

				SaveMessageEmails(messageId);

				//UnitOfWork.CommitAndRefreshChanges();

				SaveFbPostMessages(messageId,token.CourseId,userId);

				token.MessageId = messageId;
				token.Uid       = entity.Uid;

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);

				Logger.Error("save user message", token.RoomId, ex, CommonEnums.LoggerObjectTypes.Discussion);

				return false; 
			}
		}

		public List<MessageViewDTO> GetRoomMessages(int roomId, DiscussionSortFields field, CommonEnums.SortDirections dir)
		{
			try
			{
				var messages = DiscussionClassRoomRepository.GetRoomMessages(roomId).Select(s => s.Entity2MessageViewDto()).ToList();
				return BuildMessageFeedTree(messages.ToArray(), field, dir);			
			}
			catch (Exception ex)
			{
				Logger.Error("get room  feed", roomId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return new List<MessageViewDTO>();
			}
		}

		public List<MessageViewDTO> GetHashtagFeed(string hashtag, DiscussionSortFields field, CommonEnums.SortDirections dir, out long? hashtagId, out string error)
		{
			error = string.Empty;
			hashtagId = null;
			try
			{
				hashtagId = FindHashtagByName(hashtag);

				if (hashtagId == null)
				{
					error = "Hashtag " + hashtag + " not found";
					return new List<MessageViewDTO>();
				}

				var messages = DiscussionClassRoomRepository.GetHashtagMessages((long)hashtagId).Select(s => s.Entity2MessageViewDto()).ToList();

				return BuildMessageFeedTree(messages.ToArray(), field, dir);
				
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("get hashtag " + hashtag + " feed", null, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return new List<MessageViewDTO>();
			}
		}

		public List<MessageViewDTO> GetMessageFeed(Guid uid)
		{
			try
			{
				var entity = DiscussionMessageRepository.Get(x => x.Uid == uid);

				if(entity == null) return new List<MessageViewDTO>();

				var messageId = entity.ParentMessageId ?? entity.MessageId;

				var messages = DiscussionMessageRepository.GetMessageFeed(messageId).Select(s => s.Entity2MessageViewDto()).ToList();
				
				return BuildMessageFeedTree(messages.ToArray(), DiscussionSortFields.AddOn, CommonEnums.SortDirections.desc);				
			}
			catch (Exception ex)
			{
				Logger.Error("get message  feed", uid, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return new List<MessageViewDTO>();  
			}
		}

		private static List<MessageViewDTO> BuildMessageFeedTree(MessageViewDTO[] unsorted,DiscussionSortFields field, CommonEnums.SortDirections dir)
		{
			var parents = unsorted.Where(x => x.ParentMessageId == null).ToList();

			var sortedParents = new List<MessageViewDTO>();

			switch (field)
			{
				case DiscussionSortFields.AddOn:
					switch (dir)
					{
						case CommonEnums.SortDirections.desc:
							sortedParents = parents.OrderByDescending(x => x.AddOn).ToList();
							break;
						case CommonEnums.SortDirections.asc:
							sortedParents = parents.OrderBy(x => x.AddOn).ToList();
							break;
					}
					break;
				case DiscussionSortFields.CreatorName:
					switch (dir)
					{
						case CommonEnums.SortDirections.desc:
							sortedParents = parents.OrderByDescending(x => x.CreatorName).ToList();
							break;
						case CommonEnums.SortDirections.asc:
							sortedParents = parents.OrderBy(x => x.CreatorName).ToList();
							break;
					}
					break;
				case DiscussionSortFields.CourseName:
					switch (dir)
					{
						case CommonEnums.SortDirections.desc:
							sortedParents = parents.OrderByDescending(x => x.CourseName).ToList();
							break;
						case CommonEnums.SortDirections.asc:
							sortedParents = parents.OrderBy(x => x.CourseName).ToList();
							break;
					}
					break;
			}


			foreach (var parent in sortedParents)
			{
				var p = parent;

				foreach (var child in unsorted.Where(x => x.ParentMessageId != null && x.ParentMessageId == p.MessageId).OrderByDescending(x => x.PostedOn))
				{
					p.Replies.Add(child);
				}
			}

			return sortedParents;
		}

		public HashTagDTO GetHashTagDto(long tagId)
		{
			var entity = DiscussionHashtagRepository.GetById(tagId);

			return entity == null ? new HashTagDTO() : entity.Entity2HashTagDto();
		}

		public MessageViewDTO GetMessageDTO(Guid uid)
		{
			try
			{
				return DiscussionMessageRepository.GetMessage(uid).Entity2MessageViewDto();
			}
			catch (Exception ex)
			{
				Logger.Error("get message dto", uid, ex, CommonEnums.LoggerObjectTypes.Discussion);

				return null; 
			}
		}       
		
		public int? FindAuthorClassRoom(string name, int authorId)
		{
			return _FindAuthorClassRoom(name, authorId);
		} 
		
		#endregion
		#endregion        
	}
}
