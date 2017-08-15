using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LFE.Core.Enums;

namespace LFE.DataTokens
{
    public class RegisterDTO    
    {
        public RegisterDTO()
        {
            RegistrationSource = CommonEnums.eRegistrationSources.LFE;
        }

        [Required(ErrorMessage = "Email required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid e-mail format")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Invalid e-mail format")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password required")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [Display(Name = "Password")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Reenter Password")]
        public string ReenterPassword { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Nickname required")]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; }

        public DateTime? Birthday { get; set; }      
        
        public int? Gender { get; set; }

        public string FbUid { get; set; }

        public bool RequiredConfirmation { get; set; }

        public string ParentWindowURL { get; set; }

        public bool IsWidget { get; set; }

        public CommonEnums.eRegistrationSources RegistrationSource { get; set; }

        public string TrackingID { get; set; }

        public bool IsPluginRegistration { get; set; }
        public string Uid { get; set; } // installation id, used in wordpress plugin

        public string RegisterHostName { get; set; }

        public bool LicenseRequired { get; set; }

        public string LicenseKey { get; set; }
    }

    public class LoginDTO
    {
        public bool IsLoggedIn { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password, ErrorMessage = "Not a valid email")]
        public string Password { get; set; }
        
        public string LoginError { get; set; }
        
        public string RedirectUrl { get; set; }
        
        public bool IsWidget { get; set; }

        public string ParentWindowURL { get; set; }

        public string TrackingID { get; set; }

        public CommonEnums.eRegistrationSources RegistrationSource { get; set; }

        public bool LicenseRequired { get; set; }

        public string LicenseKey { get; set; }
    }

    public class LoginLogoutLogToken
    {
        public int UserId { get; set; }
        public string SessionId { get; set; }
        public string HostName { get; set; }
    }

    public class ReportToken
    {
        public ReportToken()
        {
            CourseId = null;
            BundleId = null;
            BcId = null;
        }

        private int? _userId;
        public int? UserId {
            set { _userId = value; }
            get { return _userId < 0 ? null : _userId; }
        }
        public CommonEnums.eUserEvents EventType { get; set;}
        public string NetSessionId {get; set; }
        public string AdditionalMiscData { get; set;}
        public string TrackingID {get; set;}
        public int? CourseId {get; set;}
        public int? BundleId {get; set;}
        public long? BcId {get; set;}
        public string HostName { get; set; }
    }

}
