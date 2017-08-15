using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using LFE.Portal.Areas.UserPortal.Helpers;
using LFE.Portal.Areas.UserPortal.Models;

namespace LFE.Portal.Areas.UserPortal.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserNotificationServices _notificationServices;
        private readonly IUserPortalServices _userPortalServices;
        private readonly IBillingManageServices _billingManageServices;
        //private readonly IUserSubscriptionsManageServices _subscriptionsManageServices; 
        public UserController()
        {
            _notificationServices = DependencyResolver.Current.GetService<IUserNotificationServices>();
            _userPortalServices = DependencyResolver.Current.GetService<IUserPortalServices>();
            _billingManageServices = DependencyResolver.Current.GetService<IBillingManageServices>();
            //_subscriptionsManageServices = DependencyResolver.Current.GetService<IUserSubscriptionsManageServices>();
        }

        #region views
        public ActionResult UserProfile(int? id)
        {

            UserProfilePageToken token;

            if (id == null)
            {
                token = new UserProfilePageToken
                {
                    IsValid = false
                    ,Message = "userId required"
                };
            }
            else
            {
                token = GetUserProfileDto((int) id, 4);
            }

            return View("Profile",token);
        }      

        public ActionResult _DiscussionUserProfile(int id, int pageSize)
        {
            return PartialView("User/_DiscussionUserProfile", GetUserProfileDto(id, pageSize));
        }

        public ActionResult _UserPurchases()
        {
            return PartialView("User/_UserPurchases");
        }
        #endregion

        #region api
        [AcceptVerbs("Get", "Post")]
        public ActionResult GetUserNotifications([DataSourceRequest] DataSourceRequest request)
        {
            var notifications = CurrentUserId >= 0 ? _notificationServices.GetUserNotifications(CurrentUserId).OrderByDescending(x => x.AddOn).ToArray() : new UserNotificationDTO[0];

            return Json(notifications.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserCourses()
        {
            var list = _userPortalServices.GetLearnerCourses(CurrentUserId, CurrentUserId).Select(x => x.SetCoursePageUrl(null)).OrderBy(x => x.Name).ToArray();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetUserPurchases([DataSourceRequest] DataSourceRequest request)
        {
            var list = _userPortalServices.GetUserPurchases(CurrentUserId).Where(x => x.LineType == BillingEnums.eOrderLineTypes.SALE || x.LineType == BillingEnums.eOrderLineTypes.RENTAL).ToList();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetSubscriptions([DataSourceRequest] DataSourceRequest request)
        {

            var list = _userPortalServices.GetUserPurchases(CurrentUserId).Where(x => x.LineType == BillingEnums.eOrderLineTypes.SUBSCRIPTION).ToList();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOrderLinePayments([DataSourceRequest] DataSourceRequest request, int LineId)
        {

            var list = _billingManageServices.GetOrderLinePayments(LineId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetPaymentRefunds([DataSourceRequest] DataSourceRequest request, int PaymentId)
        {

            var list = _billingManageServices.GetOrderLinePaymentRefunds(PaymentId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region invoices
        //public ActionResult CourseInvoice(int transactionId)
        //{
        //    var model = _billingManageServices.GetTransactionInvoice(transactionId);
        //    return View(model);

        //}
        #endregion


        #region post
        public void UpdateUserNotificationStatus()
        {
            if (CurrentUserId < 0) return;
            _notificationServices.UpdateUserNotificationStatus(CurrentUserId);
        } 
        #endregion         

        #region Notification link for wordpress site
        public ActionResult _UserNotificationLink()
        {
            var newNotifications = CurrentUserId < 0 ? 0 : _notificationServices.GetUserUnreadNotificationsCount(CurrentUserId);
            return PartialView(newNotifications);
        }

        public int GetUserUnreadCount()
        {
            return CurrentUserId < 0 ? 0 : _notificationServices.GetUserUnreadNotificationsCount(CurrentUserId);
        }
        #endregion
    }
}
