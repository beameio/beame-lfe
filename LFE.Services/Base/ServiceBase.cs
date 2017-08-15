using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Cach.Provider;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Domain.Model;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Infrastructure.NLogger;
using LFE.Model;
using WebMatrix.WebData;

namespace LFE.Application.Services.Base
{
	public class ServiceBase : Constants,IDisposable
	{
	    private const string S3_VIDEO_BUCKET = "https://s3.amazonaws.com/************************/"; // S3 video bucket

        public static readonly SimpleRoleProvider _rolesProvider = (SimpleRoleProvider)Roles.Provider;
		public  BaseCurrencyDTO DEFAULT_CURRENCY_BASE_TOKEN = new BaseCurrencyDTO {CurrencyId = DEFAULT_CURRENCY_ID, ISO = "USD"};
		public static JavaScriptSerializer JsSerializer { get; set; }
		public NLogLogger Logger { get; set; }
		public ICacheService CacheProxy;
		private static List<CurrencyDTO> _activeCurrencies = new List<CurrencyDTO>();

		private static readonly bool _isDebugMode = bool.Parse(Utils.GetKeyValue("isDebugMode"));

        public string BaseUrl => Utils.GetKeyValue("baseUrl");

	    #region repositories
		#region user
		public IUserRepository UserRepository { get; set; }
		public IUserRefundProgramRevisionsRepository UserRefundProgramRevisionsRepository { get; set; }
		public IUserCourseRepository UserCourseRepository { get; set; }
		public IUserCourseWatchStateRepository UserCourseWatchStateRepository { get; set; }
		public IUserBundleRepository UserBundleRepository { get; set; }
		public IUserProfileRepository UserProfileRepository { get; set; }
		public IUserCourseReviewsRepository UserCourseReviewsRepository { get; set; }
		public IUserPaymentInstrumentsRepository UserPaymentInstrumentsRepository { get; set; }
		public IUserAddressRepository UserAddressRepository { get; set; }
		public IUserNotificationRepository UserNotificationRepository { get; set; }
		public IUserSessionsRepository UserSessionsRepository { get; set; }
		public IUserSessionsEventLogsRepository UserSessionsEventLogsRepository { get; set; }  
		public IUserViewRepository UserViewRepository { get; set; }
		public IUserItemsViewRepository UserItemsViewRepository { get; set; }
		public IUserLoginsViewRepository UserLoginsViewRepository { get; set; }

		public IUserVideoStatsRepository UserVideoStatsRepository { get; set; }

		public IUserVideosRepository UserVideosRepository { get; set; }
        public IUserVideosRenditionsRepository UserVideosRenditionsRepository { get; set; }
		public IUserLoginsRepository UserLoginsRepository { get; set; }

		public ICustomEventsRepository CustomEventsRepository { get; set; }
		#endregion

		#region course
		public ICourseRepository CourseRepository { get; set; }
		public ICourseCategoryRepository CourseCategoryRepository { get; set; }
		public IBundleRepository BundleRepository { get; set; }
		public IBundleCourseRepository BundleCourseRepository { get; set; }
		public IBundleCategoryRepository BundleCategoryRepository { get; set; }
		public IChapterRepository ChapterRepository { get; set; }
		public IChapterLinkRepository ChapterLinkRepository { get; set; }
		public IChapterVideoRepository ChapterVideoRepository { get; set; }
		public ICourseChangeLogRepository CourseChangeLogRepository { get; set; }
		public IWizardStepsRepository WizardStepsRepository { get; set; }
		public ICategoryRepository CategoryRepository { get; set; }
		public ICourseAssetsRepository CourseAssetsRepository{ get; set; }
		#endregion

		#region coupons
		public ICouponRepository CouponRepository { get; set; }
		public ICouponInstanceRepository CouponInstanceRepository { get; set; }
		#endregion

		#region discussion
		public IDiscussionClassRoomRepository DiscussionClassRoomRepository { get; set; }
		public IDiscussionFollowersRepository DiscussionFollowersRepository { get; set; }
		public IDiscussionHashtagRepository DiscussionHashtagRepository { get; set; }
		public IDiscussionMessageRepository DiscussionMessageRepository { get; set; }
		public IDiscussionMessageHashtagRepository DiscussionMessageHashtagRepository { get; set; }
		public IDiscussionMessageUserRepository DiscussionMessageUserRepository { get; set; }
		public IDiscussionMessageHashtagViewRepository DiscussionMessageHashtagViewRepository { get; set; }
		#endregion

		#region web store
		public IWebStoreRepository WebStoreRepository { get; set; }
		public IWebStoreItemRepository WebStoreItemRepository { get; set; }
		public IWebStoreViewRepository WebStoreViewRepository { get; set; }
		public IWebStoreItemViewRepository WebStoreItemViewRepository { get; set; }
		public IWebStoreCategoryRepository WebStoreCategoryRepository { get; set; }
		public IWebStoresChangeLogRepository WebStoresChangeLogRepository { get; set; }
		#endregion

		#region billing
		public IPaypalPaymentRequestsRepository PaypalPaymentRequestsRepository { get; set; }
		public IPaypalIpnLogRepository PaypalIpnLogRepository { get; set; }

		public IPriceListRepository PriceListRepository { get; set; }
		public IPriceRevisionsReposiotry PriceRevisionsReposiotry { get; set; }
		public ICurrencyRepository CurrencyRepository { get; set; }
		public IRefundRequestsRepository RefundRequestsRepository { get; set; }

		public IUserPayoutStatmentsRepository UserPayoutStatmentsRepository{ get; set; }
		public IPayoutExecutionsRepository PayoutExecutionsRepository { get; set; }
		#endregion

		#region sale
		public IOrderRepository OrderRepository { get; set; }
		public IOrderLineRepository OrderLineRepository{ get; set; }
		public IOrderLinePaymentRepository OrderLinePaymentRepository { get; set; }
		public IOrderLinePaymentRefundsRepository OrderLinePaymentRefundsRepository { get; set; }
		public ITransactionRepository TransactionRepository { get; set; }
		public IOrdersViewRepository OrdersViewRepository { get; set; }
		public IOrderLinesViewRepository OrderLinesViewRepository { get; set; }
		public IOrderLinePaymentsViewRepository OrderLinePaymentsViewRepository { get; set; }

		public IOrderLinePaymentRefundsViewRepository OrderLinePaymentRefundsViewRepository { get; set; }
		public ITransactionsViewRepository TransactionsViewRepository { get; set; }
		#endregion

		#region common
		public IEmailMessageRepository EmailMessageRepository { get; set; }
		public IEmailTemplateRepository EmailTemplateRepository { get; set; }
		
		public IFacebookPostRepository FacebookPostRepository { get; set; }                

		public IS3FileInterfaceRepository S3FileInterfaceRepository { get; set; }

		public ILogTableRepository LogTableRepository { get; set; }
        public IGeoCountriesRepository GeoCountriesRepository { get; set; }
		public IGeoStatesRepository GeoStatesRepository { get; set; }
		#endregion

	    #region Fact
		public IFactEventAggRepository FactEventAggRepository { get; set; }
		public IFactDailyStatsRepository FactDailyStatsRepository { get; set; }
		public IFactDailyTotalsRepository FactDailyTotalsRepository { get; set; }
		public IFactEventAggregatesViewRepository FactEventAggregatesViewRepository { get; set; }
		public IEventLogsViewRepository EventLogsViewRepository { get; set; }
		#endregion

		#region plugin
		public IPluginInstallationsRepository PluginInstallationsRepository { get; set; }
		public IPluginInstallationStoresRepository PluginInstallationStoresRepository { get; set; }
		#endregion

		#region mailchimp
		public IChimpUserListRepository ChimpUserListRepository{ get; set; }
		public IChimpListSegmentRepository ChimpListSegmentRepository { get; set; }
		public IChimpRejectsRepository ChimpRejectsRepository { get; set; }
		#endregion

		#region quizzes
		public IQuizViewRepository QuizViewRepository { get; set; }
		public IQuizQuestionsRepository QuizQuestionsRepository { get; set; }
		public IQuizQuestionsViewRepository QuizQuestionsViewRepository { get; set; }
		public IQuizQuestionAnswerOptionsRepository QuizQuestionAnswerOptionsRepository { get; set; }
		public ICourseQuizzesRepository CourseQuizzesRepository { get; set; }
		public IStudentQuizzesRepository StudentQuizzesRepository { get; set; }
		public IStudentQuizAttemptsRepository StudentQuizAttemptsRepository { get; set; }
		public IStudentQuizAnswersRepository StudentQuizAnswersRepository{ get; set; }        
		#endregion
		
		#region certificates
		public ICertificateRepository CertificateRepository { get; set; }
		public IStudentCertificatesRepository StudentCertificatesRepository { get; set; }
		public IStudentCertificatesViewRepository StudentCertificatesViewRepository { get; set; }
		#endregion

		#region plugin
		public bool _UpdatePluginDomain(string uid, string url, out string error)
		{
			try
			{
				var entities = PluginInstallationsRepository.GetMany(x => x.UId == uid).ToList();

				if (entities.Count == 0)
				{
					error = "Plugin not found";
					return false;
				}

				foreach (var entity in entities)
				{
					entity.UpdatePluginDomain(url);    
				}

				return PluginInstallationsRepository.UnitOfWork.CommitAndRefreshChanges(out error);
			}
			catch (Exception ex)
			{
				Logger.Error("UpdatePluginDomain ::" + uid, ex, CommonEnums.LoggerObjectTypes.Plugin);

				error = FormatError(ex);
				return false;
			}
		}
		#endregion
		#endregion

