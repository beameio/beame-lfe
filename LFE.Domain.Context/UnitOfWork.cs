using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Domain.Core;
using LFE.Infrastructure.NLogger;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

namespace LFE.Domain.Context
{
    public class UnitOfWork : DbContext, IUnitOfWork
    {
        private readonly NLogLogger Logger = new NLogLogger();
        public UnitOfWork() : base("name=lfeAuthorEntities")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }

        //#region DbSets
        
        //#region user tables
        //IDbSet<Users> _users;
        //public IDbSet<Users> Users
        //{
        //    get { return _users ?? (_users = Set<Users>()); }
        //}
 
        //IDbSet<UserProfile> _userProfiles;
        //public IDbSet<UserProfile> UserProfile
        //{
        //    get { return _userProfiles ?? (_userProfiles = Set<UserProfile>()); }
        //}

        //IDbSet<UserNotifications> _userNotificationses;
        //public IDbSet<UserNotifications> UserNotifications
        //{
        //    get { return _userNotificationses ?? (_userNotificationses = Set<UserNotifications>()); }
        //}
        
        //IDbSet<USER_Courses> _userCourses;
        //public IDbSet<USER_Courses> UserCourses
        //{
        //    get { return _userCourses ?? (_userCourses = Set<USER_Courses>()); }
        //}

        //IDbSet<UserCourseReviews> _userCoursesReviews;
        //public IDbSet<UserCourseReviews> UserCourseReviews
        //{
        //    get { return _userCoursesReviews ?? (_userCoursesReviews = Set<UserCourseReviews>()); }
        //}

        //IDbSet<USER_PaymentInstruments> _billUserPreferenceses;
        //public IDbSet<USER_PaymentInstruments> USER_PaymentInstruments
        //{
        //    get { return _billUserPreferenceses ?? (_billUserPreferenceses = Set<USER_PaymentInstruments>()); }
        //}

        //IDbSet<USER_Addresses> _userAddresses;
        //public IDbSet<USER_Addresses> USER_Addresses
        //{
        //    get { return _userAddresses ?? (_userAddresses = Set<USER_Addresses>()); }
        //}

        //IDbSet<UserSessions> _userSessions;
        //public IDbSet<UserSessions> UserSessions
        //{
        //    get { return _userSessions ?? (_userSessions = Set<UserSessions>()); }
        //}

        //IDbSet<UserSessionsEventLogs> _userSessionsEventLogs;
        //public IDbSet<UserSessionsEventLogs> UserSessionsEventLogs
        //{
        //    get { return _userSessionsEventLogs ?? (_userSessionsEventLogs = Set<UserSessionsEventLogs>()); }
        //}

        //private IDbSet<vw_USER_UsersLib> _userViewUsersLib;
        //public IDbSet<vw_USER_UsersLib> vw_USER_UsersLib
        //{
        //    get { return _userViewUsersLib ?? (_userViewUsersLib = Set<vw_USER_UsersLib>()); }
        //}

        //private IDbSet<vw_USER_UserLogins> _userLogins;
        //public IDbSet<vw_USER_UserLogins> vw_USER_UserLogins
        //{
        //    get { return _userLogins ?? (_userLogins = Set<vw_USER_UserLogins>()); }
        //}        
        //#endregion

        //#region courses
        //IDbSet<Courses> _courses;
        //public IDbSet<Courses> Courses
        //{
        //    get { return _courses ?? (_courses = Set<Courses>()); }
        //}

        //IDbSet<CRS_Bundles> _courseBundles;
        //public IDbSet<CRS_Bundles> CourseBundles
        //{
        //    get { return _courseBundles ?? (_courseBundles = Set<CRS_Bundles>()); }
        //}

        //IDbSet<CRS_BundleCourses> _bundleCourse;
        //public IDbSet<CRS_BundleCourses> CRS_BundleCourses
        //{
        //    get { return _bundleCourse ?? (_bundleCourse = Set<CRS_BundleCourses>()); }
        //}

        //IDbSet<CourseCategories> _courseCategories;
        //public IDbSet<CourseCategories> CourseCategories
        //{
        //    get { return _courseCategories ?? (_courseCategories = Set<CourseCategories>()); }
        //}
            
        //IDbSet<CourseChapters> _courseChapters;
        //public IDbSet<CourseChapters> CourseChapters
        //{
        //    get { return _courseChapters ?? (_courseChapters = Set<CourseChapters>()); }
        //}
            
        //IDbSet<ChapterVideos> _courseChapterVideos;
        //public IDbSet<ChapterVideos> ChapterVideos
        //{
        //    get { return _courseChapterVideos ?? (_courseChapterVideos = Set<ChapterVideos>()); }
        //}

        //IDbSet<ChapterLinks> _courseChapterLinks;
        //public IDbSet<ChapterLinks> ChapterLinks
        //{
        //    get { return _courseChapterLinks ?? (_courseChapterLinks = Set<ChapterLinks>()); }
        //}
            
        //IDbSet<CRS_CourseChangeLog> _courseChangeLogs;
        //public IDbSet<CRS_CourseChangeLog> CRS_CourseChangeLog
        //{
        //    get { return _courseChangeLogs ?? (_courseChangeLogs = Set<CRS_CourseChangeLog>()); }
        //}
            
