using Autofac;
using Autofac.Integration.Mvc;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Domain.Context;
using LFE.Domain.Context.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebMatrix.WebData;

namespace LFE.Core.Extensions
{
	public static class AppExtensions
	{
		#region user
		private static Claim GetClaim(string type)
		{
			 return ClaimsPrincipal.Current.Claims.FirstOrDefault(c => c.Type.ToString(CultureInfo.InvariantCulture) == type);
		}

		public static int? GetCurrentUserId(this object obj)
		{
			try
			{
				return !WebSecurity.IsAuthenticated ? (int?)null : Convert.ToInt32(GetClaim(ClaimTypes.NameIdentifier).Value);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string GetCurrentUserName(this object obj)
		{
			try
			{
				return !WebSecurity.IsAuthenticated ? null : GetClaim(ClaimTypesHelper.CLAIM_FULLNAME).Value;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string GetCurrentUserEmail(this object obj)
		{
			try
			{
				return !WebSecurity.IsAuthenticated ? null : GetClaim(ClaimTypes.Email).Value;
			}
			catch (Exception)
			{
				return null;
			}
		}
		#endregion

		#region commons
		public static DateTime UtcDateTime(this object obj)
		{
			var localZone = TimeZone.CurrentTimeZone;
			var localTime = DateTime.Now;
			var utc = localZone.ToUniversalTime(localTime);

			return utc;
		}
		public static string CleanUrl(this string text)
		{
			if (text == null)
			{
				return null;
			}

			return text
				.ToLower()
				.Replace(" ", "-")
				.Replace("%20", "-")
				.Replace("+", "-")
				.Replace("*", "-")
				.Replace("!", "-")
				.Replace("&amp;", "and") // first "&amp;" then "&" since "&" is part of "&amp;"
				.Replace("&", "and");
		}
		
		public static string CleanFileName(this string text)
		{
			return String.Format("{0}{1}",Path.GetFileNameWithoutExtension(text).CleanName(),Path.GetExtension(text));
		}  

		public static string CleanName(this string text)
		{
			var result = string.Empty;
			var regex = new Regex("[0-9A-Za-z_-]+");
			foreach (Match match in regex.Matches(text))
				result += match.Value;
			return result.Equals(string.Empty) ? ShortGuid.NewGuid().ToString() : result;
		}

        public static void RemoveRoutes(this object obj,RouteValueDictionary currentRouteData)
		{
			var keyList = new List<string>(currentRouteData.Keys);

			string[] ignore = { "Area", "Controller", "Action" };
			foreach (var key in keyList)
			{
				if (!ignore.Contains(key, StringComparer.CurrentCultureIgnoreCase))
					currentRouteData.Remove(key);
			}
		}
	 
		#endregion

		#region App start
		public static void RegisterRepositories(ContainerBuilder builder)
		{
			// registration UoW - should be transferred to IoC factory
			// MVC app shouldn't be aware of repository, only services
			builder.Register(c => new UnitOfWork()).AsSelf().InstancePerHttpRequest();

			//register repositories
			#region user
			builder.Register(c => new UserRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserRefundProgramRevisionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserCourseRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserCourseWatchStateRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserBundleRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserProfileRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserCourseReviewsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new TransactionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserPaymentInstrumentsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserAddressRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserNotificationRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserSessionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserSessionsEventLogsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserItemsViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserLoginsViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserVideosRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserVideosRenditionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserLoginsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserVideoStatsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region course
			builder.Register(c => new CourseRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CourseCategoryRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new BundleRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new BundleCourseRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new BundleCategoryRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new ChapterRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new ChapterLinkRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new ChapterVideoRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CourseChangeLogRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new WizardStepsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CategoryRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CourseAssetsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region coupons
			builder.Register(c => new CouponRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CouponInstanceRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region discussion
			builder.Register(c => new DiscussionClassRoomRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new DiscussionFollowersRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new DiscussionHashtagRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new DiscussionMessageRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new DiscussionMessageHashtagRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new DiscussionMessageUserRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new DiscussionMessageHashtagViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region web store
			builder.Register(c => new WebStoreRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new WebStoreItemRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new WebStoreCategoryRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new WebStoresChangeLogRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new WebStoreItemViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new WebStoreViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region billing
			builder.Register(c => new PaypalPaymentRequestsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new PaypalIpnLogRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrderRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrderLineRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrderLinePaymentRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new TransactionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrdersViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrderLinePaymentRefundsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrderLinesViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrderLinePaymentsViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new OrderLinePaymentRefundsViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new TransactionsViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new PriceListRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new PriceRevisionsReposiotry(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CurrencyRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new RefundRequestsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserPayoutStatmentsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new PayoutExecutionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region common
			builder.Register(c => new EmailMessageRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new EmailTemplateRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new FacebookPostRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();

			builder.Register(c => new S3FileInterfaceRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new LogTableRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
		
			builder.Register(c => new GeoCountriesRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new GeoStatesRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CustomEventsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region reports
			builder.Register(c => new FactEventAggRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new FactDailyStatsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new FactDailyTotalsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new FactEventAggregatesViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new EventLogsViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new UserSessionsEventLogsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region plugins
			builder.Register(c => new PluginInstallationsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new PluginInstallationStoresRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion

			#region mailchimp
			builder.Register(c => new ChimpUserListRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new ChimpListSegmentRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new ChimpRejectsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion
            
			#region quizzes
			builder.Register(c => new QuizViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new QuizQuestionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new QuizQuestionsViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new QuizQuestionAnswerOptionsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new CourseQuizzesRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new StudentQuizzesRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new StudentQuizAttemptsRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new StudentQuizAnswersRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();            
			#endregion

			#region cert6ificates
			builder.Register(c => new CertificateRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new StudentCertificatesRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			builder.Register(c => new StudentCertificatesViewRepository(c.Resolve<UnitOfWork>())).AsImplementedInterfaces().InstancePerLifetimeScope();
			#endregion
		}

		//public static void RegisterServices(ContainerBuilder builder)
		//{
		//    builder.Register(c => new InMemoryCacheService()).AsImplementedInterfaces().SingleInstance();

		//    //external providers
		//    builder.RegisterType<BrightcoveWrapper>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<S3Wrapper>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<AmazonEmailWrapper>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<FacebookServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<PaypalServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		

		//    //common
		//    builder.RegisterType<WidgetUserServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<CourseServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<CourseWizardServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<CouponWidgetServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<DiscussionServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<EmailServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<ReportServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<SettingsServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<WebStoreServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<WidgetServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<BillingServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//    builder.RegisterType<GeoServices>().AsImplementedInterfaces().InstancePerHttpRequest();
		//}
		#endregion

        #region s3 services
        
        #endregion
	}

	public static class BillingExtensions
	{
		public static decimal BalanceToLfeCommission(this decimal balance)
		{
			try
			{
				return Math.Round((balance * Constants.LFE_COMMISSION), 2);
			}
			catch (Exception)
			{
				return 0;
			}
		}
	   
		public static decimal SalesToBalance(this decimal authorSales, decimal fees, decimal refunds, decimal refundFees, decimal affiliateCommissionCombined,decimal rgp2Keep,decimal rgp2Release)
		{
			return Math.Round(authorSales + affiliateCommissionCombined + rgp2Release - rgp2Keep - fees - refunds + refundFees, 2);
		}

		public static decimal SalesToBalance(this decimal sales, decimal fees, decimal refunds,decimal refundFees)
		{
			return Math.Round(sales - fees - refunds  + refundFees, 2);
		}


		public static decimal BalanceToPayout(this decimal balance, decimal commission)
		{
			try
			{
				return Math.Round(balance - commission, 2);
			}
			catch (Exception)
			{
				return 0;
			}
		}
		//TODO obsolete , should be fixed
		public static decimal ToLfeCommission(this decimal total, decimal fees, decimal refunds)
		{
			try
			{
				return Math.Round(((total - fees - refunds) * Constants.LFE_COMMISSION), 2);
			}
			catch (Exception)
			{
				return 0;
			}
		}

		public static decimal ToPayout(this decimal total, decimal fees, decimal refunds, decimal commission)
		{
			try
			{
				return Math.Round(((total - Math.Abs(fees) - Math.Abs(refunds) - Math.Abs(commission))), 2);
			}
			catch (Exception)
			{
				return 0;
			}
		}
	}

	public static class ItemExtensions
	{
		public static string GenerateItemPageUrl(this object obj, string author, string itemName,BillingEnums.ePurchaseItemTypes type,string trackingId = null, string mode = null,int? width = null,int? height = null,string compId = null)
		{
			var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

			var routeName = "Widget_";

			switch (type)
			{
				case BillingEnums.ePurchaseItemTypes.COURSE:
					routeName += String.Format("{0}Course", String.IsNullOrEmpty(trackingId) ? string.Empty : "Store");
					break;
				case BillingEnums.ePurchaseItemTypes.BUNDLE:
					routeName += String.Format("{0}Bundle", String.IsNullOrEmpty(trackingId) ? string.Empty : "Store");
					break;
				default:
					return string.Empty;
			}

			//Widget_StoreCourse
			//Widget_Course
			//Widget_StoreBundle
			//Widget_Bundle

			var dict = new RouteValueDictionary
			{
				{"type", type},
				{"author",author.CleanUrl()},
				{"itemName",itemName.CleanUrl()},
				{"trackingId",String.IsNullOrEmpty(trackingId) ? "" : trackingId.CleanQsParamValue()}
			};

			if(!String.IsNullOrEmpty(mode)) dict.Add("mode", mode);
			if (width != null) dict.Add("width", width.ToString().CleanQsParamValue());
			if (height != null) dict.Add("height", height.ToString().CleanQsParamValue());
			if (!String.IsNullOrEmpty(compId)) dict.Add("compId", compId.CleanQsParamValue());

			var routeUrl = url.RouteUrl(routeName,dict);

			return routeUrl;
		}

		private static string CleanQsParamValue(this string qs)
		{
			try
			{
				return !qs.Contains(",") ? qs : qs.Split(Convert.ToChar(","))[0];
			}
			catch (Exception)
			{
				return qs;
			}
		}

		public static string GenerateItemFullPageUrl(this object obj, string author, string itemName, BillingEnums.ePurchaseItemTypes type, string trackingId = null, string mode = null, int? width = null, int? height = null)
		{
			var relative = itemName.GenerateItemPageUrl(author, itemName, type, trackingId);

			if (String.IsNullOrEmpty(relative)) return string.Empty;

			return "baseUrl".GetKeyValue() + relative.Remove(0, 1);
		}

		public static string GenerateCheckoutUrl(this string action, int priceLineId, int? orderNo, string trackingId = null, string refferal = null)
		{
			var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

			var routeName = String.Format("Web{0}_Checkout", String.IsNullOrEmpty(trackingId) ? string.Empty : "Store");

			//Web_Checkout
			//WebStore_Checkout
			var dict = new RouteValueDictionary
			{
				{"action", action},
				{"id",priceLineId},
				{"ref",refferal},
				{"trackingId",String.IsNullOrEmpty(trackingId) ? "" : trackingId}
			};

			if(orderNo!=null) dict.Add("orderNo",orderNo);

			var url = urlHelper.RouteUrl(routeName, dict);//String.IsNullOrEmpty(trackingId) ? urlHelper.RouteUrl(routeName, new { id = orderNo }) : urlHelper.RouteUrl(routeName, new { id = orderNo, trackingId });          

			return url;
		}

		private static string GetKeyValue(this string value)
		{
			return ConfigurationManager.AppSettings.Get(value);
		}
	}

	public static class HubExtensions
	{
		public static string CombineInvitationUrl(this object obj, string baseUrl, string key)
		{
			//(new Uri(baseUrl)).Authority

			return String.Format("{0}//{1}/{2}/{3}?key={4}", "https:", (new Uri(baseUrl)).Authority, "Account", "HubRegistration", HttpUtility.UrlEncode(key));
		}
	}
}
