using System;
using System.Collections.Generic;
using System.Linq;
using LFE.Core.Enums;
using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class WebStoreRepository : Repository<WebStores>, IWebStoreRepository
    {
        public WebStoreRepository(IUnitOfWork unitOfWork) : base(unitOfWork){}

        public IEnumerable<WS_StoreListToken> GetOwnerStores(int? userId)
        {
            return DataContext.tvf_WS_GetStores(userId);
        }

       
        public IEnumerable<int> GetAuthorNonIncludedCourses(int storeId)
        {
            return DataContext.tvf_WS_GetAuthorNonIncludedCourses(storeId).Select(x=>x.CourseId);
        }

        public IEnumerable<WS_BaseStoreToken> GetUserAffiliateStoresLOV(int userId)
        {
            return DataContext.tvf_WS_GetUserAffiliateStoresLOV(userId);
        }

        public IEnumerable<WS_WixStoreToken> GetWixStores(int? ownerId)
        {
            return DataContext.tvf_WS_GetWixStores(ownerId);
        }

        public IEnumerable<WS_ItemsToken> SearchItems(short currencyId, CourseEnums.CourseStatus? status, string itemName = null, int? authorId = null)
        {
            return DataContext.tvf_WS_SearchItems(currencyId,status == null ? (byte?) null : (byte) status, authorId, itemName);
        }

    }

    public class WebStoreItemRepository : Repository<WebStoreItems>, IWebStoreItemRepository
    {
        public WebStoreItemRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public IEnumerable<WIDGET_CourseListToken> SearchCoursesTokens(short currencyId, int? pageID, string trackingID, int? pageSize, string wixViewMode, int? userID, string keyword)
        {
            return DataContext.sp_WIDGET_SearchCourses(currencyId,pageID, trackingID, pageSize, wixViewMode, userID, keyword);
        }

        public IEnumerable<WIDGET_CourseListToken> GetCoursesTokens(short currencyId, int? categoryId, int? pageID, string trackingID, string sort, string sortDirection, int? pageSize, string wixViewMode, int? userID)
        {
            return DataContext.sp_WIDGET_GetCourses(currencyId, categoryId, pageID, trackingID, sort, sortDirection, pageSize, wixViewMode, userID);
        }

        public IEnumerable<WIDGET_CourseListToken> GetStoreCoursesTokens(short currencyId, int? categoryId, int? pageID, string trackingID, int? pageSize,string wixViewMode, int? userID)
        {
            return DataContext.spWidget_GetStoreCourses(currencyId, categoryId, pageID, trackingID, pageSize, wixViewMode, userID);
        }

        public IEnumerable<WIDGET_CourseListToken> GetUserCoursesTokens(short currencyId, int userID, int? pageID, string sort, string sortDirection, int? pageSize)
        {
            return DataContext.sp_WIDGET_GetUsersCourses(currencyId, userID, pageID, sort, sortDirection, pageSize);
        }

        public IEnumerable<WIDGET_CourseListToken> GetAllCoursesTokens(short currencyId, int pageIndex, string sort, int pageSize, int? userID)
        {
            return DataContext.sp_Widget_GetAllCourses(currencyId, pageIndex, sort, "", pageSize, userID);
        }

        public IEnumerable<int> GetCourseWebStores(int courseId)
        {
            return DataContext.tvf_CRS_GetWebStores(courseId).Where(x=>x.HasValue).Select(x=>x.Value);
        }

        public IEnumerable<WS_AffiliateItemToken> GetUserAffiliateItems(short currencyId, int userId)
        {
            return DataContext.tvf_WS_GetUserAffiliateItems(currencyId,userId);
        }
    }

    public class WebStoreCategoryRepository : Repository<WebStoreCategories>, IWebStoreCategoryRepository
    {
        public WebStoreCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IEnumerable<WS_ItemCandidateToken> GetCategoryCourseCandidateTokens(int categoryId, int webCategoryId)
        {
            return DataContext.tvf_WS_GetCoursesByCategory(categoryId, webCategoryId);
        }

        public IEnumerable<WS_ItemCandidateToken> GetAuthorCourseCandidateTokens(int authorId, int webCategoryId)
        {
            return DataContext.tvf_WS_GetCoursesByAuthor(authorId, webCategoryId);
        }
    }

    public class WebStoresChangeLogRepository : Repository<WebStoresChangeLog>, IWebStoresChangeLogRepository
    {
        public WebStoresChangeLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

    public class WebStoreViewRepository : Repository<vw_WS_StoresLib>, IWebStoreViewRepository
    {
        public WebStoreViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class WebStoreItemViewRepository : Repository<vw_WS_Items>, IWebStoreItemViewRepository
    {
        public WebStoreItemViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
}
