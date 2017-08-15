using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.AuthorAdmin.Helpers;
using LFE.Portal.Areas.AuthorAdmin.Models;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    [Authorize]
    public class WebStoreController : BaseController
    {
        private readonly IWebStoreServices _webStoreServices;
        private readonly IAuthorAdminCategoryServices _categoryServices;
        private readonly IAuthorAdminCourseServices _courseServices;
        private readonly IAuthorAdminServices _authorAdminServices;

        public WebStoreController()
        {
            _webStoreServices = DependencyResolver.Current.GetService<IWebStoreServices>();
            _categoryServices = DependencyResolver.Current.GetService<IAuthorAdminCategoryServices>();
            _courseServices = DependencyResolver.Current.GetService<IAuthorAdminCourseServices>();
            _authorAdminServices = DependencyResolver.Current.GetService<IAuthorAdminServices>();

           
        }       

        #region views
        public ActionResult Report()
        {
            return View();
        }

        public ActionResult EditStore(Guid id)
        {
            #region validate request
            var user = GetCurrentUser();

            var isBelong2Owner = _webStoreServices.ValidateOwnerStoreByUid(user.userId, id);

            if (!isBelong2Owner)
            {
                return RedirectToAction("NonAuthorized", "Error");
            }
            #endregion

            var store =  _webStoreServices.FindStoreByUid(user.userId, id);

            var token = new EditWebStorePageToken
                {
                     user     = user
                    ,store    = store
                    ,mode     = store.id < 0 ? CommonEnums.ePageMode.insert : CommonEnums.ePageMode.edit
                    ,title    = store.id < 0 ? "Create New WebStore" : "Edit " + store.name
                };

            return View(token);
        }

        public ActionResult EditStoreByTrackId(string id)
        {
            #region validate request
            var user = GetCurrentUser();

            var isBelong2Owner = _webStoreServices.ValidateOwnerStoreByTrackingId(user.userId, id);

            if (!isBelong2Owner)
            {
                return RedirectToAction("NonAuthorized", "Error");
            }
            #endregion

            var store =  _webStoreServices.FindStoreByTrackingId(user.userId, id);

            var token = new EditWebStorePageToken
                {
                     user     = user
                    ,store    = store
                    ,mode     = store.id < 0 ? CommonEnums.ePageMode.insert : CommonEnums.ePageMode.edit
                    ,title    = store.id < 0 ? "Create New WebStore" : "Edit " + store.name
                };

            return View("EditStore",token);
        }

        public ActionResult AdminUserWebStoreManage(int? userId)
        {
            if (!IsAdminRequestAuthorized())
            {
                return RedirectToAction("NonAuthorized", "Error");
            }

            if (userId == null) return Content("<2>UserId required</h2>");

            var user = BaseAuthorServices.FindUsers(userId, null, null).FirstOrDefault();

            if (user == null) return Content("<2>User Not found</h2>");

            return View(user);
        }

        public ActionResult _AdminUserWebStoreReport(int? userId)
        {
            if (!IsAdminRequestAuthorized())
            {
                return RedirectToAction("NonAuthorized", "Error");
            }

            if (userId == null) return Content("<2>UserId required</h2>");

            var user = BaseAuthorServices.FindUsers(userId, null, null).FirstOrDefault();

            if (user == null) return Content("<2>User Not found</h2>");

            return PartialView("WebStore/_AdminUserWebStoreReport", user);

        }

        public ActionResult AdminEditStore(Guid id,int userId)
        {
            #region validate request
            if (!IsAdminRequestAuthorized())
            {
                return RedirectToAction("NonAuthorized", "Error");
            }
            #endregion

            var user = BaseAuthorServices.FindUsers(userId, null, null).FirstOrDefault();

            var store =  _webStoreServices.FindStoreByUid(userId, id);

            if (user != null)
            {
                var token = new EditWebStorePageToken
                {
                    user        = user
                    ,ownerUserId = user.userId
                    ,store       = store
                    ,mode        = store.id < 0 ? CommonEnums.ePageMode.insert : CommonEnums.ePageMode.edit
                    ,title       = store.id < 0 ? "Create New WebStore" : "Edit " + store.name
                };

                return View("AdminEditUserWebStore",token);
            }


            return View("AdminEditUserWebStore", new EditWebStorePageToken {title = "User not found"});
        }

        public ActionResult EditLfeStore()
        {
            #region validate request
            if (!IsAdminRequestAuthorized())
            {
                return RedirectToAction("NonAuthorized", "Error");
            }
            #endregion

            var user = BaseAuthorServices.FindUsers(Constants.LFE_MAIN_STORE_OWNER_ID, null, null).FirstOrDefault();
            var store = _webStoreServices.FindStoreByTrackingId(Constants.LFE_MAIN_STORE_OWNER_ID, Constants.LFE_MAIN_STORE_TRACKING_ID);

            var token = new EditWebStorePageToken
            {
                user        = user
                // ReSharper disable once PossibleNullReferenceException
                ,ownerUserId = user.userId
                ,store       = store
                ,mode        = store.id < 0 ? CommonEnums.ePageMode.insert : CommonEnums.ePageMode.edit
                ,title       = store.id < 0 ? "Create New WebStore" : "Edit " + store.name
            };

            return View("LfeMainStoreManage",token);            
        }

        public ActionResult SalesReport()
        {
            return View();
        }

        public ActionResult _SalesReport()
        {

            return PartialView("SalesReports/WebStore/Owner/_OwnerSalesReport");
        }

        #region tabs partial
        public ActionResult StoreDetails(int id,Guid Uid, int? ownerUserId=null)
        {
            var token = id < 0 ? new WebStoreEditDTO
            {
                Uid = Uid
            } : _webStoreServices.GetStoreEditDTO(id);

            token.OwnerUserId = ownerUserId ?? CurrentUserId;
         
            return PartialView("WebStore/_StoreDetails", token);
        }

        public ActionResult StoreContent(int id, string name)
        {
            return PartialView("WebStore/_StoreContents", new BaseEntityDTO
            {
                id = id
                ,name = name
            });
        }

        public ActionResult _CategoryContents(int id)
        {
            var token = new WebstoreCategoryContentsToken
            {
                CategoryId = id
            };
            return PartialView("WebStore/_CategoryContents", token);
        }

        public ActionResult StoreEmbed(int id)
        {
            return PartialView("WebStore/_StoreEmbed", new WebStoreEmbedToken
            {
                WebStoreId = id
                ,TrackingID =  _webStoreServices.GetStoreEditDTO(id).TrackingID
            });
        }
        
        public ActionResult StoreSalesReports(int id)
        {
            return PartialView("SalesReports/WebStore/Store/_StoreSalesReport", new BaseEntityDTO
            {
                id = id
            });
        }

        public ActionResult CategoryEditForm(int id, int storeId)
        {
            var token = _webStoreServices.GetCategoryEditDTO(id, storeId);
            
            return PartialView("WebStore/_EditCategory",token);
        }

        public ActionResult CategoryContentEditForm(int id, int contentCategoryId, eStoreContentKinds contentKind)
        {
            if (contentCategoryId <= 0) return Content(" invalid request data");

            switch (contentKind)
            {
                case eStoreContentKinds.Single:
                    var dto = _webStoreServices.GetCategoryItemEditDto(id, contentCategoryId);                    
                    return id< 0 ? PartialView("WebStore/_AddSingleItem",dto) : PartialView("WebStore/_EditSingleItem",dto);
                case eStoreContentKinds.LfeCategory:
                    var addByCategoryToken = new WebStoreAddByCategoryToken
                    {
                        WebStoreCategoryId = contentCategoryId                        
                    };
                    return PartialView("WebStore/_AddByLfeCategory",addByCategoryToken);
                case eStoreContentKinds.Author:
                    var addByAuthorToken = new WebStoreAddByAuthorToken
                    {
                        WebStoreCategoryId = contentCategoryId                        
                    };
                    return PartialView("WebStore/_AddByAuthor",addByAuthorToken);
                case eStoreContentKinds.My:
                    var list = _webStoreServices.GetAuthorItems(CurrentUserId, contentCategoryId, true).OrderBy(x => x.name).ToArray();
                    var addByCurrent = new WebStoreAddByCurrentUserToken
                    {
                        WebStoreCategoryId = contentCategoryId
                        ,AuthorId = CurrentUserId
                        ,ItemListToken = new ItemListToken
                        {
                            ItemList = list
                        }
                    };
                    return PartialView("WebStore/_AddByCurrentUser",addByCurrent);
            }

            return Content(" invalid request data");
        }

        public ActionResult GetCategoryItemsListPartial(int categoryId, int webCategoryId)
        {
            var list = _webStoreServices.GetLfeCategoryItems(categoryId, webCategoryId,true).OrderByDescending(x => x.type).ThenBy(x=>x.name).ToArray();
            return PartialView("WebStore/_ItemsList", new ItemListToken
            {
                ItemList = list
            });
        }

        public ActionResult GetAuthorItemsListPartial(int authorId, int webCategoryId)
        {
            var list = _webStoreServices.GetAuthorItems(authorId, webCategoryId,true).OrderBy(x => x.name).ToArray();
            return PartialView("WebStore/_ItemsList", new ItemListToken
            {
                ItemList = list
            });
        }
        #endregion
        #endregion

        #region api
        /// <summary>
        /// get users stores for portal admin
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserStores([DataSourceRequest] DataSourceRequest request,int? userId)
        {
            var list = new WebStoreGridDTO[0];

            if (userId==null) return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            list = _webStoreServices.GetOwnerStores((int) userId).OrderBy(x => x.AddOn).ThenBy(x => x.Name).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// get stores report
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStores([DataSourceRequest] DataSourceRequest request)
        {
            var list = new WebStoreGridDTO[0];

            if (CurrentUserId < 0) return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            list = _webStoreServices.GetOwnerStores(CurrentUserId).OrderBy(x => x.AddOn).ThenBy(x=>x.Name).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOwnerStoresLOV()
        {
            var list = new WebStoreGridDTO[0];

            if (CurrentUserId < 0) return Json(list, JsonRequestBehavior.AllowGet);

            list = _webStoreServices.GetOwnerStores(CurrentUserId).OrderBy(x => x.Name).ToArray();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        
        //category
        /// <summary>
        /// get store categories in content tab
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStoreCategoriesList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _webStoreServices.GetStoreCategories(id).OrderBy(x => x.index).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStoreEditCategoriesList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _webStoreServices.GetStoreEditCategories(id).OrderBy(x => x.OrderIndex).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAllAvailableItems([DataSourceRequest] DataSourceRequest request,string name,int? authorId)
        {
            var list = _webStoreServices.GetAllAvailableItems(CourseEnums.CourseStatus.Published,name,authorId).OrderBy(x => x.ItemName).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult FindAvailableItems(string name)
        {
            var list = _webStoreServices.GetAllAvailableItems(CourseEnums.CourseStatus.Published, name).OrderBy(x => x.ItemName).ToArray();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        //course
        /// <summary>
        /// get related to category courses in content tab
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCategoryItemsList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _webStoreServices.GetCategoryItems(id).OrderBy(x => x.index).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStoreCategoryItemsList([DataSourceRequest] DataSourceRequest request, int id)
        {
            var list = _webStoreServices.GetCategoryItemsList(id).OrderBy(x => x.Index).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// get categories LOV for Single Course selection tab
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCategoriesLOV()
        {
            return Json(_categoryServices.GetCategoriesLOV().ToArray(),JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// get authors for add by author tag
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAuthorsLOV()
        {
            return Json(_authorAdminServices.GetAuthorsLOV(true).ToArray(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// get category courses
        /// </summary>
        /// <param name="id"></param>
        /// <param name="webCatId"></param>
        /// <returns></returns>
        public JsonResult GetCategoryAvailableItemsLOV(int id, int webCatId)
        {
            return Json(_webStoreServices.GetLfeCategoryItems(id,webCatId,false).OrderByDescending(x=>x.type).ThenBy(x=>x.name).ToArray(), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// get course details on course selection event for single course default values
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetItemDetails(int id,BillingEnums.ePurchaseItemTypes type)
        {
            string error;
            var token = _courseServices.GetBaseItemListDto(id, type, out error);
            return Json(new JsonResponseToken
            {
                success = token != null
                ,result = token
                ,error = error
            }, JsonRequestBehavior.AllowGet);           
        }

       
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOwnerStoresSales([DataSourceRequest] DataSourceRequest request, int? periodSelectionKind)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _webStoreServices.GetOwnerStoreSales(CurrentUserId, kind,null,null,null);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStoreOnetimeSales([DataSourceRequest] DataSourceRequest request, int? periodSelectionKind, int storeId, bool onlyMy = false)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _webStoreServices.GetStoreSales(storeId, CurrentUserId, kind, onlyMy ? CurrentUserId : (int?)null, BillingEnums.eOrderLineTypes.SALE);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStoreSubscriptionSales([DataSourceRequest] DataSourceRequest request, int? periodSelectionKind, int storeId, bool onlyMy = false)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _webStoreServices.GetStoreSales(storeId, CurrentUserId, kind, onlyMy ? CurrentUserId : (int?)null, BillingEnums.eOrderLineTypes.SUBSCRIPTION);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOwnerStoresOnetimeSales([DataSourceRequest] DataSourceRequest request, int? periodSelectionKind,int? storeId,bool onlyMy = false)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _webStoreServices.GetOwnerStoreSales(CurrentUserId, kind,onlyMy ? CurrentUserId : (int?) null,storeId,BillingEnums.eOrderLineTypes.SALE);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOwnerStoresSubscriptionSales([DataSourceRequest] DataSourceRequest request, int? periodSelectionKind, int? storeId, bool onlyMy = false)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _webStoreServices.GetOwnerStoreSales(CurrentUserId, kind, onlyMy ? CurrentUserId : (int?)null, storeId, BillingEnums.eOrderLineTypes.SUBSCRIPTION);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region post
        //store

        /// <summary>
        /// destroy from grid
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DestroyStore([DataSourceRequest] DataSourceRequest request, WebStoreGridDTO dto)
        {
            if (dto != null)
            {
                string error;
                _webStoreServices.DeleteStore(dto.StoreId, out error);
            }

            return Json(new[] { dto }.ToDataSourceResult(request, ModelState));
        }

        /// <summary>
        /// save store details
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveStoreDetails(WebStoreEditDTO dto)
        {
            if (CurrentUserId < 0) return RedirectToAction("NonAuthorized", "Error");

            if (dto != null && ModelState.IsValid)
            {
                string error;

                var isNew = dto.StoreId == -1;

                var userId = dto.OwnerUserId ?? CurrentUserId;

                var result = _webStoreServices.SaveStore(ref dto,userId, out error);

                if (dto.StoreId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

                if (isNew)
                {
                    SaveUserEvent(CommonEnums.eUserEvents.STORE_CREATED,String.Format("Store \"{0}\" created",dto.StoreName),dto.TrackingID);
                }

                return Json(new JsonResponseToken
                {
                    success = result
                   ,result = new
                       {
                           id    = dto.StoreId
                           ,name = dto.StoreName
                          // ,url  = WebHelper.PreviewCourseUrl(dto.CourseName)
                       }
                   ,error = error
                });
            }

            return Json(new JsonResponseToken
            {
                success = false
               ,error = GetModelStateError(ModelState.Values.ToList())
            });            
        }

        //category

        /// <summary>
        /// save category form
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveCategoryDetails(WebStoreCategoryEditDTO dto)
        {
            if (dto == null || !ModelState.IsValid) return Json(new JsonResponseToken
                                                                {
                                                                    success = false
                                                                    ,error = GetModelStateError(ModelState.Values.ToList())
                                                                });

            string error;

            var result = _webStoreServices.SaveCategory(ref dto, out error);

            if (dto.WebStoreId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

            return Json(new JsonResponseToken
            {
                success = result
                ,result = new
                {
                    id = dto.WebStoreCategoryId
                    ,name = dto.CategoryName
                }
                ,error = error
            });
        }

        /// <summary>
        /// save categories order on client reorder event
        /// </summary>
        /// <param name="idS"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveCategoriesOrder(int[] idS)
        {
            string error;
            var result = _webStoreServices.SaveCategoriesOrder(idS,out error);
            return Json(new JsonResponseToken
                {
                    success = result
                    ,message = "Categories order saved"
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// delete category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteCategory(int? id)
        {
            if (id == null) return ErrorResponse("categoryId missing");

            string error;

            var isDeleted = _webStoreServices.DeleteCategory((int)id, out error);

            return Json(new JsonResponseToken
            {
                 success = isDeleted
                ,result  = new { id }
                ,error   = error
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteStoreCategoryFromList([DataSourceRequest] DataSourceRequest request, WebStoreCategoryEditDTO dto)
        {
            string error;

            if (dto == null) return Json(ModelState.ToDataSourceResult());

            _webStoreServices.DeleteCategory(dto.WebStoreCategoryId, out error);

            return Json(ModelState.ToDataSourceResult());
        }

        //course

        /// <summary>
        /// save single course
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveCategoryItemDetails(WebStoreItemEditDTO dto)
        {
            if (dto == null || !ModelState.IsValid) return Json(new JsonResponseToken
                                                                {
                                                                    success = false
                                                                    ,error = GetModelStateError(ModelState.Values.ToList())
                                                                });

            string error;

            var result = _webStoreServices.SaveCategoryItem(ref dto, out error);

            if (dto.WebStoreItemId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

            return Json(new JsonResponseToken
            {
                success = result
                ,result = new
                {
                    id = dto.WebStoreItemId
                    ,name = dto.ItemName
                }
                ,error = error
            });
        }

        /// <summary>
        /// save category courses order on reorder client event
        /// </summary>
        /// <param name="idS"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveCategoryItemsOrder(int[] idS)
        {
            string error;
            var result = _webStoreServices.SaveItemOrder(idS,out error);
            return Json(new JsonResponseToken
                {
                    success = result
                    ,message = "Courses order saved"
                    ,error = error
                }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveCategoryItems(string data)
        {
            string error;
            bool result;
            try
            {
                var token = JsSerializer.Deserialize<AddItems2StoreCategoryToken>(data);

                result = _webStoreServices.AddItems2Category(token,out error);
            }
            catch (Exception ex)
            {
                result = false;
                error = Utils.FormatError(ex);
            }
            

            return Json(new JsonResponseToken {success = result,error = error}, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteCategoryItem([DataSourceRequest] DataSourceRequest request, WebStoreItemListDTO dto)
        {
            string error;

            if (dto == null) return Json(ModelState.ToDataSourceResult());

            _webStoreServices.DeleteCategoryItem(dto.WebstoreItemId, out error);

            return Json(ModelState.ToDataSourceResult());
        }

        /// <summary>
        /// delete category course
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteItem(int? id)
        {
            if (id == null) return ErrorResponse("itemId missing");

            string error;

            var isDeleted = _webStoreServices.DeleteCategoryItem((int)id, out error);

            return Json(new JsonResponseToken
            {
                 success = isDeleted
                ,result  = new { id }
                ,error   = error
            });
        }

        /// <summary>
        /// save category courses from category tab
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCategoryItems(WebStoreAddByCategoryToken dto)
        {
            if (dto == null || !ModelState.IsValid) return Json(new JsonResponseToken
                                                                {
                                                                    success = false
                                                                    ,error = GetModelStateError(ModelState.Values.ToList())
                                                                });

            string error;

            var result = _webStoreServices.AddFullLfeCategoryCourses(dto.CategoryId,dto.WebStoreCategoryId,dto.ItemList.ToList(), out error);

            //if (dto.WebStoreCourseId < 0) return ErrorResponse(error ?? "Something went wrong. Please try again");

            return Json(new JsonResponseToken
            {
                success = true
                ,result = result
                ,error = error
            });
        }

        /// <summary>
        /// save author courses from category tab
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddAuthorCourses(WebStoreAddByAuthorToken dto)
        {
            if (dto == null || !ModelState.IsValid) return Json(new JsonResponseToken
                                                                {
                                                                    success = false
                                                                    ,error = GetModelStateError(ModelState.Values.ToList())
                                                                });

            string error;

            var result = _webStoreServices.AddAuthorCourses(dto.AuthorId,dto.WebStoreCategoryId,dto.ItemList.ToList(), out error);

            return Json(new JsonResponseToken
            {
                success = true
                ,result = result
                ,error = error
            });
        }
        #endregion

        #region helpers
        public JsonResult IsTrackingIDAvailable(string TrackingID,Guid Uid)
        {
            if (_webStoreServices.ValidateTrackingByUid(TrackingID,Uid))
                return Json(true, JsonRequestBehavior.AllowGet);


            var suggestedUID = String.Format(CultureInfo.InvariantCulture, "{0} is not available.", TrackingID);


            for (var i = 1; i < 100; i++)
            {
                var altCandidate = TrackingID + i;
                
                if (!_webStoreServices.ValidateTrackingByUid(altCandidate, Uid)) continue;

                suggestedUID = String.Format(CultureInfo.InvariantCulture,"{0} is not available. Try {1}.", TrackingID, altCandidate);
                break;
            }
            return Json(suggestedUID, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsStoreNameAvailable(int StoreId, string StoreName)
        {
            if (_webStoreServices.ValidateOwnerStoreName(StoreName, CurrentUserId,StoreId))
                return Json(true, JsonRequestBehavior.AllowGet);

            var suggestedName = String.Format(CultureInfo.InvariantCulture, "{0} is not available.", StoreName);

            return Json(suggestedName, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsStoreCategoryNameAvailable(int WebStoreId, string CategoryName, int WebStoreCategoryId)
        {
            if (_webStoreServices.ValidateStoreCategoryName(CategoryName,WebStoreCategoryId,WebStoreId))
                return Json(true, JsonRequestBehavior.AllowGet);

            var suggestedName = String.Format(CultureInfo.InvariantCulture, "{0} is not available.", CategoryName);

            return Json(suggestedName, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
