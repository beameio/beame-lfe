using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;
using System.ComponentModel.DataAnnotations;

namespace LFE.Portal.Areas.UserPortal.Models
{

    public enum ePurchaseReportType
    {
        OneTime
        ,Subscription
    }

    public class OtherLearnerPageToken
    {
        public LearnerListItemDTO learner { get; set; }

        public List<CourseListDTO> courses { get; set; }
    }

    public class FeedPageToken
    {
        public eFeedFilterKinds kind { get; set; }
        public long Id { get; set; }
        public string title { get; set; }
        public string TagName { get; set; }
    }

    public class UserProfilePageToken : BaseModelState
    {
        public UserProfilePageToken()
        {
            ProfileCart    = new UserProfileCartToken();
            LearnerCourses = new List<CourseListDTO>();
            AuthorCourses  = new List<CourseListDTO>();
            PageSize       = 1;
        }

        public UserProfileCartToken ProfileCart { get; set; }
        public List<CourseListDTO> LearnerCourses { get; set; }
        public List<CourseListDTO> AuthorCourses { get; set; }
        public int PageSize { get; set; }
    }
    public class AuthorProfilePageToken : BaseModelState
    {
        public AuthorProfilePageToken()
        {
            ProfileCart    = new UserProfileCartToken();
            LearnerCourses = new List<WidgetItemListDTO>();
            AuthorItems    = new List<WidgetItemListDTO>();
            PageSize       = 1;
        }

        public UserProfileCartToken ProfileCart { get; set; }
        public List<WidgetItemListDTO> LearnerCourses { get; set; }
        public List<WidgetItemListDTO> AuthorItems { get; set; }
        public int PageSize { get; set; }

        public bool ShowPurchased { get; set; }

        public bool ShowItemsListTitle { get; set; }

        public string ItemsListTitle { get; set; }
    }
    public class UserProfileCartToken
    {
        public UserProfileCartToken()
        {
            Profile    = new UserProfileDTO();
            TotalLearn = 0;
            TotalTeach = 0;
        }
        public UserProfileDTO Profile { get; set; }
        public int TotalTeach { get; set; }
        public int TotalLearn { get; set; }
    }

    public class HashtagFeedToken
    {
        public string Title { get; set; }
        public Guid? Uid { get; set; }
        public string Hashtag { get; set; }
        public long? HashtagId { get; set; }
        public int UserCoursesPageSize { get; set; }
        public bool ShowTitle { get; set; }
        public bool ShowCloseBtn { get; set; }
        public IEnumerable<MessageViewDTO> Messages { get; set; }
    }

    public class UserCoursesPageToken
    {
        public string ListId { get; set; }
        public int PageSize { get; set; }
        public string Title { get; set; }
        public List<CourseListDTO> courses { get; set; }
    }

    public class CourseViewerPageToken : BaseModelState
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }       
        public LearnerCourseViewerDTO  CourseViewerDTO { get; set; }
        public ItemAccessStateToken ItemState { get; set; }
        public string TrackingID { get; set; }
    }

    public class BundleViewerPageToken : BaseModelState
    {
        public BaseBundleDTO Bundle { get; set; }
        public UserProfileDTO Author { get; set; }
        public List<BundleCourseListDTO> BundleCourses { get; set; }
        public ItemAccessStateToken ItemState { get; set; }

        public string TrackingID { get; set; }
    }

}