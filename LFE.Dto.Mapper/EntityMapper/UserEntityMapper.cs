using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;
using System.IO;

namespace LFE.Dto.Mapper.EntityMapper
{
	public static class UserEntityMapper
	{
		public static USER_Courses NewUserCourseEntity(int courseId,int orderLineId, int userId, int? userBundleId, BillingEnums.eAccessStatuses status,DateTime? validUntil)
		{
			return new USER_Courses
			{
				UserId          = userId
				,OrderLineId    = orderLineId
				,CourseId       = courseId
				,UserBundleId   = userBundleId
				,StatusId       = (byte)status
				,ValidUntil     = validUntil
				,AddOn          = DateTime.Now
				,CreatedBy      = DtoExtensions.CurrentUserId
			};
		}

		public static USER_Bundles NewUserBundleEntity(int bundleId,int orderLineId, int userId, BillingEnums.eAccessStatuses status)
		{
			return new USER_Bundles
			{
				UserId          = userId
				,OrderLineId    = orderLineId
				,BundleId       = bundleId
				,StatusId       = (byte)status
				,AddOn          = DateTime.Now
				,CreatedBy      = DtoExtensions.CurrentUserId
			};
		}

		public static Users RegisterDto2UsersEntity(this RegisterDTO token,int? storeId) //, string passwordDigest = null
		{
			return new Users
			{
					Email                              = token.Email
					,FirstName                         = token.FirstName
					,LastName                          = token.LastName
					,Nickname                          = token.Nickname
					,RegistrationTypeId                = (byte)token.RegistrationSource
					,RegisterStoreId                   = storeId
					,AffiliateCommission               = Constants.AFFILIATE_COMMISSION_DEFAULT
					//TODO Obsolete fields , remove when move 2 SMP done
					,FacebookID                       = token.FbUid
					,PasswordDigest                    = null
					,StatusType                        = (int)UserEnums.eUserStatuses.active
					,UserTypeID                        = (int)UserEnums.eUserTypes.NormalUser
					//TODO end obsolete
					,Created                           = DateTime.Now
					,LastModified                      = DateTime.Now
					,AutoplayEnabled                   = true
					,DisplayActivitiesOnFB             = true
					,DisplayCourseNewsWeeklyOnFB       = true
					,DisplayDiscussionFeedDailyOnFB    = true
					,ReceiveCourseNewsWeeklyOnEmail    = true
					,ReceiveDiscussionFeedDailyOnEmail = true
					,ReceiveMonthlyNewsletterOnEmail   = true       
					,RegisterHostName                  = token.RegisterHostName
			};
		}

		public static UserNotifications UserEntity2UserNotification(this Users entity,long messageId,bool sendEmail,bool post2FB)
		{
			return new UserNotifications
				{
					UserId          = entity.Id
					,MessageId      = messageId
					,FbPostRequired = post2FB && !String.IsNullOrEmpty(entity.FacebookID)
					,EmailRequired  = sendEmail
					,AddOn          = DateTime.Now
					,IsRead         = false
				};
		}
		
		public static void UpdateAcountEntity(this Users entity, AccountSettingsDTO token)
		{
			entity.FirstName           = token.FirstName;
			entity.LastName            = token.LastName;
			entity.Nickname            = token.Nickname;
			entity.BioHtml             = token.BioHtml;
			entity.PictureURL          = token.PictureName;
			entity.AffiliateCommission = token.AffiliateCommission ?? entity.AffiliateCommission;
		}

		public static void UpdateAcountEntity(this Users entity, WizardAboutAuthorDTO token)
		{
			entity.BioHtml    = token.BioHtml;
			entity.PictureURL = token.PictureName;
		}        

		public static void UpdateCommunicationSettings(this Users entity, AccountSettingsDTO token)
		{
			entity.DisplayActivitiesOnFB             = token.DisplayActivitiesOnFB;
			entity.DisplayCourseNewsWeeklyOnFB       = token.DisplayCourseNewsWeeklyOnFB;
			entity.DisplayDiscussionFeedDailyOnFB    = token.DisplayDiscussionFeedDailyOnFB;
			entity.ReceiveMonthlyNewsletterOnEmail   = token.ReceiveMonthlyNewsletterOnEmail;
			entity.ReceiveDiscussionFeedDailyOnEmail = token.ReceiveDiscussionFeedDailyOnEmail;
			entity.ReceiveCourseNewsWeeklyOnEmail    = token.ReceiveCourseNewsWeeklyOnEmail;
		}

		
        public static USER_Videos InterfaceRecord2UserVideoEntity(this UserS3FileInterface entity, int userId)
        {
            if (entity.BcIdentifier == null) return null;

			return new USER_Videos
			{
				BcIdentifier        = (long)entity.BcIdentifier
				,Name               = Path.GetFileName(entity.FilePath)
				,ThumbUrl           = ""
				,Duration           = ""
				,UserId             = userId
                ,VideoStillUrl      = ""
			    ,Length             = 0
			    ,ReferenceId        = entity.BcRefId
			    ,PlaysTotal         = 0
			    ,Tags               = entity.Tags
                ,ShortDescription   = Path.GetFileNameWithoutExtension(entity.FilePath)
				,Attached2Chapter   = false
                ,S3Url              = Constants.S3_ROOT_URL + Constants.S3_VIDEO_BUCKET_NAME + entity.FilePath
                ,CreationDate       = DateTime.Now
				,InsertDate         = DateTime.Now
			};
		}

//	    private static string TagsToString(this IEnumerable<string> tags)
//	    {
//	        return tags.Aggregate("", (current, tag) => current + $"{tag},");
//	    }
	
		public static void UpdateFbId(this Users entity, string uid)
		{
			entity.FacebookID = uid;
			entity.LastModified = DateTime.Now;
		}

		public static USER_VideoStats StatsToken2UserVideoStats(this VideoStatsToken token,int userId,long bcId)
		{
			return new USER_VideoStats
			{
				UserId         = userId
				,SessionId     = Guid.NewGuid()
				,BcIdentifier  = bcId
				,ChapterId     = token.ChapterId
				,StartDate     = DateTime.Now
				,StartPosition = token.position
				,StartReason   = token.startReason
			};
		}

		public static void UpdateVideoStatsEntity(this USER_VideoStats entity, VideoStatsToken token)
		{
			entity.EndDate      = DateTime.Now;
			entity.EndPosition  = token.position;
			entity.TotalSeconds = token.position - entity.StartPosition;
			if (!String.IsNullOrEmpty(token.endReason)) entity.EndReason = token.endReason;
		}

		public static void UpdateVideoStatsEntityReason(this USER_VideoStats entity, VideoStatsReasonToken token)
		{
			if (!String.IsNullOrEmpty(token.startReason)) entity.StartReason = token.startReason;
			if (!String.IsNullOrEmpty(token.endReason)) entity.EndReason = token.endReason;
		}

		public static USER_Logins LoginLog2USER_Logins(this LoginLogoutLogToken token)
		{
			return new USER_Logins
			{
				HostName     = token.HostName,
				LoginDate    = DateTime.Now,
				NetSessionId = token.SessionId,
				UserId       = token.UserId
			};
		}
	}
}
