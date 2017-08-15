using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class EmailBaseToken
    {
        public int UserID { get; set; }
        public string ToEmail { get; set; }
        public string FullName { get; set; }
    }
    public class NotificationMessageDTO
    {
        public long NotificationId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int PosterId { get; set; }
        public string PosterName { get; set; }
        public DateTime AddOn { get; set; }
        public long MessageId { get; set; }
        public string MessageText { get; set; }
        public string MessageHTML { get; set; }
    }

    public class EmailPurchaseToken
    {
        public BaseItemDTO Item{ get; set; }
        public UserInfoDTO Seller { get; set; }
        public UserInfoDTO Buyer { get; set; }
        public DateTime SentDate { get; set; }

        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? TotalAmount { get; set; }
     
        public BillingEnums.ePaymentMethods PaymentMethod { get; set; }
        public BillingEnums.ePaymentTerms PaymentTerm { get; set; }
        public PriceLineDTO PriceLineToken { get; set; }
        public CouponBaseDTO Coupon { get; set; }
    }

    public class EmailRegistrationToken : EmailBaseToken
    {
        public string ActivationURL { get; set; }
        public DateTime SentDate { get; set; }
    }

    public class EmailForgottenPasswordToken : EmailBaseToken
    {
        public string ResetPasswordURL { get; set; }
        public DateTime SentDate { get; set; }
    }

    public class EmailSubscriptionCancelToken : EmailBaseToken
    {
        public string AuthorName { get; set; }
        public string LearnerName { get; set; }
        public string LearnerEmail { get; set; }
        public string CourseName { get; set; }
        public string Month { get; set; }
    }

    public class EmailMessageDTO
    {       
        public int UserId { get; set; }
        public string Subject { get; set; }
        public string MessageFrom { get; set; }
        public string ToEmail { get; set; }
        public string ToName { get; set; }
        public string MessageBody { get; set; }
        public EmailEnums.eSendInterfaceStatus Status { get; set; }
        public DateTime AddOn { get; set; }
    } 

    public class EmailBodyToken
    {
        public string FullName { get; set; }

        public string PosterName { get; set; }

        public string PostDate { get; set; }

        [AllowHtml]
        public string Title { get; set; }

        [AllowHtml]
        public string MessageBodyHTML { get; set; }
    }

    public class EmailTemplateDTO
    {
        public EmailTemplateDTO()
        {
            TemplateId = -1;
        }

        [Key]
        public short TemplateId { get; set; }

        public EmailEnums.eTemplateKinds Kind { get; set; }

        [DataType(DataType.Html)]
        [AllowHtml]
        public string Snipet { get; set; }
    }

    public class EmailGRPSubmitted : EmailBaseToken
    {
        public string LearnerName { get; set; }
        public string LearnerEmail { get; set; }
        public string ItemName { get; set; }
        public string ReasonText { get; set; }
    }
}
