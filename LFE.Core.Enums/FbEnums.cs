using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class FbEnums
    {
        public enum ePostInterfaceStatus
        {
            Waiting = 1,
            Posted = 2,            
            Failed = 3
        }

        public enum eFbResultFields
        {
            id, 
            email,
            access_token,
            first_name,
            last_name,
            birthday,
            gender 
        }

        public enum eFbActions
        {
             [Description("Publish")] publish_course             = 1
            ,[Description("Purchase")] purchase_course           = 2
            //,[Description("Watch Video")] watch_chapter_video    = 3
            ,[Description("View Chapter Video")] view    = 3
            ,[Description("Write comment")] comment = 4
            ,[Description("Write Review")] review          = 5
            
        }
    }
}
