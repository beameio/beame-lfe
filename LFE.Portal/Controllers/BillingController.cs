using System.Linq;
using LFE.Application.Services.Interfaces;
using LFE.Core.Enums;
using LFE.DataTokens;
using System.Web.Mvc;
using LFE.Portal.Areas.UserPortal.Models;

namespace LFE.Portal.Controllers
{
    public class BillingController : BaseController
    {
        private readonly IPaypalManageServies _paypalManageServices;
        private readonly IBillingManageServices _billingManageServices;
        
        public BillingController()
        {
            _paypalManageServices = DependencyResolver.Current.GetService<IPaypalManageServies>();
            _billingManageServices = DependencyResolver.Current.GetService<IBillingManageServices>();
         
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

        public ActionResult ReguestRefundGRP(GRPRequestToken model)
        {
            string error;
            bool refunded;

            if (string.IsNullOrEmpty(model.ReasonText) || model.OrderLineId < 1)
            {
                refunded = false;
                error = "refund reason is mandatory!";
            }
            else
            {
                refunded = _billingManageServices.ProcessRefundGRP(model, out error);
            }

            return Json(new JsonResponseToken
            {
                success = refunded,
                error = error
            }, JsonRequestBehavior.AllowGet);
        }

        public void PaypalPaymentResponse() { }
    }
}
