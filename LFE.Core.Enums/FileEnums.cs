
namespace LFE.Core.Enums
{
    public class FileEnums
    {
        public enum eFileOwners
        {
            User,
            Author,
            StaticContent,
            Course,
            Category,
            Student,
            SliderItem,
            Quiz,
            Cert
        }

       

        //exported from Brightcove API
        public enum ImageType
        {
            None = 0,
            Thumbnail = 1,
            VideoStill = 2,
            LogoOverlay = 3,
        }
    }
}
