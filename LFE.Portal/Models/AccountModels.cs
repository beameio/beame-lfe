using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Portal.Models
{
    public enum eLoginWindowMode
    {
        Login
        ,Register
    }

    public class LoginWindowToken : BaseModelState
    {
        public CommonEnums.eRegistrationSources RegistrationSource { get; set; }
        public eLoginWindowMode Mode { get; set; }
        public bool RequiredConfirmation { get; set; }
        public string ReturnUrl { get; set; }
        public string TrackingId { get; set; }

        public bool IsPlugin { get; set; }
        public string Uid { get; set; }
    }

    public class HubRegistrationToken : LoginWindowToken
    {
        public bool IsKeyValid { get; set; }
        public int HubId { get; set; }
        public Guid InvitaionUid { get; set; }
        public string InvitaionEmail { get; set; }
    }

    public enum FbPageAppAdminMatchResults 
    {
        Unknown                                    = 0
        ,NotFoundNotAuthenticated                  = 1 //User not authenticated in LFE and not found by email and/or FB UserId  => options: create new user        or associate existing login(logic for this option should be described)
        ,NotFoundAuthenticated                     = 2 //User authenticated in LFE , but not found by email and/or FB UserId    => options: create new user        or approve LFE in FB and associate existing login with lfe account
        ,FoundAndProviderdApproved                 = 3 //User found by fbUid                                                    => force sign-in
        ,FoundAndMatchedByEmail                    = 4 //User matched by email with existing login                              => associate existing login with lfe account
    }

    public class FbAdminAuthenticationResult : BaseModelState
    {
        public FbAdminAuthenticationResult()
        {
            fbUserEmail = string.Empty;
            trackingId  = string.Empty;
            state       = FbPageAppAdminMatchResults.Unknown;
        }
        public FbPageAppAdminMatchResults state { get; set; }

        public long? providerUid { get; set; }

        public int? fbUserId { get; set; }

        public string fbUserEmail { get; set; }

        public string trackingId { get; set; }      
    }

    public class UsersContext : DbContext
    {
        public UsersContext(): base("DefaultConnection"){}

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }        
        public string Email { get; set; }
        public int? RefUserId { get; set; }        
    }

    public class LoginModel : LoginDTO
    {      
    }

    public class WixLoginToken
    {
        public Guid instanceId { get; set; }
        public Guid? uid { get; set; }
        public string instanceToken { get; set; }
        public bool isSuccess { get; set; }
        public string origCompId { get; set; }
        public string compId { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    public class WidgetExternalLoginToken
    {
        public bool IsWidget { get; set; }

        public string ParentWindowURL { get; set; }
    }

    public class UserDataToken
    {
        public int UserId { get; set; }

        public string FullName { get; set; }        
    }

    public class ResetLocalPasswordToken
    {

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }


        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }

        public string PasswordResetToken { get; set; }
    }

    public class ResetPasswordPageToken
    {
        public bool IsValid { get; set; }
        public bool PasswordChanged { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public string PasswordResetToken { get; set; }
    }

    public class UserIndicatorViewModel
    {
        public bool IsLoggedIn { get; set; }

        public int Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class AccountSettingsCommandRowToken
    {
        public CommonEnums.UserRoles Role { get; set; }
        public string FormName { get; set; }
    }
    

    public class RegistrationConfirmToken : BaseModelState
    {
        public bool IsSocial { get; set; }

        public string ConfirmationMessage { get; set; }
    }
}
