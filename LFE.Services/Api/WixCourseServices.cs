using System;
using System.Linq;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Model;

namespace LFE.Application.Services.Api
{
     public class WixCourseServices : ServiceBase, IWixApiCourseServices
    {
        public void UpdateCourseChangeLog(int courseId, out string error)
        {
            error = string.Empty;
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var courseEntity = context.Courses.SingleOrDefault(c=>c.Id == courseId);

                    if (courseEntity == null)
                    {
                        error = "courseEntity not found";
                        return;
                    }

                    var entity = context.CRS_CourseChangeLog.SingleOrDefault(x => x.Uid == courseEntity.uid);

                    if (entity == null)
                    {
                        context.CRS_CourseChangeLog.Add(new CRS_CourseChangeLog
                        {
                            Uid = courseEntity.uid
                            ,LastUpdateOn = DateTime.Now
                        });
                    }
                    else
                    {
                        entity.LastUpdateOn = DateTime.Now;
                        
                    }

                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("API::Update Course log", ex, CommonEnums.LoggerObjectTypes.Course);
            }
        }

        public DateTime? GetCourseLastUpdate(Guid uid, out string error)
        {
            error = string.Empty;
            try
            {
                using (var context = new lfeAuthorEntities())
                {
                    var entity = context.CRS_CourseChangeLog.SingleOrDefault(x => x.Uid == uid);

                    return entity?.LastUpdateOn;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("API::Get Course LastUpdate", ex, CommonEnums.LoggerObjectTypes.Course);
                return null;
            }
        }
    }
}