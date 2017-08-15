using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LFE.Application.Services.Base;
using LFE.Application.Services.Helper;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.DtoMappers;
using LFE.Dto.Mapper.Helper;

namespace LFE.Application.Services
{
    public class WidgetServices : ServiceBase, IWidgetServices,IWidgetWebStoreServices
    {
        private readonly IFacebookServices _facebookServices;
        private readonly IWidgetCourseServices _itemServices;
        private readonly IQuizWidgetServices _quizWidgetServices;
        public WidgetServices()
        {
            _facebookServices      = DependencyResolver.Current.GetService<IFacebookServices>();
            _itemServices          = DependencyResolver.Current.GetService<IWidgetCourseServices>();
            _quizWidgetServices = DependencyResolver.Current.GetService<IQuizWidgetServices>();
        }

        #region interface implementation

        #region Index
        public WidgetWebStoreDTO GetWidgetStoreDto(string trackingID)
        {
            try
            {
                var entity = WebStoreRepository.Get(x => x.TrackingID == trackingID);

                return entity!=null ? entity.Entity2WidgetStoreDto() : null;

            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWebStore", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        #region fetch items from SP's
        public IndexModelViewToken GetAllCoursesView(short currencyId, int pageIndex, string sort, int pageSize, int? userID)
        {
            try
            {
                var courses = WebStoreItemRepository.GetAllCoursesTokens(currencyId,pageIndex, sort, pageSize, userID);
                var coursesList = courses.Select(item => item.Entity2CourseListDto(_itemServices.IsItemAccessAllowed4User(userID, item.ItemId, item.ItemTypeId), IsUnderRGP(item.AuthorID), GetStoreItemPrices(item.ItemId, item.ItemTypeId, currencyId), currencyId)).ToList();


                var totalCourses = 0;
                if (coursesList.Any())
                {
                    totalCourses = coursesList.First().TotalCourses;
                }

                var model = new IndexModelViewToken
                {
                    ItemsList   = coursesList
                    ,PageIndex    = pageIndex
                    ,PageSize     = pageSize
                    ,Sort         = sort
                    ,TotalCourses = totalCourses
                    ,CategoryName = "_All"
                };

                return model;

            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetAllCoursesView", ex, userID, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }
        }
        public List<WidgetItemListDTO> GetCoursesList(short currencyId, string trackingID, int? categoryID, int? pageID, string sort, string sortDirection, int? pageSize, string wixViewMode, int? userID)
        {
            try
            {
                var courses = String.IsNullOrEmpty(sort) || sort.ToLower() == "default" ? WebStoreItemRepository.GetStoreCoursesTokens(currencyId,categoryID, pageID, trackingID,pageSize, wixViewMode, userID) : WebStoreItemRepository.GetCoursesTokens(currencyId,categoryID, pageID, trackingID, sort, sortDirection, pageSize, wixViewMode, userID);

                return courses.Select(item => item.Entity2CourseListDto(_itemServices.IsItemAccessAllowed4User(userID, item.ItemId, item.ItemTypeId), IsUnderRGP(item.AuthorID), GetStoreItemPrices(item.ItemId, item.ItemTypeId, currencyId), currencyId)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetCoursesList", ex, categoryID, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }
        public List<WidgetItemListDTO> SearchCourses(short currencyId, int? pageID, string trackingID, int? pageSize, string wixViewMode, int? userID, string keyword)
        {
            try
            {
                var courses = WebStoreItemRepository.SearchCoursesTokens(currencyId,pageID, trackingID, pageSize, wixViewMode, userID, keyword);

                return courses.Select(item => item.Entity2CourseListDto(_itemServices.IsItemAccessAllowed4User(userID, item.ItemId, item.ItemTypeId), IsUnderRGP(item.AuthorID),GetStoreItemPrices(item.ItemId, item.ItemTypeId, currencyId), currencyId)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::SearchCourses", ex,pageID, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }
        public List<WidgetItemListDTO> GetUserCoursesList(short currencyId, int userID, int? pageID, string sort, string sortDirection, int? pageSize)
        {
            try
            {
                var courses = WebStoreItemRepository.GetUserCoursesTokens(currencyId,userID, pageID, sort, sortDirection, pageSize);

                return courses.Select(item => item.Entity2CourseListDto(_itemServices.IsItemAccessAllowed4User(userID, item.ItemId, item.ItemTypeId), IsUnderRGP(item.AuthorID),GetStoreItemPrices(item.ItemId, item.ItemTypeId, currencyId), currencyId)).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetUserCoursesList", ex, userID, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        #region model view
        public IndexModelViewToken GetIndexModelView(short currencyId, string trackingID, int pageIndex, string sort, int pageSize, int? categoryID, string categoryName, int? userID, string wixViewModel)
        {
            try
            {
                List<WidgetItemListDTO> coursesList;

                if (!string.IsNullOrEmpty(categoryName) && categoryName.ToLower().Trim() == "mycourses")
                {
                    coursesList = GetUserCoursesList(currencyId,Convert.ToInt32(userID), pageIndex, sort, "", pageSize).ToList();
                    foreach (var course in coursesList)
                    {
                        course.IsItemOwner = true;
                    }
                }
                else
                {
                    coursesList = GetCoursesList(currencyId,trackingID, categoryID, pageIndex, sort, "", pageSize, wixViewModel, userID).ToList();
                }

                var totalCourses = 0;
                if (coursesList.Any())
                {
                    totalCourses = coursesList.First().TotalCourses;
                }

                var model = new IndexModelViewToken
                {
                    ItemsList   = coursesList
                    ,PageIndex    = pageIndex
                    ,PageSize     = pageSize
                    ,Sort         = sort
                    ,TotalCourses = totalCourses
                    ,CategoryName = categoryName
                };

                return model;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetIndexModelView", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        public IndexModelViewToken GetAuthorIndexModelViewToken(short currencyId, string trackingID, int authorId)
        {
            try
            {
                var courses =   WebStoreItemViewRepository.GetMany(x => x.TrackingID == trackingID && x.ItemStatusId == (byte)CourseEnums.CourseStatus.Published && x.AuthorID == authorId)
                                .Select(item => item.Entity2CourseListDto(_itemServices.IsItemAccessAllowed4User(CurrentUserId, item.ItemId, item.ItemTypeId), GetStoreItemPrices(item.ItemId, item.ItemTypeId, currencyId)))
                                .ToList();

                 var model = new IndexModelViewToken
                {
                    ItemsList     = courses
                    ,PageIndex    = 1
                    ,PageSize     = NumItemsInPage(null,null)
                    ,TotalCourses = courses.Count
                };

                return model;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetAuthorIndexModelViewToken",authorId, ex, CommonEnums.LoggerObjectTypes.Widget);

                return new IndexModelViewToken();
            }
        }

        public IndexModelViewToken SearchModelView(short currencyId, string trackingID, int pageIndex, int pageSize, int? userID, string wixViewModel, string keyword)
        {
            try
            {
                var coursesList = SearchCourses(currencyId,pageIndex, trackingID, pageSize, wixViewModel, userID, keyword).ToList();
                
                var totalCourses = 0;
                if (coursesList.Any())
                {
                    totalCourses = coursesList.First().TotalCourses;
                }

                var model = new IndexModelViewToken
                {
                    ItemsList   = coursesList
                    ,PageIndex    = pageIndex
                    ,PageSize     = pageSize
                    ,Sort         = ""
                    ,TotalCourses = totalCourses
                    ,CategoryName = "Search Results"
                };

                return model;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::SearchModelView", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }
        #endregion
        #endregion

        public WidgetCategoryDTO GetWebstoreCategory(string categoryUrlName, int webStoreID)
        {
            try
            {
                if (String.IsNullOrEmpty(categoryUrlName)) return null;

                var catUrlName = categoryUrlName.TrimString().ToLower().OptimizedUrl();
                var entity = WebStoreCategoryRepository.Get(x => x.WebStoreID == webStoreID && x.IsPublic && x.CategoryUrlName == catUrlName);
                return entity.Entity2WidgetCategoryDTO();
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWebstoreCategory", ex, webStoreID, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        public List<WidgetCategoryDTO> GetWebStoreCategories(int webStoreID)
        {

            try
            {
                var categories = WebStoreCategoryRepository.GetMany(x => x.WebStoreID == webStoreID); // && x.IsPublic
                return categories.Select(item => item.Entity2WidgetCategoryDTO()).OrderBy(x=>x.Ordinal).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWebStoreCategories", ex, webStoreID, CommonEnums.LoggerObjectTypes.WebStore);

                return null;
            }
        }
        
        public string GetParentURL()
        {
            try
            {
                var widgetURL = HttpContext.Current.Request.Url.AbsoluteUri;

                //string parentWindowURL = HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : "";
                //string parentWindowURL = "";
                //if (HttpContext.Current.Request.Cookies["parentURL"] != null)
                //{
                //    parentWindowURL = HttpContext.Current.Request.Cookies["parentURL"].Value;
                //}
                var parentWindowURL = "";
                if (HttpContext.Current.Request.QueryString["parentUrl"] != null)
                {
                    parentWindowURL = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.QueryString["parentUrl"]);
                }

                if (string.IsNullOrEmpty(parentWindowURL)) return parentWindowURL;

                var hash = "";
                if (parentWindowURL.Contains("#"))
                {
                    hash = parentWindowURL.Substring(parentWindowURL.IndexOf("#", StringComparison.Ordinal));
                }

                int lfeUrlIndex = parentWindowURL.ToLower().IndexOf("&lfe_app_url=", StringComparison.Ordinal);
                if (lfeUrlIndex > 0)
                {
                    parentWindowURL = parentWindowURL.Substring(0, lfeUrlIndex);
                }

                parentWindowURL = parentWindowURL + (parentWindowURL.Contains('?') ? "" : "?") + "&lfe_app_url=" + HttpContext.Current.Server.UrlEncode(widgetURL) + hash;

                return parentWindowURL;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetParentURL", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        public string GetWixParentURL(string sss)
        {
            try
            {
                string widgetURL = HttpContext.Current.Request.Url.AbsoluteUri;

                string parentWindowURL = HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : "";
                if (!string.IsNullOrEmpty(parentWindowURL))
                {
                    string hash = "";
                    if (parentWindowURL.Contains("#"))
                    {
                        hash = parentWindowURL.Substring(parentWindowURL.IndexOf("#", StringComparison.Ordinal));
                    }

                    int lfeUrlIndex = parentWindowURL.ToLower().IndexOf("&lfe_app_url=", StringComparison.Ordinal);
                    if (lfeUrlIndex > 0)
                    {
                        parentWindowURL = parentWindowURL.Substring(0, lfeUrlIndex);
                    }

                    parentWindowURL = parentWindowURL + (parentWindowURL.Contains('?') ? "" : "?") + "&lfe_app_url=" + HttpContext.Current.Server.UrlEncode(widgetURL) + hash;
                }

                return parentWindowURL;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWixParentURL", ex, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }
        }


        //public BaseModelViewToken GetUserPurchasesBaseModelToken()
        //{
        //    try
        //    {
        //        var categoryName = "mycourses";                    
                

        //        var modelHeader = new BaseModelViewToken
        //            {
        //                WebStore              = webStore
        //                ,Category             = selectedCategory
        //                ,CategoriesList       = GetWebStoreCategories(webStore.WebStoreID)
        //                ,CategoryName         = categoryName
        //                ,ParentURL            = GetParentURL()
        //                ,Width                = width
        //                ,Height               = height
        //                ,Status               = webStore.Status
        //                ,IsSingleCourseStore  = isSingleCourseStore                        
        //                ,NumCourses           = numCourses
        //                ,IsUnderSSL           = false
        //            };

        //        return modelHeader;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Widget::GetBaseModelToken", ex, CommonEnums.LoggerObjectTypes.Widget);
        //        return null;
        //    }
        //}

        public BaseModelViewToken GetBaseModelToken(string trackingID, string categoryUrlName, string wixViewMode, int? width, int? height)
        {
            try
            {
                var webStore = GetWidgetStoreDto(trackingID);

                if (webStore == null && (string.IsNullOrEmpty(categoryUrlName) || categoryUrlName.ToLower() != "mycourses")) { return null; }

                var categoryName = "_All";
                var isSingleCourseStore = true;
                WidgetCategoryDTO selectedCategory = null;
                int numCourses = 0;
                var isUserPurchasesCategory = false;
                if (webStore != null)
                {
                    selectedCategory = GetWebstoreCategory(categoryUrlName, webStore.WebStoreID);

                    
                    if (selectedCategory != null)
                    {
                        categoryName = selectedCategory.CategoryName;
                    }
                    else if (!string.IsNullOrEmpty(categoryUrlName) && categoryUrlName.ToLower() == "mycourses")
                    {
                        categoryName = categoryUrlName;
                        isUserPurchasesCategory = true;
                    }


                    var categoriesList = GetWebStoreCategories(webStore.WebStoreID);
                   
                    numCourses = GetStoreNumCourses(categoriesList);
                    if (numCourses > 1)
                    {
                        isSingleCourseStore = false;
                    }    
                }
                else
                {
                    if (!string.IsNullOrEmpty(categoryUrlName) && categoryUrlName.ToLower() == "mycourses")
                    {
                        categoryName = categoryUrlName;
                        isUserPurchasesCategory = true;
                    }
                }
                


                var modelHeader = new BaseModelViewToken
                    {
                        WebStore                 = webStore
                        ,Category                = selectedCategory
                        ,CategoriesList          = webStore != null ? GetWebStoreCategories(webStore.WebStoreID) : new List<WidgetCategoryDTO>()
                        ,CategoryName            = categoryName
                        ,ParentURL               = GetParentURL()
                        ,Width                   = width
                        ,Height                  = height
                        ,Status                  = webStore != null ? webStore.Status : WebStoreEnums.StoreStatus.Unknown
                        ,IsSingleCourseStore     = isSingleCourseStore
                        ,WixAppLastUpdateDate    = "1970-10-06T12:46:12.39"// GetWixLastUpdateString(trackingGuid)
                        ,WixViewMode             = string.IsNullOrEmpty(wixViewMode) ? "site" : wixViewMode
                        ,NumCourses              = numCourses
                        ,IsUnderSSL              = false
                        ,TrackingId              = trackingID
                        ,IsUserPurchasesCategory = isUserPurchasesCategory
                    };

                return modelHeader;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetBaseModelToken", ex, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }
        }

        public BaseModelViewToken GetStoreBaseModelToken(string trackingID)
        {
            try
            {
                var webStore = GetWidgetStoreDto(trackingID);

                if (webStore == null) { return null; }
                
                var categoriesList = GetWebStoreCategories(webStore.WebStoreID);
                var isSingleCourseStore = true;
                var numCourses = GetStoreNumCourses(categoriesList);
                if (numCourses > 1)
                {
                    isSingleCourseStore = false;
                }


                var modelHeader = new BaseModelViewToken
                    {
                        WebStore              = webStore
                        ,Category             = null
                        ,CategoriesList       = GetWebStoreCategories(webStore.WebStoreID)
                        ,CategoryName         = string.Empty
                        ,ParentURL            = GetParentURL()
                        ,Status               = webStore.Status                        
                        ,IsSingleCourseStore  = isSingleCourseStore
                        ,WixAppLastUpdateDate = "1970-10-06T12:46:12.39"// GetWixLastUpdateString(trackingGuid)
                        ,WixViewMode          = "site"
                        ,NumCourses           = numCourses
                        ,IsUnderSSL           = false
                        ,TrackingId           = trackingID
                    };

                return modelHeader;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetBaseModelToken", ex, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }
        }
        #endregion

        #region Course
        public CoursePurchaseDTO GetPlaceHolderCoursePurchaseDTO()
        {
            var userDTO = new UserBaseDTO
            {
                userId     = -1
                ,firstName = "Author"
                ,lastName  = ""
                ,fullName  = "Author Name"
            };


            var chapters = new List<ContentTreeViewItemDTO>();

            var firstChapter = new ContentTreeViewItemDTO
            {
                id      = -1
                ,name   = "First Chapter"
                ,bcId   = null
                ,type   = CourseEnums.eContentTreeViewItemType.chapter
                ,desc   = null
                ,thumb  = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,videos = new List<ContentTreeViewItemDTO>()
            };

            var secondChapter = new ContentTreeViewItemDTO
            {
                id      = -1
                ,name   = "Second Chapter"
                ,bcId   = null
                ,type   = CourseEnums.eContentTreeViewItemType.chapter
                ,desc   = null
                ,thumb  = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,videos = new List<ContentTreeViewItemDTO>()
            };
            chapters.Add(firstChapter);
            chapters.Add(secondChapter);

            var firstVideo = new ContentTreeViewItemDTO
            {
                id        = -1
                ,name     = "Video Title"
                ,bcId     = "-1"
                ,type     = CourseEnums.eContentTreeViewItemType.video
                ,desc     = "Video Summary of first video"
                ,thumb    = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,duration = "07:45"
                ,IsOpen   = true
            };

            var secondVideo = new ContentTreeViewItemDTO
            {
                id        = -1
                ,name     = "Video Title"
                ,bcId     = "-1"
                ,type     = CourseEnums.eContentTreeViewItemType.video
                ,desc     = "Video Summary of second video"
                ,thumb    = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,duration = "15:30"
                ,IsOpen   = false
            };

            firstChapter.videos.Add(firstVideo);
            firstChapter.videos.Add(secondVideo);
            secondChapter.videos.Add(firstVideo);
            secondChapter.videos.Add(secondVideo);


            var token = new CoursePurchaseDTO
            {
                User                     = userDTO
                ,CourseName              = "Course Name"
                ,ThumbUrl                = "/Content/images/webstore/Video-placeholder-image.jpg"
                ,MetaTags                = ""
                ,Rating                  = 5
                ,NumSubscribers          = 0
                ,Price                   = Convert.ToDecimal(0)
                ,IsFreeCourse            = false
                ,IntroHtml               = "Course Title<br/>Course Description"
                ,OverviewVideoIdentifier = ""
                ,CourseUrlName           = ""
                ,ItemState               = new ItemAccessStateToken() 
                ,Chapters                = chapters

            };

            return token;
        }

        public CoursePurchaseDTO GetCoursePurchaseDTO(short currencyId, int id, int? userId, string trackingId)
        {
            try
            {
                var entity = CourseRepository.GetById(id);

                if (entity == null) return null;

                var prices = GetStoreItemPrices(id, BillingEnums.ePurchaseItemTypes.COURSE, currencyId);
                
                if (!prices.Any()) prices = new List<PriceLineDTO>();

                var token = entity.CourseEntity2CoursePurchaseDto(GetItemRegularPrice(prices),GetItemMonthlySubscriptionPrice(prices));

                token.PriceLines       = prices;
                token.TrackingID       = trackingId;
                token.User             = _GetAuthorBaseDto(entity.AuthorUserId);
                token.ItemState        = _itemServices.GetCourseAccessState4User(userId, id);
                token.Chapters         = _itemServices.GetCourseChaptersList(id);
                token.VideosNavigation = token.Chapters.ChapterTreeList2VideoNavigation();

                //var itemEntity = WebStoreItemViewRepository.GetMany(x => x.ItemId == id && x.ItemTypeId == BillingEnums.ePurchaseItemTypes.COURSE).FirstOrDefault();
                //if (itemEntity != null)
                //{
                //    token.NumSubscribers = itemEntity.NumSubscribers;
                //}

                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetCoursePurchaseDTO", id, ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        public BundlePurchaseDTO GetBundlePurchaseDTO(short currencyId, int id, int? userId, string trackingId)
        {
            try
            {
                var entity = BundleRepository.GetById(id);

                if (entity == null) return null;

                var prices = GetStoreItemPrices(id, BillingEnums.ePurchaseItemTypes.BUNDLE, currencyId);

                if(!prices.Any()) prices = new List<PriceLineDTO>();

                var token = entity.BundleEntity2BundlePurchaseDto(GetItemRegularPrice(prices), GetItemMonthlySubscriptionPrice(prices));

                token.PriceLines    = prices;
                token.TrackingID    = trackingId;
                token.User          = _GetAuthorBaseDto(entity.AuthorId);
                token.ItemState     = _itemServices.GetBundleAccessState4User(userId, id);
                token.BundleCourses = _itemServices.GetBundleCoursesList(id,trackingId);
                token.IsValid       = true;
                
                return token;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetBundlePurchaseDTO", id, ex, CommonEnums.LoggerObjectTypes.Widget);

                return new BundlePurchaseDTO
                {
                    IsValid = false
                    ,Message = Utils.FormatError(ex)
                };
            }
        }

        private int GetStoreNumCourses(IEnumerable<WidgetCategoryDTO> categories)
        {
            try
            {
                var categoriesIDs = categories.Select(x => x.WebStoreCategoryID).ToList();
                var courses = WebStoreItemRepository.GetMany(x => categoriesIDs.Contains(x.WebStoreCategoryID));

                return courses.Count();
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetStoreNumCourses", ex, CommonEnums.LoggerObjectTypes.Widget);
                return -1;
            }
        }

        public AuthorPurchaseDTO GetAuthorContentDTO(short currencyId, int authorID)//, int currentCourseID
        {
            try
            {
                var courses = CourseRepository.GetMany(x => x.AuthorUserId == authorID).Select(item => item.Entity2CourseWidgetListDto(GetItemRegularPrice(item.Id, BillingEnums.ePurchaseItemTypes.COURSE, currencyId), GetItemMonthlySubscriptionPrice(item.Id, BillingEnums.ePurchaseItemTypes.COURSE, currencyId))).ToList();

                var user = UserRepository.Get(x => x.Id == authorID);

                if (user != null)
                {
                    foreach (var item in courses)
                    {
                        item.AuthorName = user.Entity2FullName();
                    }

                    return user.UserAndCourses2AuthorPurchaseDTO(courses);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetAuthorContentDTO", authorID, ex, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }
        }

        public WidgetCourseReviewsToken GetCourseReviews(int courseID)
        {
            try
            {
                var reviews = CourseRepository.GetAllCourseReviews(courseID).Select(x => x.UserReviewEntity2ReviewDTO()).OrderByDescending(x => x.Date);
                var course = CourseRepository.Get(x => x.Id == courseID);

                return new WidgetCourseReviewsToken
                {
                    ReviewsList = reviews.ToList()
                    ,Item     = course.Entity2CourseWidgetListDto(null,null)
                };

            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetCourseReviews", courseID, ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        public int NumItemsInPage(int? width, int? height)
        {
            if (width == null || height == null)
            {
                //1366×768 most common screen resolution
                width = 1366;
                height = 768;
            }

            var numItemsInRow = (int)width / 220;
            if (numItemsInRow == 0) { numItemsInRow = 1; }

            height = height - 190;
            if (height < 250) { height = 500; }
            var numItemInCol = (int)height / 250;
            if (numItemInCol == 0) { numItemInCol = 1; }

            var pagesize = numItemsInRow * numItemInCol;

            return pagesize;
        }
        #endregion

        #region registration
        public void PostFasebookRegistartionMessage(ReviewMessageDTO messageToken)
        {
            try
            {
                var postDto = messageToken.UserRegistration2MessageDTO();

                if (postDto == null) return;

                //save user message
                string error;
                _facebookServices.SavePostMessage(postDto, out error);

                //save app post
                postDto.IsAppPagePost = true;
                _facebookServices.SavePostMessage(postDto, out error);
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::post facebook registration message", null, ex, CommonEnums.LoggerObjectTypes.Widget);

            }
        }

        #endregion

        #region Wix
        private BaseModelViewToken GetExternalEmptyModelToken(string wixInstanceId, int? width, int? height)
        {
            try
            {
                return new BaseModelViewToken
                {
                    WebStore = new WidgetWebStoreDTO
                    {
                        WebStoreName   = "Store Name"
                        ,WixInstanceID = wixInstanceId
                        ,CurrencyId    = DEFAULT_CURRENCY_ID 
                    }
                    ,ParentURL    = GetParentURL()
                    ,Width        = width
                    ,Height       = height
                    ,CategoryName = "Category Name"
                    ,Category     = new WidgetCategoryDTO
                        {
                            CategoryName   = "Category Name"
                            ,LfeCategoryID = -1
                            ,Ordinal       = 1
                        }
                    ,CategoriesList       = new List<WidgetCategoryDTO>()
                    ,Status               = WebStoreEnums.StoreStatus.Published
                    ,IsSingleCourseStore  = true
                    ,WixAppLastUpdateDate = "1970-10-06T12:46:12.39" //!string.IsNullOrEmpty(instanceID) ? GetWixLastUpdateString(new Guid(instanceID)) : "1970-10-06T12:46:12.39"
                    ,WixViewMode          = "editor"
                    ,IsUnderSSL           = false
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWixEmptyModelToken::", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        public BaseModelViewToken GetFacebookBaseModelToken(string trackingId, string categoryUrlName, string viewMode, int? width, int? height)
        {
            try
            {
                BaseModelViewToken baseModel;
                if (string.IsNullOrEmpty(trackingId))
                {
                    baseModel = GetExternalEmptyModelToken(string.Empty,width, height);                   
                }
                else
                {
                    var webStore = GetWidgetStoreDto(trackingId);

                    baseModel = GetExternalAppBaseToken(webStore, categoryUrlName, viewMode, string.Empty,width, height);
                }

                baseModel.IsUnderSSL = true;
                baseModel.TrackingId = trackingId;
                return baseModel;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetFacebookBaseModelToken", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }

        }

        public BaseModelViewToken GetWixBaseModelToken(string instance, string categoryUrlName, string wixViewMode, int? width, int? height, string userIdStr)
        {
            try
            {
                if (string.IsNullOrEmpty(instance))
                {
                    return GetExternalEmptyModelToken(string.Empty,width, height);
                }

                string error;
                var instanceDTO = instance.DecodeInstance2WixInstanceDTO(out error);

                if (instanceDTO == null)
                {
                    return new BaseModelViewToken
                    {
                        IsValid = false
                        ,Message = error
                    };
                }

                var webStore = GetWidgetStoreDto(instanceDTO.instanceId.ToString());

                return GetExternalAppBaseToken(webStore, categoryUrlName, wixViewMode,instanceDTO.instanceId.ToString(), width, height);
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWixBaseModelToken", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }

        }

        public WidgetWebStoreDTO GetWixInstanceStore(string instance)
        {
            string error;
            var instanceDTO = instance.DecodeInstance2WixInstanceDTO(out error);

            return instanceDTO == null ? null : GetWidgetStoreDto(instanceDTO.instanceId.ToString());
        }

        private BaseModelViewToken GetExternalAppBaseToken(WidgetWebStoreDTO webStore, string categoryUrlName, string viewMode,string wixInstanceId, int? width, int? height)
        {
            try
            {
                if (webStore == null)
                {
                    return GetExternalEmptyModelToken(wixInstanceId,width, height);
                }

                var categoriesList = GetWebStoreCategories(webStore.WebStoreID);
                var isSingleCourseStore = true;
                var numCourses = GetStoreNumCourses(categoriesList);
                if (numCourses > 1)
                {
                    isSingleCourseStore = false;
                }


                WidgetCategoryDTO selectedCategory = null;
                string categoryName = "";
                if (numCourses <= 0)
                {
                    categoryName = "Category Name";
                    selectedCategory = new WidgetCategoryDTO
                    {
                        CategoryName   = "category Name"
                        ,LfeCategoryID = -1
                        ,Ordinal       = 1
                    };
                }
                else if (!string.IsNullOrEmpty(categoryUrlName) || numCourses > 0)
                {
                    selectedCategory = GetWebstoreCategory(categoryUrlName, webStore.WebStoreID);
                    categoryName = "_All";
                    if (selectedCategory != null)
                    {
                        categoryName = selectedCategory.CategoryName;
                    }
                    else if (!string.IsNullOrEmpty(categoryUrlName) && categoryUrlName.ToLower() == "mycourses")
                    {
                        categoryName = categoryUrlName;
                    }
                }


                var modelHeader = new BaseModelViewToken
                {
                    WebStore              = webStore
                    ,CategoryName         = categoryName
                    ,Category             = selectedCategory
                    ,CategoriesList       = categoriesList
                    ,ParentURL            = GetParentURL()
                    ,Width                = width
                    ,Height               = height
                    ,Status               = webStore.Status
                    ,IsSingleCourseStore  = isSingleCourseStore
                    ,NumCourses           = numCourses
                    ,WixAppLastUpdateDate = "1970-10-06T12:46:12.39" //GetWixLastUpdateString(instanceDTO.instanceId)
                    ,WixViewMode          = viewMode
                    ,IsUnderSSL           = false                   
                };

                return modelHeader;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetExternalAppBaseToken", ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }
        
        public CoursePurchaseDTO GetWixDefaultCourse(List<int> categoriesIdsList, int? userId)
        {
            try
            {
                //var webstoreCourse = WebStoreItemRepository.Get(x => x.IsActive && categoriesIdsList.Contains(x.WebStoreCategoryID));
                
                //if(webstoreCourse.CourseId!=null)
                
                //return GetCoursePurchaseDTO((int) webstoreCourse.CourseId, userId,string.Empty);

                //return null;

                var webstoreCourses = WebStoreItemRepository.GetMany(x => x.IsActive && categoriesIdsList.Contains(x.WebStoreCategoryID)).ToArray();

                if (!webstoreCourses.Any()) return null;

                var webstoreCourse = webstoreCourses.FirstOrDefault(x => x.CourseId != null);

                if (webstoreCourse == null) return null;

                return webstoreCourse.CourseId != null ? GetCoursePurchaseDTO(DEFAULT_CURRENCY_ID, (int)webstoreCourse.CourseId, userId, string.Empty) : null;

                //TODO add bundle logic
                //if (webstoreCourse.BundleId != null)
                //    return GetBundlePurchaseDTO((int)webstoreCourse.BundleId, userId);
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWixDefaultCourse", ex, userId, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }

        }

        public BundlePurchaseDTO GetWixDefaultBundle(List<int> categoriesIdsList, int? userId, string trackingId)
        {
            try
            {
                var webstoreCourse = WebStoreItemRepository.Get(x => x.IsActive && categoriesIdsList.Contains(x.WebStoreCategoryID));

                if (webstoreCourse.BundleId != null)
                    return GetBundlePurchaseDTO(DEFAULT_CURRENCY_ID,(int)webstoreCourse.BundleId, userId, trackingId);

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::GetWixDefaultBundle", ex, userId, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }

        }

        #endregion

        #region item
        #region private helpers
        private ItemInfoToken FindBundleByUrlName(string authorName, string urlName)
        {
            try
            {
                if (String.IsNullOrEmpty(urlName)) return null;

                var bundleUrlName = urlName.TrimString().OptimizedUrl();

                var entities = BundleRepository.GetMany(x => x.BundleUrlName == bundleUrlName).ToList();

                if (entities.Count().Equals(0)) return null;

                if (entities.Count().Equals(1)) return entities[0].BundleEntity2ItemInfoToken(UserRepository.GetById(entities[0].AuthorId), IsUnderRGP(entities[0].AuthorId));

                var query = (from course in BundleRepository.GetAll()
                             join user in UserRepository.GetAll() on course.AuthorId equals user.Id
                             where course.BundleUrlName == urlName.Trim()
                             select new { author = user.Entity2FullName().OptimizedUrl(), course }).Where(
                        x => x.author == authorName).Select(x => x.course).ToList();

                return query.Count != 1 ? null : query[0].BundleEntity2ItemInfoToken(UserRepository.GetById(entities[0].AuthorId), IsUnderRGP(entities[0].AuthorId));
            }
            catch (Exception ex)
            {
                Logger.Error("find bundle by url name " + urlName, null, ex, CommonEnums.LoggerObjectTypes.UserCourse);

                return null;
            }
        }

        private BundleDetailsToken GetBundleDetailsToken(int itemId,string trackingId)
        {
            var token = new BundleDetailsToken
            {
                BundleCourses = _itemServices.GetBundleCoursesList(itemId,trackingId)
            };

            foreach (var course in token.BundleCourses)
            {
                var regular = GetItemRegularPrice(course.id, BillingEnums.ePurchaseItemTypes.COURSE, DEFAULT_CURRENCY_ID);

                if (regular != null) course.price = (decimal)regular;
                else
                {
                    var monthly = GetItemMonthlySubscriptionPrice(itemId, BillingEnums.ePurchaseItemTypes.BUNDLE, DEFAULT_CURRENCY_ID);

                    if (monthly != null) course.price = 12 * ((decimal)monthly);
                    else
                    {
                        course.price = 0;
                    }
                }
            }
            token.TotalCoursesWorth = token.BundleCourses.Sum(x => x.price);

            return token;
        }
        #endregion

        #region wix
        public ItemProductPageToken GetWixDefaultItem(List<int> categoriesIdsList, int? userId)
        {
            try
            {
                var webstoreItems = WebStoreItemViewRepository.GetMany(x => categoriesIdsList.Contains(x.WebStoreCategoryID)).ToArray();//WebStoreItemRepository.GetMany(x => x.IsActive && categoriesIdsList.Contains(x.WebStoreCategoryID)).ToArray();

                if (!webstoreItems.Any()) return null;

                var item = webstoreItems[0];

                var infoToken = GetItemInfoToken(item.ItemId, Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(item.ItemTypeId));

                infoToken.VideoInfoToken = infoToken.OverviewVideoIdentifier != null ? GetVideoInfoToken((long)infoToken.OverviewVideoIdentifier) : DefaulVideoInfoToken;

                var state = userId != null ? GetItemAccessState4User(userId, item.ItemId, Utils.ParseEnum<BillingEnums.ePurchaseItemTypes>(item.ItemTypeId)) : new ItemAccessStateToken { IsPublished = infoToken.IsPublished };

                return ItemInfoToken2ItemProductPageToken(infoToken, state, string.Empty);
            }
            catch (Exception ex)
            {
                Logger.Error("Get Wix Default Item", ex, CommonEnums.LoggerObjectTypes.Widget);
                return null;
            }
        }


        public ItemProductPageToken GetPlaceHolderItemInfoToken()
        {
            var chapters = new List<ContentTreeViewItemDTO>();

            var firstChapter = new ContentTreeViewItemDTO
            {
                id      = -1
                ,name   = "First Chapter"
                ,bcId   = null
                ,type   = CourseEnums.eContentTreeViewItemType.chapter
                ,desc   = null
                ,thumb  = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,videos = new List<ContentTreeViewItemDTO>()
            };

            var secondChapter = new ContentTreeViewItemDTO
            {
                id      = -1
                ,name   = "Second Chapter"
                ,bcId   = null
                ,type   = CourseEnums.eContentTreeViewItemType.chapter
                ,desc   = null
                ,thumb  = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,videos = new List<ContentTreeViewItemDTO>()
            };
            chapters.Add(firstChapter);
            chapters.Add(secondChapter);

            var firstVideo = new ContentTreeViewItemDTO
            {
                id        = -1
                ,name     = "Video Title"
                ,bcId     = "-1"
                ,type     = CourseEnums.eContentTreeViewItemType.video
                ,desc     = "Video Summary of first video"
                ,thumb    = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,duration = "07:45"
                ,IsOpen   = true
            };

            var secondVideo = new ContentTreeViewItemDTO
            {
                id        = -1
                ,name     = "Video Title"
                ,bcId     = "-1"
                ,type     = CourseEnums.eContentTreeViewItemType.video
                ,desc     = "Video Summary of second video"
                ,thumb    = "/Content/images/webstore/Video-placeholder-image-small.jpg"
                ,duration = "15:30"
                ,IsOpen   = false
            };

            firstChapter.videos.Add(firstVideo);
            firstChapter.videos.Add(secondVideo);
            secondChapter.videos.Add(firstVideo);
            secondChapter.videos.Add(secondVideo);


            var token = new ItemProductPageToken
            {
                 ItemId                  = -1
                ,ItemName                = "Course Template"
                ,ItemType                = BillingEnums.ePurchaseItemTypes.COURSE
                ,IntroHtml               =  "Course Title<br/>Course Description"
                ,Rating                  = 5
                ,IsFreeItem              = false
                ,OverviewVideoIdentifier = 3084309361001
                ,ThumbUrl                = "/Content/images/webstore/Video-placeholder-image.jpg"
                ,Author                  = new BaseUserInfoDTO
                                            {
                                                UserId = -1
                                                ,FullName = "Author Name"
                                            }
                 ,Contents                = chapters
                 ,Currency                = new BaseCurrencyDTO{CurrencyId = DEFAULT_CURRENCY_ID,ISO = "$"}
                 ,PriceLines              = new List<PriceLineDTO>()
                 ,ItemState               = new ItemAccessStateToken()
            };

            var pt =  new PriceLineDTO
                     {
                         PriceLineID = -1
                         ,Currency   =  new BaseCurrencyDTO{CurrencyId = DEFAULT_CURRENCY_ID,ISO = "$"}
                         ,IsValid    = true
                         ,Price      = 10
                         ,PriceType  = BillingEnums.ePricingTypes.ONE_TIME
                     };

            pt.Title = pt.PriceLineToken2Title();            
            pt.Name = pt.PriceLineToken2Name();
            token.VideoInfoToken = DefaulVideoInfoToken;
            token.PriceLines.Add(pt);

            pt = new PriceLineDTO
                     {
                         PriceLineID       = -1
                         ,Currency         =  new BaseCurrencyDTO{CurrencyId = DEFAULT_CURRENCY_ID,ISO = "$"}
                         ,IsValid          = true
                         ,Price            = 5
                         ,PriceType        = BillingEnums.ePricingTypes.SUBSCRIPTION
                         ,NumOfPeriodUnits = 12
                         ,PeriodType       = BillingEnums.eBillingPeriodType.MONTH
                     };

            pt.Title = pt.PriceLineToken2Title();
            pt.Name = pt.PriceLineToken2Name();

            token.PriceLines.Add(pt);           
            return token;
        }
        #endregion
        public ItemInfoToken FindItemByUrlName(string author, string itemName, BillingEnums.ePurchaseItemTypes type)
        {
            if (String.IsNullOrEmpty(itemName)) return new ItemInfoToken
                                                                        {
                                                                            IsValid = false
                                                                            ,Message = "Item name required"
                                                                        };

            try
            {
                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        var course = CourseRepository.FindCourseByUrlName(itemName, author);
                        if (course != null)
                        {
                            var token = course.CourseEntity2ItemInfoToken(UserRepository.GetById(course.AuthorUserId), IsUnderRGP(course.AuthorUserId));
                            if (token.OverviewVideoIdentifier != null)
                                token.VideoInfoToken = GetVideoInfoToken((long)token.OverviewVideoIdentifier);

                            token.IsValid = true;
                            return token;
                        }
                        return new ItemInfoToken
                        {
                            IsValid = false
                            ,Message = "Course not found"
                        };
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        var bundle = FindBundleByUrlName(author, itemName);
                        if (bundle != null)
                        {
                            bundle.IsValid = true;
                            if (bundle.OverviewVideoIdentifier != null)
                                bundle.VideoInfoToken = GetVideoInfoToken((long)bundle.OverviewVideoIdentifier);
                            return bundle;
                        }
                        return new ItemInfoToken
                        {
                            IsValid = false
                            ,Message = "Bundle not found"
                        };
                    default:
                        return new ItemInfoToken
                                    {
                                        IsValid = false
                                        ,Message = "Unknown Item Type"
                                    };
                }
            }
            catch (Exception ex)
            {
                
                Logger.Error("find item by url::" + author + "::" + itemName,ex,CommonEnums.LoggerObjectTypes.Widget);
                return new ItemInfoToken
                {
                    IsValid = false
                    ,Message = Utils.FormatError(ex)
                };
            }
        }

        public ItemInfoToken GetItemInfoToken(int id, BillingEnums.ePurchaseItemTypes type)
        {
            try
            {
                switch (type)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        var course = CourseRepository.GetById(id);
                        if (course != null)
                        {
                            var token = course.CourseEntity2ItemInfoToken(UserRepository.GetById(course.AuthorUserId), IsUnderRGP(course.AuthorUserId));
                            if (token.OverviewVideoIdentifier != null)
                                token.VideoInfoToken = GetVideoInfoToken((long)token.OverviewVideoIdentifier);
                            token.IsValid = true;
                            return token;
                        }
                        return new ItemInfoToken
                        {
                            IsValid = false
                            ,Message = "Course not found"
                        };
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        var bundleEntity = BundleRepository.GetById(id);                        
                        if (bundleEntity != null)
                        {
                            var bundle = bundleEntity.BundleEntity2ItemInfoToken(UserRepository.GetById(bundleEntity.AuthorId), IsUnderRGP(bundleEntity.AuthorId));
                            if (bundle.OverviewVideoIdentifier != null)
                                bundle.VideoInfoToken = GetVideoInfoToken((long)bundle.OverviewVideoIdentifier);
                            bundle.IsValid = true;
                            return bundle;
                        }
                        return new ItemInfoToken
                        {
                            IsValid = false
                            ,Message = "Bundle not found"
                        };
                    default:
                        return new ItemInfoToken
                                    {
                                        IsValid = false
                                        ,Message = "Unknown Item Type"
                                    };
                }
            }
            catch (Exception ex)
            {
                
                Logger.Error("find item by id::",ex,id,CommonEnums.LoggerObjectTypes.Widget);
                return new ItemInfoToken
                {
                    IsValid = false
                    ,Message = Utils.FormatError(ex)
                };
            }
        }

        public ItemInfoToken GetItemInfoTokenByPriceLineId(int lineId, out string error)
        {
            error = string.Empty;
            try
            {
                var priceLineEntity = PriceListRepository.GetById(lineId);

                if (priceLineEntity == null)
                {
                    error = "Price list entity not found";
                    return null;
                }

                var priceToken = priceLineEntity.Entity2PriceLineDto(GetCurrencyDto(priceLineEntity.CurrencyId));

                return GetItemInfoToken(priceToken.ItemId, priceToken.ItemType);
            }
            catch (Exception ex)
            {
                error = Utils.FormatError(ex);

                Logger.Error("GetItemInfoTokenByPriceLineId " + lineId, lineId, ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }

        public ItemAccessStateToken GetItemAccessState4User(int? userId, int id, BillingEnums.ePurchaseItemTypes type)
        {
            return _itemServices.GetItemAccessState4User(userId, id, (byte) type);
        }

        public ItemProductPageToken ItemInfoToken2ItemProductPageToken(ItemInfoToken token, ItemAccessStateToken accessState,string trackingId)
        {
            try
            {
                var pageToken = token.ItemInfoToken2ItemProductPageToken(accessState, trackingId, GetItemSubscribers(token.ItemId, token.ItemType));

                if (!pageToken.IsFreeItem)
                {
                    var currencyId = String.IsNullOrEmpty(trackingId) ? DEFAULT_CURRENCY_ID : GetStoreCurrencyByTrackingId(trackingId);

                    var prices = GetStoreItemPrices(token.ItemId, token.ItemType, currencyId);

                    if (!prices.Any()) prices = GetAllItemPrices(token.ItemId, token.ItemType);

                    if (!prices.Any()) prices = new List<PriceLineDTO>();

                    pageToken.PriceLines = prices;

                    pageToken.IsItemUnderRGP = token.IsAuthorUnderRGP && prices.Any(x => x.PriceType == BillingEnums.ePricingTypes.ONE_TIME);// && (String.IsNullOrEmpty(trackingId)  || x.Currency.CurrencyId == currencyId));
                    
                    foreach (var p in pageToken.PriceLines)
                        p.IsItemUnderRGP = pageToken.IsItemUnderRGP && p.PriceType == BillingEnums.ePricingTypes.ONE_TIME;
                }
                else
                {
                    pageToken.PriceLines = GetItemFreePrice(token.ItemId, token.ItemType);
                }

                switch (token.ItemType)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        pageToken.Contents = _itemServices.GetCourseChaptersList(token.ItemId);
                        break;
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        pageToken.BundleDetails = GetBundleDetailsToken(token.ItemId,trackingId);
                        break;
                }

                return pageToken;
            }
            catch (Exception ex)
            {
                Logger.Error("create item product page token",ex,token.ItemId,CommonEnums.LoggerObjectTypes.Widget);
                return new ItemProductPageToken
                {
                    IsValid = false
                    ,Message = Utils.FormatError(ex)
                };
            }
        }

        public ItemViewerPageToken ItemInfoToken2ItemViewerPageToken(ItemInfoToken token, ItemAccessStateToken accessState, string trackingId)
        {
            try
            {
                var pageToken = token.ItemInfoToken2ItemViewerPageToken(accessState,trackingId);

                pageToken.HasCertificateOnComplete = _HasCertificateOnComplete(token.ItemId);

                switch (token.ItemType)
                {
                    case BillingEnums.ePurchaseItemTypes.COURSE:
                        //TODO temp for initializing of course classrooms
                        #region class room init

                        if (pageToken.ClassRoomId == null)
                        {
                            var roomId = _FindAuthorClassRoom(token.ItemName, token.Author.UserId);
                            if (roomId == null)
                            {
                                string error;
                                var room = new DiscussionClassRoomDTO
                                {
                                    AuthorId = token.Author.UserId
                                    ,Name    = token.ItemName
                                };

                                _SaveClassRoom(ref room, token.Author.UserId, CurrentUserId, out error);

                                if (room.RoomId >= 0)
                                {
                                    pageToken.ClassRoomId = room.RoomId;
                                    CourseRepository.UnitOfWork.CommitAndRefreshChanges();
                                    token.ClassRoomId = room.RoomId;
                                }
                            }
                            else
                            {
                                pageToken.ClassRoomId = roomId;
                                CourseRepository.UnitOfWork.CommitAndRefreshChanges();
                                token.ClassRoomId = roomId;
                            }
                        }
                        #endregion

                        #region set properties
                        #region old
                        //pageToken.Contents = _itemServices.GetCourseChaptersList(token.ItemId);

                        ////add quizzes
                        //var checkChapterAvailability = false;
                        //var quizzes = _quizWidgetServices.GetUserCourseQuizzes(token.ItemId,CurrentUserId).Where(x=>x.IsActive).OrderBy(x=>x.AvailableAfter).ToList();
                        //foreach (var quiz in quizzes)
                        //{

                        //    if (quiz.AvailableAfter != null)
                        //    {
                        //        checkChapterAvailability = true;

                        //        var index = pageToken.Contents.FindIndex(a => a.type == CourseEnums.eContentTreeViewItemType.chapter && a.index == ((int)quiz.AvailableAfter - 1));

                        //        index = index < 0 ? pageToken.Contents.Count - 1 : index + 1;

                        //        pageToken.Contents.Insert(index, quiz.Quiz2ContentTreeViewItemDto());
                        //    }
                        //    else
                        //    {
                        //        pageToken.Contents.Insert(pageToken.Contents.Count - 1, quiz.Quiz2ContentTreeViewItemDto());
                        //    }
                        //}

                        //if (checkChapterAvailability)
                        //{
                        //    var q = quizzes;

                        //    var requiredQuiz = q.Where(x=>x.AvailableAfter != null && x.IsMandatory && !x.Passed).OrderBy(x=>x.AvailableAfter).FirstOrDefault();

                        //    if (requiredQuiz != null)
                        //    {
                        //        // ReSharper disable once PossibleInvalidOperationException
                        //        var lastAllowedChapter = (int)requiredQuiz.AvailableAfter;

                        //        var chaps = pageToken.Contents.Where(x=>x.type == CourseEnums.eContentTreeViewItemType.chapter && x.index > lastAllowedChapter-1).ToList();

                        //        foreach (var chap in chaps)
                        //        {
                        //            chap.available = false;
                        //        }

                        //        var nonAvailableQuizzes = q.Where(x => x.AvailableAfter == null || (x.AvailableAfter != null && x.AvailableAfter > lastAllowedChapter)).Select(x => x.CourseQuizId).ToList();

                        //        var qz =  pageToken.Contents.Where(x=>x.type == CourseEnums.eContentTreeViewItemType.quiz && x.quizId != null && nonAvailableQuizzes.Contains((Guid)x.quizId)).ToList();

                        //        foreach (var quiz in qz)
                        //        {
                        //            quiz.available = false;
                        //        }
                        //    }
                        //}

                        ////add g2t assets
                        //var assets = _itemServices.GetCourseG2TAssetsList(token.ItemId);

                        //foreach (var asset in assets)
                        //{
                        //    pageToken.Contents.Add(asset);
                        //} 
                        #endregion

                        pageToken.Contents = GetCourseContentsList(token.ItemId);

                        pageToken.VideosNavigation = pageToken.Contents.ChapterTreeList2VideoNavigation();


                        //assets


                        var watchEntity = UserCourseWatchStateRepository.Get(x => x.UserId == CurrentUserId && x.CourseId == token.ItemId);

                        if (watchEntity == null) return pageToken;

                        pageToken.LastChapterId = watchEntity.LastChapterID ?? -1;
                        pageToken.LastVideoId = watchEntity.LastVideoID ?? -1;
                        #endregion
                        break;
                    case BillingEnums.ePurchaseItemTypes.BUNDLE:
                        pageToken.BundleDetails = GetBundleDetailsToken(token.ItemId,trackingId);
                        break;
                }
                
                return pageToken;
            }
            catch (Exception ex)
            {
                Logger.Error("create item viewer page token", ex, token.ItemId, CommonEnums.LoggerObjectTypes.Widget);
                return new ItemViewerPageToken
                {
                    IsValid = false
                    ,Message = Utils.FormatError(ex)
                };
            }
        }

        public List<ContentTreeViewItemDTO> GetCourseContentsList(int itemId)
        {
            try
            {
                var contents =  _itemServices.GetCourseChaptersList(itemId);

                //add quizzes
                var checkChapterAvailability = false;
                var quizzes = _quizWidgetServices.GetUserCourseQuizzes(itemId, CurrentUserId).Where(x => x.Status == QuizEnums.eQuizStatuses.PUBLISHED && x.IsAttached).OrderBy(x => x.AvailableAfter).ToList();
                foreach (var quiz in quizzes)
                {

                    if (quiz.AvailableAfter != null)
                    {
                        checkChapterAvailability = true;

                        var index = contents.FindIndex(a => a.type == CourseEnums.eContentTreeViewItemType.chapter && a.index == ((int)quiz.AvailableAfter - 1));

                        index = index < 0 ? contents.Count - 1 : index + 1;

                        contents.Insert(index, quiz.Quiz2ContentTreeViewItemDto());
                    }
                    else
                    {
                        contents.Insert(contents.Count - 1, quiz.Quiz2ContentTreeViewItemDto());
                    }
                }

                if (!checkChapterAvailability) return contents;


                var q = quizzes;

                var requiredQuiz = q.Where(x => x.AvailableAfter != null && x.IsMandatory && !x.Passed).OrderBy(x => x.AvailableAfter).FirstOrDefault();

                if (requiredQuiz == null) return contents;


                // ReSharper disable once PossibleInvalidOperationException
                var lastAllowedChapter = (int)requiredQuiz.AvailableAfter;

                var chaps = contents.Where(x => x.type == CourseEnums.eContentTreeViewItemType.chapter && x.index > lastAllowedChapter - 1).ToList();

                foreach (var chap in chaps)
                {
                    chap.available = false;
                }

                var nonAvailableQuizzes = q.Where(x => x.AvailableAfter == null || (x.AvailableAfter != null && x.AvailableAfter > lastAllowedChapter)).Select(x => x.QuizId).ToList();

                var qz = contents.Where(x => x.type == CourseEnums.eContentTreeViewItemType.quiz && x.quizId != null && nonAvailableQuizzes.Contains((Guid)x.quizId)).ToList();

                foreach (var quiz in qz)
                {
                    quiz.available = false;
                }

                //add g2t assets
                //contents.AddRange(_itemServices.GetCourseG2TAssetsList(itemId));
                
                return contents;

            }
            catch (Exception ex)
            {
                Logger.Error("GetCourseContentsList", ex, itemId, CommonEnums.LoggerObjectTypes.Widget);
                return new List<ContentTreeViewItemDTO>();
            }
        }

        public int GetItemSubscribers(int id, BillingEnums.ePurchaseItemTypes type)
        {
            switch (type)
            {
                case BillingEnums.ePurchaseItemTypes.COURSE:
                    return UserCourseRepository.Count(x => x.CourseId == id);
                case BillingEnums.ePurchaseItemTypes.BUNDLE:
                    return UserBundleRepository.Count(x => x.BundleId == id);
                default:
                    return 0;
            }
        }


        public List<ReviewDTO> GetItemReviews(int id)
        {
            try
            {
                return CourseRepository.GetAllCourseReviews(id).Select(x => x.UserReviewEntity2ReviewDTO()).OrderByDescending(x => x.Date).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Widget::Get Item Reviews", id, ex, CommonEnums.LoggerObjectTypes.Widget);

                return null;
            }
        }
        #endregion

        #region IWidgetWebStoreServices implementation
        public int? ValidateTrackingId(string trackingId)
        {
            return FindStoreId(trackingId);
        }

        public short GetStoreCurrencyByTrackingId(string trackingId)
        {
            return _GetStoreCurrencyByTrackingId(trackingId);
        }
        #endregion

        #endregion

    }
}