		#region .ctor
		public ServiceBase()
		{
			#region init repositories
			#region user
			UserRepository                       = DependencyResolver.Current.GetService<IUserRepository>();
			UserRefundProgramRevisionsRepository = DependencyResolver.Current.GetService<IUserRefundProgramRevisionsRepository>();
			UserCourseRepository                 = DependencyResolver.Current.GetService<IUserCourseRepository>();
			UserCourseWatchStateRepository       = DependencyResolver.Current.GetService<IUserCourseWatchStateRepository>();
			UserBundleRepository                 = DependencyResolver.Current.GetService<IUserBundleRepository>();
			UserProfileRepository                = DependencyResolver.Current.GetService<IUserProfileRepository>();
			UserCourseReviewsRepository          = DependencyResolver.Current.GetService<IUserCourseReviewsRepository>();			
			UserPaymentInstrumentsRepository     = DependencyResolver.Current.GetService<IUserPaymentInstrumentsRepository>();
			UserAddressRepository                = DependencyResolver.Current.GetService<IUserAddressRepository>();
			UserNotificationRepository           = DependencyResolver.Current.GetService<IUserNotificationRepository>();
			UserSessionsRepository               = DependencyResolver.Current.GetService<IUserSessionsRepository>();
			UserSessionsEventLogsRepository      = DependencyResolver.Current.GetService<IUserSessionsEventLogsRepository>();
			UserViewRepository                   = DependencyResolver.Current.GetService<IUserViewRepository>();
			UserItemsViewRepository              = DependencyResolver.Current.GetService<IUserItemsViewRepository>();
			UserVideosRepository                 = DependencyResolver.Current.GetService<IUserVideosRepository>();
            UserVideosRenditionsRepository       = DependencyResolver.Current.GetService<IUserVideosRenditionsRepository>();
			UserVideoStatsRepository             = DependencyResolver.Current.GetService<IUserVideoStatsRepository>();
			UserLoginsViewRepository             = DependencyResolver.Current.GetService<IUserLoginsViewRepository>();
			UserLoginsRepository                 = DependencyResolver.Current.GetService<IUserLoginsRepository>();
			CustomEventsRepository               = DependencyResolver.Current.GetService<ICustomEventsRepository>();
			#endregion

			#region course
			CourseRepository          = DependencyResolver.Current.GetService<ICourseRepository>();
			CourseCategoryRepository  = DependencyResolver.Current.GetService<ICourseCategoryRepository>();
			BundleRepository          = DependencyResolver.Current.GetService<IBundleRepository>();
			BundleCourseRepository    = DependencyResolver.Current.GetService<IBundleCourseRepository>();
			BundleCategoryRepository  = DependencyResolver.Current.GetService<IBundleCategoryRepository>();
			ChapterRepository         = DependencyResolver.Current.GetService<IChapterRepository>();
			ChapterLinkRepository     = DependencyResolver.Current.GetService<IChapterLinkRepository>();
			ChapterVideoRepository    = DependencyResolver.Current.GetService<IChapterVideoRepository>();
			CourseChangeLogRepository = DependencyResolver.Current.GetService<ICourseChangeLogRepository>();
			WizardStepsRepository     = DependencyResolver.Current.GetService<IWizardStepsRepository>();
			CategoryRepository        = DependencyResolver.Current.GetService<ICategoryRepository>();
			CourseAssetsRepository    = DependencyResolver.Current.GetService<ICourseAssetsRepository>();
			#endregion

			#region coupons
			CouponRepository         = DependencyResolver.Current.GetService<ICouponRepository>();
			CouponInstanceRepository = DependencyResolver.Current.GetService<ICouponInstanceRepository>();
			#endregion

			#region discussion
			DiscussionClassRoomRepository = DependencyResolver.Current.GetService<IDiscussionClassRoomRepository>();
			DiscussionFollowersRepository = DependencyResolver.Current.GetService<IDiscussionFollowersRepository>();
			DiscussionHashtagRepository = DependencyResolver.Current.GetService<IDiscussionHashtagRepository>();
			DiscussionMessageRepository = DependencyResolver.Current.GetService<IDiscussionMessageRepository>();
			DiscussionMessageHashtagRepository = DependencyResolver.Current.GetService<IDiscussionMessageHashtagRepository>();
			DiscussionMessageUserRepository = DependencyResolver.Current.GetService<IDiscussionMessageUserRepository>();
			DiscussionMessageHashtagViewRepository = DependencyResolver.Current.GetService<IDiscussionMessageHashtagViewRepository>();
			#endregion

			#region web store
			WebStoreRepository           = DependencyResolver.Current.GetService<IWebStoreRepository>();
			WebStoreItemRepository       = DependencyResolver.Current.GetService<IWebStoreItemRepository>();
			WebStoreItemViewRepository   = DependencyResolver.Current.GetService<IWebStoreItemViewRepository>();
			WebStoreViewRepository       = DependencyResolver.Current.GetService<IWebStoreViewRepository>();
			WebStoreCategoryRepository   = DependencyResolver.Current.GetService<IWebStoreCategoryRepository>();
			WebStoresChangeLogRepository = DependencyResolver.Current.GetService<IWebStoresChangeLogRepository>();
			#endregion

			#region billing
			PaypalPaymentRequestsRepository           = DependencyResolver.Current.GetService<IPaypalPaymentRequestsRepository>();
			PaypalIpnLogRepository                    = DependencyResolver.Current.GetService<IPaypalIpnLogRepository>();
			PriceListRepository                       = DependencyResolver.Current.GetService<IPriceListRepository>();
			PriceRevisionsReposiotry                  = DependencyResolver.Current.GetService<IPriceRevisionsReposiotry>();
			CurrencyRepository                        = DependencyResolver.Current.GetService<ICurrencyRepository>();
			RefundRequestsRepository                  = DependencyResolver.Current.GetService<IRefundRequestsRepository>();
			UserPayoutStatmentsRepository             = DependencyResolver.Current.GetService<IUserPayoutStatmentsRepository>();
			PayoutExecutionsRepository                = DependencyResolver.Current.GetService<IPayoutExecutionsRepository>();
			#endregion

			#region sale
			OrderRepository                       = DependencyResolver.Current.GetService<IOrderRepository>();
			OrderLineRepository                   = DependencyResolver.Current.GetService<IOrderLineRepository>();
			OrdersViewRepository                  = DependencyResolver.Current.GetService<IOrdersViewRepository>();
			OrderLinesViewRepository              = DependencyResolver.Current.GetService<IOrderLinesViewRepository>();
			OrderLinePaymentRepository            = DependencyResolver.Current.GetService<IOrderLinePaymentRepository>();
			OrderLinePaymentRefundsRepository     = DependencyResolver.Current.GetService<IOrderLinePaymentRefundsRepository>();
			OrderLinePaymentsViewRepository       = DependencyResolver.Current.GetService<IOrderLinePaymentsViewRepository>();
			OrderLinePaymentRefundsViewRepository = DependencyResolver.Current.GetService<IOrderLinePaymentRefundsViewRepository>();
			TransactionRepository                 = DependencyResolver.Current.GetService<ITransactionRepository>();
			TransactionsViewRepository            = DependencyResolver.Current.GetService<ITransactionsViewRepository>();
			#endregion

			#region common
			EmailMessageRepository = DependencyResolver.Current.GetService<IEmailMessageRepository>();
			EmailTemplateRepository = DependencyResolver.Current.GetService<IEmailTemplateRepository>();

			FacebookPostRepository = DependencyResolver.Current.GetService<IFacebookPostRepository>();

			LogTableRepository = DependencyResolver.Current.GetService<ILogTableRepository>();

			S3FileInterfaceRepository = DependencyResolver.Current.GetService<IS3FileInterfaceRepository>();

			GeoCountriesRepository = DependencyResolver.Current.GetService<IGeoCountriesRepository>();
			GeoStatesRepository = DependencyResolver.Current.GetService<IGeoStatesRepository>();
			#endregion
            
		    #region  fact
			FactEventAggRepository             = DependencyResolver.Current.GetService<IFactEventAggRepository>();
			FactDailyStatsRepository           = DependencyResolver.Current.GetService<IFactDailyStatsRepository>();
			FactDailyTotalsRepository          = DependencyResolver.Current.GetService<IFactDailyTotalsRepository>();
			FactEventAggregatesViewRepository  = DependencyResolver.Current.GetService<IFactEventAggregatesViewRepository>();
			EventLogsViewRepository            = DependencyResolver.Current.GetService<IEventLogsViewRepository>();
			#endregion

			#region plugins
			PluginInstallationsRepository = DependencyResolver.Current.GetService<IPluginInstallationsRepository>();
			PluginInstallationStoresRepository = DependencyResolver.Current.GetService<IPluginInstallationStoresRepository>();
			#endregion

			#region  mailchimp
			ChimpUserListRepository = DependencyResolver.Current.GetService<IChimpUserListRepository>();
			ChimpListSegmentRepository = DependencyResolver.Current.GetService<IChimpListSegmentRepository>();
			ChimpRejectsRepository = DependencyResolver.Current.GetService<IChimpRejectsRepository>();
			#endregion

	        #region quizzes    			
			QuizViewRepository                  = DependencyResolver.Current.GetService<IQuizViewRepository>();
			QuizQuestionsRepository             = DependencyResolver.Current.GetService<IQuizQuestionsRepository>();
			QuizQuestionsViewRepository         = DependencyResolver.Current.GetService<IQuizQuestionsViewRepository>();
			QuizQuestionAnswerOptionsRepository = DependencyResolver.Current.GetService<IQuizQuestionAnswerOptionsRepository>();
			CourseQuizzesRepository             = DependencyResolver.Current.GetService<ICourseQuizzesRepository>();
			StudentQuizzesRepository            = DependencyResolver.Current.GetService<IStudentQuizzesRepository>();
			StudentQuizAttemptsRepository       = DependencyResolver.Current.GetService<IStudentQuizAttemptsRepository>();
			StudentQuizAnswersRepository        = DependencyResolver.Current.GetService<IStudentQuizAnswersRepository>();
			
			#endregion

			#region certificates
			CertificateRepository             = DependencyResolver.Current.GetService<ICertificateRepository>();
			StudentCertificatesRepository     = DependencyResolver.Current.GetService<IStudentCertificatesRepository>();
			StudentCertificatesViewRepository = DependencyResolver.Current.GetService<IStudentCertificatesViewRepository>();
			#endregion

			#endregion

			JsSerializer        = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
			CacheProxy          = DependencyResolver.Current.GetService<ICacheService>();
			Logger              = DependencyResolver.Current.GetService<NLogLogger>();

			if(_activeCurrencies.Count > 0) return;

			_activeCurrencies = GetCurrencies(true);
		} 
		#endregion

