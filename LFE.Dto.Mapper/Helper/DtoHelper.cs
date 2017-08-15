using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Infrastructure.NLogger;
using LFE.Model;

namespace LFE.Dto.Mapper.Helper
{
    public class DtoHelper
    {
        public static NLogLogger Logger { get; set; }

        public DtoHelper()
        {
            Logger = new NLogLogger();
        }
      
    }

    enum TimeSpanKind
    {
        Years = 0,
        Months = 1,
        Weeks = 2,
        Days = 3,
        Yesterday = 4,
        Hours = 5,
        Minutes = 6,
        Seconds = 7
    }

    public static class DtoExtensions
    {
        public static int CurrentUserId
        {
            get
            {
                var userId = "get user id".GetCurrentUserId();
                return userId ?? -1;
            }
        }

        public static CommonEnums.eAdminStatuses ToStatus(this byte statusId)
        {
            return Utils.ParseEnum<CommonEnums.eAdminStatuses>(statusId);
        }

        //public const string COURSE_PAGE_URL = "{0}courses/{1}/{2}";

        //public const string BUNDLE_PAGE_URL = "{0}bundles/{1}/{2}";

        public const string MESSAGE_PAGE_URL = "{0}UserPortal/Discussion/Message/{1}";

        public static string GenerateCourseFullPageUrl(this object obj, string authorName, string courseName, string trackingId , string mode = null)
        {
            return authorName.GenerateItemFullPageUrl(authorName, courseName, BillingEnums.ePurchaseItemTypes.COURSE, trackingId);
        }

        public static string GenerateCoursePageUrl(this object obj, string authorName, string courseName, string trackingId , string mode = null)
        {
            return authorName.GenerateItemPageUrl(authorName, courseName, BillingEnums.ePurchaseItemTypes.COURSE, trackingId);
        }

        public static string GenerateBundleFullPageUrl(this object obj, string authorName, string bundleName, string trackingId, string mode = null)
        {
            return authorName.GenerateItemFullPageUrl(authorName, bundleName, BillingEnums.ePurchaseItemTypes.BUNDLE, trackingId);
        }

        public static string GenerateBundlePageUrl(this object obj, string authorName, string bundleName, string trackingId , string mode = null)
        {
            return authorName.GenerateItemPageUrl(authorName, bundleName, BillingEnums.ePurchaseItemTypes.BUNDLE, trackingId);
        }

        public static string GenerateStoreItemPageUrl(this object obj, string authorName, string courseName,string categoryName, string trackingId)
        {
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return url.RouteUrl("WebStore_CourseFull", new { trackingID = trackingId, controller = "Course", action = "Index", categoryName = categoryName.OptimizedUrl(), authorName = authorName.OptimizedUrl(), courseName = courseName.OptimizedUrl() });
        }                

        #region Datetime functions
        private static TimeSpanKind ToTimeSpanKindSince(this DateTimeOffset sourceDateTime, DateTimeOffset compareToDateTime, out int span)
        {
            TimeSpan timeSpan = compareToDateTime.Subtract(sourceDateTime);

            if (timeSpan.TotalDays / 365 >= 1)
            {
                span = Convert.ToInt32(Math.Round(timeSpan.TotalDays / 365));
                return TimeSpanKind.Years;
            }

            if (timeSpan.TotalDays / 30 >= 1)
            {
                span = Convert.ToInt32(Math.Round(timeSpan.TotalDays / 30));
                return TimeSpanKind.Months;
            }

            if (timeSpan.TotalDays / 7 >= 1)
            {
                span = Convert.ToInt32(Math.Round(timeSpan.TotalDays / 7));
                return TimeSpanKind.Weeks;
            }
            if (timeSpan.Days > 1)
            {
                span = timeSpan.Days;
                return TimeSpanKind.Days;
            }

            if (timeSpan.Days == 1)
            {
                span = timeSpan.Days;
                return TimeSpanKind.Yesterday;
            }

            if (timeSpan.Hours >= 1)
            {
                span = timeSpan.Hours;
                return TimeSpanKind.Hours;
            }

            if (timeSpan.Minutes >= 1)
            {
                span = timeSpan.Minutes;
                return TimeSpanKind.Minutes;
            }

            span = timeSpan.Seconds;
            return TimeSpanKind.Seconds;
        }

