using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.Core.Utils;
using LFE.DataTokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    public class PayoutController : BaseController
    {
        private readonly IPayoutServices _payoutServices;

        public PayoutController()
        {
            _payoutServices = DependencyResolver.Current.GetService<IPayoutServices>();
            
        }

        #region views
        public ActionResult PayoutMonthlyStatement()
        {
            return View();
        }

        public ActionResult PayoutExecutionReport(int? id)
        {
            return View(id);
        }
        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetPayoutExecutions([DataSourceRequest] DataSourceRequest request)
        {
            
            var list = _payoutServices.GetPayoutExecutions(null);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetPayoutStatements([DataSourceRequest] DataSourceRequest request,int execId)
        {

            var list = _payoutServices.GetPayoutStatments(execId);
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult GetMonthlyPayoutReport(int? year, int? month, int? userId)
        {
            var token = _payoutServices.GetMonthlyPayoutReport(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, userId, null);          

            return PartialView("Payout/_MonthlyPayoutCurrenciesReport", token);
        }

        public ActionResult ExportPayoutReport(int? year, int? month)
        {
            var reportYear  = year ?? DateTime.Now.Year;
            var reportMonth = month ?? DateTime.Now.Month;
            var token       = _payoutServices.GetMonthlyPayoutReport(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, null, null);

            var output = new MemoryStream();
            var writer = new StreamWriter(output, Encoding.UTF8);

            writer.Write("Name,");
            writer.Write("Payout Method,");
            writer.Write("Email,");
            writer.Write("Total");

            writer.WriteLine();

            foreach (var currencySummary in token.CurrencyRows)
            {
                foreach (var payment in currencySummary.Rows.OrderBy(p => p.PayoutSettings.PayoutType))
                {
                    var ps = payment.PayoutSettings;

                    writer.Write(payment.Seller.FullName);
                    writer.Write(",");
                    writer.Write(ps != null && ps.PayoutType != null ? ps.PayoutType.ToString() : "Not defined");
                    writer.Write(",");
                    writer.Write(ps != null && !String.IsNullOrEmpty(ps.Email)
                        ? payment.PayoutSettings.Email
                        : payment.Seller.Email);
                    writer.Write(",");
                    writer.Write(payment.Payout.FormatMoney(2));
                    writer.WriteLine();
                }

            }
            writer.Flush();
            output.Position = 0;

            var fileName = String.Format("{0}_{1}_paypal_payout.csv", reportYear, reportMonth);

            return File(output, "text/comma-separated-values", fileName);
        }

        public ActionResult ExecutePayout(int year, int month,List<BasePayoutSelectionToken> include)
        {
            string error;
            int execId;
            var result = _payoutServices.ExecuteMonthlyPayout(year, month,include,out execId,out error);
            return Json(new JsonResponseToken
            {
                success = result
                ,error = error
                ,result = execId
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RunBatchAgain(int id)
        {
            string error;
            var result = _payoutServices.RunBatchAgain(id, out error);

            return Json(new JsonResponseToken{success = result,error = error}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RunSinglePaymentAgain(int id)
        {
            string error;
            var result = _payoutServices.RunSinglePaymentAgain(id, out error);

            return Json(new JsonResponseToken { success = result, error = error }, JsonRequestBehavior.AllowGet);
        }
    }
}
