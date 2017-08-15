using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LFE.Application.Services.Interfaces
{
    public interface IWixApiWebStoreServices : IDisposable
    {
        void UpdateWebStoreLog(int storeId, out string error);
        void UpdateCourseWebStoresLog(int courseId, out string error);
        DateTime? GetWebStoreLastUpdate(Guid wixInstanceId, out string error);
        DateTime? GetWebStoreLastUpdate(string trackingId, out string error);
    }

    public interface IWixApiCourseServices : IDisposable
    {
        void UpdateCourseChangeLog(int courseId, out string error);
        DateTime? GetCourseLastUpdate(Guid uid, out string error);
    }
}
