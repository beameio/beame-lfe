using LFE.Core.Enums;
using LFE.Domain.Core;
using LFE.Model;
using System;
using System.Collections.Generic;

namespace LFE.Domain.Model
{
    public interface IUserRepository : IRepository<Users>
    {
        List<vw_USER_UserLogins> SearchUsers(int? userId, int? typeId, DateTime? logFrom, DateTime? logTo, DateTime? regFrom, DateTime? regTo, bool isGrp, int? roleId);

        vw_USER_UsersLib GetUser(string email,out string error);

        IEnumerable<USER_VideoToken> GetUserVideos(int? userId);
        List<USER_VideoInfoToken> SearchUserVideos(DateTime from, DateTime to, int? userId, bool? attachedOnly);
        IEnumerable<Users> FindUsers(int? userId, int? statusId, int? typeId);

        SALE_SalesStatisticToken GetAuthorSalesStatistic(int? userId, DateTime from, DateTime to);
        IEnumerable<CRS_LearnerToken> GetAuthorSubscribers(int userId);    
        /// <summary>
        /// get courses by authorId, when userId using to detect if course belong to current logged user as learner
        /// </summary>
        /// <param name="authorId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<USER_CourseToken> GetAuthorCourses(int authorId, int userId);
        IEnumerable<USER_ItemToken> SearchUserItems(short currencyId, int? id, int? itemId, CourseEnums.CourseStatus? status = null);
        IEnumerable<USER_AuthorWithCourseCountToken> GetAuthorsLOV(bool onlyPublished = false);

        IEnumerable<vw_USER_EventsLog> GetEventLogs(int? userId, short? eventTypeId, DateTime from, DateTime to, int? courseId, int? bundleId, int? storeId, long? sessionId);

        USER_StatisticToken GetUserStatistic(int userId);
        IEnumerable<USER_StatisticToken> GetUsersStatistic();

        USER_MonthlyStatementToken GetUserMonthlyStatementRefunds(int userId, int year, int month, short currencyId);
        IEnumerable<USER_MonthlyStatementToken> GetUserMonthlyStatementSales(int userId, int year, int month,short currencyId);
    }

    public interface IUserRefundProgramRevisionsRepository : IRepository<USER_RefundProgramRevisions> { }
    public interface IUserProfileRepository : IRepository<UserProfile>{}

    public interface IUserCourseRepository : IRepository<USER_Courses>
    {
        IEnumerable<CRS_LearnerToken> GetOtherLearners(int userId, int courseId);

        /// <summary>
        /// get courses for learner, when userId using to detect if course belong to current logged user as learner
        /// </summary>
        /// <param name="learnerId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<LRNR_ItemToken> GetLearnerCourses(int learnerId,int userId);
    }

    public interface IUserCourseWatchStateRepository : IRepository<USER_CourseWatchState>
    {
    }

    public interface IUserBundleRepository : IRepository<USER_Bundles>
    {
        
    }

    public interface IUserCourseReviewsRepository : IRepository<UserCourseReviews>
    {

    }

    public interface IUserNotificationRepository : IRepository<UserNotifications>
    {
        List<USER_NotificationToken> GetUserNotifications(int userId);
        USER_NotificationToken GetUserNotification(int messageId);
        void UpdateUserNotificationStatus(int userId);
    }

    

    public interface IUserSessionsRepository : IRepository<UserSessions> { }
    
    public interface IUserSessionsEventLogsRepository : IRepository<UserSessionsEventLogs> { }

    public interface IUserLoginsViewRepository : IRepository<vw_USER_UserLogins> { }

    public interface IUserViewRepository : IGetRepository<vw_USER_UsersLib> { }
    public interface IUserItemsViewRepository : IGetRepository<vw_USER_Items> { }

    public interface IUserPaymentInstrumentsRepository : IRepository<USER_PaymentInstruments>
    {

    }

    public interface IUserAddressRepository : IRepository<USER_Addresses>
    {
        List<USER_BillingAddressToken> GetUserAddresses(int? userId, int? addressId);
    }

    public interface IUserVideosRepository : IRepository<USER_Videos>
    {

    }
    public interface IUserVideosRenditionsRepository : IRepository<USER_VideosRenditions>
    {

    }
    public interface IUserLoginsRepository : IRepository<USER_Logins>
    {

    }

    public interface IUserVideoStatsRepository : IRepository<USER_VideoStats>
    {

    }
}
