using LFE.DataTokens;
using LFE.Portal.Helpers;

namespace LFE.Portal.Areas.UserPortal.Helpers
{
    public static class GeneralExtensions
    {
        
        public static CourseListDTO SetCoursePageUrl(this CourseListDTO dto,string trackingId,string mode = null)
        {
            var token = dto;

            token.CoursePageUrl = Extensions.GenerateCoursePageUrl(null,token.AuthorFullName,token.Name,trackingId,mode);
            return token;
        }
    }
}