        //IDbSet<CRS_WizardStepsLOV> _wizardStepsLovs;
        //public IDbSet<CRS_WizardStepsLOV> CRS_WizardStepsLOV
        //{
        //    get { return _wizardStepsLovs ?? (_wizardStepsLovs = Set<CRS_WizardStepsLOV>()); }
        //}
            
        //IDbSet<Categories> _categories;
        //public IDbSet<Categories> Categories
        //{
        //    get { return _categories ?? (_categories = Set<Categories>()); }
        //}            
        //#endregion
            
        //#region coupons
        //IDbSet<Coupons> _coupons;
        //public IDbSet<Coupons> Coupons
        //{
        //    get { return _coupons ?? (_coupons = Set<Coupons>()); }
        //} 

        //IDbSet<CouponInstances> _couponInstances;
        //public IDbSet<CouponInstances> CouponInstances
        //{
        //    get { return _couponInstances ?? (_couponInstances = Set<CouponInstances>()); }
        //}        
        //#endregion

        //#region helpers
        //IDbSet<Parameters> _parameters;
        //public IDbSet<Parameters> Parameters
        //{
        //    get { return _parameters ?? (_parameters = Set<Parameters>()); }
        //}

        //IDbSet<LogTable> _logTable;
        //public IDbSet<LogTable> LogTable
        //{
        //    get { return _logTable ?? (_logTable = Set<LogTable>()); }
        //}
        //#endregion

        //#region web stores
        //IDbSet<WebStores> _webStores;
        //public IDbSet<WebStores> WebStores
        //{
        //    get { return _webStores ?? (_webStores = Set<WebStores>()); }
        //}

        //IDbSet<WebStoreItems> _webStoreCourses;
        //public IDbSet<WebStoreItems> WebStoreItems
        //{
        //    get { return _webStoreCourses ?? (_webStoreCourses = Set<WebStoreItems>()); }
        //}
        
        //IDbSet<WebStoreCategories> _webStoreCategories;
        //public IDbSet<WebStoreCategories> WebStoreCategories
        //{
        //    get { return _webStoreCategories ?? (_webStoreCategories = Set<WebStoreCategories>()); }
        //}
        
        //IDbSet<WebStoresChangeLog> _webStorecChangeLogs;
        //public IDbSet<WebStoresChangeLog> WebStoresChangeLog
        //{
        //    get { return _webStorecChangeLogs ?? (_webStorecChangeLogs = Set<WebStoresChangeLog>()); }
        //}
        
        //IDbSet<UserS3FileInterface> _userS3FileInterfaces;
        //public IDbSet<UserS3FileInterface> UserS3FileInterface
        //{
        //    get { return _userS3FileInterfaces ?? (_userS3FileInterfaces = Set<UserS3FileInterface>()); }
        //}
        
        //#endregion

        //#region discussion
        //IDbSet<DSC_ClassRoom> _dscClassRooms;
        //public IDbSet<DSC_ClassRoom> DSC_ClassRoom
        //{
        //    get { return _dscClassRooms ?? (_dscClassRooms = Set<DSC_ClassRoom>()); }
        //}

        //IDbSet<DSC_Followers> _dscFollowers;
        //public IDbSet<DSC_Followers> DSC_Followers
        //{
        //    get { return _dscFollowers ?? (_dscFollowers = Set<DSC_Followers>()); }
        //}
        
        //IDbSet<DSC_Hashtags> _dscHashtags;
        //public IDbSet<DSC_Hashtags> DSC_Hashtags
        //{
        //    get { return _dscHashtags ?? (_dscHashtags = Set<DSC_Hashtags>()); }
        //}
        
        //IDbSet<DSC_Messages> _dscMessages;
        //public IDbSet<DSC_Messages> DSC_Messages
        //{
        //    get { return _dscMessages ?? (_dscMessages = Set<DSC_Messages>()); }
        //}
        
        //IDbSet<DSC_MessageHashtags> _dscMessagHashtags;
        //public IDbSet<DSC_MessageHashtags> DSC_MessageHashtags
        //{
        //    get { return _dscMessagHashtags ?? (_dscMessagHashtags = Set<DSC_MessageHashtags>()); }
        //}

        //IDbSet<DSC_MessageUsers> _dscMessagUsers;
        //public IDbSet<DSC_MessageUsers> DSC_MessageUsers
        //{
        //    get { return _dscMessagUsers ?? (_dscMessagUsers = Set<DSC_MessageUsers>()); }
        //}

        //private IDbSet<vw_DSC_MessageHashtags> _dscViewMessageHashtags;
        //public IDbSet<vw_DSC_MessageHashtags> vw_DSC_MessageHashtags
        //{
        //    get { return _dscViewMessageHashtags ?? (_dscViewMessageHashtags = Set<vw_DSC_MessageHashtags>()); }
        //}
        //#endregion
            
        //#region emails
        //IDbSet<EMAIL_Messages> _emailMessages;
        //public IDbSet<EMAIL_Messages> EMAIL_Messages
        //{
        //    get { return _emailMessages ?? (_emailMessages = Set<EMAIL_Messages>()); }
        //}

