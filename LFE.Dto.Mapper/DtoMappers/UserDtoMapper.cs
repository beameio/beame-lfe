using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class UserDtoMapper 
    {

        private static int CalculateActivityScore(this int userId, int courses,
                                                        int bundles,
                                                        int chapters,
                                                        int videos,
                                                        int logins,
                                                        int purchases,
                                                        int stores)
        {
            return bundles + courses + chapters + videos + purchases + stores;
        }

        public static int CalculateActivityScore(this USER_StatisticToken entity)
        {
            return entity.UserId.CalculateActivityScore(entity.courses,entity.bundles,entity.chapters,entity.videos,entity.logins,entity.purchases,entity.stores);
        }

        public static UserStatisticToken Entoty2UserStatisticToken(this USER_StatisticToken entity)
        {
            return new UserStatisticToken
            {
                UserId     = entity.UserId
                ,FullName  = entity.Entity2FullName()
                ,FirstName = entity.FirstName
                ,LastName  = entity.LastName
                ,Email     = entity.Email
                ,courses   = entity.courses
                ,bundles   = entity.bundles
                ,chapters  = entity.chapters
                ,videos    = entity.videos
                ,purchases = entity.purchases
                ,stores    = entity.stores
                ,logins    = entity.logins
                ,Score     = entity.CalculateActivityScore()
            };
        }

        public static UserGridViewDto LoginEntity2UserGridViewDto(this vw_USER_UserLogins entity)
        {
            return new UserGridViewDto
            {
                UserProfileId     = entity.Id
                ,UserId           = entity.UserId
                ,Email            = entity.Email
                ,FullName         = entity.Entity2FullName()
                ,FirstName        = entity.FirstName
                ,LastName         = entity.LastName
                ,Nickname         = entity.Nickname
                ,LastLogin        = entity.LastLogin
                ,IsConfirmed      = entity.IsConfirmed
                ,IsSocialLogin    = entity.Provider != null// || !String.IsNullOrEmpty(entity.FacebookID)
                ,ProviderName     = entity.Provider ?? (!String.IsNullOrEmpty(entity.FacebookID) ? "facebook" : "")
                ,RegisterDate     = entity.RegisterDate.Date
                ,RegisterTime     = entity.RegisterDate
                ,Status           = Utils.ParseEnum<UserEnums.eUserStatuses>(entity.StatusType.ToString())
                ,RegistrationSource = Utils.ParseEnum<CommonEnums.eRegistrationSources>(entity.RegistrationTypeId)
                ,PictureUrl       = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                ,ActivityScore    = entity.UserId.CalculateActivityScore(entity.courses,entity.bundles,entity.chapters,entity.videos,entity.logins,entity.purchases,entity.stores)
                ,LoginsCount      = entity.logins
            };
        }

        public static UserEditDTO LoginEntity2UserEditDto(this vw_USER_UserLogins entity)
        {
            return new UserEditDTO
            {
                UserProfileId     = entity.Id
                ,UserId           = entity.UserId
                ,Email            = entity.Email
                ,FullName         = entity.Entity2FullName()
                ,FirstName        = entity.FirstName
                ,LastName         = entity.LastName
                ,Nickname         = entity.Nickname
                ,LastLogin        = entity.LastLogin
                ,IsConfirmed      = entity.IsConfirmed
                ,IsSocialLogin    = entity.Provider != null// || !String.IsNullOrEmpty(entity.FacebookID)
                ,ProviderName     = entity.Provider ?? (!String.IsNullOrEmpty(entity.FacebookID) ? "facebook" : "")
                ,Status           = Utils.ParseEnum<UserEnums.eUserStatuses>(entity.StatusType.ToString())
                ,PictureUrl       = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                ,RegistrationSource = Utils.ParseEnum<CommonEnums.eRegistrationSources>(entity.RegistrationTypeId)
            };
        }

        public static UserDTO ViewEntity2UserDto(this vw_USER_UsersLib entity)
        {
            return new UserDTO
            {
                UserProfileId           = entity.Id
                ,UserId                 = entity.UserId
                ,Email                  = entity.Email
                ,FullName               = entity.Entity2FullName()
                ,FirstName              = entity.FirstName
                ,LastName               = entity.LastName
                ,Nickname               = entity.Nickname
                ,LastLogin              = entity.LastLoginDate
                ,IsConfirmed            = entity.IsConfirmed
                ,IsPayoutOptionsDefined = entity.PayoutTypeId != null
            };
        }

        public static UserDTO Entity2UserDto(this Users entity)
        {
            return new UserDTO
            {
                 UserId       = entity.Id                
                ,Email        = entity.Email
                ,FullName     = entity.Entity2FullName()
                ,FirstName    = entity.FirstName
                ,LastName     = entity.LastName
                ,Nickname     = entity.Nickname
                ,LastLogin    = entity.LastLogin
                ,FacebookId   = entity.FacebookID
            };
        }

        public static UserNotificationDTO Entity2UserNotificationDto(this USER_NotificationToken entity)
        {
            return new UserNotificationDTO
            {
                 MessageId       = entity.MessageId
                ,Uid             = entity.Uid
                ,Kind            = Utils.ParseEnum<eMessageKinds>(entity.MessageKindId)
                ,HTMLMessage     = entity.HtmlMessage
                ,MessageText     = entity.Text
                ,PostedOn        = ((DateTimeOffset)entity.AddOn).ToVerbalDateSinceNow()
                ,AddOn           = entity.AddOn
                ,CreatorName     = entity.Entity2FullName()
                ,CreatorPhotoUrl = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                ,CourseName      = entity.CourseName
                ,RoomName        = entity.RoomName
                ,isRead          = entity.IsRead
            };
        }

        public static UserViewDto Entity2UserViewDTO(this Users entity)
        {
            return new UserViewDto
                {
                     userId    = entity.Id
                    ,nickname  = entity.Nickname
                    ,email     = entity.Email
                    ,firstName = entity.FirstName
                    ,lastName  = entity.LastName
                    ,fullName  = entity.Entity2FullName()
                    ,statusId  = entity.StatusType
                    ,typeId    = entity.UserTypeID
                    ,birthDate = entity.BirthDate
                    ,genderId  = entity.Gender
                    ,lastLogin = entity.LastLogin
                };
        }

        public static UserBaseDTO Entity2UserBaseDto(this Users entity)
        {
            return new UserBaseDTO
                {
                     userId    = entity.Id
                    ,firstName = entity.FirstName
                    ,lastName  = entity.LastName
                    ,fullName  = entity.Entity2FullName()
                };
        }

        public static BaseUserInfoDTO Entity2BaseUserInfoDto(this Users entity)
        {
            return new BaseUserInfoDTO
                                    {
                                        UserId    = entity.Id
                                        ,FullName = entity.Entity2FullName()
                                        ,Email    = entity.Email
                                    };
        }

        public static UserInfoDTO Entity2UserInfoDto(this Users entity)
        {
            return new UserInfoDTO
            {
                UserId      = entity.Id
                ,FullName   = entity.Entity2FullName()
                ,Email      = entity.Email
                ,FacebookId = entity.FacebookID
            };
        }
        public static UserInfoDTO UserDto2UserInfoDto(this UserDTO token)
        {
            return new UserInfoDTO
            {
                UserId      = token.UserId
                ,FullName   = token.FullName
                ,Email      = token.Email
                ,FacebookId = token.FacebookId
            };
        }
        public static UserTagItemDTO Entity2TagItemDto(this Users entity)
        {
            return new UserTagItemDTO
            {
                value  = entity.Id
                ,label = entity.Entity2FullName()
                ,image = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
            };
        }

        public static UserProfileDTO Entity2ProfileDto(this Users entity)
        {
            return new UserProfileDTO
                        {
                             userId    = entity.Id
                            ,nickname  = entity.Nickname
                            ,email     = entity.Email
                            ,firstName = entity.FirstName
                            ,lastName  = entity.LastName
                            ,fullName  = entity.Entity2FullName()
                            ,bioHtml   = entity.BioHtml
                            ,typeId    = entity.UserTypeID
                            ,birthDate = entity.BirthDate
                            ,genderId  = entity.Gender
                            ,PhotoUrl  = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                        };
        }

        public static AccountSettingsDTO UserEntity2AccountSettingsDTO(this Users entity)
        {
            return new AccountSettingsDTO
            {
                UserId                             = entity.Id
                ,FirstName                         = entity.FirstName
                ,LastName                          = entity.LastName
                ,Email                             = entity.Email
                ,Nickname                          = entity.Nickname
                ,BioHtml                           = entity.BioHtml
                ,PictureURL                        = String.IsNullOrEmpty(entity.PictureURL) ? string.Empty : Constants.ImageBaseUrl + entity.PictureURL
                ,PictureName                       = String.IsNullOrEmpty(entity.PictureURL) ? string.Empty : entity.PictureURL
                //,IsSocialLogin                     = !String.IsNullOrEmpty(entity.FacebookID)
                ,FbUid                             = String.IsNullOrEmpty(entity.FacebookID) ? (long?) null : Int64.Parse(entity.FacebookID)
                ,DisplayActivitiesOnFB             = entity.DisplayActivitiesOnFB
                ,DisplayCourseNewsWeeklyOnFB       = entity.DisplayCourseNewsWeeklyOnFB
                ,DisplayDiscussionFeedDailyOnFB    = entity.DisplayDiscussionFeedDailyOnFB
                ,ReceiveMonthlyNewsletterOnEmail   = entity.ReceiveMonthlyNewsletterOnEmail
                ,ReceiveDiscussionFeedDailyOnEmail = entity.ReceiveDiscussionFeedDailyOnEmail
                ,ReceiveCourseNewsWeeklyOnEmail    = entity.ReceiveCourseNewsWeeklyOnEmail
                ,AffiliateCommission               = entity.AffiliateCommission
            };
        }

        public static BaseUserInfoDTO SalesOrderLine2BuyerInfoDto(this vw_SALE_OrderLines orderLineEntity)
        {
            return new BaseUserInfoDTO
            {
                UserId   = orderLineEntity.BuyerUserId,
                Email    = orderLineEntity.BuyerEmail,
                FullName = orderLineEntity.Entity2BuyerFullName()
            };
        }

        #region learner
        public static SubscriberDTO LearnerToken2SubscriberDto(this CRS_LearnerToken token)
        {
            return new SubscriberDTO
            {
                id        = token.UserId
                ,name     = token.Entity2LearnerFullName()
                ,fbUid    = token.FacebookID
                ,photoUrl = token.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                ,email    = token.Email
                ,url      = token.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
            };
        }

        public static LearnerListItemDTO LearnerToken2LearnerListItemDTO(this CRS_LearnerToken entity)
        {
            
            return new LearnerListItemDTO
            {
                 id       = entity.UserId
                ,name     = entity.Entity2LearnerFullName()
                ,fbUid    = entity.FacebookID
                ,photoUrl = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
            };
        }

        public static LearnerListItemDTO Entity2LearnerListItemDTO(this Users entity)
        {
            
            return new LearnerListItemDTO
            {
                id        = entity.Id
                ,name     = entity.Entity2FullName()
                ,fbUid    = entity.FacebookID
                ,photoUrl = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
            };
        }
        #endregion

        #region fb
        public static IDictionary<string, string> FbResponceToken2Dictionary(this FbResponse token,string access_token)
        {
            return new Dictionary<string, string>
            {
                {FbEnums.eFbResultFields.id.ToString(), token.id.ToString()},
                {FbEnums.eFbResultFields.email.ToString(),token.email},
                {FbEnums.eFbResultFields.access_token.ToString(),access_token},
                {FbEnums.eFbResultFields.first_name.ToString(),token.first_name},
                {FbEnums.eFbResultFields.last_name.ToString(),token.last_name},
                {FbEnums.eFbResultFields.birthday.ToString(),token.birthday.ToString()},
                {FbEnums.eFbResultFields.gender.ToString(),token.gender}
            };
        }
        #endregion
    }
}
