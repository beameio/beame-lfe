using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;
using System;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class LogDtoMapper
    {
        #region private helpers
        private static string GetUserName(this vw_USER_EventsLog entity)
        {
            return entity.UserID == null ? string.Empty : DtoExtensions.CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        private static string Entity2CourseAuthorFullName(this vw_USER_EventsLog entity)
        {
            return entity.CourseAuthorId == null ? string.Empty : DtoExtensions.CombineFullName(entity.CourseAuthorFirstName, entity.CourseAuthorLastName, entity.CourseAuthorNickname);
        }

        private static string Entity2BundleAuthorFullName(this vw_USER_EventsLog entity)
        {
            return entity.BundleAuthorId == null ? string.Empty : DtoExtensions.CombineFullName(entity.BundleAuthorFirstName, entity.BundleAuthorLastName, entity.BundleAuthorNickname);
        }

        private static string Entity2VideoAuthorFullName(this vw_USER_EventsLog entity)
        {
            return entity.VideoAuthorId == null ? string.Empty : DtoExtensions.CombineFullName(entity.VideoAuthorFirstName, entity.VideoAuthorLastName, entity.VideoAuthorNickname);
        }
        private static string Entity2StoreOwnerFullName(this vw_USER_EventsLog entity)
        {
            return entity.StoreOwnerUserID == null ? string.Empty : DtoExtensions.CombineFullName(entity.StoreOwnerFirstName, entity.StoreOwnerLastName, entity.StoreOwnerNickname);
        }

        private static string Entity2ItemPageUrl(this vw_USER_EventsLog entity)
        {
            if (entity.CourseId != null) return entity.GenerateCoursePageUrl(entity.Entity2CourseAuthorFullName(), entity.CourseName,entity.TrackingID);

            if (entity.BundleId != null) return entity.GenerateBundlePageUrl(entity.Entity2BundleAuthorFullName(), entity.BundleName,entity.TrackingID);

            return string.Empty;
        }

        private static string Entity2ItemName(this vw_USER_EventsLog entity)
        {
            if (entity.CourseId != null) return entity.CourseName;

            if (entity.BundleId != null) return entity.BundleName;

            return string.Empty;
        }

        private static string Entity2ItemAuthorFullName(this vw_USER_EventsLog entity)
        {
            if (entity.CourseId != null) return entity.Entity2CourseAuthorFullName();

            if (entity.BundleId != null) return entity.Entity2BundleAuthorFullName();

            return string.Empty;
        } 
        #endregion

        public static UserEventDTO Entity2UserEventDto(this vw_USER_EventsLog entity)
        {
            return new UserEventDTO
            {
                 EventId                   = entity.EventID
                ,SessionId                 = entity.SessionId
                ,UserId                    = entity.UserID
                ,UserName                  = entity.GetUserName()
                ,UserPhotoUrl              = entity.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                ,AspNetSessionId           = entity.NetSessionId
                ,IPAddress                 = entity.IPAddress
                ,HostName                  = entity.HostName
                ,HttpHeaders               = entity.HttpHeaders                
                ,SessionDate               = entity.SessionDate
                ,EventType                 = entity.TypeName
                ,AdditionalData            = entity.AdditionalData.TrimString()
                ,EventDate                 = entity.EventDate

                ,ItemId                    = entity.CourseId ?? entity.BundleId
                ,ItemName                  = entity.Entity2ItemName()
                ,ItemPageUrl               = entity.Entity2ItemPageUrl()
                ,ItemAuthorName            = entity.Entity2ItemAuthorFullName()
                
                ,StoreId                   = entity.WebStoreId
                ,StoreName                 = entity.StoreName
                ,StoreOwnerName            = entity.Entity2StoreOwnerFullName()
                ,StoreUrl                  = entity.WixSiteUrl

                ,BcIdentifier              = entity.VideoBcIdentifier
                ,VideoName                 = entity.VideoName
                ,VideoAuthorName           = entity.Entity2VideoAuthorFullName()
            };
        }

        public static SystemLogDTO Entity2SystemLogDto(this ADMIN_SystemLogToken entity)
        {
            return new SystemLogDTO
            {
                id          = entity.id
                ,AddOn      = entity.CreateDate
                ,Origin     = entity.Origin
                ,Exception  = entity.Exception
                ,StackTrace = entity.StackTrace
                ,IntId      = entity.RecordIntId
                ,Module     = entity.RecordObjectType
                ,Level      = entity.LogLevel
                ,Message    = entity.Message
                ,HostName   = entity.HostName
                ,IpAddress  = entity.IPAddress
                ,SessionId  = entity.SessionId
                ,User       = entity.UserId != null ? new BaseUserInfoDTO
                                                                        {
                                                                            UserId = (int)entity.UserId
                                                                            ,FullName = entity.Entity2FullName()
                                                                        } : new BaseUserInfoDTO{UserId = -1,FullName = string.Empty}
            };
        }

        public static SystemLogDTO Entity2SystemLogDto(this LogTable entity)
        {
            return new SystemLogDTO
            {
                id          = entity.id
                ,AddOn      = entity.CreateDate
                ,Origin     = entity.Origin
                ,Exception  = entity.Exception
                ,StackTrace = entity.StackTrace
                ,IntId      = entity.RecordIntId
                ,Module     = entity.RecordObjectType
                ,Level      = entity.LogLevel
                ,Message    = entity.Message
            };
        }

        public static FileInterfaceLogDTO LogToken2FileInterfaceLogDto(this LOG_FileInterfaceToken token)
        {
            string status;

            try
            {
                status = Utils.GetEnumDescription(Utils.ParseEnum<ImportJobsEnums.eFileInterfaceStatus>(token.Status));
            }
            catch (Exception)
            {
                status = token.Status;
            }

            return new FileInterfaceLogDTO
            {
                FileId        = token.FileId                
                ,BcIdentifier = token.BcIdentifier                
                ,ETag         = token.ETag
                ,ContentType  = token.ContentType
                ,FilePath     = token.FilePath
                ,FileSize     = token.FileSize
                ,UpdateOn     = token.UpdateOn
                ,AddOn        = token.AddOn                
                ,Status       = status
                ,User         = new UserLogDTO
                                {
                                    id    = token.UserId
                                    ,name = token.Entity2FullName()
                                    ,url  = token.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                                }
            };
        }

        public static EmailInterfaceLogDTO LogToken2EmailInterfaceLogDto(this LOG_EmailInterfaceToken token)
        {
            return new EmailInterfaceLogDTO
            {
                EmailId        = token.EmailId
                ,Subject       = token.Subject
                ,ToEmail       = token.Email
                ,Error         = token.Error
                ,SendOn        = token.SendOn
                ,AddOn         = token.AddOn
                ,Status        = token.Status
                ,User          = new UserLogDTO
                                {
                                    id    = token.UserId
                                    ,name = token.Entity2FullName()
                                    ,url  = token.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                                }
            };
        }

        public static FbPostInterfaceLogDTO LogToken2FbPostInterfaceLogDto(this LOG_FbPostInterfaceToken token)
        {
            var dto = new FbPostInterfaceLogDTO
            {
                PostId         = token.PostId
                ,Title         = token.Title
                ,Message       = token.Message
                ,ImageUrl      = token.ImageUrl
                ,LinkedName    = token.LinkedName
                ,Caption       = token.Caption
                ,Error         = token.Error
                ,PostOn        = token.PostOn
                ,FbPostId      = token.FbPostId
                ,AddOn         = token.AddOn
                ,Status        = token.Status
            };

            if (token.UserId != null)
            {
                dto.User = new UserLogDTO
                                {
                                    id    = (int) token.UserId
                                    ,name = token.Entity2FullName()
                                    ,url  = token.Entity2PhotoUrl(Constants.ImageBaseUrl,Constants.DefaultAvatarUrl)
                                };
            }
            return dto;
        }
    }
}
