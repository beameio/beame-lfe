using LFE.Core.Enums;
using LFE.DataTokens;
using System;
using System.Collections.Generic;
using System.IO;


namespace LFE.Application.Services.Interfaces
{
    public interface IUserPortalServices : IDisposable
    {
        LearnerListItemDTO GetLearnerListItemDTO(int userId);
        List<CourseListDTO> GetLearnerCourses(int learnerId, int? userId);
        List<CourseListDTO> GetAuthorCourses(int authorId, int? userId);
        List<OrderLineDTO> GetUserPurchases(int userId);

        void UpdateCourseStateAndCreateStory(int courseId, int userId, int chapterId, int videoId, long? bcId = null, bool createStory = true);
        void CreateStoryView(int courseId, int userId, int chapterId, int videoId, long? bcId = null);
        //void UpdateUserFbAccessToken(int userId, string accessToken);
        UserProfileDTO GetUserProfileDto(int id);
    }

    public interface IWidgetUserServices : IDisposable
    {
        List<WidgetItemListDTO> GetAuthorItems(int authorId, int? userId,string trackingId);
        List<WidgetItemListDTO> GetLearnerPurchasedItems(int learnerId, int? userId);
        UserProfileDTO GetAuthorProfileDto(int id);

        bool SaveUserVideoStats(int userId, VideoStatsToken token,out Guid sessionId,out string error);
        bool UpdateVideoStatsReason(VideoStatsReasonToken token, out string error);
        VideoInfoToken GetVideoRenditions(long bcId);
    }

    public interface IAuthorAdminServices : IDisposable
    {
     
        List<UserViewDto> FindUsers(int? userId, UserEnums.eUserStatuses? status, UserEnums.eUserTypes? type);
        List<UserVideoDto> GetAuthorVideos(int userId, bool useCache = false);
        int GetAuthorVideosCount(int userId, bool useCache = false);
        List<BaseListDTO> GetAuthorsLOV(bool onlyPublished = false);
        UserVideoDto GetVideoToken(long identifier, int userId);

        //files and video
        bool SaveAuthorS3File(int userId, string fileName, string eTag, string contentType, long? size, ImportJobsEnums.eFileInterfaceStatus status, out int fileId, out string error,string tags = null);
        bool SaveS3VideoFile(int userId, string fileName, string refId, string contentType, long? size, ImportJobsEnums.eFileInterfaceStatus status, string tags, out int fileId, out string error);
        bool SaveS3VideoFile(VideoUploadToken token, int userId, ImportJobsEnums.eFileInterfaceStatus status,out int fileId, out string error);
        void UpdateS3InterfaceRecord(int fileId, ImportJobsEnums.eFileInterfaceStatus status, string error);
        bool UpdateS3InterfaceRecord(string refId, ImportJobsEnums.eFileInterfaceStatus status, out int fileId, out long bcId, out string error);
        bool UpdateS3InterfaceRecord(long bcId, ImportJobsEnums.eFileInterfaceStatus status, out int fileId, out string error);
        //void HandleFtpCallback(long? id = null, string referenceId = null, string entity = null, string action = null, string error = null, string status = null);
        string GetInterfacedFileName(int fileId);
        bool SaveUserVideoFromInterface(int fileId, int userId, out string error);
        bool SaveS3TranscodeJob(int userId, string id, out string error);
        bool SaveS3TranscoderResponse(string data, out string error);
        bool SaveVideo(UserVideoDto dto, out string error);
        //bool SaveVideoImage(VideoImageDto dto, out string error);

        bool SaveVideoThumb(long bcId, string fileName, Stream stream, out string error);
        VideoInfoToken GetVideoToken(long bcId);
        List<UserVideoDto> GetAuthorUnporcessedVideos(int userId);
        List<FileInterfaceLogDTO> GetAuthorFileInterfaceLogs(int userId);
        bool DeleteVideo(long identifier, out string error);
        bool DeleteWaitingVideo(int fileId, out string error);

        //reports
        AuthorStatisticSummaryDTO GetAuthorStatistic(int userId);
        List<OrderLineDTO> GetAuthorSales(int userId, ReportEnums.ePeriodSelectionKinds periodKind);        
        List<OrderLineDTO> GetAuthorSales(int authorId,ReportEnums.ePeriodSelectionKinds periodKind,BillingEnums.eOrderLineTypes lineType,int? storeId);
        List<SalesAnalyticChartDTO> GetSalesChartData(int userId, ReportEnums.ePeriodSelectionKinds periodSelectionKind, ReportEnums.eChartGroupping groupBy);
        List<SubscriberDTO> GetAuthorSubscribers(int userId, int? courseId = null);

        //S3 video
        //bool SaveS3VideoResponse(Rendition token,bool deleteSource, out string error);
        
       
    }

    public interface IUserEventLoggerServices : IDisposable
    {
        bool Report(ReportToken token);
    }

    public interface IUserNotificationServices : IDisposable
    {
        bool SaveNotification(int userId, long? messageId, out string error);
        List<UserNotificationDTO> GetUserNotifications(int userId);

        UserNotificationDTO GetNotificationToken(int notifId);

        int GetUserUnreadNotificationsCount(int userId);
        void UpdateUserNotificationStatus(int userId);

    }

    public interface IWixUserServices : IDisposable
    {
        //user
        // Users FindUserByWixUid(Guid uid);
        bool DisconnectWixUser(int userID, Guid instanceId);
        string GetWixZombieTrackingId(Guid instanceId, int userId);
        WidgetWebStoreDTO GetAndUpdateZombieStore(Guid instanceId, int userId);
        UserDTO FindUserDtoByWixInstanceId(Guid wixInstaceId);
        //store
        //   bool CreateUserStore(WixRegisterStoreDTO token, out string error);
        int? FindSotreByInstanceId(int userId, Guid instanceId, out string error);
    }

    public interface IPortalAdminUserServices : IDisposable
    {
        List<UserGridViewDto> GetUsers(int? profileUserId = null, int? userId = null, string name = null, string email = null);

        List<UserGridViewDto> SearchUsers(int? userId, int? typeId, DateTime? logFrom, DateTime? logTo, DateTime? regFrom, DateTime? regTo, bool isGrp, int? roleId);

        UserEditDTO GetUserEditDTO(int userId);

        UserStatisticToken GetUserStatisticToken(int userId);

        List<UserStatisticToken> GetUsersStatistic();
    }
}
