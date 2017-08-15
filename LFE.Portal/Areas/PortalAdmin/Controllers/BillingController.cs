using System.IO;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Portal.Areas.AuthorAdmin.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;


namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    public class BillingController : BaseController
    {
        private readonly IPaypalServices _paypalServices;
        private readonly IPaypalManageServies _paypalManageServices;
        private readonly IBillingManageServices _billingManageServices;
        private readonly IUserSubscriptionsManageServices _subscriptionsManageServices;
        private readonly IS3Wrapper _s3Wrapper;
        public BillingController()
        {
            _paypalManageServices        = DependencyResolver.Current.GetService<IPaypalManageServies>();
            _billingManageServices       = DependencyResolver.Current.GetService<IBillingManageServices>();
            _paypalServices              = DependencyResolver.Current.GetService<IPaypalServices>();
            _subscriptionsManageServices = DependencyResolver.Current.GetService<IUserSubscriptionsManageServices>();
            _s3Wrapper                   = DependencyResolver.Current.GetService<IS3Wrapper>();
        }

        public ActionResult SaleOrdersReport()
        {
            return View();
        }

        public ActionResult ScheduledSubscriptionPayments()
        {

            return View();
        }

        public ActionResult CompletedPayments()
        {

            return View();
        }

        public ActionResult TransactionsReport()
        {
            return View(_billingManageServices.GetTransactionFiltersLov());
        }

        public ActionResult _RefundOrderLinePayment(int? paymentId)
        {
            return PartialView("Billing/_RefundOrderLinePayment",_billingManageServices.GetPaymentRefundDTO(paymentId));
        }

        public ActionResult _CustomTrxWindow(int? id)
        {
            return PartialView("Billing/_CustomTrxWindow", _billingManageServices.GetCustomTrxToken(id));
        }

        [HttpPost]
        public JsonResult CreateCustomTrx(CustomTrxDTO token)
        {
            string error;
            var result = _billingManageServices.SaveCustomTrx(token, out error);

            return Json(new JsonResponseToken {success = result, error = error}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DoRefund(RefundOrderLinePaymentDTO token)
        {
            string error;
            var result = _paypalManageServices.RefundPaymentTransaction(token, out error) && _billingManageServices.BlockUserCourseAccessAndCancelOrderOnRefund(token,out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DoPayment(int? id)
        {
            if (id == null) return ErrorResponse("id required");
            string error;
            var result = _paypalServices.ExecuteSubscriptionScheduledPaymentWithStoredCreditCard((int)id, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChargeScheduledPayments(DateTime? sd)
        {
            string error;
            var result = _paypalServices.ExecuteSubscriptionScheduledPaymentWithStoredCreditCard(sd, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,
                error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CancelSubscription(int? id, BillingEnums.ePaymentMethods paymentMethod)
        {
            if (id == null) return ErrorResponse("id required");
            string error;
            var result = (paymentMethod == BillingEnums.ePaymentMethods.Saved_Instrument || _paypalManageServices.CancelSubscription((int)id, out error) ) && _billingManageServices.BlockUserCourseAccessAndCancelOrderOnSubscriptionCancel((int) id, out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTransactionFees()
        {
            string error;
            int found;
            int updated;
            var result = _paypalManageServices.UpdateMerchantTrxFees(out found,out updated,out error);

            return Json(new JsonResponseToken
            {
                success = result
                ,message =  result ? string.Format("{0} transactions found. {1} updated",found,updated) : string.Empty
                ,error = error
            }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult SearchTransactions([DataSourceRequest] DataSourceRequest request
                                                                                , int periodSelectionKind
                                                                                , int? authorId = null
                                                                                , int? courseId = null
                                                                                , int? bundleId = null
                                                                                , int? buyerId = null
                                                                                , BillingEnums.eTransactionTypes? type = null)
        {

            var kind = periodSelectionKind.ToPeriodSelectionKind(); 

            var list = _billingManageServices.SearchTransactions(kind, authorId, buyerId, courseId,bundleId, type);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOrders([DataSourceRequest] DataSourceRequest request, 
                                        int periodSelectionKind,
                                        int? buyerId = null,
                                        int? sellerId=null,
                                        int? courseId=null,
                                        int? bundleId=null,
                                        int? storeId=null,
                                        bool? isSubscription = null,
                                        BillingEnums.eOrderStatuses? status = null)
        {

            var kind = periodSelectionKind.ToPeriodSelectionKind();

            var list = _billingManageServices.SearchOrders(kind,buyerId,sellerId,courseId,bundleId,storeId,isSubscription,status);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOrderLines([DataSourceRequest] DataSourceRequest request,Guid OrderId)
        {

            var list = _billingManageServices.GetOrderLines(OrderId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOrderLinePayments([DataSourceRequest] DataSourceRequest request, int LineId)
        {

            var list = _billingManageServices.GetOrderLinePayments(LineId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetSellerPayments([DataSourceRequest] DataSourceRequest request, int sellerId, int year, int month, short currencyId)
        {

            var list = _billingManageServices.GetSellerPayments(sellerId,year,month,currencyId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAffiliatePayments([DataSourceRequest] DataSourceRequest request, int sellerId, int year, int month, short currencyId)
        {

            var list = _billingManageServices.GetAffiliatePayments(sellerId, year, month, currencyId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRefundProgramPayments([DataSourceRequest] DataSourceRequest request, int sellerId, int year, int month, short currencyId, bool released)
        {

            var list = _billingManageServices.GetRefundProgramPayments(sellerId, year, month, currencyId,released);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSellerRefunds([DataSourceRequest] DataSourceRequest request, int sellerId, int year, int month, short currencyId)
        {

            var list = _billingManageServices.GetSellerRefunds(sellerId, year, month,currencyId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetPaymentRefunds([DataSourceRequest] DataSourceRequest request, int PaymentId)
        {

            var list = _billingManageServices.GetOrderLinePaymentRefunds(PaymentId);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOrderLineTransactions([DataSourceRequest] DataSourceRequest request, int LineId)
        {

            var list = _billingManageServices.GetOrderLineTransactions(LineId).OrderBy(x=>x.PaymentNumber).ThenByDescending(x=>x.TrxDate).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCompletedPayments([DataSourceRequest] DataSourceRequest request, int periodSelectionKind, BillingEnums.ePaymentTypes? type, int? orderNum)
        {
            var kind = periodSelectionKind.ToPeriodSelectionKind();
            var list = _billingManageServices.GetCompletedPayments(kind,type,orderNum).OrderByDescending(x => x.CompletedDate).ToArray();

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetScheduledPayments([DataSourceRequest] DataSourceRequest request, bool? onlyCC = null)
        {

            var list = _subscriptionsManageServices.SearchPayments( ReportEnums.ePeriodSelectionKinds.all,
                                                                    orderId:null,
                                                                    orderNum:null,
                                                                    sellerUserId:null,
                                                                    buyerUserId:null,
                                                                    courseId:null,
                                                                    bundleId:null,
                                                                    paymentStatus:BillingEnums.ePaymentStatuses.SCHEDULED,
                                                                    orderStatus:BillingEnums.eOrderStatuses.ACTIVE, 
                                                                    onlySavedCards:onlyCC);

            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        //public ActionResult PaypalSubscriptionTooltip(string id)
        //{
        //    string error;
        //    var token = _paypalManageServices.GetSubscriptionProfileDetails(id,out error);
        //    if (token == null) return Content(String.Format("<h2>{0}</h2>",error));
        //    return PartialView("Billing/_PaypalSubscriptionInfo",token);
        //}

        //public ActionResult SubscriptionPaymentsTooltip(Guid? id)
        //{            
        //    var list = _subscriptionsManageServices.GetSubscriptionPayments(id);
        //    return PartialView("Billing/_SubscriptionPayments", list);
        //}


        public ActionResult MonthlyStatementTab(AuthorStatementRequestToken request)
        {
            var token = _billingManageServices.GetAuthorMonthlyStatement(request);
            return PartialView("Billing/_MonthlyStatementTab",token);
        }

        public ActionResult MonthlyStatement(AuthorStatementRequestToken request)
        {
            var token = _billingManageServices.GetAuthorMonthlyStatement(request);
            return View(token);
        }

        public ActionResult SendMonthlyStatement(AuthorStatementRequestToken request,string img)
        {
            var token = _billingManageServices.GetAuthorMonthlyStatement(request);

            if (!token.IsValid) return ErrorResponse(token.Message);

            string error;

            var imageBytes = Convert.FromBase64String(img.Substring(img.IndexOf(',') + 1));

            var stream = new MemoryStream(imageBytes.Length);
            
            stream.Write(imageBytes, 0, imageBytes.Length);

            var fileName = FileEnums.eFileOwners.Author + "/Charts/" + ShortGuid.NewGuid() + ".png";
            var etag = _s3Wrapper.Upload(fileName, "image/png", stream, out error);            
            var url = string.IsNullOrEmpty(etag) ? "#" : _s3Wrapper.GetFileURL(fileName);

            token.ChartImageUrl = url;

            var body = RenderRazorViewToString("MonthlyStatementEmail", token);
            
            var result = _billingManageServices.SendAuthorMonthlyStatement(token,body,out error);

            return Json(new JsonResponseToken{success = result,error = error}, JsonRequestBehavior.AllowGet);
        }
    }
}