		#region properties
        public VideoInfoToken DefaulVideoInfoToken
        {
            get
            {
                return GetVideoInfoToken(LFE_DEFAULT_VIDEO_BCID);
            }
        }
		public int CurrentUserId
		{
			get
			{
				var userId = this.GetCurrentUserId();
				return userId ?? -1;
			}
		}

		public static List<CurrencyDTO> ActiveCurrencies => _activeCurrencies;

	    public bool IsDebugMode
		{
			get { return _isDebugMode; }
		}
		#endregion

		#region currency services
        public int CurrencyToDecimal(string iso)
        {
            try
            {
                var cur = ActiveCurrencies.FirstOrDefault(x => x.ISO == iso);

                return cur != null ? (cur.KeepDecimal ? 2 : 0) : 2;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        public bool CurrencyToBoolean(string iso)
        {
            try
            {
                var curr = iso.ToUpper();

                var cur = ActiveCurrencies.FirstOrDefault(x => x.ISO == curr);

                return cur?.KeepDecimal ?? true;
            }
            catch (Exception)
            {
                return true;
            }
        }
		private List<CurrencyDTO> GetCurrencies(bool onlyActive)
		{
			// string error = string.Empty;
			try
			{
				if(CurrencyRepository == null) return new List<CurrencyDTO>(); 

				var currenccies = onlyActive ? CurrencyRepository.GetMany(x => x.IsActive).OrderBy(x => x.CurrencyName) : CurrencyRepository.GetAll().OrderBy(x => x.CurrencyName);

				return currenccies.Select(x => x.Entity2CurrencyDTO()).ToList();
			}
			catch (Exception ex)
			{
				// error = Utils.FormatError(ex);
				Logger.Error("get currencies::" + onlyActive, null, ex, CommonEnums.LoggerObjectTypes.Billing);
				return new List<CurrencyDTO>();
			}
		}
		#endregion

		#region public services
		public static string SerializeObject(dynamic T)
		{
			return JsSerializer.Serialize(T);
		}
		
		public IEnumerable<T> GetCachedListByKey<T>(string key, ICacheService cacheProxy) where T : class
		{
			try
			{
				var result = cacheProxy.Get<List<T>>(key);

				var cachedList = result?.ToArray();

				return result != null && cachedList.Any() ? cachedList : null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public string FormatError(Exception ex)
		{
			return Utils.FormatError(ex);
		}

	    public string DoHttpPost(string url,NameValueCollection parameters)
	    {
	        try
	        {
                using (var client = new WebClient())
                {                    
                   var responsebytes = client.UploadValues(url, "POST", parameters);
                   return Encoding.UTF8.GetString(responsebytes);
                }
	        }
	        catch (Exception ex)
	        {
                Logger.Error(ex);
	            return string.Empty;
	        }
	    }
		#endregion

		#region period conversion helpers
		public static DateRangeToken PeriodSelection2DateRange(ReportEnums.ePeriodSelectionKinds selection,DateTime? f = null,DateTime? t = null)
		{
			var from = f ?? SqlDateTime.MinValue.Value;
			var to   = t ?? SqlDateTime.MaxValue.Value.AddDays(-1);			

			var today = DateTime.Today.AddDays(1).AddSeconds(-1);

			switch (selection)
			{
				case ReportEnums.ePeriodSelectionKinds.thisMonth:
					from = new DateTime(today.Year, today.Month, 1);
					to = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
					break;
				case ReportEnums.ePeriodSelectionKinds.lastMonth:
					//logic changed to last 30 days
					//var last = DateTime.Now.AddMonths(-1);
					//from = new DateTime(last.Year, last.Month, 1);
					//to = from.AddMonths(1).AddSeconds(-1);

					to = today;
					from = today.AddDays(-30);
					break;
				case ReportEnums.ePeriodSelectionKinds.previousMonth:
					var now = DateTime.Now;
					var previous = now.AddMonths(-1);
					from = new DateTime(previous.Year,previous.Month,1);
					to = from.AddMonths(1).AddSeconds(-1);
					break;
				case ReportEnums.ePeriodSelectionKinds.week:
					to = today;
					from = today.AddDays(-7).AddSeconds(1);
					break;
				case ReportEnums.ePeriodSelectionKinds.last90:
					to = today;
					from = today.AddDays(-90);
					break;
				case ReportEnums.ePeriodSelectionKinds.last180:
					to = today;
					from = today.AddDays(-180);
					break;
			}

			var token = new DateRangeToken
			{
				@from = from
				,to = to
			};

			return token;
		}

		public DateRangeToken TwoPeriodsDateRange(ReportEnums.ePeriodSelectionKinds period)
		{
			var first = PeriodSelection2DateRange(period);
			var second = PeriodKindToDateRange(period, true);

			var dates = new DateRangeToken
			{
				from = second.from
				,to = first.to
			};

			return dates;
		}

		public DateRangeToken PeriodKindToDateRange(ReportEnums.ePeriodSelectionKinds period, bool isInCompareMode)
		{
			return isInCompareMode ? Period2Previous(period) : PeriodSelection2DateRange(period);
		}

		public DateRangeToken Period2Previous(ReportEnums.ePeriodSelectionKinds period)
		{
			DateRangeToken dates = null;

			try
			{
				if (period == ReportEnums.ePeriodSelectionKinds.thisMonth)
				{
					var toDate = DateTime.Today.AddDays(1 - DateTime.Today.Day).AddSeconds(-1);
					var fromDate = new DateTime(toDate.Year, toDate.Month, 1);
					dates = new DateRangeToken { from = fromDate, to = toDate };
				}
				else
				{
					dates = PeriodSelection2DateRange(period);
					var span = dates.to.Subtract(dates.from);
					var toDate = dates.from.AddSeconds(-1);
					dates = new DateRangeToken { to = toDate, from = toDate.Subtract(span) };
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Period2Previous",ex,CommonEnums.LoggerObjectTypes.Reports);
			}

			return dates;
		}

		public static DateRangeToken PreviousRangeFromRange(DateRangeToken datesFrom)
		{
			var span = datesFrom.to.Subtract(datesFrom.from);
			var toDate = datesFrom.from.AddSeconds(-1);

			return new DateRangeToken { to = toDate, @from = toDate.Subtract(span) };
		}
		#endregion

		#region shared services
		#region private helpers
		private bool IsRoomNameValid(DiscussionClassRoomDTO dto, int ownerId, out string error)
		{
			error = string.Empty;
			try
			{
				if (dto.RoomId < 0)
				{
					return !DiscussionClassRoomRepository.IsAny(x => x.CreatedBy == ownerId && x.Name == dto.Name);
				}

				return !DiscussionClassRoomRepository.IsAny(x => x.CreatedBy == ownerId && x.Name == dto.Name && x.ClassRoomId != dto.RoomId);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}
		}
		#endregion

		

		public bool _SaveNotification(int userId, long? messageId, out string error)
		{
			error = string.Empty;
			try
			{
				if (messageId == null) return true;

				if (UserNotificationRepository.IsAny(x => x.UserId == userId && x.MessageId == messageId)) return true;

				var user = UserRepository.GetById(userId);

				if (user == null)
				{
					error = "user entity not found";
					return false;
				}

				var entity = user.UserEntity2UserNotification((long)messageId, user.ReceiveDiscussionFeedDailyOnEmail, user.DisplayDiscussionFeedDailyOnFB && !String.IsNullOrEmpty(user.FacebookID));

				UserNotificationRepository.Add(entity);

				UserNotificationRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("Save user notification", userId, ex, CommonEnums.LoggerObjectTypes.UserNotification);
				return false;
			}
		}

		public UserBaseDTO _GetAuthorBaseDto(int authorId)
		{
			try
			{
				return UserRepository.GetById(authorId).Entity2UserBaseDto();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public List<ReviewDTO> _GetAuthorReviews(int userId, ReportEnums.ePeriodSelectionKinds? periodKind)
		{
			try
			{
				var dates = PeriodSelection2DateRange(periodKind ?? ReportEnums.ePeriodSelectionKinds.all);
				return CourseRepository.GetAuthorReviews(userId, dates.from, dates.to).Select(x => x.Entity2ReviewDTO()).OrderByDescending(x => x.Date).ToList();
			}
			catch (Exception ex)
			{
				Logger.Error("get author reviews", userId, ex, CommonEnums.LoggerObjectTypes.Course);

				return new List<ReviewDTO>();
			}
		}

		public bool IsUnderRGP(int userId)
		{
			var entity = UserRepository.GetById(userId);
			return entity != null && entity.JoinedToRefundProgram;
		}
		
		public int? _FindAuthorClassRoom(string name, int authorId)
		{
			var entity = DiscussionClassRoomRepository.Get(x => x.Name == name && x.AuthorId == authorId);
			return entity == null ? (int?)null : entity.ClassRoomId;
		} 

		public bool _SaveClassRoom(ref DiscussionClassRoomDTO dto, int authorId,int userId, out string error)
		{
			if (authorId < 0)
			{
				error = "authorId missing";
				return false;
			}

			if (!IsRoomNameValid(dto, authorId, out error))
			{
				error = String.IsNullOrEmpty(error) ? "Room Name already exists" : error;
				return false;
			}

			try
			{
				if (dto.RoomId < 0) //new room
				{
					var entity = dto.Dto2ClassRoomEntity(authorId, userId);
					
					DiscussionClassRoomRepository.Add(entity);

					DiscussionClassRoomRepository.UnitOfWork.CommitAndRefreshChanges();
					
					dto.RoomId = entity.ClassRoomId;
				}
				else
				{
					var entity = DiscussionClassRoomRepository.GetById(dto.RoomId);

					if (entity == null)
					{
						error = "Room entity not found";
						return false;
					}

					entity.UpdateClassRoomEntity(dto,authorId);

					DiscussionClassRoomRepository.UnitOfWork.CommitAndRefreshChanges();
				}

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save class room", dto.RoomId, ex, CommonEnums.LoggerObjectTypes.Discussion);
				return false;
			}
		}

		public List<WebStoreGridDTO> _GetOwnerStores(int id)
		{
			return WebStoreRepository.GetOwnerStores(id).Select(x => x.Entity2StoreListDto()).ToList();
		}

		public int? FindStoreId(string trackingId)
		{
			try
			{
				var tid = trackingId.TrimString().ToLower();

				var entity = WebStoreRepository.GetMany(x => x.TrackingID.ToLower().Replace(" ", "-")
				.Replace("%20", "-")
				.Replace("+", "-")
				.Replace("*", "-")
				.Replace("!", "-")
				.Replace("&amp;", "and") 
				.Replace("&", "and") == tid).FirstOrDefault();
				//var entity1 = WebStoreRepository.GetAll().Select(x => new BaseListDTO { id = x.StoreID, name = x.TrackingID.OptimizedUrl() }).FirstOrDefault(x => x.name.ToLower() == trackingId.TrimString().ToLower());
				return entity == null ? (int?)null : entity.StoreID;
			}
			catch (Exception ex)
			{
				Logger.Error("FindStoreId::" + trackingId + "::" + Utils.FormatError(ex), ex, CommonEnums.LoggerObjectTypes.WebStore);
				return null;
			}
		}
		public short GetStoreDefaultCurrency(string trackingId)
		{
			try
			{
				var tid = trackingId.TrimString().ToLower();
				var entity = WebStoreRepository.GetMany(x => x.TrackingID.ToLower().Replace(" ", "-")
							.Replace("%20", "-")
							.Replace("+", "-")
							.Replace("*", "-")
							.Replace("!", "-")
							.Replace("&amp;", "and") 
							.Replace("&", "and") == tid).Select(x => new { id = x.StoreID, name = x.TrackingID.OptimizedUrl(), currencyId = x.DefaultCurrencyId }).FirstOrDefault();

				return entity == null ? DEFAULT_CURRENCY_ID : (entity.currencyId ?? DEFAULT_CURRENCY_ID);
			}
			catch (Exception ex)
			{
				Logger.Error("Find Store CurrencyId::" + trackingId + "::" + Utils.FormatError(ex), ex, CommonEnums.LoggerObjectTypes.WebStore);
				return DEFAULT_CURRENCY_ID;
			}
		}

		public List<BaseCurrencyDTO> _GetUserCurrencies(int userId,DateTime? from,DateTime? to)
		{
			try
			{
				using (var context = new lfeAuthorEntities())
				{
					var currencies = context.tvf_DB_GetAuthorPeriodCurrenciesLOV(from,to,userId).ToList();

					if (currencies.Any()) return currencies.Where(x => x.EventCnt > 0).OrderByDescending(x => x.EventCnt).Select(x => x.Entity2BaseCurrencyDto()).OrderBy(x => x.CurrencyId).ToList();
				}

				return ActiveCurrencies.Select(x => x.ToBaseCurrencyDto()).ToList();
			}
			catch (Exception ex)
			{

				Logger.Error("Get user currencies", userId, ex, CommonEnums.LoggerObjectTypes.Dashboard);

				return ActiveCurrencies.Select(x => x.ToBaseCurrencyDto()).ToList();
			}
		} 
		#endregion

		#region account
		public void AddRole2User(int userId, CommonEnums.UserRoles role)
		{
		    try
		    {
		        var userEntity = UserRepository.GetById(userId);

		        if (userEntity == null) return;

		        if (!_rolesProvider.GetRolesForUser(userEntity.Email).Contains(role.ToString()))
		        {
		            _rolesProvider.AddUsersToRoles(new[] {userEntity.Email}, new[] {role.ToString()});
		        }
		    }
		    catch (Exception ex)
		    {
		        Logger.Error(ex);
		    }
		}
		#endregion

		#region videos
		#region private helpers
		private bool IsChapterLinkTitleValid(ChapterLinkEditDTO dto, out string error)
		{
			error = string.Empty;
			try
			{
				if (dto.LinkId < 0)
				{
					return !ChapterLinkRepository.IsAny(x => x.CourseChapterId == dto.ChapterId && x.LinkText == dto.Title);
				}

				return !ChapterLinkRepository.IsAny(x => x.CourseChapterId == dto.ChapterId && x.LinkText == dto.Title && x.Id != dto.LinkId);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}
		}
		private bool IsChapterVideoTitleValid(ChapterVideoEditDTO dto, out string error)
		{
			error = string.Empty;
			try
			{
				if (dto.VideoId < 0)
				{
					return !ChapterVideoRepository.IsAny(x => x.CourseChapterId == dto.ChapterId && x.VideoTitle == dto.Title);
				}

				return !ChapterVideoRepository.IsAny(x => x.CourseChapterId == dto.ChapterId && x.VideoTitle == dto.Title && x.Id != dto.VideoId);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}
		}
		#endregion

        public bool isVideoAttached(string bcid)
        {
            if (String.IsNullOrEmpty(bcid)) return false;
            long bcIdLong;

            if (!Int64.TryParse(bcid, out bcIdLong)) return false;

            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    return context.ChapterVideos.Any(x => x.VideoSupplierIdentifier == bcid) ||
                            context.Courses.Any(x => x.OverviewVideoIdentifier == bcid) ||
                            context.CRS_Bundles.Any(x => x.OverviewVideoIdentifier == bcid) ||
                            context.QZ_QuizQuestionsLib.Any(x => x.BcIdentifier != null && x.BcIdentifier == bcIdLong);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Is video attached " + bcid, ex, CommonEnums.LoggerObjectTypes.PortalAdmin);
                return false;
            }
        }

        public int _GetVideoChapterUsage(long bcId)
        {
            try
            {
                var id = bcId.ToString();
                return ChapterVideoRepository.Count(x => x.VideoSupplierIdentifier == id) 
                        + CourseRepository.Count(x => x.OverviewVideoIdentifier != null && x.OverviewVideoIdentifier == id)
                        + BundleRepository.Count(x => x.OverviewVideoIdentifier != null && x.OverviewVideoIdentifier == id)
                        + QuizQuestionsRepository.Count(x => x.BcIdentifier != null && x.BcIdentifier == bcId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

		public bool SaveChapterVideoDTO(ref ChapterVideoEditDTO dto, out string error)
		{
			if (dto.ChapterId < 0)
			{
				error = "chapterId missing";
				return false;
			}

			if (!IsChapterVideoTitleValid(dto, out error))
			{
				error = String.IsNullOrEmpty(error) ? "Video Name already exists" : error;
				return false;
			}
			var chapId = dto.ChapterId;
			try
			{
				if (dto.OrderIndex == null)
				{
					dto.OrderIndex = ChapterVideoRepository.Count(x => x.CourseChapterId == chapId);
				}

				if (dto.VideoId < 0) //new video
				{
					var entity = dto.EditDto2ChapterVideoEntity();

					ChapterVideoRepository.Add(entity);

					if (!ChapterVideoRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

					dto.VideoId = entity.Id;

					return true;
				}
				else
				{
					var entity = ChapterVideoRepository.GetById(dto.VideoId);

					if (entity == null)
					{
						error = "Video entity not found";
						return false;
					}

					entity.UpdateChapterVideoEntity(dto);

					return ChapterVideoRepository.UnitOfWork.CommitAndRefreshChanges(out error);
				}

			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save chapter video dto", dto.VideoId, ex, CommonEnums.LoggerObjectTypes.Course);
				return false;
			}
		}

		public bool _SaveChapterLink(ref ChapterLinkEditDTO dto, out string error)
		{
			if (dto.ChapterId < 0)
			{
				error = "chapterId missing";
				return false;
			}

			if (!IsChapterLinkTitleValid(dto, out error))
			{
				error = String.IsNullOrEmpty(error) ? "Link Title already exists" : error;
				return false;
			}
			var chapId = dto.ChapterId;
			try
			{
				if (dto.OrderIndex == null)
				{
					dto.OrderIndex = ChapterLinkRepository.Count(x => x.CourseChapterId == chapId);
				}

				if (dto.LinkId < 0) //new link
				{
					ChapterLinkRepository.Add(dto.EditDto2ChapterLinkEntity());
				}
				else
				{
					var entity = ChapterLinkRepository.GetById(dto.LinkId);

					if (entity == null)
					{
						error = "Link entity not found";
						return false;
					}

					entity.UpdateChapterLinkEntity(dto);
				}

				ChapterLinkRepository.UnitOfWork.CommitAndRefreshChanges();

				var name = dto.Title;
				var cid = dto.ChapterId;
				
				dto.LinkId = ChapterLinkRepository.Get(x => x.CourseChapterId == cid && x.LinkText == name).Id;

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("save chapter link dto", dto.LinkId, ex, CommonEnums.LoggerObjectTypes.Course);
				return false;
			}
		}

        public UserVideoDto _GetVideoToken(long identifier, int userId)
        {

            var video = UserVideosRepository.Get(x => x.BcIdentifier == identifier);

            if (video == null) return new UserVideoDto { userId = userId };

            var dto = video.VideoEntity2VideoDTO(userId, _GetVideoChapterUsage(identifier));

            var rendition = UserVideosRenditionsRepository.GetMany(x => x.VideoId == video.VideoId).OrderByDescending(x=>x.EncodingRate).FirstOrDefault();
            if (rendition != null)
            {
                dto.videoUrl = rendition.CloudFrontPath.ToCloudfrontSignedUrl();
            }

            return dto;           
        }

	    public VideoInfoToken GetVideoInfoToken(long bcId)
	    {
	        var entity = UserVideosRepository.Get(x => x.BcIdentifier == bcId);

            if(entity == null) return new VideoInfoToken{IsValid = false,Message = "video entity not found",BcIdentifier = bcId};
            
            if (entity.UserId == null) return new VideoInfoToken { IsValid = false, Message = "owner not defined", BcIdentifier = bcId };
	        
            var renditions = UserVideosRenditionsRepository.GetMany(x => x.VideoId == entity.VideoId).ToList();

            var t = new VideoInfoToken
            {
                 BcIdentifier= bcId
                ,PlayListUrl = string.IsNullOrEmpty(entity.PlaylistUrl) ? "" : entity.PlaylistUrl.Replace(S3_VIDEO_BUCKET, S3_CLOUDFRONT_ROOT).ToCloudfrontSignedUrl()
                ,Renditions  = renditions.Select(x=>x.Entity2RenditionDto()).ToList()
                ,ThumbUrl    = bcId.CombimeVideoUrl((int) entity.UserId,CommonEnums.eVideoPictureTypes.Thumb).ToCloudfrontSignedUrl()
                ,StillUrl    = bcId.CombimeVideoUrl((int) entity.UserId,CommonEnums.eVideoPictureTypes.Still).ToCloudfrontSignedUrl()
                ,IsValid     = true
            };


	        foreach (var rend in t.Renditions)
	        {
	            rend.CloudFrontPath = rend.CloudFrontPath.ToCloudfrontSignedUrl();
	        }
            
	        return t;

	    }
		#endregion

		#region course shared services
		public void _DeleteCourse(int courseId)
		{
			CourseRepository.Delete(x => x.Id == courseId);
			CourseRepository.UnitOfWork.CommitAndRefreshChanges();
		}
		public bool IsCourseNameValid(int courseId,string courseName, int authorId, out string error)
		{
			if (!courseName.IsObjectNameValid(out error)) return false;
			try
			{
				if (courseId < 0)
				{
					return !CourseRepository.IsAny(x => x.AuthorUserId == authorId && x.CourseName == courseName);
				}

				return !CourseRepository.IsAny(x => x.AuthorUserId == authorId && x.CourseName == courseName && x.Id != courseId);
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return false;
			}
		}

		public bool _HasCertificate(int courseId)
		{
			var hasCertificate = CertificateRepository.IsAny(x => x.CourseId == courseId && x.IsActive);
			return hasCertificate;
		}

		public bool _HasCertificateOnComplete(int courseId)
		{
			var hasQuizWithCertificate = CourseQuizzesRepository.IsAny(x =>x.CourseId == courseId && x.StatusId == (byte) QuizEnums.eQuizStatuses.PUBLISHED && x.IsAttached && x.AttachCertificate);
			return _HasCertificate(courseId) && !hasQuizWithCertificate;
		}

		#endregion

		#region prices

		public decimal GetItemAffiliateCommission(int itemId, BillingEnums.ePurchaseItemTypes type,int authorId)
		{
			switch (type)
			{
				case BillingEnums.ePurchaseItemTypes.COURSE:
					var course = CourseRepository.GetById(itemId);

					if (course != null ) return course.AffiliateCommission;
					break;
				case BillingEnums.ePurchaseItemTypes.BUNDLE:
					var bundle = BundleRepository.GetById(itemId);

					if (bundle != null ) return bundle.AffiliateCommission;
					break;
			}

			var seller = UserRepository.GetById(authorId);

			return seller != null ? seller.AffiliateCommission : AFFILIATE_COMMISSION_DEFAULT;
		}
		public short _GetStoreCurrencyByTrackingId(string trackingId)
		{
			return String.IsNullOrEmpty(trackingId) ? DEFAULT_CURRENCY_ID : GetStoreDefaultCurrency(trackingId);
		}
		public PriceLineDTO GetItemPriceToken(int lineId)
		{
			var entity = PriceListRepository.GetById(lineId);
			
			return entity==null ? null : entity.Entity2PriceLineDto(GetCurrencyDto(entity.CurrencyId));
		}
		public BaseCurrencyDTO GetCurrencyTokenByIso(string iso)
		{
			return GetCurrencyDto(iso).ToBaseCurrencyDto();
		}
		public BaseCurrencyDTO GetItemPriceCurrencyToken(int? lineId)
		{
			if (lineId == null) return DEFAULT_CURRENCY_BASE_TOKEN;

			var entity = PriceListRepository.GetById((int)lineId);

			return entity == null ? DEFAULT_CURRENCY_BASE_TOKEN: GetCurrencyDto(entity.CurrencyId).ToBaseCurrencyDto();
		}
		public List<PriceLineDTO> GetStoreItemPrices(int itemId, byte itemTypeId, short currencyId)
		{
			var prices = GetItemPrices(itemId, Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(itemTypeId), currencyId);

			return prices.Any() ? prices : GetItemDefaultPrices(itemId, itemTypeId, DEFAULT_CURRENCY_ID);
		}

		public List<PriceLineDTO> GetStoreItemPrices(int itemId, BillingEnums.ePurchaseItemTypes itemType, short currencyId)
		{
			var prices = PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && !x.IsDeleted && x.CurrencyId == currencyId).OrderBy(x => x.PriceTypeId).Select(x => x.Entity2PriceLineDto(GetCurrencyDto(currencyId))).ToList();

			return prices.Any() ? prices : GetItemPrices(itemId, itemType, DEFAULT_CURRENCY_ID);
		}

		public List<PriceLineDTO> GetItemFreePrice(int itemId, BillingEnums.ePurchaseItemTypes itemType)
		{
			var prices = PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && !x.IsDeleted && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.FREE).Select(x => x.Entity2PriceLineDto(new CurrencyDTO { CurrencyId = DEFAULT_CURRENCY_ID })).ToList();

			return prices.Any() ? prices : GetItemPrices(itemId, itemType, DEFAULT_CURRENCY_ID);
		}

		public List<PriceLineDTO> GetItemPrices(int itemId, byte itemTypeId, short currencyId)
		{
			return GetItemPrices(itemId, Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(itemTypeId), currencyId);
		}

		public List<PriceLineDTO> GetItemDefaultPrices(int itemId, byte itemTypeId, short currencyId)
		{
			var allPrices = GetAllItemPrices(itemId, Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(itemTypeId));

			if (!allPrices.Any()) return new List<PriceLineDTO>();

			var currencyPrices = allPrices.Where(x => x.Currency.CurrencyId == currencyId).ToList();

			return currencyPrices.Any() ? currencyPrices : allPrices;
		}

		public List<PriceLineDTO> GetItemPrices(int itemId,BillingEnums.ePurchaseItemTypes itemType,short currencyId )
		{
			return PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && !x.IsDeleted && x.CurrencyId == currencyId).OrderBy(x=>x.PriceTypeId).Select(x => x.Entity2PriceLineDto(GetCurrencyDto(currencyId))).ToList();
		}
		public List<PriceLineDTO> GetAllItemPrices(int itemId, BillingEnums.ePurchaseItemTypes itemType)
		{
			return PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && !x.IsDeleted).OrderBy(x => x.PriceTypeId).Select(x => x.Entity2PriceLineDto(GetCurrencyDto(x.CurrencyId))).ToList();
		}
		public bool IsItemPricesExists(int itemId, BillingEnums.ePurchaseItemTypes itemType)
		{
			return PriceListRepository.IsAny(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && !x.IsDeleted);
		}

		private string getPriceTypeAbbr(BillingEnums.ePricingTypes priceType)
		{
			switch (priceType)
			{
				case BillingEnums.ePricingTypes.ONE_TIME:
					return "O";
				case BillingEnums.ePricingTypes.SUBSCRIPTION:
					return "S";
				case BillingEnums.ePricingTypes.RENTAL:
					return "R";
			}

			return string.Empty;
		}

		public string GetItemDefaultPriceName(int itemId, BillingEnums.ePurchaseItemTypes itemType, short currencyId,bool isFree)
		{
			if (isFree) return "Free";

			var prices = PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && x.CurrencyId == currencyId && !x.IsDeleted).OrderBy(x=>x.PriceTypeId).ToList();

			string symbol;
			CurrencyDTO currency;
			if (prices.Any())
			{
				if (prices.Count > 1) return "Multiple plans";

				currency = ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == currencyId);
				symbol = currency != null ? currency.Symbol : string.Empty;
				return String.Format("{0}{1} ({2})", symbol, prices[0].Price.FormatPrice(0), getPriceTypeAbbr(Utils.ParseEnum<BillingEnums.ePricingTypes>(prices[0].PriceTypeId)));
			}

			prices = PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && !x.IsDeleted).OrderBy(x => x.PriceTypeId).ThenByDescending(x=>x.CurrencyId).ToList();

			if (!prices.Any()) return "Not found";

			if (prices.Count > 1) return "Multiple plans";

			currency = ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == prices[0].CurrencyId);
			symbol = currency != null ? currency.Symbol : string.Empty;
			return String.Format("{0}{1} ({2})", symbol, prices[0].Price.FormatPrice(0), getPriceTypeAbbr(Utils.ParseEnum<BillingEnums.ePricingTypes>(prices[0].PriceTypeId)));
		}

		public decimal? GetItemRegularPrice(List<PriceLineDTO> prices)
		{
			return GetItemPrice(prices, BillingEnums.ePricingTypes.ONE_TIME, null);
		}

		public decimal? GetItemMonthlySubscriptionPrice(List<PriceLineDTO> prices)
		{
			return GetItemPrice(prices, BillingEnums.ePricingTypes.SUBSCRIPTION, BillingEnums.eBillingPeriodType.MONTH);
		}

		public decimal? GetItemPrice(List<PriceLineDTO> prices, BillingEnums.ePricingTypes priceType, BillingEnums.eBillingPeriodType? periodType)
		{
			try
			{
				var token = prices.Where(x =>  x.PriceType == priceType && (periodType == null || x.PeriodType == periodType)).OrderByDescending(x => x.PriceLineID).FirstOrDefault();

				return token != null ? token.Price : (decimal?)null;
			}
			catch (Exception ex)
			{
				Logger.Error("get item price from list", ex, CommonEnums.LoggerObjectTypes.Billing);
				return null;
			}
		}

		public decimal? GetItemRegularPrice(int itemId, BillingEnums.ePurchaseItemTypes itemType, short currencyId)
		{
			return GetItemPrice(itemId,itemType,currencyId,BillingEnums.ePricingTypes.ONE_TIME,null);
		}

		public decimal? GetItemMonthlySubscriptionPrice(int itemId, BillingEnums.ePurchaseItemTypes itemType, short currencyId)
		{
			return GetItemPrice(itemId, itemType, currencyId, BillingEnums.ePricingTypes.SUBSCRIPTION,BillingEnums.eBillingPeriodType.MONTH);
		}

		public decimal? GetItemDefaultRegularPrice(int itemId, BillingEnums.ePurchaseItemTypes itemType)
		{
			var prices = PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.ONE_TIME && !x.IsDeleted).OrderBy(x=>x.CurrencyId).ToList();

			return prices.Any() ? prices[0].Price : (decimal?) null;
		}

		public decimal? GetItemDefaultMonthlySubscriptionPrice(int itemId, BillingEnums.ePurchaseItemTypes itemType)
		{
			var prices = PriceListRepository.GetMany(x => x.ItemId == itemId && x.ItemTypeId == (byte)itemType && x.PriceTypeId == (byte)BillingEnums.ePricingTypes.SUBSCRIPTION && x.PeriodTypeId == (byte)BillingEnums.eBillingPeriodType.MONTH && !x.IsDeleted).OrderBy(x => x.CurrencyId).ToList();

			return prices.Any() ? prices[0].Price : (decimal?)null;
		}

		public decimal? GetItemPrice(int itemId, BillingEnums.ePurchaseItemTypes itemType, short currencyId,BillingEnums.ePricingTypes priceType,BillingEnums.eBillingPeriodType? periodType)
		{
			try
			{
				var entity = PriceListRepository.GetMany(x=>x.ItemId == itemId 
													 && x.ItemTypeId == (byte)itemType 
													 && x.PriceTypeId==(byte)priceType
													 && x.CurrencyId == currencyId
													 && (periodType == null || x.PeriodTypeId == (byte?)periodType)
													 && !x.IsDeleted).OrderByDescending(x=>x.PriceLineId).FirstOrDefault();

				return entity != null ? entity.Price : (decimal?) null;
			}
			catch (Exception ex)
			{
				Logger.Error("get item price",itemId,ex,CommonEnums.LoggerObjectTypes.Billing);
				return null;
			}
		}
	   
		public bool IsPriceLineValid(PriceLineDTO dto, out string error)
		{
			error = string.Empty;
			try
			{
				if (dto.Price == 0 && dto.PriceType != BillingEnums.ePricingTypes.FREE)
				{
					error = "Price should be greater as zero";
					return false;
				}

				if (dto.Currency == null && dto.PriceType != BillingEnums.ePricingTypes.FREE)
				{
					error = "Currency Required";
					return false;
				}

				bool isExists;

				switch (dto.PriceType)
				{
					case BillingEnums.ePricingTypes.ONE_TIME:
						isExists = PriceListRepository.IsAny(x => x.ItemId == dto.ItemId
														&& x.ItemTypeId == (byte) dto.ItemType
														&& x.PriceTypeId == (byte) dto.PriceType
														&& x.CurrencyId == dto.Currency.CurrencyId
														&& !x.IsDeleted);
						break;
					case BillingEnums.ePricingTypes.SUBSCRIPTION:
						isExists = PriceListRepository.IsAny(x => x.ItemId == dto.ItemId
														&& x.ItemTypeId == (byte) dto.ItemType
														&& x.PriceTypeId == (byte) dto.PriceType
														&& x.PeriodTypeId == (byte)BillingEnums.eBillingPeriodType.MONTH
														&& x.CurrencyId == dto.Currency.CurrencyId
														&& !x.IsDeleted);
						break;
					case BillingEnums.ePricingTypes.RENTAL:
						isExists = PriceListRepository.IsAny(x => x.ItemId == dto.ItemId
														&& x.ItemTypeId == (byte)dto.ItemType
														&& x.PriceTypeId == (byte)dto.PriceType
														&& x.PeriodTypeId == (byte)dto.PeriodType
														&& x.NumOfPeriodUnits == dto.NumOfPeriodUnits
														&& x.CurrencyId == dto.Currency.CurrencyId
														&& !x.IsDeleted);
						break;
					case BillingEnums.ePricingTypes.FREE:
						isExists = PriceListRepository.IsAny(x => x.ItemId == dto.ItemId
														&& x.ItemTypeId == (byte)dto.ItemType
														&& x.PriceTypeId == (byte)dto.PriceType
														&& !x.IsDeleted);
						break;
					default:
						error = "unknown price type";

						return false;
				}

				if (!isExists) return true;

				error = "Price line already exists";
				return false;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("validate price line",ex,dto.PriceLineID,CommonEnums.LoggerObjectTypes.Course);
				return false;
			}
		}

		public CurrencyDTO GetCurrencyDto(short currencyId)
		{
		   return ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == currencyId) ?? ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == DEFAULT_CURRENCY_ID);
		}
		public CurrencyDTO GetCurrencyDto(string iso)
		{
			return ActiveCurrencies.FirstOrDefault(x => x.ISO == iso) ?? ActiveCurrencies.FirstOrDefault(x => x.CurrencyId == DEFAULT_CURRENCY_ID);
		}
		#endregion