        //IDbSet<EMAIL_Templates> _emailTemplates;
        //public IDbSet<EMAIL_Templates> EMAIL_Templates
        //{
        //    get { return _emailTemplates ?? (_emailTemplates = Set<EMAIL_Templates>()); }
        //}
            
        //#endregion
            
        //#region FB
        //IDbSet<FB_PostInterface> _fbPostInterfac;
        //public IDbSet<FB_PostInterface> FB_PostInterface
        //{
        //    get { return _fbPostInterfac ?? (_fbPostInterfac = Set<FB_PostInterface>()); }
        //}
        //#endregion

        //#region sale
        //IDbSet<SALE_Orders> _saleOrders;
        //public IDbSet<SALE_Orders> SALE_Orders
        //{
        //    get { return _saleOrders ?? (_saleOrders = Set<SALE_Orders>()); }
        //}

        //IDbSet<SALE_OrderLines> _saleOrderLines;
        //public IDbSet<SALE_OrderLines> SALE_OrderLines
        //{
        //    get { return _saleOrderLines ?? (_saleOrderLines = Set<SALE_OrderLines>()); }
        //}

        //IDbSet<SALE_OrderLinePayments> _saleOrderpLinePayments;
        //public IDbSet<SALE_OrderLinePayments> SALE_OrderLinePayments
        //{
        //    get { return _saleOrderpLinePayments ?? (_saleOrderpLinePayments = Set<SALE_OrderLinePayments>()); }
        //}

        //IDbSet<SALE_OrderLinePaymentRefunds> _saleOrderLinePaymentRefunds;
        //public IDbSet<SALE_OrderLinePaymentRefunds> SALE_OrderLinePaymentRefunds
        //{
        //    get { return _saleOrderLinePaymentRefunds ?? (_saleOrderLinePaymentRefunds = Set<SALE_OrderLinePaymentRefunds>()); }
        //}

        //IDbSet<SALE_Transactions> _saleTransactions;
        //public IDbSet<SALE_Transactions> SALE_Transactions
        //{
        //    get { return _saleTransactions ?? (_saleTransactions = Set<SALE_Transactions>()); }
        //}
        //#endregion 

        //#region billing
        //IDbSet<PAYPAL_PaymentRequests> _paypalPaymentRequests;
        //public IDbSet<PAYPAL_PaymentRequests> PAYPAL_PaymentRequests
        //{
        //    get { return _paypalPaymentRequests ?? (_paypalPaymentRequests = Set<PAYPAL_PaymentRequests>()); }
        //}
        //#endregion

        //#region geo
        //IDbSet<GEO_CountriesLib> _geoCountries;
        //public IDbSet<GEO_CountriesLib> GEO_CountriesLib
        //{
        //    get { return _geoCountries ?? (_geoCountries = Set<GEO_CountriesLib>()); }
        //}

        //IDbSet<GEO_States> _geoStates;
        //public IDbSet<GEO_States> GEO_States
        //{
        //    get { return _geoStates ?? (_geoStates = Set<GEO_States>()); }
        //}
        //#endregion
        //#endregion

        #region interface implementation

        public DbSet<TEntity> CreateSet<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }

        public void Attach<TEntity>(TEntity item) where TEntity : class
        {
            //attach and set as unchanged
            Entry(item).State = EntityState.Unchanged;
        }

        public void SetModified<TEntity>(TEntity item) where TEntity : class
        {
            //this operation also attach item in object state manager
            Entry(item).State = EntityState.Modified;
        }

        public void Commit()
        {
            base.SaveChanges();
        }

        public bool CommitAndRefreshChanges()
        {
            string error;
            return CommitAndRefreshChanges(out error);
        }

        public bool CommitAndRefreshChanges(out string error)
        {
            error = string.Empty;
            try
            {
                base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.ToList().ForEach(entry => entry.OriginalValues.SetValues(entry.GetDatabaseValues()));
                error = Utils.FormatError(ex);
                Logger.Error("UoW :" + error, ex, CommonEnums.LoggerObjectTypes.UnitOfWork);
                return false;

            }
            catch (DbEntityValidationException e)
            {

                var sb = new StringBuilder();
                foreach (var eve in e.EntityValidationErrors)
                {
                    sb.AppendLine(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                                    eve.Entry.Entity.GetType().Name,
                                                    eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        sb.AppendLine(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                                    ve.PropertyName,
                                                    ve.ErrorMessage));
                    }
                }
                error = sb.ToString();
                Logger.Error("UoW DbEntityValidationException exception: " + sb, e, CommonEnums.LoggerObjectTypes.UnitOfWork);
                return false;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("UoW :" + Utils.FormatError(ex), ex, CommonEnums.LoggerObjectTypes.UnitOfWork);
                return false;
            }
            return true;
        }

        public void RollbackChanges()
        {
            // set all entities in change tracker 
            // as 'unchanged state'
            ChangeTracker.Entries()
                                .ToList()
                                .ForEach(entry => entry.State = EntityState.Unchanged);
        } 
        #endregion
    }
}
