using System;
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class CourseEnums        
    {
        [Flags]
        public enum CourseStatus
        {
             [Description("Draft")] Draft           = 1
            ,[Description("Published")] Published   = 2
           // ,[Description("Private mode")] Private  = 4
        }

        public enum eCourseContentKind
        {
            Chapter = 1
            ,Quiz = 2
        }
        public enum eContentTreeViewItemType
        {
            chapter,
            video,
            asset,
            quiz
        }
        public enum eChapterLinkKind
        {
            [Description("Chapter material")] Document = 1
           ,[Description("External link")] Link        = 2
        }

        public enum CouponType
        {
             [Description("Percentage Discount")] PERCENT        = 1
            ,[Description("Fixed USD Discount")] FIXED           = 2
            ,[Description("Free Course")] FREE                   = 3
            ,[Description("Subscription Discount")] SUBSCRIPTION = 4
        }

        public enum eCouponKinds
        {
             [Description("All of my content")] Author = 1
            ,[Description("Course")] Course = 2
            ,[Description("Bundle")] Bundle = 3
        }

        public enum eAssetTypes
        {
             [Description("Video")] VIDEO      = 1
            ,[Description("Link")] LINK        = 2
            ,[Description("File")] FILE        = 3            
        }

        #region wizard
        public enum eWizardSteps
        {
            //Id's referenced to CRS_WizardStepsLOV
            //Introduction is invisible step, using only for initializing of wizard object
             Introduction        = 0
            ,CourseContentGroup  = -1
            ,CourseName          = 1
            ,VideoManager        = 2
           // ,ChapterManage       = 3
            ,ChapterContents     = 3
            ,MarketingInfoGroup  = -11
            ,CourseVisuals       = 4
            ,CourseMeta          = 5
            ,AboutAuthor         = 6
            ,CorseSettingsGroup  = -111
            ,CoursePrice         = 7
            ,Publish             = 8
            ,Finish              = -2
        }

        public enum ePublichChecklist
        {
            [Description("Name and Description")] Course
            ,[Description("Any chapter")] Chapters
            ,[Description("Any contents")] AnyContents
            ,[Description("Course thumb")] CourseThumb
            ,[Description("Course promo video")] CoursePromoVideo
            ,[Description("Price")] Price
        }

        public enum eWizardSetpModes
        {
             Unknown = -1            
            ,Current = 1
            ,Allowed = 2
            ,Disable = 3
            ,Dummy   = 4
        }
        #endregion
    }
}
