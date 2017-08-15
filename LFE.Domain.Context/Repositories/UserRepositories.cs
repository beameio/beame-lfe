using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Domain.Context.Repositories
{
    public class UserRepository : Repository<Users>, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public vw_USER_UsersLib GetUser(string email, out string error)
        {
            error = string.Empty;
            try
            {
                if (DataContext == null)
                {
                    error = "Get user::" + email + "::DataContext is null";

                    return null;
                }
                return DataContext.tvf_USER_GetUser(email).FirstOrDefault();
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return null;
            }
        }

        public IEnumerable<USER_VideoToken> GetUserVideos(int? userId)
        {
            return DataContext.tvf_USER_GetVideos(userId);
        }

        public IEnumerable<Users> FindUsers(int? userId, int? statusId, int? typeId)
        {
            var users = DataContext.tvf_USER_GetUsers(userId, statusId, typeId).ToList();

            return users;
        }

        public List<vw_USER_UserLogins> SearchUsers(int? userId, int? typeId,DateTime? logFrom,DateTime? logTo,DateTime? regFrom,DateTime? regTo, bool isGrp, int? roleId)
        {
            var users = DataContext.tvf_USER_SearchUsers(userId, typeId, regFrom, regTo, logFrom, logTo, isGrp, roleId).OrderByDescending(x => x.RegisterDate).ToList();

            return users;
        }

        public SALE_SalesStatisticToken GetAuthorSalesStatistic(int? userId, DateTime from, DateTime to)
        {
            return DataContext.tvf_SALE_GetSellerSalesStatistic(from, to, userId).FirstOrDefault();
        }

        public IEnumerable<CRS_LearnerToken> GetAuthorSubscribers(int userId)
        {
            return DataContext.tvf_USER_GetAllSubscribers(userId);
        }

        public List<USER_VideoInfoToken> SearchUserVideos(DateTime from, DateTime to, int? userId, bool? attachedOnly)
        {
            return DataContext.tvf_USER_SearchVideos(from, to, userId, attachedOnly).ToList();
        }
        public IEnumerable<USER_ItemToken> SearchUserItems(short currencyId, int? id, int? itemId, CourseEnums.CourseStatus? status = null)
        {
            return DataContext.tvf_USER_SearchItems(currencyId,id, itemId,(byte?) status);
        }

        public IEnumerable<USER_AuthorWithCourseCountToken> GetAuthorsLOV(bool onlyPublished = false)
        {
            return DataContext.tvf_USER_GetAuthorsWithCourseCount(onlyPublished);
        }

        public IEnumerable<USER_CourseToken> GetAuthorCourses(int authorId, int userId)
        {
            return DataContext.tvf_USER_GetAuthorCourses(Constants.DEFAULT_CURRENCY_ID,authorId, userId);
        }

        public IEnumerable<vw_USER_EventsLog> GetEventLogs(int? userId, short? eventTypeId, DateTime from, DateTime to, int? courseId, int? bundleId, int? storeId, long? sessionId)
        {
            return DataContext.tvf_USER_GetEventsLog(from, to, userId,courseId,bundleId,storeId, sessionId, eventTypeId);
        }

        public USER_StatisticToken GetUserStatistic(int userId)
        {
            return DataContext.tvf_USER_GetStatistic(userId).FirstOrDefault();
        }

        public IEnumerable<USER_StatisticToken> GetUsersStatistic()
        {
            return DataContext.tvf_USER_GetStatistic(null);
        }

        public USER_MonthlyStatementToken GetUserMonthlyStatementRefunds(int userId, int year, int month, short currencyId)
        {
            return DataContext.tvf_USER_GetMonthlyStatementRefunds(year, month, userId,currencyId).FirstOrDefault();
        }

        public IEnumerable<USER_MonthlyStatementToken> GetUserMonthlyStatementSales(int userId, int year, int month, short currencyId)
        {
            return DataContext.tvf_USER_GetMonthlyStatementSales(year, month, userId,currencyId);
        }
    }

    public class UserRefundProgramRevisionsRepository : Repository<USER_RefundProgramRevisions>, IUserRefundProgramRevisionsRepository
    {
        public UserRefundProgramRevisionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
    {
        public UserProfileRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class UserCourseReviewsRepository : Repository<UserCourseReviews>, IUserCourseReviewsRepository
    {
        public UserCourseReviewsRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class UserCourseRepository : Repository<USER_Courses>, IUserCourseRepository
    {
        public UserCourseRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}

        public IEnumerable<CRS_LearnerToken> GetOtherLearners(int userId, int courseId)
        {
            return DataContext.tvf_CRS_GetOtherLearners(userId, courseId);
        }

        public IEnumerable<LRNR_ItemToken> GetLearnerCourses(int learnerId, int userId)
        {
            return DataContext.tvf_LRNR_GetCourses(Constants.DEFAULT_CURRENCY_ID,learnerId, userId);
        }
    }

    public class UserCourseWatchStateRepository: Repository<USER_CourseWatchState>, IUserCourseWatchStateRepository
    {
        public UserCourseWatchStateRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class UserNotificationRepository : Repository<UserNotifications>, IUserNotificationRepository
    {
        public UserNotificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}

        public List<USER_NotificationToken> GetUserNotifications(int userId)
        {
            return DataContext.tvf_USER_GetNotifications(userId).ToList();
        }

        public USER_NotificationToken GetUserNotification(int messageId)
        {
            return DataContext.tvf_USER_GetNotification(messageId).FirstOrDefault();
        }

        public void UpdateUserNotificationStatus(int userId)
        {
            DataContext.sp_USER_UpdateNotificationStatus(userId);
        }
    }

    public class UserBundleRepository : Repository<USER_Bundles>, IUserBundleRepository
    {
        public UserBundleRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class UserSessionsRepository : Repository<UserSessions>, IUserSessionsRepository
    {
        public UserSessionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class UserSessionsEventLogsRepository : Repository<UserSessionsEventLogs>, IUserSessionsEventLogsRepository
    {
        public UserSessionsEventLogsRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class UserViewRepository : Repository<vw_USER_UsersLib>, IUserViewRepository
    {
        public UserViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class UserItemsViewRepository : Repository<vw_USER_Items>, IUserItemsViewRepository
    {
        public UserItemsViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class UserLoginsViewRepository : Repository<vw_USER_UserLogins>, IUserLoginsViewRepository
    {
        public UserLoginsViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}
    }

    public class UserPaymentInstrumentsRepository : Repository<USER_PaymentInstruments>, IUserPaymentInstrumentsRepository
    {
        public UserPaymentInstrumentsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class UserAddressRepository : Repository<USER_Addresses>, IUserAddressRepository
    {
        public UserAddressRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        
        public List<USER_BillingAddressToken> GetUserAddresses(int? userId, int? addressId)
        {
            return DataContext.tvf_USER_GetBillingAddresses(userId, addressId).ToList();
        }
    }

    public class UserVideosRepository : Repository<USER_Videos>, IUserVideosRepository
    {
        public UserVideosRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
    public class UserVideosRenditionsRepository : Repository<USER_VideosRenditions>, IUserVideosRenditionsRepository
    {
        public UserVideosRenditionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
    public class UserLoginsRepository : Repository<USER_Logins>, IUserLoginsRepository
    {
        public UserLoginsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class UserVideoStatsRepository : Repository<USER_VideoStats>, IUserVideoStatsRepository
    {
        public UserVideoStatsRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

}
