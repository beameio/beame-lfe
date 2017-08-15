using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface IWebStoreRepository : IRepository<WebStores>
    {
        IEnumerable<WS_StoreListToken> GetOwnerStores(int? userId);
        IEnumerable<int> GetAuthorNonIncludedCourses(int storeId);
        IEnumerable<WS_BaseStoreToken> GetUserAffiliateStoresLOV(int userId);
        IEnumerable<WS_WixStoreToken> GetWixStores(int? ownerId);

        IEnumerable<WS_ItemsToken> SearchItems(short currencyId, CourseEnums.CourseStatus? status, string itemName = null, int? authorId = null);
        
    }

    public interface IWebStoreItemRepository : IRepository<WebStoreItems>
    {

        IEnumerable<WIDGET_CourseListToken> GetCoursesTokens(short currencyId,int? categoryId, int? pageID, string trackingID, string sort, string sortDirection, int? pageSize, string wixViewMode, int? userID);

        IEnumerable<WIDGET_CourseListToken> GetStoreCoursesTokens(short currencyId, int? categoryId, int? pageID, string trackingID, int? pageSize, string wixViewMode, int? userID);
        IEnumerable<WIDGET_CourseListToken> GetUserCoursesTokens(short currencyId, int userID, int? pageID, string sort, string sortDirection, int? pageSize);
        IEnumerable<WIDGET_CourseListToken> GetAllCoursesTokens(short currencyId, int pageIndex, string sort, int pageSize, int? userID);
        IEnumerable<WIDGET_CourseListToken> SearchCoursesTokens(short currencyId, int? pageID, string trackingID, int? pageSize, string wixViewMode, int? userID, string keyword);
        IEnumerable<int> GetCourseWebStores(int courseId);
        IEnumerable<WS_AffiliateItemToken> GetUserAffiliateItems(short currencyId, int userId);

    }

    public interface IWebStoreCategoryRepository : IRepository<WebStoreCategories>
    {
        IEnumerable<WS_ItemCandidateToken> GetCategoryCourseCandidateTokens(int categoryId, int webCategoryId);
        IEnumerable<WS_ItemCandidateToken> GetAuthorCourseCandidateTokens(int authorId, int webCategoryId);
    }

    public interface IWebStoresChangeLogRepository : IRepository<WebStoresChangeLog>{ }

    public interface IWebStoreViewRepository : IGetRepository<vw_WS_StoresLib> { }

    public interface IWebStoreItemViewRepository : IGetRepository<vw_WS_Items> { }
    
}
