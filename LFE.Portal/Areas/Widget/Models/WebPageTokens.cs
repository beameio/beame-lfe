using System;
using System.ComponentModel;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.DataTokens;
using System.Collections.Generic;
using LFE.Portal.Areas.UserPortal.Models;
using LFE.Portal.Areas.WixEndPoint.Models;

namespace LFE.Portal.Areas.Widget.Models
{

    public enum ePageMode
    {
        Product
        ,Viewer
    }
    public enum eActionKinds
    {
         POST
        ,AJAX
    }

    public enum ePricesDisplayMode
    {
         SHORT
        ,FULL
    }

    public enum eUserQuizActions
    {
        LoadQuiz             = 1
        ,StartQuiz           = 2
        ,LoadQuestion        = 3
        ,FinishQuiz          = 4
        ,QuizIntro           = 5
        ,ContactAuthor       = 6
        ,ContactAuthorResult = 7
    }

    public enum eScAuthenticationMode
    {
        [Description("Existing User")] log
        ,[Description("Social Login")] soc
        ,[Description("New LFE Registration")] reg
    }

    public class UserQuizRequestToken
    {
        public eUserQuizActions quizAction { get; set; }
        public Guid? quizId { get; set; }
        public int? questIndex { get; set; }
    }

    public class UserQuizPropBoxToken
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string CssClass { get; set; }
        public string SpanCssClass { get; set; }
    }

    public class NotFoundToken
    {
        public string Title { get; set; }
        public string FirstMessage { get; set; }
        public string SecondMessage { get; set; }
    }


    public class PaypalCompleteRequestToken
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class IndexModelView
    {
        public List<WidgetItemListDTO> CoursesList { get; set; }
        public WidgetCategoryDTO Category { get; set; }
        public WidgetWebStoreDTO WebStore { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Sort { get; set; }
        public int TotalCourses { get; set; }
        public List<WidgetCategoryDTO> CategoriesList { get; set; }
        public string CategoryName { get; set; }
    }

    public class PaymentWindowScriptsToken
    {
        public string PurchaseActionUrl { get; set; }
    }

    public class ItemPricesPageToken
    {
        public ePricesDisplayMode Mode { get; set; }

        public List<PriceLineDTO> PriceLines { get; set; }

        public string TrackingID { get; set; }
    }

    public class PriceBoxToken
    {
        public List<PriceLineDTO> prices { get; set; }

        public BillingEnums.ePricingTypes type { get; set; }

        public ePricesDisplayMode Mode { get; set; }

        public byte EvenOrOdd { get; set; }

        public bool ShowOrIcon { get; set; }

        public string TrackingID { get; set; }
    }

    public class PriceNameBoxToken
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public string currency { get; set; }
        public string iso { get; set; }
        public string cssCLass { get; set; }
        public bool IsUnderGRP { get; set; }
    }

    public class ItemReviewsPageToken
    {
        public int itemId { get; set; }
        public List<ReviewDTO> reviews { get; set; }

        public int raiting { get; set; }

    }

    public class BundleContentsPageToken 
    {
        public ItemIntroToken Item { get; set; }
        public List<BundleCourseListDTO> courses { get; set; }

        public ePageMode mode { get; set; }

    }

    public class ItemIntroToken : BaseItemToken
    {
        [AllowHtml]
        public string IntroHtml { get; set; }
        public long? OverviewVideoIdentifier { get; set; }
        
        public VideoInfoToken VideoInfoToken { get; set; }
        public string TrackingID { get; set; }
    }

  

    public class PurchaseResultToken : BaseModelState
    {
        public PurchaseResultToken()
        {
            ActionKind = eActionKinds.AJAX;
        }

        public bool ShowPage { get; set; }

        public string ItemName { get; set; }

        public string TrackingId { get; set; }

        public int PriceLineId { get; set; }

        public string RedirectUrl { get; set; }

        public eActionKinds ActionKind { get; set; }
    }

    public class FacebookLayoutToken : FacebookPageToken
    {
        public SettingsViewToken Settings { get; set; }
    }

    public class OtherLearnersPageToken
    {
        public AuthorProfilePageToken AuthorProfile { get; set; }

        public int ItemId { get; set; }
    } 

    #region checkout
    public class CheckoutBaseToken : BaseModelState
    {
        public CheckoutBaseToken() { }

        public CheckoutBaseToken(string error, bool isValid)
        {
            IsValid = isValid;
            Message = error;
        }
        public ItemInfoToken ItemInfo { get; set; }

        public WidgetWebStoreDTO WidgetSotre { get; set; }

        public ItemPurchaseDataToken PurchaseDataToken { get; set; }

        public ItemPurchaseCompleteToken PurchaseCompleteToken { get; set; }
        public int PriceLineId { get; set; }

        public string TrackingId { get; set; }

        public string Refferal { get; set; }

        public string ItemPageUrl { get; set; }

        public bool IsFreeItem { get; set; }
    }

    public class CheckoutLayoutToken
    {
        public string Refferal { get; set; }

        public ItemInfoToken ItemInfo { get; set; }
    }
    #endregion
 
    #region quiz
    public class AttemptCounterToken
    {
        public byte? Attempts { get; set; }
        public int UserAttempts { get; set; }

        public bool IsCompleteForm { get; set; }
    }

    public class QuizUserActionsToken
    {
        public Guid QuizId { get; set; }
        public UserQuizStateToken State { get; set; }

        public int? AttemptsRemaining { get; set; }
    }
    #endregion
}