		#region billing and paypal helpers
		public List<SALE_OrderLineViewToken> SearchOrderLines(DateTime from, DateTime to,
			int? sellerId = null,
			int? buyerId = null,
			int? storeOwnerId = null,
			int? courseId = null,
			int? bundleId = null,
			int? storeId = null,
			BillingEnums.eOrderLineTypes? lineType = null)
		{
			return OrderLinesViewRepository.SearchOrderLines(from,to,sellerId,buyerId,storeOwnerId,courseId,bundleId,storeId,lineType).OrderByDescending(x=>x.OrderDate).ToList();
		}
		public int? RequestEntity2CouponInstanceId(int authorId,int? courseId,int? bundleId,string couponCode)
		{
			try
			{
				if (String.IsNullOrEmpty(couponCode)) return null;
				var code = couponCode.OptimizedUrl();
				var inst = CouponInstanceRepository.Get(x => x.CouponCode.ToLower() == code
													&& (
														   (courseId != null && x.Coupons.CourseId == courseId) ||
														   (bundleId != null && x.Coupons.BundleId == bundleId) ||
														   (x.Coupons.OwnerUserId != null && x.Coupons.OwnerUserId == authorId)
													   )
												   );

				return inst != null ? (int?)inst.Id : null;
			}
			catch (Exception ex)
			{
				Logger.Error("get paypal request coupon instanceId", ex, CommonEnums.LoggerObjectTypes.Billing);
				return null;
			}
		}

