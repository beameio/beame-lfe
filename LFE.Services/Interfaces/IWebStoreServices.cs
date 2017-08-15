using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IWebStoreWixServices : IDisposable
    {
       bool AddItemToStore(Guid itemUid, int storeId, out string error);
       bool UpdateWixSettings(ref WixSettingsJsonToken token, UserDTO currentUser, out string error);
       bool UpdateWixSiteUrl(ref WixStoreUrlToken token,  out string error);

    }

    public interface IWebStoreFacebookServices : IDisposable
    {
        bool CreateOrValidateUserFbStore(int userId, string pageId, out string error);
        bool UpdateFacebookSettings(ref FacebookSettingsJsonToken token, UserDTO currentUser, out string error);
    }

    public interface IWidgetWebStoreServices
    {
        int? ValidateTrackingId(string trackingId);

        short GetStoreCurrencyByTrackingId(string trackingId);
    }

    public interface IPluginStoreServices : IDisposable
    {
        bool SaveStore(ref BaseWebStoreDTO dto, int ownerId,int? sourceStoreId, out string error);
    }

    public interface IWebStoreServices : IDisposable
    {
        //store
        bool ValidateOwnerStoreByUid(int ownerId, Guid uid);
        bool ValidateOwnerStoreByTrackingId(int ownerId, string trackingId);
        bool ValidateTrackingByUid(string trackId, Guid uid);
        bool ValidateOwnerStoreName(string name, int ownerId, int storeId);
        BaseEntityDTO FindStoreByUid(int ownerId, Guid uid);
        BaseEntityDTO FindStoreByTrackingId(int ownerId, string trackingId);
        List<WebStoreGridDTO> GetOwnerStores(int id);
        bool DeleteStore(int storeId, out string error);


        WebStoreEditDTO GetStoreEditDTO(int id);
        bool SaveStore(ref WebStoreEditDTO dto,int ownerId, out string error);        
        bool AddOwnerCourses2Store(int storeId, out string error);
        bool AddCourse2Store(int storeId, int courseId, out string error);

        //categories
        bool ValidateStoreCategoryName(string name, int categoryId, int storeId);
        List<BaseListDTO> GetStoreCategories(int storeId);
        List<WebStoreCategoryEditDTO> GetStoreEditCategories(int storeId);
        WebStoreCategoryEditDTO GetCategoryEditDTO(int categoryId, int storeId);
        bool SaveCategory(ref WebStoreCategoryEditDTO dto, out string error);
        bool DeleteCategory(int categoryId, out string error);
        bool SaveCategoriesOrder(int[] categoryIds, out string error);

        //courses
        List<WebStoreCourseListDTO> GetCategoryItems(int categoryId);
        List<WebStoreItemListDTO> GetCategoryItemsList(int categoryId);
        WebStoreItemEditDTO GetCategoryItemEditDto(int webItemId, int webCategoryId);
        bool SaveCategoryItem(ref WebStoreItemEditDTO dto, out string error);
        bool DeleteCategoryItem(int itemId, out string error);
        bool DeleteCategoryItem(int webStoreCategoryId,int itemId,BillingEnums.ePurchaseItemTypes type, out string error);
        bool SaveCategoryItem(int webStoreCategoryId, int itemId, BillingEnums.ePurchaseItemTypes type, out string error);
        bool AddItems2Category(AddItems2StoreCategoryToken data, out string error);
        bool SaveItemOrder(int[] itemIds, out string error);
        bool AddCourse2AuthorStores(int userId, int courseId, out string error);

        List<WebStoreAffiliateItemDTO> GetUserAffiliateItems(int userId);
        List<BaseListDTO> GetUserAffiliateStoresLOV(int userId);
        
        List<WebStoreItemListDTO> GetAllAvailableItems(CourseEnums.CourseStatus? status = CourseEnums.CourseStatus.Published, string itemName = null, int? authorId = null);
        List<BaseWebstoreItemListDTO> GetLfeCategoryItems(int categoryId, int webCategoryId,bool includeAttached);
        List<BaseWebstoreItemListDTO> GetAuthorItems(int authorId, int webCategoryId, bool includeAttached);
        bool AddFullLfeCategoryCourses(int? categoryId, int webCategoryId, List<BaseWebstoreItemListDTO> items, out string error);
        bool AddAuthorCourses(int? authorId, int webCategoryId, List<BaseWebstoreItemListDTO> items, out string error);

        //reports
        List<OrderLineDTO> GetStoreSales(int storeId, int userId, ReportEnums.ePeriodSelectionKinds periodKind, int? sellerId, BillingEnums.eOrderLineTypes? lineType);
        List<OrderLineDTO> GetOwnerStoreSales(int userId, ReportEnums.ePeriodSelectionKinds periodKind, int? sellerId, int? storeId, BillingEnums.eOrderLineTypes? lineType);
        List<SalesAnalyticChartDTO> GetSalesChartData(int userId, ReportEnums.ePeriodSelectionKinds periodSelectionKind, ReportEnums.eChartGroupping groupBy);

        //LOV
        List<UserBaseDTO> GetStoreAuthorsLOV(int storeId);
        List<ItemLovToken> GetStoreItemsLOV(int storeId);

    }

  
}
