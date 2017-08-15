using System;
using System.Collections.Generic;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface IWidgetServices : IDisposable
    {
        #region Index
        WidgetWebStoreDTO GetWidgetStoreDto( string trackingID);

        #region fetch items from SP's
        List<WidgetItemListDTO> GetCoursesList(short currencyId,string trackingID, int? categoryID, int? pageID, string sort, string sortDirection, int? pageSize, string wixViewMode, int? userID);
        List<WidgetItemListDTO> GetUserCoursesList(short currencyId,int userID, int? pageID, string sort, string sortDirection, int? pageSize);
        List<WidgetItemListDTO> SearchCourses(short currencyId,int? pageID, string trackingID, int? pageSize, string wixViewMode, int? userID, string keyword);
        IndexModelViewToken GetAllCoursesView(short currencyId,int pageIndex, string sort, int pageSize, int? userID);

        #region model view
        IndexModelViewToken GetIndexModelView(short currencyId,string trackingID, int pageIndex, string sort, int pageSize, int? categoryID, string categoryName, int? userID, string wixViewModel);
        IndexModelViewToken GetAuthorIndexModelViewToken(short currencyId, string trackingID, int authorId);
        IndexModelViewToken SearchModelView(short currencyId,string trackingID, int pageIndex, int pagesize, int? userID, string wixViewModel, string keyword); 
        #endregion
        #endregion

        WidgetCategoryDTO GetWebstoreCategory(string categoryUrlName, int webStoreID);
        List<WidgetCategoryDTO> GetWebStoreCategories(int webStoreID);        
        BaseModelViewToken GetBaseModelToken(string trackingID, string categoryUrlName, string wixViewMode, int? width, int? height);
        BaseModelViewToken GetStoreBaseModelToken(string trackingID);
        string GetParentURL();
        int NumItemsInPage(int? width, int? height);        

        #endregion

        #region Course
        CoursePurchaseDTO GetCoursePurchaseDTO(short currencyId,int id, int? userId, string trackingId);
        BundlePurchaseDTO GetBundlePurchaseDTO(short currencyId, int id, int? userId, string trackingId);
        AuthorPurchaseDTO GetAuthorContentDTO(short currencyId, int authorID); //, int currentCourseID
        WidgetCourseReviewsToken GetCourseReviews(int courseID);
        #endregion
        
        #region registration
        void PostFasebookRegistartionMessage(ReviewMessageDTO messageToken);
        #endregion

        #region Wix
        CoursePurchaseDTO GetPlaceHolderCoursePurchaseDTO();
        BaseModelViewToken GetWixBaseModelToken(string instance,  string categoryUrlName, string wixViewMode, int? width, int? height, string userIdStr);
        WidgetWebStoreDTO GetWixInstanceStore(string instance);
        CoursePurchaseDTO GetWixDefaultCourse(List<int> categoriesIdsList, int? userId);
        BundlePurchaseDTO GetWixDefaultBundle(List<int> categoriesIdsList, int? userId, string trackingId);
        #endregion

        #region Facebook
        BaseModelViewToken GetFacebookBaseModelToken(string trackingId, string categoryUrlName, string viewMode, int? width, int? height);
        #endregion

        #region item
        ItemProductPageToken GetWixDefaultItem(List<int> categoriesIdsList, int? userId);
        
        ItemProductPageToken GetPlaceHolderItemInfoToken();
        
        ItemInfoToken FindItemByUrlName(string author, string itemName, BillingEnums.ePurchaseItemTypes type);

        ItemInfoToken GetItemInfoToken(int id, BillingEnums.ePurchaseItemTypes type);

        ItemAccessStateToken GetItemAccessState4User(int? userId, int id, BillingEnums.ePurchaseItemTypes type);

        ItemProductPageToken ItemInfoToken2ItemProductPageToken(ItemInfoToken token,ItemAccessStateToken accessState, string trackingId);

        ItemViewerPageToken ItemInfoToken2ItemViewerPageToken(ItemInfoToken token, ItemAccessStateToken accessState, string trackingId);
        
        List<ContentTreeViewItemDTO> GetCourseContentsList(int itemId);
        int GetItemSubscribers(int id, BillingEnums.ePurchaseItemTypes type);

        List<ReviewDTO> GetItemReviews(int id);

        ItemInfoToken GetItemInfoTokenByPriceLineId(int lineId, out string error);
        #endregion
    }


}
