
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Core.Utils;

namespace LFE.DataTokens
{
    public class UserBaseDTO
    {
        public int userId { get; set; }

        [DisplayName("First name")]
        public string firstName { get; set; }

        [DisplayName("Last name")]
        public string lastName { get; set; }

        [DisplayName("Full name")]
        public string fullName { get; set; }
    }
    public class BaseUserDTO
    {
        public string nickname { get; set; }
        public int userId { get; set; }
    }

   

    public class BaseUserInfoDTO
    {

        public BaseUserInfoDTO() { }

        public BaseUserInfoDTO(int userId, string name)
        {
            UserId   = userId;
            FullName = name;
        }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }

    public class UserDTO : BaseUserInfoDTO
    {
        //Primary Key of UsersProfile table
        public int? UserProfileId { get; set; }

        public CommonEnums.eRegistrationSources RegistrationSource { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Nickname { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime RegisterTime { get; set; }

        public DateTime RegisterDate { get; set; }

        public bool IsConfirmed { get; set; }

        public bool IsPayoutOptionsDefined { get; set; }

        public string FacebookId { get; set; }
    }

    public class UserInfoDTO : BaseUserInfoDTO
    {
        public string FacebookId { get; set; }
     
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class UserGridViewDto : UserDTO
    {
        public UserEnums.eUserStatuses Status { get; set; }

        public string PictureUrl { get; set; }

        public bool IsSocialLogin { get; set; }

        public string ProviderName { get; set; }

        public int ActivityScore { get; set; }

        public int LoginsCount { get; set; }
    }

    public class UserStatisticToken
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int courses { get; set; }
        public int bundles { get; set; }
        public int chapters { get; set; }
        public int videos { get; set; }
        public int logins { get; set; }
        public int purchases { get; set; }
        public int stores { get; set; }

        public int Score { get; set; }
    }

    public class UserEditDTO : UserDTO
    {
        public UserEditDTO()
        {
            Roles = new List<string>();
        }

        public UserEditDTO(string name)
        {
            UserId        = -1;
            UserProfileId = -1;
            FullName      = name;
            Roles         = new List<string>();
        }

        public UserEnums.eUserStatuses Status { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        public List<string> Roles { get; set; }

        public string PictureUrl { get; set; }
        public bool IsSocialLogin { get; set; }

        public string ProviderName { get; set; }

    }

    public class UserViewDto : UserBaseDTO
    {
        public UserViewDto()
        {
            userId = -1;
        }

        [DisplayName("Type")]
        public int typeId { get; set; }

        [DisplayName("Status")]
        public int statusId { get; set; }

        [DisplayName("Nick")]
        public string nickname { get; set; }

        public int? genderId { get; set; }

        [DisplayName("Email")]
        public string email { get; set; }

        [DisplayName("Birth date")]
        public DateTime? birthDate { get; set; }

        public DateTime? lastLogin { get; set; }
    }

    public class UserProfileDTO : UserViewDto
    {
        [AllowHtml]
        public string bioHtml { get; set; }

        public string PhotoUrl { get; set; }       
    }

    public class UserPayoutSettingsDTO : BaseModelState
    {
        public UserPayoutSettingsDTO()
        {
            PayoutType = BillingEnums.ePayoutTypes.PAYPAL;
        }

        public int UserId { get; set; }

        public BillingEnums.ePayoutTypes PayoutType { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Address { get; set; }
        public BillingAddressDTO BillingAddress { get; set; }
    }

    public class VideoImageDto
    {
        public long identifier { get; set; }

        public FileEnums.ImageType Type { get; set; }

        public string ImageName { get; set; }

        public Stream _Stream { get; set; }

        public string ContentType { get; set; }
    }

    public class UserVideoDto : BaseUserDTO
    {
        public UserVideoDto()
        {
            uses = 0;
            identifier = "-1";
        }

        public int? fileId { get; set; }
        public string identifier { get; set; }

        public string bcid { get; set; }

        [DisplayName("Video Name")]
        public string title { get; set; }
        public int views { get; set; }
        public int uses { get; set; }
        public TimeSpan? duration { get; set; }
        public long millisec { get; set; }
        public string minutes { get; set; }
        public string thumbUrl { get; set; }
        public string stillUrl { get; set; }
        public string videoUrl { get; set; }
        public DateTime addon { get; set; }
        public IEnumerable<string> tags { get; set; }
        public string tagsStr { get; set; }
        public ImportJobsEnums.eFileInterfaceStatus status { get; set; }
        public string name { get; set; }
    }

   
    public class UserTagItemDTO
    {
        public int value { get; set; }

        public string label { get; set; }

        public string image { get; set; }
        
    }

    public class UserNotificationDTO : MessageViewDTO
    {
        public bool isRead { get; set; }
    }

    public class AccountSettingsDTO
    {
        public AccountSettingsDTO()
        {
            Role = CommonEnums.UserRoles.Learner;
            AffiliateCommission = Constants.AFFILIATE_COMMISSION_DEFAULT;
        }
        public int UserId { get; set; }

        public CommonEnums.UserRoles Role { get; set; }

        [DisplayName("Registered email")]
        public string Email { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Nickname")]
        public string Nickname { get; set; }

        [DisplayName("About me")]
        [AllowHtml]
        public string BioHtml { get; set; }

        public bool IsSocialLogin { get; set; }
        public long? FbUid { get; set; }
        public FbUser FbUser { get; set; }

        public string PictureURL { get; set; }
        public string PictureName { get; set; }

        [DataType(DataType.Password)]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm new password")]
        public string ReenterPassword { get; set; }

        [DisplayName("Display my activities on LFE Facebook wall")]
        public bool DisplayActivitiesOnFB { get; set; }

        [DisplayName("Receive LFE Newsletter (Monthly) by Email")]
        public bool ReceiveMonthlyNewsletterOnEmail { get; set; }

        [DisplayName("Affiliate Commission Percent")]
        public decimal? AffiliateCommission { get; set; }

        public bool DisplayDiscussionFeedDailyOnFB { get; set; }
        public bool ReceiveDiscussionFeedDailyOnEmail { get; set; }
        public bool DisplayCourseNewsWeeklyOnFB { get; set; }
        public bool ReceiveCourseNewsWeeklyOnEmail { get; set; }

        public bool ShowCancelButton { get; set; }
    }

    public class LearnerListItemDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string photoUrl { get; set; }
        public string fbUid { get; set; }
    }

    public class SubscriberDTO : LearnerListItemDTO
    {
        public string email { get; set; }
        public string url { get; set; }
    }


    public class VideoStatsToken
    {
        public int position { get; set; }
        public bool isInProgress { get; set; }
        public Guid? sessionId { get; set; }
        public long? bcId { get; set; }

        public int? ChapterId { get; set; }
        public UserEnums.eVideoActions? action { get; set; }

        public string endReason { get; set; }
        public string startReason { get; set; }
    }

    public class VideoStatsReasonToken
    {
        public Guid? sessionId { get; set; }
        public UserEnums.eVideoActions? action { get; set; }
        public string reason { get; set; }
        public string endReason { get; set; }
        public string startReason { get; set; }
    }
}

#region not in use
//public class UserDto : BaseUserDTO
//{

//    //application definitions
//    public int typeId { get; set; }
//    public int statusId { get; set; }

//    //base personal data
//    public int? genderId { get; set; }
//    public string email { get; set; }
//    public string firstName { get; set; }
//    public string lastName { get; set; }
//    public DateTime? birthDate { get; set; }

//    //additional personal data
//    [AllowHtml]
//    public string bioHtml { get; set; }
//    public string authorPictureURL { get; set; }
//    public string pictureURL { get; set; }

//    //authentication
//    public string activationToken { get; set; }
//    public long? FbId { get; set; }
//    public string googleId { get; set; }
//    public string pwdDigest { get; set; }

//    //related properties
//    public int? addressId { get; set; }
//    public int? authorId { get; set; }
//    public bool autoplayEnabled { get; set; }

//    //sales
//    public string salesforce_id { get; set; }
//    public string salesforce_checksum { get; set; }

//    //service dates 
//    public DateTime addOn { get; set; }
//    public DateTime modifiedOn { get; set; }
//    public DateTime? activationExpiration { get; set; }
//    public DateTime? lastLogin { get; set; }
//}
#endregion