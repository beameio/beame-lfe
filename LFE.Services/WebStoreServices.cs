using LFE.Application.Services.Base;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.EntityMapper;
using LFE.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Application.Services
{
    public class WebStoreServices : ServiceBase, IWebStoreServices, IWebStoreWixServices, IWebStoreFacebookServices, IPluginStoreServices
    {

        #region common
        private void UpdateWebStoreLog(int storeId, out string error)
        {
            error = string.Empty;
            try
            {
                var entity = WebStoresChangeLogRepository.Get(x => x.StoreId == storeId);

                if (entity == null)
                {
                    WebStoresChangeLogRepository.Add(new WebStoresChangeLog
                    {
                        StoreId = storeId
                        ,
                        LastUpdateOn = DateTime.Now
                    });
                }
                else
                {
                    entity.LastUpdateOn = DateTime.Now;

                    WebStoresChangeLogRepository.Update(entity);
                }

                WebStoresChangeLogRepository.UnitOfWork.CommitAndRefreshChanges();
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("Update WebStore log", ex, CommonEnums.LoggerObjectTypes.WebStore);
            }
        }


        private string GenerateStoreName(UserDTO currentUser, out string error)
        {
            try
            {
                error = "";

                var initialName = currentUser.FullName + " Store";

                var webstores = WebStoreRepository.GetMany(x => x.OwnerUserID == currentUser.UserId && x.StoreName.ToLower().Contains(initialName.ToLower())).ToList();

                if (webstores.Any())
                {
                    for (var i = 2; i <= 1000; i++)
                    {
                        var stores = webstores.Where(x => x.StoreName.ToLower() == (initialName.ToLower() + " " + i.ToString())).ToList();
                        if (!stores.Any())
                        {
                            return initialName + " " + i;
                        }
                    }
                    return initialName;
                }
                return initialName;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return null;
            }
        }

        #endregion

        #region IWebStoreServices implementation

        #region private helpers
        private bool IsStoreNameValid(WebStoreEditDTO dto, int ownerId, out string error)
        {
            if (!dto.StoreName.IsObjectNameValid(out error)) return false;

            try
            {
                if (dto.StoreId < 0)
                {
                    return !WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName == dto.StoreName);
                }

                return !WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName == dto.StoreName && x.StoreID != dto.StoreId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool IsStoreNameValid(BaseWebStoreDTO dto, int ownerId, out string error)
        {
            if (!dto.Name.IsObjectNameValid(out error)) return false;

            try
            {
                if (dto.StoreId < 0)
                {
                    return !WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName == dto.Name);
                }

                return !WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName == dto.Name && x.StoreID != dto.StoreId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }
        
        private bool IsTrackingIdValid(int storeId, string trackingId, out string error)
        {
            error = string.Empty;
            try
            {
                if (storeId < 0)
                {
                    return !WebStoreRepository.IsAny(x => x.TrackingID == trackingId);
                }

                return !WebStoreRepository.IsAny(x => x.TrackingID == trackingId && x.StoreID != storeId);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                return false;
            }
        }

        private bool IsStoreCategoryNameValid(WebStoreCategoryEditDTO dto)
        {
            try
            {
                return dto.WebStoreCategoryId >= 0 ? !WebStoreCategoryRepository.IsAny(x => x.WebStoreID == dto.WebStoreId && x.CategoryName.ToLower() == dto.CategoryName.ToLower() && x.WebStoreCategoryID != dto.WebStoreCategoryId)
                                                    : !WebStoreCategoryRepository.IsAny(x => x.WebStoreID == dto.WebStoreId && x.CategoryName.ToLower() == dto.CategoryName.ToLower());

            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsCategoryItemNameValid(WebStoreItemEditDTO dto)
        {
            try
            {

                 return dto.WebStoreItemId >= 0 ? !WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == dto.WebStoreCategoryId && x.ItemName.ToLower() == dto.ItemName.ToLower() && x.WebstoreItemId != dto.WebStoreItemId)
                                                   : !WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == dto.WebStoreCategoryId && x.ItemName.ToLower() == dto.ItemName.ToLower());

            }
            catch (Exception)
            {
                return false;
            }
        }

        private int FindWebstoreItemId(WebStoreItemEditDTO dto,out string error)
        {
            error = string.Empty;
            try
            {
                List<WebStoreItems> l;
                switch (dto.ItemType)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        l = WebStoreItemRepository.GetMany(x => x.WebStoreCategoryID == dto.WebStoreCategoryId && x.CourseId != null && x.CourseId == dto.ItemId).ToList();
                        break;
                        
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        l = WebStoreItemRepository.GetMany(x => x.WebStoreCategoryID == dto.WebStoreCategoryId && x.BundleId != null && x.BundleId == dto.ItemId).ToList();
                        break;                      
                    default:
                        error = "Unknown type";
                        return -1;
                }

                return l.Count().Equals(0) ? -1 : l[0].WebstoreItemId;

            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                return -1;
            }
        }

        private bool IsCategoryItemNameValid(int webCategoryId, string name)
        {
            try
            {
                return !WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == webCategoryId && x.ItemName.ToLower() == name.ToLower());

            }
            catch (Exception)
            {
                return false;
            }
        }

        private void UpdateCategoryIndieces(int storeId)
        {
            var storeCategories = WebStoreCategoryRepository.GetMany(x => x.WebStoreID == storeId).OrderBy(x => x.Ordinal).ToList();

            for (var i = 0; i < storeCategories.Count; i++)
            {
                storeCategories[i].UpdateCategoryEntityOrderIndex(i);
            }

            WebStoreCategoryRepository.UnitOfWork.CommitAndRefreshChanges();
        }

        private void UpdateCourseIndieces(int webCategoryId)
        {
            var storeCategories = WebStoreItemRepository.GetMany(x => x.WebStoreCategoryID == webCategoryId).OrderBy(x => x.Ordinal).ToList();

            for (var i = 0; i < storeCategories.Count; i++)
            {
                storeCategories[i].UpdateCourseEntityOrderIndex(i);
            }

            WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges();
        }

        private bool AddItemList2Category(int webCategoryId, IReadOnlyCollection<BaseWebstoreItemListDTO> items, out string error)
        {
            error = string.Empty;

            if (items.Count.Equals(0)) return true;

            if (webCategoryId < 0)
            {
                error = "Store Category missing";
                return false;
            }

            try
            {
                var index = GetCategoryItems(webCategoryId).Count();

                foreach (var item in items.Where(item => item.storeItemId == null && item.attach).Where(item => IsCategoryItemNameValid(webCategoryId, item.name)))
                {
                    switch (item.type)
                    {
                        case BillingEnums.ePurchaseItemTypes.COURSE:
                            var entity = CourseRepository.GetById(item.id);

                            if (entity == null) continue;

                            var token = new WebStoreItemEditDTO
                            {
                                ItemId = entity.Id
                                ,
                                ItemName = entity.CourseName
                                ,
                                WebStoreCategoryId = webCategoryId
                                ,
                                Description = entity.Description
                                ,
                                IsActive = true
                                ,
                                OrderIndex = index
                                ,
                                ItemType = item.type
                            };

                            WebStoreItemRepository.Add(token.WebStoreItemEditDto2WebStoreItemEntity());
                            break;
                        case BillingEnums.ePurchaseItemTypes.BUNDLE:
                            var bentity = BundleRepository.GetById(item.id);

                            if (bentity == null) continue;

                            token = new WebStoreItemEditDTO
                            {
                                ItemId = bentity.BundleId
                                ,
                                ItemName = bentity.BundleName
                                ,
                                WebStoreCategoryId = webCategoryId
                                ,
                                Description = bentity.BundleDescription
                                ,
                                IsActive = true
                                ,
                                OrderIndex = index
                                ,
                                ItemType = item.type
                            };

                            WebStoreItemRepository.Add(token.WebStoreItemEditDto2WebStoreItemEntity());
                            break;
                    }

                    index++;
                }

                return WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save courses list", null, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }
        private BaseEntityDTO FindStoreByOwnerAndTrackingId(int ownerId, string trackingId,out string error)
        {
            error = string.Empty;
            try
            {
                var trackId = trackingId.OptimizedUrl().ToLower();


                var store = WebStoreRepository.Get(x=>x.TrackingID == trackId);

                if (store == null) return new BaseEntityDTO("New WebStore", Guid.NewGuid());

                if (store.OwnerUserID != ownerId)
                {
                    error = "Store belong to user " + store.OwnerUserID;
                    return null;
                }

                return new BaseEntityDTO
                {
                    id    = store.StoreID
                    ,name = store.StoreName
                    ,Uid  = store.uid
                };
            }
            catch (Exception ex)
            {
                Logger.Error("find store by uid", ownerId, ex, CommonEnums.LoggerObjectTypes.WebStore);

                return null;
            }
        }
        #endregion

        #region interface implementation
        #region helpers
        public BaseEntityDTO FindStoreByUid(int ownerId, Guid uid)
        {
            try
            {
                var entity = WebStoreRepository.Get(x => x.OwnerUserID == ownerId && x.uid == uid);

                return entity == null ? new BaseEntityDTO("New WebStore", uid) : entity.Entity2BaseEntityDto();
            }
            catch (Exception ex)
            {
                Logger.Error("find store by uid", ownerId, ex, CommonEnums.LoggerObjectTypes.WebStore);

                return null;
            }
        }

        public BaseEntityDTO FindStoreByTrackingId(int ownerId, string trackingId)
        {
            try
            {
                var trackId = trackingId.ToLower();
                var entity = WebStoreRepository.Get(x => x.OwnerUserID == ownerId && x.TrackingID.ToLower() == trackId);

                return entity == null ? new BaseEntityDTO("New WebStore", Guid.NewGuid()) : entity.Entity2BaseEntityDto();
            }
            catch (Exception ex)
            {
                Logger.Error("find store by uid", ownerId, ex, CommonEnums.LoggerObjectTypes.WebStore);

                return null;
            }
        }

        //public WidgetWebStoreDTO GetWebStore(string trackingID)
        //{
        //    try
        //    {
        //        var entity = WebStoreRepository.Get(x => x.TrackingID == trackingID);

        //        return entity != null ? entity.Entity2WidgetStoreDto() : null;

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Widget::GetWebStore", ex, CommonEnums.LoggerObjectTypes.Widget);

        //        return null;
        //    }
        //}
        public bool ValidateOwnerStoreByUid(int ownerId, Guid uid)
        {
            try
            {
                var entity = WebStoreRepository.Get(x => x.uid == uid);

                return entity == null || WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.uid == uid);
            }
            catch (Exception ex)
            {
                Logger.Error("validate owner store by uid", ownerId, ex, CommonEnums.LoggerObjectTypes.WebStore);

                return false;
            }
        }
        public bool ValidateOwnerStoreByTrackingId(int ownerId, string trackingId)
        {
            try
            {
                var trackId = trackingId.ToLower();
                var entity = WebStoreRepository.Get(x => x.TrackingID.ToLower() == trackId);

                return entity == null || WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.TrackingID.ToLower() == trackId);
            }
            catch (Exception ex)
            {
                Logger.Error("validate owner store by uid", ownerId, ex, CommonEnums.LoggerObjectTypes.WebStore);

                return false;
            }
        }
        public bool ValidateTrackingByUid(string trackId, Guid uid)
        {
            try
            {
                var entity = WebStoreRepository.Get(x => x.TrackingID.ToLower() == trackId.ToLower());

                return entity == null || (WebStoreRepository.IsAny(x => x.TrackingID == trackId && x.uid == uid) && !WebStoreRepository.IsAny(x => x.TrackingID.ToLower() == trackId.ToLower() && x.uid != uid));
            }
            catch (Exception ex)
            {
                Logger.Error("validate store trackingId", null, ex, CommonEnums.LoggerObjectTypes.WebStore);

                return false;
            }
        }

        public bool ValidateOwnerStoreName(string name, int ownerId, int storeId)
        {
            try
            {
                if (storeId < 0)
                {
                    var exists = WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName.ToLower() == name.ToLower());

                    return !exists;
                }

                return !WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName.ToLower() == name.ToLower() && x.StoreID != storeId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ValidateStoreCategoryName(string name, int categoryId, int storeId)
        {
            try
            {
                if (categoryId < 0)
                {
                    var exists = WebStoreCategoryRepository.IsAny(x => x.WebStoreID == storeId && x.CategoryName.ToLower() == name.ToLower());

                    return !exists;
                }

                return !WebStoreCategoryRepository.IsAny(x => x.WebStoreID == storeId && x.CategoryName.ToLower() == name.ToLower() && x.WebStoreCategoryID != categoryId);
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region store
        public List<WebStoreGridDTO> GetOwnerStores(int id)
        {
            return _GetOwnerStores(id).Where(x => x.Status != WebStoreEnums.StoreStatus.Deleted).ToList();
        }

        public bool DeleteStore(int storeId, out string error)
        {
            try
            {
                var entity = WebStoreRepository.GetById(storeId);

                if (entity == null)
                {
                    error = "store entity not found";
                    return false;
                }

                var isDeletedFromDBAllowed = !OrderRepository.IsAny(x => x.WebStoreId == storeId);

                if (isDeletedFromDBAllowed)
                {
                    WebStoreRepository.Delete(entity);
                }
                else
                {
                    entity.StatusId = (short)WebStoreEnums.StoreStatus.Deleted;
                    entity.UpdateOn = DateTime.Now;
                    entity.UpdatedBy = CurrentUserId;
                }

                return WebStoreRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete store", storeId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public WebStoreEditDTO GetStoreEditDTO(int id)
        {
            return WebStoreRepository.GetById(id).Entity2StoreEditDto();
        }

        public bool SaveStore(ref WebStoreEditDTO dto, int ownerId, out string error)
        {
            if (ownerId < 0)
            {
                error = "ownerId missing";
                return false;
            }

            if (!IsStoreNameValid(dto, ownerId, out error))
            {
                error = String.IsNullOrEmpty(error) ? "Store Name already exists" : error;
                return false;
            }

            if (!IsTrackingIdValid(dto.StoreId, dto.TrackingID, out error))
            {
                error = String.IsNullOrEmpty(error) ? "TrackingID already exists" : error;
                return false;
            }

            try
            {
                WebStores entity;
                if (dto.StoreId < 0) //new store
                {
                    entity = dto.EditDto2StoreEntity(ownerId);
                    WebStoreRepository.Add(entity);
                }
                else
                {
                    entity = WebStoreRepository.GetById(dto.StoreId);

                    if (entity == null)
                    {
                        error = "Store entity not found";
                        return false;
                    }

                    entity.UpdateStoreEntity(dto);
                }

                WebStoreRepository.UnitOfWork.CommitAndRefreshChanges();

                dto.StoreId = entity.StoreID;

                //add role
                AddRole2User(ownerId, CommonEnums.UserRoles.Author);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save store dto", dto.StoreId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }        

        //private bool IsStoreNameValid(string storeName, int storeId, int ownerId)
        //{
        //    try
        //    {
        //        if (storeId < 0)
        //        {
        //            return !WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName == storeName);
        //        }

        //        return !WebStoreRepository.IsAny(x => x.OwnerUserID == ownerId && x.StoreName ==storeName && x.StoreID != storeId);
        //    }
        //    catch (Exception ex)
        //    {

        //        return false;
        //    }
        //}

        public bool AddOwnerCourses2Store(int storeId, out string error)
        {
            error = string.Empty;

            try
            {
                var courseIds = WebStoreRepository.GetAuthorNonIncludedCourses(storeId).ToList();

                foreach (var courseId in courseIds)
                {
                    AddCourse2Store(storeId, courseId, out error);
                    UpdateWebStoreLog(storeId, out error);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("add owner courses to store");
                return false;
            }
        }

        public bool AddCourse2Store(int storeId, int courseId, out string error)
        {
            error = string.Empty;

            try
            {
                var courseEntity = CourseRepository.GetById(courseId);

                if (courseEntity == null)
                {
                    error = "course entity not found";
                    return false;
                }

                var webCategoryIds = new List<int>();

                var index = WebStoreCategoryRepository.Count(x => x.WebStoreID == storeId);

                //create categories
                //1. get course categories
                var courseCategories = CourseRepository.GetCourseCategories(courseId).OrderBy(x => x.CategoryName).ToList();

                foreach (var category in courseCategories)
                {
                    //2. check if web category exists
                    var cat = category;
                    var webCategoryEntity = WebStoreCategoryRepository.Get(x => x.WebStoreID == storeId && x.CategoryName.ToLower() == cat.CategoryName.ToLower());

                    if (webCategoryEntity != null)
                    {
                        //add webCategoryId to list
                        if (webCategoryIds.All(x => x != webCategoryEntity.WebStoreCategoryID))
                        {
                            webCategoryIds.Add(webCategoryEntity.WebStoreCategoryID);
                        }
                    }
                    else
                    {
                        //create webstore category
                        var dto = category.CategoryEntity2WebStoreCategoryEditDto(storeId, index);

                        if (!SaveCategory(ref dto, out error)) continue;

                        if (webCategoryIds.Any(x => x == dto.WebStoreCategoryId)) continue;

                        index++;
                        webCategoryIds.Add(dto.WebStoreCategoryId);
                    }
                }

                //save categories courses
                foreach (var webCategoryId in webCategoryIds)
                {
                    var courseDto = courseEntity.CourseEntity2WebStoreCourseEditDto(webCategoryId);
                    SaveCategoryItem(ref courseDto, out error);
                }

                WebStoreCategoryRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("add course to store");
                return false;
            }
        }
        #endregion

        #region categories
        public List<BaseListDTO> GetStoreCategories(int storeId)
        {
            try
            {
                return WebStoreCategoryRepository.GetMany(x => x.WebStoreID == storeId).Select(x => x.CategoryEntity2BaseListDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get store categories", storeId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<BaseListDTO>();
            }
        }

        public List<WebStoreCategoryEditDTO> GetStoreEditCategories(int storeId)
        {
            try
            {
                return WebStoreCategoryRepository.GetMany(x => x.WebStoreID == storeId).Select(x => x.Entity2CategoryEditDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get store categories", storeId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<WebStoreCategoryEditDTO>();
            }
        }

        public WebStoreCategoryEditDTO GetCategoryEditDTO(int categoryId, int storeId)
        {
            if (categoryId < 0)
            {
                return new WebStoreCategoryEditDTO(storeId)
                {
                    OrderIndex = GetStoreCategories(storeId).Count()
                };
            }

            var entity = WebStoreCategoryRepository.GetById(categoryId);

            return entity != null ? entity.Entity2CategoryEditDto() : null;
        }

        public bool SaveCategory(ref WebStoreCategoryEditDTO dto, out string error)
        {
            error = string.Empty;

            if (dto.WebStoreId < 0)
            {
                error = "storeId missing";
                return false;
            }

            if (!IsStoreCategoryNameValid(dto))
            {
                error = "Category Name already exists";
                return false;
            }

            try
            {
                if (dto.OrderIndex == null)
                {
                    dto.OrderIndex = GetStoreCategories(dto.WebStoreId).Count();
                }

                if (dto.WebStoreCategoryId < 0) //new category
                {
                    var categoryEntity = dto.EditDto2CategoryEntity();
                    WebStoreCategoryRepository.Add(categoryEntity);
                    WebStoreCategoryRepository.UnitOfWork.CommitAndRefreshChanges();

                    dto.WebStoreCategoryId = categoryEntity.WebStoreCategoryID;
                }
                else
                {
                    var entity = WebStoreCategoryRepository.GetById(dto.WebStoreCategoryId);

                    if (entity == null)
                    {
                        error = "Category entity not found";
                        return false;
                    }

                    entity.UpdateCategoryEntity(dto);

                    WebStoreCategoryRepository.UnitOfWork.CommitAndRefreshChanges();

                    dto.WebStoreCategoryId = entity.WebStoreCategoryID;
                }


                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save category dto", dto.WebStoreId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public bool DeleteCategory(int categoryId, out string error)
        {
            error = string.Empty;

            try
            {
                var entity = WebStoreCategoryRepository.GetById(categoryId);

                if (entity == null)
                {
                    error = "Category entity not found";
                    return false;
                }

                var storeId = entity.WebStoreID;

                WebStoreCategoryRepository.Delete(entity);

                WebStoreCategoryRepository.UnitOfWork.CommitAndRefreshChanges();

                UpdateCategoryIndieces(storeId);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete category", categoryId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public bool SaveCategoriesOrder(int[] categoryIds, out string error)
        {
            error = string.Empty;

            try
            {
                var i = 0;

                foreach (var entity in categoryIds.Select(categoryId => WebStoreCategoryRepository.GetById(categoryId)).Where(entity => entity != null))
                {
                    entity.UpdateCategoryEntityOrderIndex(i);

                    i++;
                }

                WebStoreCategoryRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save category order", null, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }
        #endregion

        #region courses
        public List<WebStoreCourseListDTO> GetCategoryItems(int categoryId)
        {
            try
            {
                return WebStoreItemRepository.GetMany(x => x.WebStoreCategoryID == categoryId).Select(x => x.CourseEntity2BaseListDto()).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get webstore category items", categoryId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<WebStoreCourseListDTO>();
            }
        }

        public List<WebStoreItemListDTO> GetCategoryItemsList(int categoryId)
        {
            try
            {
                return WebStoreItemViewRepository.GetMany(x => x.WebStoreCategoryID == categoryId).Select(x => x.ItemEntity2WebStoreItemListDto(GetItemDefaultRegularPrice(x.ItemId,Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(x.ItemTypeId)),GetItemDefaultMonthlySubscriptionPrice(x.ItemId,Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(x.ItemTypeId)))).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get webstore category items", categoryId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<WebStoreItemListDTO>();
            }
        }

        public WebStoreItemEditDTO GetCategoryItemEditDto(int webItemId, int webCategoryId)
        {
            if (webItemId < 0)
            {
                return new WebStoreItemEditDTO(webCategoryId)
                {
                    OrderIndex = WebStoreItemRepository.Count(x => x.WebStoreCategoryID == webCategoryId)
                };
            }

            var entity = WebStoreItemRepository.GetById(webItemId);

            return entity != null ? entity.Entity2CourseEditDto() : null;
        }

        public bool SaveCategoryItem(ref WebStoreItemEditDTO dto, out string error)
        {
            error = string.Empty;

            if (dto.WebStoreCategoryId < 0)
            {
                error = "Category missing";
                return false;
            }

            var webStoreItemId = FindWebstoreItemId(dto, out error);
            var isAttached = webStoreItemId >= 0;

            if (!String.IsNullOrEmpty(error))return false;

            if (!isAttached && !IsCategoryItemNameValid(dto))
            {
                error = "Item Name already exists";
                return false;
            }

            try
            {
                if (dto.OrderIndex == null || dto.OrderIndex < 0)
                {
                    dto.OrderIndex = GetCategoryItems(dto.WebStoreCategoryId).Count();
                }

                if (dto.WebStoreItemId < 0 && webStoreItemId < 0) //new course
                {
                    var itemEntity = dto.WebStoreItemEditDto2WebStoreItemEntity();
                    WebStoreItemRepository.Add(itemEntity);
                    WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges();
                    dto.WebStoreItemId = itemEntity.WebstoreItemId;
                }
                else
                {
                    var id = webStoreItemId < 0 ? dto.WebStoreItemId : webStoreItemId;

                    var entity = WebStoreItemRepository.GetById(id);

                    if (entity == null)
                    {
                        error = "Item entity not found";
                        return false;
                    }

                    entity.UpdateCourseEntity(dto);
                    WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges();
                    dto.WebStoreItemId = entity.WebstoreItemId;
                }

                return dto.WebStoreItemId >= 0;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save item dto", dto.WebStoreItemId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public bool DeleteCategoryItem(int itemId, out string error)
        {
            error = string.Empty;

            try
            {
                var entity = WebStoreItemRepository.GetById(itemId);

                if (entity == null)
                {
                    error = "Course entity not found";
                    return false;
                }

                var webCategoryId = entity.WebStoreCategoryID;

                WebStoreItemRepository.Delete(entity);

                WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges();

                UpdateCourseIndieces(webCategoryId);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete course", itemId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public bool DeleteCategoryItem(int webStoreCategoryId, int itemId, BillingEnums.ePurchaseItemTypes type, out string error)
        {
            error = string.Empty;

            try
            {
                WebStoreItems entity = null;
                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        entity = WebStoreItemRepository.Get(x => x.WebStoreCategoryID == webStoreCategoryId && x.CourseId == itemId);
                        break;
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        entity = WebStoreItemRepository.Get(x => x.WebStoreCategoryID == webStoreCategoryId && x.BundleId == itemId);
                        break;
                }

                if (entity == null)
                {
                    error = "item entity not found";
                    return false;
                }

                WebStoreItemRepository.Delete(entity);

                WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges();

                UpdateCourseIndieces(webStoreCategoryId);

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete store category item", itemId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public bool SaveCategoryItem(int webStoreCategoryId, int itemId, BillingEnums.ePurchaseItemTypes type, out string error)
        {
            error = string.Empty;

            try
            {
                var index = GetCategoryItems(webStoreCategoryId).Count();

                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        var entity = CourseRepository.GetById(itemId);

                        if (entity == null)
                        {
                            error = "course entity not found";
                            return false;
                        }

                        if (WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == webStoreCategoryId && x.CourseId == itemId)) return true;

                        if (!IsCategoryItemNameValid(webStoreCategoryId, entity.CourseName))
                        {
                            error = "course with same name already exists";
                            return false;
                        }

                        var token = new WebStoreItemEditDTO
                        {
                            ItemId              = entity.Id
                            ,ItemName           = entity.CourseName
                            ,WebStoreCategoryId = webStoreCategoryId
                            ,Description        = entity.Description
                            ,IsActive           = true
                            ,OrderIndex         = index
                            ,ItemType           = type
                        };

                        WebStoreItemRepository.Add(token.WebStoreItemEditDto2WebStoreItemEntity());
                        break;
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        var bentity = BundleRepository.GetById(itemId);

                        if (bentity == null)
                        {
                            error = "bundle entity not found";
                            return false;
                        }

                        if (WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == webStoreCategoryId && x.BundleId == itemId)) return true;

                        token = new WebStoreItemEditDTO
                        {
                            ItemId              = bentity.BundleId
                            ,ItemName           = bentity.BundleName
                            ,WebStoreCategoryId = webStoreCategoryId
                            ,Description        = bentity.BundleDescription
                            ,IsActive           = true
                            ,OrderIndex         = index
                            ,ItemType           = type
                        };

                        WebStoreItemRepository.Add(token.WebStoreItemEditDto2WebStoreItemEntity());
                        break;
                }

                return WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges(out error);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("delete store category item", itemId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public bool AddItems2Category(AddItems2StoreCategoryToken data, out string error)
        {
            error = string.Empty;
            var total = 0;
            foreach (var item in data.items)
            {
                string tempError;
                if (SaveCategoryItem(data.categoryId, item.id, item.type, out tempError)) total++;
                else
                {
                    error += tempError;
                }
            }

            return total == data.items.Count;
        }

        public bool SaveItemOrder(int[] itemIds, out string error)
        {
            error = string.Empty;

            try
            {
                var i = 0;

                foreach (var entity in itemIds.Select(courseId => WebStoreItemRepository.GetById(courseId)).Where(entity => entity != null))
                {
                    entity.UpdateCourseEntityOrderIndex(i);

                    i++;
                }

                WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges();

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save course order", null, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }

        public bool AddFullLfeCategoryCourses(int? categoryId, int webCategoryId, List<BaseWebstoreItemListDTO> items, out string error)
        {
            return AddItemList2Category(webCategoryId, items, out error);
        }

        public bool AddAuthorCourses(int? authorId, int webCategoryId, List<BaseWebstoreItemListDTO> items, out string error)
        {
            return AddItemList2Category(webCategoryId, items, out error);
        }
        
        public List<WebStoreItemListDTO> GetAllAvailableItems(CourseEnums.CourseStatus? status = CourseEnums.CourseStatus.Published, string itemName = null, int? authorId = null)
        {
            try
            {
                var items =  WebStoreRepository.SearchItems(DEFAULT_CURRENCY_ID,status, string.IsNullOrEmpty(itemName) ? null : itemName.TrimString(), authorId).ToArray();
                
                return items.Select(x => x.ItemEntity2WebStoreItemListDto()).ToList();

            }
            catch (Exception ex)
            {
                Logger.Error("get web store items", null, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<WebStoreItemListDTO>();
            }
        }

        public List<BaseWebstoreItemListDTO> GetLfeCategoryItems(int categoryId, int webCategoryId, bool includeAttached)
        {
            try
            {
                var list = new List<BaseWebstoreItemListDTO>();
                var items = WebStoreCategoryRepository.GetCategoryCourseCandidateTokens(categoryId, webCategoryId).ToList();

                var i = 0;
                foreach (var token in items.Where(token => includeAttached || token.WebstoreItemId == null))
                {
                    list.Add(token.Entity2CourseListDto(i));
                    i++;
                }
                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("get web category courses", categoryId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<BaseWebstoreItemListDTO>();
            }
        }

        public List<BaseWebstoreItemListDTO> GetAuthorItems(int authorId, int webCategoryId, bool includeAttached)
        {
            try
            {
                var list = new List<BaseWebstoreItemListDTO>();
                var courses = WebStoreCategoryRepository.GetAuthorCourseCandidateTokens(authorId, webCategoryId).ToList();

                for (var i = 0; i < courses.Count(); i++)
                {
                    list.Add(courses[i].Entity2CourseListDto(i));
                }

                return list;
            }
            catch (Exception ex)
            {
                Logger.Error("get author courses", authorId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<BaseWebstoreItemListDTO>();
            }
        }

        public bool AddCourse2AuthorStores(int userId, int courseId, out string error)
        {
            error = string.Empty;

            try
            {
                var stores = WebStoreRepository.GetMany(x => x.OwnerUserID == userId).ToList();

                Logger.Debug("Add new course to author stores ::" + courseId + " :: " + stores.Count + " stores found");

                foreach (var store in stores)
                {
                    var courseIds = WebStoreRepository.GetAuthorNonIncludedCourses(store.StoreID);

                    if (!courseIds.Contains(courseId)) return true;

                    AddCourse2Store(store.StoreID, courseId, out error);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("add course to user stores", ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }
        #endregion

        #region store lov
        public List<UserBaseDTO> GetStoreAuthorsLOV(int storeId)
        {
            try
            {
                return WebStoreItemViewRepository.GetMany(x => x.StoreID == storeId && x.ItemStatusId == (byte) CourseEnums.CourseStatus.Published && x.AuthorName != null)
                        .GroupBy(x => new {x.AuthorID, x.AuthorName})
                        .Select(x => new UserBaseDTO {userId = x.Key.AuthorID, fullName = x.Key.AuthorName})
                        .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get store authors lov",storeId,ex,CommonEnums.LoggerObjectTypes.WebStore);
                return new List<UserBaseDTO>();
            }
        }

        public List<ItemLovToken> GetStoreItemsLOV(int storeId)
        {
            try
            {
                return WebStoreItemViewRepository.GetMany(x => x.StoreID == storeId && x.ItemStatusId == (byte)CourseEnums.CourseStatus.Published)
                        .GroupBy(x => new { x.ItemId, x.ItemName, x.ItemTypeId, x.AuthorID, x.AuthorName,x.TrackingID })
                        .Select(x => new ItemLovToken { ItemId = x.Key.ItemId, ItemName = x.Key.ItemName, ItemType = Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(x.Key.ItemTypeId), ItemPageUrl = x.GenerateItemPageUrl(x.Key.AuthorName.CleanUrl(), x.Key.ItemName.CleanUrl(), Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(x.Key.ItemTypeId),x.Key.TrackingID), User = new UserBaseDTO { userId = x.Key.AuthorID, fullName = x.Key.AuthorName } })
                        .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get store items lov", storeId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<ItemLovToken>();
            }
        }
        #endregion

        #region reports

        /// <summary>
        /// get store sales
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="periodKind"></param>
        /// <param name="sellerId"></param>
        /// <param name="lineType"></param>
        /// <returns></returns>
        public List<OrderLineDTO> GetStoreSales(int storeId, int userId, ReportEnums.ePeriodSelectionKinds periodKind, int? sellerId, BillingEnums.eOrderLineTypes? lineType)
        {
            try
            {
                var store = WebStoreRepository.GetById(storeId);

                if (store == null || store.OwnerUserID != userId) return new List<OrderLineDTO>();

                var dates = PeriodSelection2DateRange(periodKind);

                return SearchOrderLines(dates.@from, dates.to, sellerId: sellerId, buyerId: null, storeOwnerId: null, courseId: null, bundleId: null, storeId: storeId, lineType: lineType).Select(x => x.Entity2OrderLineDto()).ToList();
                //SearchOrderLines(dates.@from, dates.to,null,null,null,null,null,storeId,lineType).Select(x => x.Entity2OrderLineDto()).ToList();
                //WebStoreRepository.GetStoreSales(storeId, dates.@from, dates.to).Select(x => x.Entity2OrderLineDto()).OrderByDescending(x => x.OrderDate).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get store sales", storeId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<OrderLineDTO>();
            }
        }

        /// <summary>
        /// get sales by ownerId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="periodKind"></param>
        /// <param name="sellerId"></param>
        /// <param name="storeId"></param>
        /// <param name="lineType"></param>
        /// <returns></returns>
        public List<OrderLineDTO> GetOwnerStoreSales(int userId, ReportEnums.ePeriodSelectionKinds periodKind, int? sellerId, int? storeId, BillingEnums.eOrderLineTypes? lineType)
        {
            try
            {
                var dates = PeriodSelection2DateRange(periodKind);

                return SearchOrderLines(dates.@from, dates.to, sellerId: sellerId, buyerId: null, storeOwnerId: userId, courseId: null, bundleId: null, storeId: storeId, lineType: lineType).Select(x => x.Entity2OrderLineDto()).ToList();
                //WebStoreRepository.GetOwnerStoreSales(userId, dates.@from, dates.to,sellerId,storeId,lineType).Select(x => x.Entity2OrderLineDto()).OrderByDescending(x => x.OrderDate).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("get owner store sales", userId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return new List<OrderLineDTO>();
            }
        }

        public List<SalesAnalyticChartDTO> GetSalesChartData(int userId, ReportEnums.ePeriodSelectionKinds periodSelectionKind, ReportEnums.eChartGroupping groupBy)
        {
            var trx = GetOwnerStoreSales(userId, periodSelectionKind, null, null, null).ToList();

            if (trx.Count == 0) return new List<SalesAnalyticChartDTO>();

            var curDate = trx.Min(x => x.OrderDate).AddDays(-1);
            var endDate = trx.Max(x => x.OrderDate);

            var datePoints = new List<DateTime>();

            while (curDate <= endDate)
            {
                datePoints.Add(curDate);

                curDate = curDate.AddDays(1);
            }

            var points = datePoints.ToList().Select(period => new SalesAnalyticChartDTO
            {
                date = period,
                total = trx.Where(x => x.OrderDate.Date == period.Date).Sum(t => t.TotalAmount)
            }).ToList();

            //var to = points.Sum(o => o.total);

            return points;
        }

        public List<WebStoreAffiliateItemDTO> GetUserAffiliateItems(int userId)
        {
            return WebStoreItemRepository.GetUserAffiliateItems(DEFAULT_CURRENCY_ID,userId).Select(x => x.Entity2AffiliateItemDto()).ToList();
        }

        public List<BaseListDTO> GetUserAffiliateStoresLOV(int userId)
        {
            return WebStoreRepository.GetUserAffiliateStoresLOV(userId).Select(x => x.BaseStoreToken2BaseListDto()).ToList();
        }
        #endregion

        #endregion

        #endregion

        #region IWebStoreWixServices

        public bool AddItemToStore(Guid itemUid, int storeId,out string error)
        {

            error = string.Empty;

            try
            {
                //find store
                var store = WebStoreRepository.GetById(storeId);

                if (store == null)
                {
                    error = "Store entity not found";
                    return false;
                }


                 //check category
                var categories = WebStoreCategoryRepository.GetMany(x => x.WebStoreID == storeId).ToArray();

                int categoryId;

                if (!categories.Any())
                {
                    //create category
                    var categoryToken = new WebStoreCategoryEditDTO
                    {
                        WebStoreId    = storeId
                        ,IsPublic     = true
                        ,OrderIndex   = 1
                        ,CategoryName = "Main Category"
                        ,Description  = "Wix default category"
                    };

                    if (!SaveCategory(ref categoryToken, out error)) return false;

                    categoryId = categoryToken.WebStoreCategoryId;

                    //WebStoreItemRepository.ReloadContext();
                }
                else
                {
                     //take first
                    categoryId = categories[0].WebStoreCategoryID;
                }

                BillingEnums.ePurchaseItemTypes itemType;
                int itemId;
                string itemName;
                //find item
                var course = CourseRepository.Get(x => x.uid == itemUid);

                if (course != null)
                {
                    itemId   = course.Id;
                    itemType = BillingEnums.ePurchaseItemTypes.COURSE;
                    itemName = course.CourseName;

                    if (WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == categoryId && x.CourseId == itemId)) return true;
                }
                else
                {
                    var bundle = BundleRepository.Get(x => x.uid == itemUid);

                    if (bundle != null)
                    {
                        itemId   = bundle.BundleId;
                        itemType = BillingEnums.ePurchaseItemTypes.BUNDLE;
                        itemName = bundle.BundleName;

                        if (WebStoreItemRepository.IsAny(x => x.WebStoreCategoryID == categoryId && x.BundleId == itemId)) return true;
                    }
                    else
                    {
                        error = "item not found";
                        return false;                        
                    }
                }

               

                //add item to category
                var categoryItemToken = new WebStoreItemEditDTO
                {
                    WebStoreCategoryId = categoryId
                    ,ItemId = itemId
                    ,ItemType = itemType
                    ,ItemName = itemName
                    ,IsActive = true
                };

                if (!SaveCategoryItem(ref categoryItemToken, out error)) return false;

                UpdateWebStoreLog(storeId,out  error);

                return true;
            }
            catch (Exception ex)
            {
                error = FormatError(ex);
                Logger.Error("Add item to wix store::" +  storeId+ "::item::" + itemUid,ex,CommonEnums.LoggerObjectTypes.Wix);
                return false;
            }
        }
        public bool UpdateWixSiteUrl(ref WixStoreUrlToken token,  out string error)
        {
             try
            {
                error = "";
             
                if (token == null || string.IsNullOrEmpty(token.WixSiteUrl))
                {
                    error = "Wix site url is missing.";
                    return false;
                }

                var entity = WebStoreRepository.GetById(token.StoreId);
                if (entity == null)
                {
                    error = "store not found";
                    return false;
                }

                entity.UpdateStoreWixSiteUrl(token);
                
                return WebStoreRepository.UnitOfWork.CommitAndRefreshChanges() && _UpdatePluginDomain(entity.TrackingID,token.WixSiteUrl,out error);
             }
             catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                if (token != null)
                {
                    Logger.Error("save wix site url", ex, token.StoreId, CommonEnums.LoggerObjectTypes.WebStore);
                }
                else
                {
                    Logger.Error("save wix site url", ex, CommonEnums.LoggerObjectTypes.WebStore);
                }
                return false;
            }
        }

        

        public bool UpdateWixSettings(ref WixSettingsJsonToken jsonToken, UserDTO currentUser, out string error)
        {
            if (currentUser == null)
            {
                error = "Please re-login to LFE";
                return false;
            }

            var token = new WixSettingsToken
            {
                FontColor = jsonToken.cpFontColor,
                BackgroundColor = jsonToken.cpBackgroundColor,
                TabsFontColor = jsonToken.cpTabsFontColor,
                IsShowBorder = jsonToken.cbIsShowBorder,
                IsTransparent = jsonToken.cbIsTransparent,
                IsShowTitleBar = jsonToken.cbIsShowTitleBar,
                InstanceId = jsonToken.InstanceId,
                StoreName = jsonToken.txtStoreName,
                StoreId = jsonToken.StoreId,
                UniqueId = jsonToken.UniqueId,
                WixSiteUrl = jsonToken.WixSiteUrl
            };


            var trackingID = token.InstanceId;// +currentUser.UserId;

            if (!IsTrackingIdValid(jsonToken.StoreId ?? -1, trackingID, out error))
            {
                error = String.IsNullOrEmpty(error) ? "TrackingId already exists" : error;

                return false;
            }

            try
            {
                if (token.StoreId == null || token.StoreId < 0) //new store
                {
                    if (string.IsNullOrEmpty(token.StoreName))
                    {
                        //token.StoreName = currentUser.FullName + " Store " + DateTime.Now;
                        token.StoreName = GenerateStoreName(currentUser, out error);
                    }
                }
                var storeDto = token.WixSettingsToken2EditDto(currentUser.UserId, trackingID);
                
                var saveStatus = SaveStore(ref storeDto, currentUser.UserId, out error);

                if (saveStatus)
                {
                    var entity = WebStoreRepository.GetById(storeDto.StoreId);
                    if (entity == null)
                    {
                        error = "store not found";
                        return false;
                    }
                    entity.WixInstanceId   = storeDto.WixInstanceId;
                    entity.FontColor       = storeDto.FontColor;
                    entity.BackgroundColor = storeDto.BackgroundColor;
                    entity.TabsFontColor   = storeDto.TabsFontColor;
                    entity.IsTransparent   = storeDto.IsTransparent;
                    entity.IsShowBorder    = storeDto.IsShowBorder;
                    entity.IsShowTitleBar  = storeDto.IsShowTitleBar;
                    if (!string.IsNullOrEmpty(storeDto.WixSiteUrl)) { entity.WixSiteUrl = storeDto.WixSiteUrl; }

                    saveStatus = WebStoreRepository.UnitOfWork.CommitAndRefreshChanges(out error);
                }

                jsonToken.StoreId = storeDto.StoreId;
                jsonToken.txtStoreName = storeDto.StoreName;// entity.StoreName;
                jsonToken.UniqueId = storeDto.Uid.ToString();  //entity.uid.ToString();

                return saveStatus;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save wix store dto", ex, token.StoreId, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }


        #endregion

        #region IWebStoreFacebookServices
        public bool CreateOrValidateUserFbStore(int userId, string pageId, out string error)
        {
            
            var trackingId = pageId;

            var store = FindStoreByOwnerAndTrackingId(userId, trackingId,out error);

            if (store != null) return true;

            if (!String.IsNullOrEmpty(error)) return false;

            //create store
            var storeToken = userId.FbPageId2StoreEditDto(trackingId);

            return SaveStore(ref storeToken, userId, out error);
        }
        public bool UpdateFacebookSettings(ref FacebookSettingsJsonToken jsonToken, UserDTO currentUser, out string error)
        {
            if (currentUser == null)
            {
                error = "Please re-login to LFE";
                return false;
            }

            var token = new WixSettingsToken
            {
                FontColor       = jsonToken.cpFontColor,
                BackgroundColor = jsonToken.cpBackgroundColor,
                TabsFontColor   = jsonToken.cpTabsFontColor,
                IsShowBorder    = jsonToken.cbIsShowBorder,
                IsTransparent   = jsonToken.cbIsTransparent,
                IsShowTitleBar  = jsonToken.cbIsShowTitleBar,
                InstanceId      = null,
                StoreName       = jsonToken.txtStoreName,
                StoreId         = jsonToken.StoreId,
                UniqueId        = jsonToken.UniqueId,
                WixSiteUrl      = null,
                TrackingID      = jsonToken.TrackingId
            };


            if (!IsTrackingIdValid(jsonToken.StoreId ?? -1, token.TrackingID, out error))
            {
                error = String.IsNullOrEmpty(error) ? "TrackingId already exists" : error;

                return false;
            }

            try
            {
                WebStores entity;

                if (token.StoreId == null || token.StoreId < 0) //new store
                {
                    if (string.IsNullOrEmpty(token.StoreName))
                    {
                        //token.StoreName = currentUser.FullName + " Store " + DateTime.Now;
                        token.StoreName = GenerateStoreName(currentUser, out error);
                    }
                }
                var storeDto = token.WixSettingsToken2EditDto(currentUser.UserId, token.TrackingID);
                var saveStatus = SaveStore(ref storeDto, currentUser.UserId, out error);

                if (saveStatus)
                {
                    entity = WebStoreRepository.GetById(storeDto.StoreId);
                    if (entity == null)
                    {
                        error = "store not found";
                        return false;
                    }

                    entity.FontColor = storeDto.FontColor;
                    entity.BackgroundColor = storeDto.BackgroundColor;
                    entity.TabsFontColor = storeDto.TabsFontColor;
                    entity.IsTransparent = storeDto.IsTransparent;
                    entity.IsShowBorder = storeDto.IsShowBorder;
                    entity.IsShowTitleBar = storeDto.IsShowTitleBar;

                    saveStatus = WebStoreRepository.UnitOfWork.CommitAndRefreshChanges();
                }

                jsonToken.StoreId = storeDto.StoreId;
                jsonToken.txtStoreName = storeDto.StoreName;// entity.StoreName;
                jsonToken.UniqueId = storeDto.Uid.ToString();  //entity.uid.ToString();

                return saveStatus;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save wix store dto", ex, token.StoreId, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }
        

        #endregion

        #region IPluginStoreServices
        public bool SaveStore(ref BaseWebStoreDTO dto, int ownerId, int? sourceStoreId, out string error)
        {
            if (ownerId < 0)
            {
                error = "ownerId missing";
                return false;
            }

            if (!IsStoreNameValid(dto, ownerId, out error))
            {
                error = String.IsNullOrEmpty(error) ? "Store Name already exists" : error;
                return false;
            }

            if (!IsTrackingIdValid(dto.StoreId, dto.TrackingID, out error))
            {
                error = String.IsNullOrEmpty(error) ? "TrackingID already exists" : error;
                return false;
            }

            try
            {
                #region new store
                if (sourceStoreId == null)
                {
                    WebStores entity;
                    if (dto.StoreId < 0) //new store
                    {
                        entity = dto.EditDto2StoreEntity(ownerId);
                        WebStoreRepository.Add(entity);
                    }
                    else
                    {
                        entity = WebStoreRepository.GetById(dto.StoreId);

                        if (entity == null)
                        {
                            error = "Store entity not found";
                            return false;
                        }

                        entity.UpdateStoreEntity(dto);
                    }

                    if (!WebStoreRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                    dto.StoreId = entity.StoreID;

                    //add role
                    AddRole2User(ownerId, CommonEnums.UserRoles.Author);

                    return true;
                } 
                #endregion

                
                #region copy from existing

                var sourceEntity = WebStoreRepository.GetById((int) sourceStoreId);

                if (sourceEntity == null)
                {
                    error = "Source store entity not found";
                    return false;
                }

                var newEntity = sourceEntity.CloneStoreEntity(dto.Name, dto.TrackingID);
                WebStoreRepository.Add(newEntity);

                if (!WebStoreRepository.UnitOfWork.CommitAndRefreshChanges(out error)) return false;

                dto.StoreId = newEntity.StoreID;

                var categoryList = WebStoreCategoryRepository.GetMany(x => x.WebStoreID == (int)sourceStoreId).ToList();
                foreach (var category in categoryList)
                {
                    var newCategory = category.CloneCategoryEntity(dto.StoreId);
                    WebStoreCategoryRepository.Add(newCategory);
                    if (!WebStoreCategoryRepository.UnitOfWork.CommitAndRefreshChanges(out error)) continue;

                    int WebStoreCategoryID = category.WebStoreCategoryID;
                    var itemList = WebStoreItemRepository.GetMany(x => x.WebStoreCategoryID == WebStoreCategoryID).ToList();
                    foreach (var item in itemList)
                    {
                        WebStoreItemRepository.Add(item.CloneWsCategoryItemEntity(newCategory.WebStoreCategoryID));
                    }
                    WebStoreItemRepository.UnitOfWork.CommitAndRefreshChanges();
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);
                Logger.Error("save store dto", dto.StoreId, ex, CommonEnums.LoggerObjectTypes.WebStore);
                return false;
            }
        }
        #endregion
    }

}
