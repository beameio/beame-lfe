using System;
using System.Linq;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.Model;

namespace LFE.Application.Services.Api
{
    public class WixWebStoreServices : ServiceBase, IWixApiWebStoreServices
    {

        public void UpdateWebStoreLog(int storeId, out string error)
        {
            error = string.Empty;
            try
            {

                using (var context = new lfeAuthorEntities())
                {
                    var entity = context.WebStoresChangeLog.SingleOrDefault(x => x.StoreId == storeId);

                    if (entity == null)
                    {
                        context.WebStoresChangeLog.Add(new WebStoresChangeLog
                        {
                            StoreId = storeId
                            ,LastUpdateOn = DateTime.Now
                        });
                    }
                    else
                    {
                        entity.LastUpdateOn = DateTime.Now;
                     
                    }

                    context.SaveChanges();
                }
                
                
                //var entity = WebStoresChangeLogRepository.Get(x => x.StoreId == storeId);

                //if (entity == null)
                //{
                //    WebStoresChangeLogRepository.Add(new WebStoresChangeLog
                //    {
                //        StoreId       = storeId
                //        ,LastUpdateOn = DateTime.Now
                //    });
                //}
                //else
                //{
                //    entity.LastUpdateOn = DateTime.Now;

                //    WebStoresChangeLogRepository.Update(entity);
                //}

                //WebStoresChangeLogRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("Update WebStore log", ex, CommonEnums.LoggerObjectTypes.WebStore);
            }
        }

        public void UpdateCourseWebStoresLog(int courseId, out string error)
        {
            error = string.Empty;
            try
            {
                var sotres = WebStoreItemRepository.GetCourseWebStores(courseId).ToList();

                foreach (var storeId in sotres)
                {
                    var id = storeId;
                    var entity = WebStoresChangeLogRepository.Get(x => x.StoreId == id);

                    if (entity == null)
                    {
                        WebStoresChangeLogRepository.Add(new WebStoresChangeLog
                        {
                             StoreId = storeId
                            ,LastUpdateOn = DateTime.Now
                        });
                    }
                    else
                    {
                        entity.LastUpdateOn = DateTime.Now;

                        WebStoresChangeLogRepository.Update(entity);
                    }
                }

                WebStoresChangeLogRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("Update WebStore log", ex, CommonEnums.LoggerObjectTypes.WebStore);
            }
        }

        public DateTime? GetWebStoreLastUpdate(Guid wixInstanceId, out string error)
        {
            error = string.Empty;
            try
            {
                var storeEntity = WebStoreRepository.Get(x=>x.WixInstanceId!=null && x.WixInstanceId==wixInstanceId);

                if (storeEntity == null)
                {
                    error = "store entity not found";
                    return null;
                }

                var entity = WebStoresChangeLogRepository.Get(x => x.StoreId == storeEntity.StoreID);

                return entity != null ? entity.LastUpdateOn : null;
            }
            catch (Exception ex)
            {
                Logger.Error("Get WebStore LastUpdate", ex, CommonEnums.LoggerObjectTypes.WebStore);
                return null;
            }
        }

        public DateTime? GetWebStoreLastUpdate(string trackingId, out string error)
        {
            error = string.Empty;
            try
            {
                var storeEntity = WebStoreRepository.Get(x => x.TrackingID == trackingId);

                if (storeEntity == null)
                {
                    error = "store entity not found";
                    return null;
                }

                var entity = WebStoresChangeLogRepository.Get(x => x.StoreId == storeEntity.StoreID);

                return entity != null ? entity.LastUpdateOn : null;
            }
            catch (Exception ex)
            {
                Logger.Error("Get WebStore LastUpdate", ex, CommonEnums.LoggerObjectTypes.WebStore);
                return null;
            }
        }
    }
}