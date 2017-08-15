using LFE.Core.Enums;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace LFE.DataTokens
{

    public class BaseItemToken : BaseModelState
    {
        public int ItemId { get; set; }
        public BillingEnums.ePurchaseItemTypes ItemType { get; set; }
        public string ItemName { get; set; }
        public Guid? ProvisionUid { get; set; }
        public Guid? Uid { get; set; }
    }
    public class ItemLovToken : BaseItemToken
    {
        public UserBaseDTO User { get; set; }

        public string ItemPageUrl { get; set; }
    }

    public class ItemInfoToken : BaseItemToken
    {
        //common
        public BaseUserInfoDTO Author { get; set; }

        public string MetaTags { get; set; }

        [AllowHtml]
        public string IntroHtml { get; set; }
        public long? OverviewVideoIdentifier { get; set; }
        public VideoInfoToken VideoInfoToken { get; set; }
        public int? ClassRoomId { get; set; }
        public int Rating { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsPublished { get; set; }
        public string ThumbUrl { get; set; }
        public bool IsAuthorUnderRGP { get; set; }
        public int NumOfSubscribers { get; set; }
        public bool DisplayOtherLearnersTab { get; set; }
    }

    public class ItemPurchaseCompleteToken : ItemInfoToken
    {
        public PriceLineDTO PriceToken { get; set; }

        public BaseUserInfoDTO BuyerInfo { get; set; }

        public string TrackingID { get; set; }

        public decimal FinalPrice { get; set; }
    }

    public class ItemPurchaseDataToken : BasePurchaseDataToken
    {
        public ItemPurchaseDataToken()
        {
            UserSavedCards = new List<PaymentInstrumentDTO>();
            CreditCard = new CreditCardDTO();
            BillingAddress = new BillingAddressDTO();
            SavePaymentInstrument = true;
            PaymentMethod = BillingEnums.ePaymentMethods.Credit_Card;
            IsValid = true;
        }
        public BillingEnums.ePurchaseItemTypes Type { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; }

    }

    public class BasePurchaseDataToken : BaseModelState
    {
        public BasePurchaseDataToken()
        {
            UserSavedCards = new List<PaymentInstrumentDTO>();
            CreditCard = new CreditCardDTO();
            BillingAddress = new BillingAddressDTO();
            SavePaymentInstrument = true;
            PaymentMethod = BillingEnums.ePaymentMethods.Credit_Card;
            IsValid = true;
        }

        public PriceLineDTO PriceToken { get; set; }

        public decimal Price { get; set; }
        public decimal? MonthlySubscriptionPrice { get; set; }
        public string CouponCode { get; set; }
        public bool IsPurchased { get; set; }
        public bool IsFree { get; set; }
        public bool BuySubscription { get; set; }
        public bool SavePaymentInstrument { get; set; }
        public BillingEnums.ePaymentMethods PaymentMethod { get; set; }
        public UserBaseDTO Author { get; set; }
        public BillingAddressDTO BillingAddress { get; set; }
        public List<BillingAddressViewToken> BillingAddresses { get; set; }
        public CreditCardDTO CreditCard { get; set; }
        public Guid? PaymentInstrumentId { get; set; }
        public List<PaymentInstrumentDTO> UserSavedCards { get; set; }
        public string TrackingID { get; set; }

    }

    public class ItemPurchaseDTO : ItemInfoToken
    {
        public BaseCurrencyDTO Currency { get; set; }

        public List<PriceLineDTO> PriceLines { get; set; }
        public bool IsItemUnderRGP { get; set; }

    }
    public class VideoInfoToken : BaseModelState
    {
        public long? BcIdentifier { get; set; }
        public string PlayListUrl { get; set; }
        //  public string S3Url { get; set; }
        public string ThumbUrl { get; set; }
        public string StillUrl { get; set; }

        public List<RenditionDTO> Renditions { get; set; }
    }

    public class RenditionDTO
    {
        public long RenditionId { get; set; }
        //  public string S3Url { get; set; }
        public string CloudFrontPath { get; set; }

        public string VideoCodec { get; set; }
        public string VideoContainer { get; set; }
        public int? EncodingRate { get; set; }
        public int? FrameHeight { get; set; }
        public int? FrameWidth { get; set; }
    }

    public class ItemProductPageToken : ItemPurchaseDTO
    {


        public List<ContentTreeViewItemDTO> Contents { get; set; }

        //bundle
        public BundleDetailsToken BundleDetails { get; set; }

        //additional

        public string TrackingID { get; set; }
        public ItemAccessStateToken ItemState { get; set; }
    }

    public class BundleDetailsToken
    {
        public List<BundleCourseListDTO> BundleCourses { get; set; }
        public decimal TotalCoursesWorth { get; set; }
    }

    public class ItemViewerPageToken : ItemInfoToken
    {

        //course properties
        public List<ContentTreeViewItemDTO> Contents { get; set; }
        public VideoNavigationToken[] VideosNavigation { get; set; }
        public int LastChapterId { get; set; }
        public int LastVideoId { get; set; }

        //bundle
        public BundleDetailsToken BundleDetails { get; set; }

        //additional
        public string TrackingID { get; set; }
        public ItemAccessStateToken ItemState { get; set; }

        public bool HasCertificateOnComplete { get; set; }
    }
}