        public static string ToVerbalTimeSpanSince(this DateTimeOffset sourceDateTime, DateTimeOffset compareToDateTime)
        {
            var verbalSpan = String.Empty;

            int spanValue;
            var spanKind = sourceDateTime.ToTimeSpanKindSince(compareToDateTime, out spanValue);

            switch (spanKind)
            {
                case TimeSpanKind.Years:
                    verbalSpan = "year";
                    break;
                case TimeSpanKind.Months:
                    verbalSpan = "month";
                    break;
                case TimeSpanKind.Weeks:
                    verbalSpan = "week";
                    break;
                case TimeSpanKind.Days:
                    verbalSpan = "day";
                    break;
                case TimeSpanKind.Yesterday:
                    verbalSpan = "day";
                    break;
                case TimeSpanKind.Hours:
                    verbalSpan = "hour";
                    break;
                case TimeSpanKind.Minutes:
                    verbalSpan = "minute";
                    break;
                case TimeSpanKind.Seconds:
                    verbalSpan = "second";
                    break;
            }

            return string.Format("{0} {2}{1}", spanValue, spanValue > 1 ? "s" : string.Empty, verbalSpan);

        }

        public static string ToVerbalDateSinceNow(this DateTimeOffset sourceDateTime)
        {
            var verbalSpan = String.Empty;

            int spanValue;
            var spanKind = sourceDateTime.ToTimeSpanKindSince(DateTimeOffset.Now, out spanValue);

            switch (spanKind)
            {
                case TimeSpanKind.Years:
                    verbalSpan = sourceDateTime.ToString("dd-MMM-yy", CultureInfo.InvariantCulture);
                    break;
                case TimeSpanKind.Months:
                    verbalSpan = sourceDateTime.ToString("dd-MMM-yy", CultureInfo.InvariantCulture);
                    break;
                case TimeSpanKind.Weeks:
                    verbalSpan = string.Format("{0} {2}{1} ago", spanValue > 1 ? spanValue.ToString() : "a", spanValue > 1 ? "s" : string.Empty, "week");
                    break;
                case TimeSpanKind.Days:
                    verbalSpan = "last " + sourceDateTime.ToString("dddd", CultureInfo.InvariantCulture);
                    break;
                case TimeSpanKind.Yesterday:
                    verbalSpan = "yesterday";
                    break;
                case TimeSpanKind.Hours:
                    verbalSpan = string.Format("{0} hour{1} ago", spanValue, spanValue > 1 ? "s" : string.Empty);
                    break;
                case TimeSpanKind.Minutes:
                    verbalSpan = string.Format("{0} minute{1} ago", spanValue, spanValue > 1 ? "s" : string.Empty);
                    break;
                case TimeSpanKind.Seconds:
                    verbalSpan = string.Format("{0} second{1} ago", spanValue, spanValue > 1 ? "s" : string.Empty);
                    break;
            }

            return verbalSpan;

        }

        public static string Date2String(this DateTime? date, string pattern = null, string culture = "en-US")
        {
            return date == null ? string.Empty : Date2String((DateTime)date, pattern, culture);
        }

        public static string Date2String(this DateTime date, string pattern = null, string culture = "en-US")
        {
            var format = String.IsNullOrEmpty(pattern) ? "dd MMM yyyy" : pattern;

            return date.ToString(format, String.IsNullOrEmpty(culture) ? CultureInfo.CurrentCulture : new CultureInfo(culture));
        } 
        #endregion

        #region Full name
        public static string CombineFullName(string first, string last, string nick)
        {
            return !String.IsNullOrEmpty(first) || !String.IsNullOrEmpty(last)
                ? String.Format("{0} {1}", first, last)
                : nick;
        }

