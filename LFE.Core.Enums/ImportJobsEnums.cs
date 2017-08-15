
using System;
using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class ImportJobsEnums
    {
        [Flags]
        public enum eJobStatuses
        {
             [Description("Created")] CREATED                 = 0
            ,[Description("In Progress")] IN_PROGRESS         = 1
            ,[Description("Success")] SUCCESS                 = 2
            ,[Description("Partial Success")] PARTIAL_SUCCESS = 4
            ,[Description("Failed")] FAILED                   = 8
            ,[Description("Cancel")] CANCEL                   = 16
            
        }

        public enum eFileInterfaceStatus
        {
            [Description("Init")] Init                                     = 1,
            [Description("Waiting")] Waiting                               = 2,
            [Description("In Progress")] InProgress                        = 3,
            [Description("Transferred")] Transferred                       = 4,
            [Description("Waiting for Sync")] WaitingForBrightcoveSynch    = 5,
            [Description("Waiting for Upload")] WaitingForBrightcoveUpload = 8,
            [Description("Failed")] Failed                                 = 6,
            [Description("Downloaded")] Downloaded                         = 7,
            [Description("Submitted")] Submitted                           = 9,
            [Description("Unauthorized")] Unauthorized                     = 10,
            [Description("Uploaded")] UploadedToS3                         = 11,
            [Description("Retranscode In Progress")] RetranscodeInProgress = 12,
            [Description("Retranscode Completed")] RetranscodeCompleted    = 13

        }

        public enum eAssetTypes
        {
            Unknown
            ,Video
            ,Document
            ,CourseThumb
            ,CoursePromoVideo
            ,CoursePromoVideoThumb
            ,Audio
            ,File
            ,EBook
            ,Presentation
            ,ExternalLink
            ,SourceCode
        }
    }
}
