using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LFE.DataTokens
{
    public enum eWixPermisssions
    {
        Unknown,
        OWNER
    }

    public class WixInstanceDTO
    {
        public Guid instanceId { get; set; }
        public Guid? uid { get; set; }
        public DateTime? signDate { get; set; }
        public string ipAndPort { get; set; }
        public Guid? vendorProductId { get; set; }
        public bool demoMode { get; set; }
        public eWixPermisssions? permissions { get; set; }
        public string instanceToken { get; set; }
    }

    public class WixLoginDTO : LoginDTO
    {
        public Guid instanceId { get; set; }
        public Guid? uid { get; set; }
        public string instanceToken { get; set; }
        public bool isSuccess { get; set; }
        public string compId { get; set; }
        public string origCompID { get; set; }
    }

    public class WixUserRegisterDTO : RegisterDTO
    {
        public WixUserRegisterDTO() { }

        public WixUserRegisterDTO(Guid? uid, Guid instanceId, string instanceToken, string compIdToken, string origCompIDToken)
        {
            this.instanceToken = instanceToken;
            InstanceId = instanceId;           
            compId = compIdToken;
            origCompId = origCompIDToken;
        }

        //in registerDTO define Guid? wixUid
        // public new Guid WixUid { get; set; }
        public Guid InstanceId { get; set; }
        public string instanceToken { get; set; }
        public string compId { get; set; }
        public string origCompId { get; set; }
    }

    public class WixRegisterStoreDTO
    {
        public WixRegisterStoreDTO()
        {
            StoreId = -1;
        }

        public int UserId { get; set; }

        public Guid InstanceId { get; set; }

        public int StoreId { get; set; }

        [Required]
        [DisplayName("Store Name")]
        public string StoreName { get; set; }
    }


    public class WixSettingsToken
    {
        public string FontColor { get; set; }

        public string BackgroundColor { get; set; }

        public string TabsFontColor { get; set; }

        public bool IsTransparent { get; set; }

        public bool IsShowBorder { get; set; }

        public bool IsShowTitleBar { get; set; }

        public string StoreName { get; set; }

        public string InstanceId { get; set; }

        public int? StoreId { get; set; }

        public string UniqueId { get; set; }
        public string WixSiteUrl { get; set; }
        public string TrackingID { get; set; }        
    }


    public class WixSettingsJsonToken
    {
        public string cpFontColor { get; set; }
        public string cpBackgroundColor { get; set; }
        public string cpTabsFontColor { get; set; }
        public bool cbIsTransparent { get; set; }
        public bool cbIsShowBorder { get; set; }
        public bool cbIsShowTitleBar { get; set; }
        public string txtStoreName { get; set; }
        public int? StoreId { get; set; }
        public string InstanceId { get; set; }
        public string UniqueId { get; set; }
       // public string TrackingId { get; set; }
        public string  WixSiteUrl { get; set; }
    }


    public class WixStoreUrlToken
    {
        public int StoreId { get; set; }     
        public string WixSiteUrl { get; set; }
    }

    public class FacebookSettingsJsonToken
    {
        public string cpFontColor { get; set; }
        public string cpBackgroundColor { get; set; }
        public string cpTabsFontColor { get; set; }
        public bool cbIsTransparent { get; set; }
        public bool cbIsShowBorder { get; set; }
        public bool cbIsShowTitleBar { get; set; }
        public string txtStoreName { get; set; }
        public int? StoreId { get; set; }
        public string TrackingId { get; set; }
        public string UniqueId { get; set; }      
    }    
}
