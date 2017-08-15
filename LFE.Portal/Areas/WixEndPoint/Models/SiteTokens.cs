using System;
using System.Collections.Generic;
using LFE.DataTokens;

namespace LFE.Portal.Areas.WixEndPoint.Models
{
    public class IndexViewToken
    {
        public WixInstanceDTO Instance { get; set; }

        public int? userId { get; set; }

        public int? storeId { get; set; }

        public string compId { get; set; }

        public string origCompId { get; set; }
    }

    public class SettingsViewToken
    {
        public SettingsViewToken()
        {
            UserCoursesList = new List<CourseListDTO>();
        }

        public WixInstanceDTO Instance { get; set; }

        public int? UserId { get; set; }

        public int? StoreId { get; set; }

        public string StoreName { get; set; }

        public string FontColor { get; set; }

        public string BackgroundColor { get; set; }

        public string TabsFontColor { get; set; }

        public bool IsTransparent {get; set;}

        public bool IsShowBorder {get; set;}

        public bool IsShowTitleBar {get; set; }

        public string UniqueId { get; set; }

        public List<CourseListDTO> UserCoursesList { get; set; }

        public string TrackingID { get; set; }
        //public List<int> StoreCoursesIds { get; set; }
    }


    public class WixExternalLoginToken
    {
        public Guid? wixUid { get; set; }
        public string returnUrl { get; set; }

        public string trackingId { get; set; }

    }
}