		public int? RequestEntity2WebStoreId(string trackingID)
		{
			try
			{
				if (String.IsNullOrEmpty(trackingID)) return null;

				var inst = WebStoreRepository.Get(x => x.TrackingID.ToLower() == trackingID);

				return inst != null ? (int?)inst.StoreID : null;
			}
			catch (Exception ex)
			{
				Logger.Error("get paypal request webStoreid", ex, CommonEnums.LoggerObjectTypes.Billing);
				return null;
			}
		}

		public string GetItemNameByIds(int? courseId, int? bundleId)
		{
			try
			{
				if (courseId == null && bundleId == null) return string.Empty;

				var name = courseId != null ? CourseRepository.GetById((int)courseId).CourseName : BundleRepository.GetById((int)bundleId).BundleName;

				return String.IsNullOrEmpty(name) ? string.Empty : name;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
		//public string CombineOrderItemName(int? courseId,int? bundleId,bool isSubscription)
		//{
		//    try
		//    {
		//        if (courseId == null && bundleId == null) return string.Empty;

		//        var name = courseId != null ? CourseRepository.GetById((int)courseId).CourseName : BundleRepository.GetById((int)bundleId).BundleName;

		//        return String.IsNullOrEmpty(name) ? string.Empty : String.Format("{0} - {1}", name, isSubscription ? "Monthly subscription" : "Unlimited Access");
		//    }
		//    catch (Exception)
		//    {
		//        return string.Empty;
		//    }
		//}
		public string CombineOrderItemName(RequestPurchaseItemNameToken token)
		{
			try
			{
				var name = token.type == BillingEnums.ePurchaseItemTypes.COURSE ? CourseRepository.GetById(token.itemId).CourseName : BundleRepository.GetById(token.itemId).BundleName;

				return String.IsNullOrEmpty(name) ? string.Empty : String.Format("{0} - {1}", name, token.priceLineDto.PriceLineToken2PurchaseNameSuffix());
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
		public int GetOrderLineId(Guid orderId, BillingEnums.eOrderLineTypes type)
		{
			try
			{
				//for now 2014-10-15 we expecting only one order line per order header
				var entity = OrderLineRepository.Get(x => x.OrderId == orderId);//OrderLineRepository.Get(x=>x.OrderId == orderId && x.LineTypeId == (byte)type);

				return entity != null ? entity.LineId : -1;

			}
			catch (Exception)
			{
				return -1;
			}
		}

		public int GetSellerUserId(int? courseId, int? bundleId, out string error)
		{
			error = string.Empty;
			try
			{
				if (courseId != null || bundleId != null) return courseId != null ? CourseRepository.GetById((int)courseId).AuthorUserId : BundleRepository.GetById((int)bundleId).AuthorId;

				error = "course or bundle required";
				return -1;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				return -1;
			}
		}

		public bool SaveSaleTransaction(int lineId, 
										int? paymentId, 
										int? refundId, 
										BillingEnums.eTransactionTypes type, 
										decimal amount, 
										DateTime trxDate, 
										string externalTrxId, 
										decimal? fee, 
										Guid? requestId, 
										string remark,
										int? sourceTrxId, 
										out string error)
		{
			TransactionRepository.Add(new SALE_Transactions
			{
				OrderLineId            = lineId
				,PaymentId             = paymentId
				,RefundId              = refundId
				,Amount                = amount
				,TransactionTypeId     = (byte)type
				,TransactionDate       = trxDate
				,ExternalTransactionID = externalTrxId
				,Fee                   = fee ?? 0
				,RequestId             = requestId
				,SourceTransactionId   = sourceTrxId
				,Remarks               = remark
				,AddOn                 = DateTime.Now
			});

			return TransactionRepository.UnitOfWork.CommitAndRefreshChanges(out error);
		}

		public bool CreateOrderLinePayment( int lineId, 
											decimal amount, 
											DateTime? paymentDate, 
											DateTime scheduledDate, 
											short paymentNum, 
											BillingEnums.ePaymentStatuses status, 
											BillingEnums.ePaymentTypes type, 
											out int paymentId, 
											out string error)
		{
			
			paymentId = -1;
			
			var paymentEntity = new SALE_OrderLinePayments
			{
				OrderLineId    = lineId
				,Amount        = amount
				,Currency      = OrderLineRepository.FindLineCurrencyISO(lineId) //"USD"
				,PaymentDate   = paymentDate
				,ScheduledDate = scheduledDate
				,PaymentNumber = paymentNum
				,StatusId      = (byte)status
				,TypeId        = (byte)type
				,AddOn         = DateTime.Now
			};

			OrderLinePaymentRepository.Add(paymentEntity);

			if (!OrderLinePaymentRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

			paymentId = paymentEntity.PaymentId;

			return true;
		}

		public bool CreateOrderLinePaymentRefund(int paymentId,decimal amount, DateTime refundDate, BillingEnums.ePaymentTypes type,out int refundId ,out string error)
		{
			
			refundId = -1;
			
			var refundEntity = new SALE_OrderLinePaymentRefunds
			{
				PaymentId      = paymentId
				,Amount        = amount
				,RefundDate    = refundDate
				,TypeId        = (byte)type
				,AddOn         = DateTime.Now
			};

			OrderLinePaymentRefundsRepository.Add(refundEntity);

			if (!OrderLinePaymentRefundsRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

			refundId = refundEntity.RefundId;

			return true;
		}

		public int GetSubscriptionPaymentId(int orderLineId, short paymentNum)
		{
			try
			{
				var entity = OrderLinePaymentRepository.Get(x=>x.OrderLineId == orderLineId && x.PaymentNumber == paymentNum);

				return entity != null ? entity.PaymentId : -1;                
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public int GetScheduledSubscriptionPaymentId(int orderLineId, out short paymentNum)
		{
			paymentNum = -1;

			try
			{
				var paymentEntity = OrderLinePaymentRepository.Get(x => x.OrderLineId == orderLineId && x.PaymentNumber > 1 && x.StatusId == (byte)BillingEnums.ePaymentStatuses.SCHEDULED);


				if (paymentEntity == null)
				{
					return -1;
				}

				paymentNum = paymentEntity.PaymentNumber;

				return paymentEntity.PaymentId;
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public bool UpdateSubscriptionPayment(int paymentId, DateTime? paymentDate, BillingEnums.ePaymentStatuses status, out string error)
		{

			var paymentEntity = OrderLinePaymentRepository.GetById(paymentId);

			if (paymentEntity == null)
			{
				error = "payment entity not found";
				return false;
			}

			paymentEntity.StatusId   = (byte)status;
			if (paymentDate         != null) paymentEntity.PaymentDate = paymentDate;
			paymentEntity.UpdateDate = DateTime.Now;
			paymentEntity.UpdatedBy  = CurrentUserId;

			return OrderLinePaymentRepository.UnitOfWork.CommitAndRefreshChanges(out error);            
		}

		public bool AttachCourseOrBundle2User(int userId, int orderLineId, int? courseId, int? bundleId, DateTime? validUntil, out string error)
		{
			if (courseId != null || bundleId != null) return courseId != null ? AttachCourse2User((int)courseId, orderLineId, userId, null, validUntil, out error) : AttachBundle2User((int)bundleId, orderLineId, userId, out error);

			error = "courseId or bundleId required";
			return false;
		}

		private bool AttachBundle2User(int bundleId, int orderLineId, int userId, out string error)
		{
			error = string.Empty;
			try
			{
				var userBundles = UserBundleRepository.GetMany(x => x.UserId == userId && x.BundleId == bundleId && x.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE).ToList();
				int userBundleId;

				if (userBundles.Count().Equals(0))
				{
					var userBundleEntity = UserEntityMapper.NewUserBundleEntity(bundleId, orderLineId, userId, BillingEnums.eAccessStatuses.ACTIVE);

					UserBundleRepository.Add(userBundleEntity);

					var saved = UserBundleRepository.UnitOfWork.CommitAndRefreshChanges(out error);

					if (!saved)
					{
						error = "user bundle saving failed. please, contact support team::" + error;
						return false;
					}

					userBundleId = userBundleEntity.UserBundleId;
				}
				else
				{
					userBundleId = userBundles.Count().Equals(1) ? userBundles[0].UserBundleId : userBundles[userBundles.Count - 1].UserBundleId;
				}

				var bundleCourses = BundleCourseRepository.GetMany(x => x.BundleId == bundleId && x.IsActive).ToList();

				foreach (var course in bundleCourses)
				{
					if (!AttachCourse2User(course.CourseId, orderLineId, userId, userBundleId, null, out error)) return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::Attach bundle to user::" + userId, ex, bundleId, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}

		}

		public bool AttachCourse2User(int courseId, int orderLineId, int userId, int? userBundleId,DateTime? validUntil, out string error)
		{
			error = string.Empty;
			try
			{
				#region add role
				var courseEntity = CourseRepository.GetById(courseId);
				if(courseEntity != null && courseEntity.AuthorUserId != userId) AddRole2User(userId,CommonEnums.UserRoles.Learner);
				#endregion

				//if (UserCourseRepository.IsAny(x =>x.OrderLineId == orderLineId)) return true;
				
				//UserCourseRepository.Add(UserEntityMapper.NewUserCourseEntity(courseId, orderLineId, userId, userBundleId, BillingEnums.eAccessStatuses.ACTIVE, validUntil));

				//return UserCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);

				var userCourseExists = UserCourseRepository.IsAny(x => x.UserId == userId && x.CourseId == courseId && x.OrderLineId == orderLineId && (userBundleId == null || x.UserBundleId == userBundleId));

				if (!userCourseExists)
				{
					UserCourseRepository.Add(UserEntityMapper.NewUserCourseEntity(courseId, orderLineId, userId, userBundleId, BillingEnums.eAccessStatuses.ACTIVE, validUntil));

					var courseSaved = UserCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);

					if (courseSaved) return true;

					error = "user course saving failed. please, contact support team::" + error;

					return false;
				}

				var entity = UserCourseRepository.Get(x => x.UserId == userId && x.CourseId == courseId && (userBundleId == null || x.UserBundleId == userBundleId));

				if (entity.StatusId == (byte)BillingEnums.eAccessStatuses.ACTIVE) return true;

				entity.StatusId   = (byte)BillingEnums.eAccessStatuses.ACTIVE;
				entity.UpdateDate = DateTime.Now;
				entity.ValidUntil = validUntil;

				if (CurrentUserId > 0) entity.UpdatedBy = CurrentUserId;

				return UserCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);

			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::attach course to user", ex, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}

		public bool SHARED_BlockUserCourseAccess(int lineId, bool isSubscription, out string error)
		{
			try
			{
				//var lineEntity = OrderLinesViewRepository.Get(x => x.LineId == lineId);

				var bundleEntity = UserBundleRepository.Get(x => x.OrderLineId == lineId); 
			
				if (bundleEntity != null)
				{
					bundleEntity.StatusId = (byte)BillingEnums.eAccessStatuses.CANCELED;
					bundleEntity.UpdateDate = DateTime.Now;
					bundleEntity.UpdatedBy = CurrentUserId;
					UserBundleRepository.Update(bundleEntity); //Fix 
					UserBundleRepository.UnitOfWork.CommitAndRefreshChanges();
				}


				var entities = UserCourseRepository.GetMany(x => x.OrderLineId == lineId).ToList();
			  
				foreach (var entity in entities)
				{
					if (isSubscription)
					{
						var paymentDone = _IsMonthlySubscriptionPaymentDone(lineId, out error);

						if (!String.IsNullOrEmpty(error)) return false;

						if (paymentDone)
						{
							var validUntil    = this.NextMonthFirst().AddSeconds(-1);
							entity.ValidUntil = validUntil;
							entity.StatusId   = (byte)BillingEnums.eAccessStatuses.SUSPENDED;
						}
						else
						{
							entity.StatusId = (byte)BillingEnums.eAccessStatuses.CANCELED;
						}
					}
					else
					{
						entity.StatusId = (byte)BillingEnums.eAccessStatuses.CANCELED;    
					}
					
					entity.UpdateDate = DateTime.Now;
					entity.UpdatedBy = CurrentUserId;
					UserCourseRepository.Update(entity); //Fix
				}
				
				return UserCourseRepository.UnitOfWork.CommitAndRefreshChanges(out error);

			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::block user course access", ex, lineId, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}

		public bool SHARED_CancelOrder(int lineId, out string error,BillingEnums.eOrderStatuses status = BillingEnums.eOrderStatuses.CANCELED)
		{
			try
			{
				var lineEntity = OrderLineRepository.GetById(lineId);

				if (lineEntity == null)
				{
					error = "order line entity not found";
					return false;
				}

				var orderId = lineEntity.OrderId;

				var scheduledPayments = OrderLinePaymentsViewRepository.GetMany(x => x.OrderId == orderId && x.StatusId == (byte)BillingEnums.ePaymentStatuses.SCHEDULED);

				foreach (var payment in scheduledPayments)
				{
					UpdateSubscriptionPayment(payment.PaymentId, null, BillingEnums.ePaymentStatuses.CANCELED, out error);
				}

				var orderEntity = OrderRepository.GetById(orderId);

				if (orderEntity == null)
				{
					error = "order entity not found";
					return false;
				}

				orderEntity.StatusId    = (byte)status;
				orderEntity.CancelledOn = DateTime.Now;
				orderEntity.UpdateDate  = DateTime.Now;
				orderEntity.UpdatedBy   = CurrentUserId;

				return OrderRepository.UnitOfWork.CommitAndRefreshChanges(out error);

			}
			catch (Exception ex)
			{
				error = Utils.FormatError(ex);
				Logger.Error("BILLING::cancel order", ex, lineId, CommonEnums.LoggerObjectTypes.Billing);
				return false;
			}
		}

		public bool _IsMonthlySubscriptionPaymentDone(int lineId, out string error)
		{
			error = string.Empty;
			try
			{
				return OrderLinePaymentRepository.IsAny(x=>x.OrderLineId == lineId && x.ScheduledDate.Month == DateTime.Now.Month && x.StatusId == (byte)BillingEnums.ePaymentStatuses.COMPLETED);
			}
			catch (Exception ex)
			{
				error = FormatError(ex);
				return false;
			}
		}
		#endregion

		#region event log services
		public static string GetVisitorIPAddress(bool GetLan = false)
		{
			try
			{
				var visitorIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

				if (String.IsNullOrEmpty(visitorIPAddress))
					visitorIPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

				if (string.IsNullOrEmpty(visitorIPAddress))
					visitorIPAddress = HttpContext.Current.Request.UserHostAddress;

				if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
				{
					GetLan = true;
					visitorIPAddress = string.Empty;
				}

				if (!GetLan) return visitorIPAddress;

				if (!string.IsNullOrEmpty(visitorIPAddress)) return visitorIPAddress;

				//This is for Local(LAN) Connected ID Address
				var stringHostName = Dns.GetHostName();
				//Get Ip Host Entry
				var ipHostEntries = Dns.GetHostEntry(stringHostName);
				//Get Ip Address From The Ip Host Entry Address List
				var arrIpAddress = ipHostEntries.AddressList;

				try
				{
					visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
				}
				catch
				{
					try
					{
						visitorIPAddress = arrIpAddress[0].ToString();
					}
					catch
					{
						try
						{
							arrIpAddress = Dns.GetHostAddresses(stringHostName);
							visitorIPAddress = arrIpAddress[0].ToString();
						}
						catch
						{
							visitorIPAddress = "127.0.0.1";
						}
					}
				}
				return visitorIPAddress;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		//private static string GetHostName()
		//{
		//    try
		//    {
		//        return Dns.GetHostName();
		//    }
		//    catch (Exception)
		//    {

		//        return string.Empty;
		//    }
		//}

		private static string GetHttpHeaders()
		{
			try
			{
				var headers = string.Empty;

				var httpContext = HttpContext.Current;
				var headerList = httpContext.Request.Headers;

				for (var i = 0; i < headerList.Count; i++)
				{
					var key = headerList.GetKey(i);
					var value = headerList.GetValues(i);

					if(key==null  || value==null || value.Count().Equals(0)) continue;

					headers += String.Format("{0}:{1};",key,value[0]);
				}

				return headers;
			}
			catch (Exception)
			{

				return string.Empty;
			}
		}

		private static string GetCurrentUri()
		{
			try
			{
				return HttpContext.Current.Request.Url.AbsoluteUri;
			}
			catch (Exception)
			{

				return string.Empty;
			}
		}
		public bool WriteEventRecord(int? userId, CommonEnums.eUserEvents eventType, string netSessionId, string additionalMiscData = null, string trackingID = null, int? courseId = null, int? bundleId = null, long? bcId = null, string hostName = null)
		{
			try
			{
			   
				UserSessions sessionEntity = null;

				var sessions = UserSessionsRepository.GetMany(x => x.NetSessionId == netSessionId).ToList();

				if (sessions.Any())
				{
					if (userId == null)
					{
						sessionEntity = sessions.FirstOrDefault(x => x.UserID == null && x.NetSessionId == netSessionId);
					}
					else
					{
						sessionEntity = sessions.FirstOrDefault(x => x.UserID == userId  && netSessionId == x.NetSessionId) ?? sessions.FirstOrDefault(x => x.UserID == null && netSessionId == x.NetSessionId);
					}
				}

				if (sessionEntity == null)
				{
					sessionEntity = new UserSessions
					{
						UserID        = userId
						,NetSessionId = netSessionId
						,EventDate    = DateTime.Now
						,HostName     = hostName
						,IPAddress    = GetVisitorIPAddress()
						,HttpHeaders  = GetHttpHeaders(),
					};

					UserSessionsRepository.Add(sessionEntity);
					UserSessionsRepository.UnitOfWork.CommitAndRefreshChanges();
				}
				else
				{
					if (sessionEntity.UserID == null && userId != null)
					{
						sessionEntity.UserID = userId;
						UserSessionsRepository.UnitOfWork.CommitAndRefreshChanges();
					}
				}               

				var sessionId = sessionEntity.SessionId;
				var storeId = FindStoreId(trackingID);
				var eventEntity = new UserSessionsEventLogs
				{
					SessionId         = sessionId,
					EventTypeID       = (short)eventType,
					AdditionalData    = additionalMiscData,
					WebStoreId        = storeId,
					CourseId          = courseId == null || courseId < 0 ? null : courseId,
					BundleId          = bundleId == null || bundleId < 0 ? null : bundleId,
					VideoBcIdentifier = bcId,
					AubsoluteUri      = GetCurrentUri(),
					EventDate         = DateTime.Now,
					ExportToFact      = false,
					HostName          = hostName
				};

				UserSessionsEventLogsRepository.Add(eventEntity);

				UserSessionsEventLogsRepository.UnitOfWork.CommitAndRefreshChanges();

				//check store_view event
				if (eventType != CommonEnums.eUserEvents.STORE_VIEW || storeId == null) return true;

				var store = WebStoreRepository.GetById((int) storeId);
				
				if (store == null || !String.IsNullOrEmpty(store.SiteUrl)) return true;

				store.SiteUrl = hostName;
				WebStoreRepository.UnitOfWork.CommitAndRefreshChanges();

				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("save user event",ex,CommonEnums.LoggerObjectTypes.EventLogs);
				return false;
			}
		}
		#endregion

		#region common
		private static readonly string[] adminEmailList = { "************************", "***********************", "******************************" };
		public void SendAdminMail(string subject,string message,string error)
		{
			try
			{
				var _amazonEmailWrapper = DependencyResolver.Current.GetService<IAmazonEmailWrapper>();

				foreach (var email in adminEmailList)
				{
					var messageToken = new EmailMessageDTO
					{
							 Subject     = subject
							,MessageFrom = "noreplay@**************************"
							,UserId      = CurrentUserId
							,ToEmail     = email
							,ToName      = email
							,Status      = EmailEnums.eSendInterfaceStatus.Waiting
							,AddOn       = DateTime.Now
							,MessageBody = $"{message}<br/><br/>{error}"
					};
					var entity = messageToken.Dto2EmailMesageEntity();
					EmailMessageRepository.Add(entity);

					if(!EmailMessageRepository.UnitOfWork.CommitAndRefreshChanges()) continue;
					string mailError;
					_amazonEmailWrapper.SendEmail(entity.EmailId, out mailError);
				}                
			}
			catch (Exception ex)
			{
				Logger.Error("send admin email message::" + error, null, ex, CommonEnums.LoggerObjectTypes.Email);
			}
			
		}
		#endregion

		#region dispose
		public void Dispose()
		{
			try
			{
				UserRepository.Dispose();
				UserVideosRepository.Dispose();
				UserVideoStatsRepository.Dispose();
				UserProfileRepository.Dispose();
				UserCourseReviewsRepository.Dispose();
				UserCourseRepository.Dispose();
				UserCourseWatchStateRepository.Dispose();
				UserBundleRepository.Dispose();
				UserNotificationRepository.Dispose();
				UserSessionsRepository.Dispose();
				UserSessionsEventLogsRepository.Dispose();
				UserViewRepository.Dispose();
				UserItemsViewRepository.Dispose();
				UserLoginsViewRepository.Dispose();
				CustomEventsRepository.Dispose();


				CourseRepository.Dispose();
				CourseCategoryRepository.Dispose();
				BundleRepository.Dispose();
				BundleCourseRepository.Dispose();
				BundleCategoryRepository.Dispose();
				ChapterRepository.Dispose();
				ChapterLinkRepository.Dispose();
				ChapterVideoRepository.Dispose();
				CourseChangeLogRepository.Dispose();
				WizardStepsRepository.Dispose();
				CategoryRepository.Dispose();
				CourseAssetsRepository.Dispose();

				CouponRepository.Dispose();
				CouponInstanceRepository.Dispose();

				DiscussionClassRoomRepository.Dispose();
				DiscussionFollowersRepository.Dispose();
				DiscussionHashtagRepository.Dispose();
				DiscussionMessageRepository.Dispose();
				DiscussionMessageHashtagRepository.Dispose();
				DiscussionMessageUserRepository.Dispose();
				DiscussionMessageUserRepository.Dispose();

				WebStoreRepository.Dispose();
				WebStoreItemRepository.Dispose();
				WebStoreViewRepository.Dispose();
				WebStoreItemViewRepository.Dispose();
				WebStoreCategoryRepository.Dispose();
				WebStoresChangeLogRepository.Dispose();

				PaypalPaymentRequestsRepository.Dispose();
				PaypalIpnLogRepository.Dispose();
				PriceListRepository.Dispose();
				PriceRevisionsReposiotry.Dispose();
				CurrencyRepository.Dispose();

				OrderRepository.Dispose();
				OrderLineRepository.Dispose();
				OrdersViewRepository.Dispose();
				OrderLinesViewRepository.Dispose();
				OrderLinePaymentRefundsRepository.Dispose();
				OrderLinePaymentRepository.Dispose();
				OrderLinePaymentsViewRepository.Dispose();
				OrderLinePaymentRefundsViewRepository.Dispose();
				TransactionRepository.Dispose();
				TransactionsViewRepository.Dispose();
				UserPayoutStatmentsRepository.Dispose();
				PayoutExecutionsRepository.Dispose();

				EmailMessageRepository.Dispose();
				EmailTemplateRepository.Dispose();

				FacebookPostRepository.Dispose();

				LogTableRepository.Dispose();

				S3FileInterfaceRepository.Dispose();

				
				FactEventAggRepository.Dispose();
				FactDailyStatsRepository.Dispose();
				FactDailyTotalsRepository.Dispose();
				FactEventAggregatesViewRepository.Dispose();
				EventLogsViewRepository.Dispose();

				ChimpUserListRepository.Dispose();
				ChimpListSegmentRepository.Dispose();
				ChimpRejectsRepository.Dispose();

				QuizViewRepository.Dispose();
				QuizQuestionsRepository.Dispose();
				QuizQuestionsViewRepository.Dispose();
				QuizQuestionAnswerOptionsRepository.Dispose();
				CourseQuizzesRepository.Dispose();
				StudentQuizzesRepository.Dispose();
				StudentQuizAttemptsRepository.Dispose();
				StudentQuizAnswersRepository.Dispose();

				CertificateRepository.Dispose();
				StudentCertificatesRepository.Dispose();
				StudentCertificatesViewRepository.Dispose();
			}
			catch (Exception ex)
			{
				Logger.Error("Service base dispose",ex,CommonEnums.LoggerObjectTypes.Unknown);
			}
		} 
		#endregion
	}
}