        public static string Entity2FullName(this Users entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2FullName(this ADMIN_SystemLogToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string RegisterToken2FullName(this RegisterDTO token)
        {
            return CombineFullName(token.FirstName, token.LastName, string.Empty);
        }

        public static string Entity2FullName(this vw_USER_UsersLib entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2FullName(this vw_USER_UserLogins entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2FullName(this vw_WS_StoresLib entity)
        {
            return CombineFullName(entity.OwnerFirstName, entity.OwnerLastName, entity.OwnerNickname);
        }
        public static string Entity2FullName(this vw_CERT_StudentCertificates entity)
        {
            return CombineFullName(entity.StudentFirstName, entity.StudentLastName, entity.StudentNickname);
        }
        public static string Entity2FullName(this UDEMY_JobToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2FullName(this USER_StatisticToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2FullName(this ADMIN_SaleSummaryToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2FullName(this PO_MonthlyPayoutToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2FullName(this APP_PluginsInstallationsRep entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, null);
        }
        public static string Entity2AuthorFullName(this USER_CourseListToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }
        //
        public static string Entity2AuthorFullName(this QZ_StudentAttemptToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }
        //
        public static string Entity2AuthorFullName(this vw_QZ_CourseQuizzes entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2AuthorFullName(this QZ_StudentQuizInfoToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }
        public static string Entity2StudentFullName(this QZ_StudentQuizInfoToken entity)
        {
            return CombineFullName(entity.StudentFirstName, entity.StudentLastName, entity.StudentNickname);
        }
        public static string Entity2AuthorFullName(this CRS_BundleListToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }

        public static string Entity2AuthorFullName(this USER_CourseToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }
        public static string Entity2FullName(this USER_VideoInfoToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2FullName(this DSC_RoomMessageToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2FullName(this USER_AuthorWithCourseCountToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2FullName(this EMAIL_NotificationMessageToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2PosterFullName(this EMAIL_NotificationMessageToken entity)
        {
            return CombineFullName(entity.PosterFirstName, entity.PosterLastName, entity.PosterNickname);
        }

        public static string Entity2PosterFullName(this DSC_NotificationsFbToken entity)
        {
            return CombineFullName(entity.PosterFirstName, entity.PosterLastName, entity.PosterNickname);
        }

        public static string Entity2AuthorFullName(this DSC_NotificationsFbToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }

        public static string Entity2AuthorFullName(this CRS_ReviewAuthorMessageToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }

        public static string Entity2WriterFullName(this CRS_ReviewAuthorMessageToken entity)
        {
            return CombineFullName(entity.ReviewWriterFirstName, entity.ReviewWriterLastName, entity.ReviewWriterNickname);
        }

        public static string Entity2AuthorFullName(this CRS_ReviewLearnerMessageToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }

        public static string Entity2AuthorFullName(this CRS_BundleCourseToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickName);
        }

        public static string Entity2AuthorFullName(this USER_ItemToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }
        public static string Entity2AuthorFullName(this LRNR_ItemToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickname);
        }

        public static string Entity2AuthorFullName(this vw_USER_Items entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickName);
        }
        public static string Entity2WriterFullName(this CRS_ReviewLearnerMessageToken entity)
        {
            return CombineFullName(entity.ReviewWriterFirstName, entity.ReviewWriterLastName, entity.ReviewWriterNickname);
        }

        public static string Entity2LearnerFullName(this CRS_ReviewLearnerMessageToken entity)
        {
            return CombineFullName(entity.LearnerFirstName, entity.LearnerLastName, entity.LearnerNickname);
        }

        public static string Entity2BuyerFullName(this vw_SALE_Transactions entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }

        public static string Entity2SellerFullName(this vw_SALE_Transactions entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }

        public static string Entity2LearnerFullName(this CRS_LearnerToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2StoreOwnerFullName(this WS_AffiliateItemToken entity)
        {
            return CombineFullName(entity.OwnerFirstName, entity.OwnerLastName, entity.OwnerNickName);
        }
        public static string Entity2StoreOwnerFullName(this WS_WixStoreToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2AuthorFullName(this WS_AffiliateItemToken entity)
        {
            return CombineFullName(entity.AuthorFirstName, entity.AuthorLastName, entity.AuthorNickName);
        }

        public static string Entity2SellerFullName(this vw_SALE_OrderLines entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }

        public static string Entity2SellerFullName(this DB_SaleDetailsToken entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }

        public static string Entity2SellerFullName(this DB_SubscriptionCancelToken entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }

        public static string Entity2SellerFullName(this DB_CouponUsageToken entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }

        public static string Entity2SellerFullName(this DB_RefundDetailToken entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }

        public static string Entity2SellerFullName(this SALE_OrderLineViewToken entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }

        public static string Entity2SellerFullName(this vw_SALE_Orders entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }
        public static string Entity2SellerFullName(this vw_SALE_OrderLinePayments entity)
        {
            return CombineFullName(entity.SellerFirstName, entity.SellerLastName, entity.SellerNickName);
        }
        public static string Entity2BuyerFullName(this vw_SALE_OrderLinePayments entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }

        public static string Entity2StoreOwnerFullName(this vw_SALE_OrderLines entity)
        {
            return CombineFullName(entity.StoreOwnerFirstName, entity.StoreOwnerLastName, entity.StoreOwnerNickname);
        }

        public static string Entity2StoreOwnerFullName(this vw_SALE_OrderLinePayments entity)
        {
            return CombineFullName(entity.StoreOwnerFirstName, entity.StoreOwnerLastName, entity.StoreOwnerNickname);
        }

        public static string Entity2StoreOwnerFullName(this DB_SaleDetailsToken entity)
        {
            return CombineFullName(entity.StoreOwnerFirstName, entity.StoreOwnerLastName, entity.StoreOwnerNickname);
        }

        public static string Entity2StoreOwnerFullName(this DB_RefundDetailToken entity)
        {
            return CombineFullName(entity.StoreOwnerFirstName, entity.StoreOwnerLastName, entity.StoreOwnerNickname);
        }
        public static string Entity2StoreOwnerFullName(this DB_CouponUsageToken entity)
        {
            return CombineFullName(entity.StoreOwnerFirstName, entity.StoreOwnerLastName, entity.StoreOwnerNickname);
        }
        public static string Entity2StoreOwnerFullName(this DB_SubscriptionCancelToken entity)
        {
            return CombineFullName(entity.StoreOwnerFirstName, entity.StoreOwnerLastName, entity.StoreOwnerNickname);
        }

        public static string Entity2BuyerFullName(this vw_SALE_OrderLinePaymentRefunds entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }
        public static string Entity2BuyerFullName(this vw_SALE_OrderLines entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }

        public static string Entity2BuyerFullName(this DB_SaleDetailsToken entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }

        public static string Entity2BuyerFullName(this DB_CouponUsageToken entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }

        public static string Entity2BuyerFullName(this DB_RefundDetailToken entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }

        public static string Entity2BuyerFullName(this DB_SubscriptionCancelToken entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }

        public static string Entity2BuyerFullName(this SALE_OrderLineViewToken entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }
        public static string Entity2BuyerFullName(this vw_SALE_Orders entity)
        {
            return CombineFullName(entity.BuyerFirstName, entity.BuyerLastName, entity.BuyerNickName);
        }
        public static string Entity2AuthorFullName(this CRS_CourseToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2AuthorFullName(this CRS_BundleInfoToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        public static string Entity2FullName(this USER_NotificationToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2FullName(this LOG_FileInterfaceToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        
        public static string Entity2FullName(this LOG_EmailInterfaceToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        
        public static string Entity2FullName(this LOG_FbPostInterfaceToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }

        public static string Entity2FullName(this DROP_JobToken entity)
        {
            return CombineFullName(entity.FirstName, entity.LastName, entity.Nickname);
        }
        #endregion

        #region Photo url
        private static string GetPhotoUrl(string photoUrl, string fbUid, string imageBase, string defaultAvatarUrl)
        {
            return String.IsNullOrEmpty(photoUrl) ? ( String.IsNullOrEmpty(fbUid) ? defaultAvatarUrl :  fbUid.FbUid2ImageUrl() ) : ( photoUrl.StartsWith("http") ? photoUrl : imageBase + photoUrl );
        }

        private static string FbUid2ImageUrl(this string fbUid)
        {
            return "http://graph.facebook.com/"+fbUid+"/picture?type=large";
        }

        public static string Entity2PhotoUrl(this Users entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this DSC_RoomMessageToken entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this CRS_LearnerToken entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this vw_USER_UserLogins entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this vw_USER_EventsLog entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this LOG_FileInterfaceToken entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this LOG_FbPostInterfaceToken entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this LOG_EmailInterfaceToken entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2PhotoUrl(this USER_NotificationToken entity, string imageBaseUrl, string defaultAvatarUrl)
        {
            return GetPhotoUrl(entity.PictureURL, entity.FacebookID, imageBaseUrl, defaultAvatarUrl);
        }

        public static string Entity2SignatureUrl(this CERT_CertificateLib entity, string imageBaseUrl, string defaultSignatureUrl)
        {
            return SitgnatureUrl2FullUrl(entity.SitgnatureUrl, imageBaseUrl, defaultSignatureUrl);
        }
        public static string Entity2SignatureUrl(this vw_CERT_StudentCertificates entity, string imageBaseUrl, string defaultSignatureUrl)
        {
            return SitgnatureUrl2FullUrl(entity.SitgnatureUrl,imageBaseUrl,defaultSignatureUrl);
        }

        private static string SitgnatureUrl2FullUrl(string signUrl, string imageBaseUrl, string defaultSignatureUrl)
        {
            return String.IsNullOrEmpty(signUrl) ? defaultSignatureUrl : imageBaseUrl + signUrl;
        }
        #endregion

        #region course thumb url
        public static string ToThumbUrl(this string imagePath, string imageBaseUrl)
        {
            return String.IsNullOrEmpty(imagePath) ? string.Empty : imageBaseUrl + imagePath;
        }
        #endregion

        #region web store

        public static string StoreItemName2DisplayName(this string name, BillingEnums.ePurchaseItemTypes type)
        {
            return String.Format("{0} {1}",type==BillingEnums.ePurchaseItemTypes.COURSE ? string.Empty : "(Bundle)" ,name);
        }

        public static string NameWithContentCounts2DisplayName(this string name, int coursesCnt, int bundlesCnt)
        {
            return name + String.Format("({0} {1})",
                coursesCnt > 0 ? String.Format("{0} courses", coursesCnt) : string.Empty,
                bundlesCnt > 0 ? String.Format("{0} bundles", bundlesCnt) : string.Empty);

        }
        #endregion

        #region pricing

        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this SALE_OrderLineViewToken entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId = entity.CurrencyId ?? Constants.DEFAULT_CURRENCY_ID
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }
        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this vw_SALE_OrderLines entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId = entity.CurrencyId ?? Constants.DEFAULT_CURRENCY_ID
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }

        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this DB_SaleDetailsToken entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }

        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this DB_CouponUsageToken entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }

        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this DB_SubscriptionCancelToken entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId ?? -1
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }

        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this DB_RefundDetailToken entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }
        //
        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this PO_MonthlyPayoutToken entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId
                        ,CurrencyName = entity.CurrencyName
                        ,ISO          = entity.ISO
                        ,Symbol       = entity.Symbol
                    };
        }
        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this ADMIN_SaleSummaryToken entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId ?? -1
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }
        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this vw_SALE_Transactions entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId ?? -1
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }
        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this vw_SALE_OrderLinePayments entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }

        public static BaseCurrencyDTO Entity2BaseCurrencyDto(this vw_SALE_OrderLinePaymentRefunds entity)
        {
            return new BaseCurrencyDTO
                    {
                        CurrencyId    = entity.CurrencyId
                        ,CurrencyName = entity.CurrencyName ?? string.Empty
                        ,ISO          = entity.ISO ?? string.Empty
                        ,Symbol       = entity.Symbol ?? string.Empty
                    };
        }
        public static string ToPriceLineName(this PriceLineDTO token)
        {
            return token.PriceType == BillingEnums.ePricingTypes.FREE ? "Free" : (token.PriceType == BillingEnums.ePricingTypes.RENTAL ? String.Format("{0} {1}", token.NumOfPeriodUnits, Utils.GetEnumDescription(token.PeriodType)) : null) ;
        }

        #endregion
    }
}
