using System.Web;
using LFE.Application.Services.Interfaces;
using LFE.Core.Extensions;
using LFE.Dto.Mapper.Helper;
using LFE.Portal.Areas.Widget.Models;
using System;
using System.Web.Mvc;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class CheckoutBaseController : BaseController
    {
        public IWidgetServices _widgetServices { get; private set; }
        public CheckoutBaseToken CheckoutBase { get; private set; }

        public CheckoutLayoutToken CheckoutLayoutToken { get; private set; }

        public new HttpContextBase HttpContext
        {
            get
            {
                var context = new HttpContextWrapper(System.Web.HttpContext.Current);
                return context;
            }
        }
        public CheckoutBaseController()
        {
           _widgetServices = DependencyResolver.Current.GetService<IWidgetServices>();

            #region Layout
            CheckoutLayoutToken = new CheckoutLayoutToken { Refferal = (HttpContext.Request.QueryString["ref"]) ?? HttpContext.Request["referral"] };
            ViewData["CheckoutLayoutViewToken"] = CheckoutLayoutToken; 
            #endregion


            #region checkout base
            var id = ((HttpContext.Request.RequestContext.RouteData.Values["id"] as string) ?? HttpContext.Request.QueryString["id"]) ?? HttpContext.Request["PriceToken.PriceLineID"];

            if (id == null)
            {
                CheckoutBase = new CheckoutBaseToken("Required parameter missing", false);
                return;
            }
            int priceLineid;
            if (!Int32.TryParse(id, out priceLineid))
            {
                CheckoutBase = new CheckoutBaseToken("Required parameter invalid", false);
                return;
            }

            string error;
            var infoToken = _widgetServices.GetItemInfoTokenByPriceLineId(priceLineid, out error);

            if (infoToken == null || !infoToken.IsValid)
            {
                CheckoutBase = new CheckoutBaseToken(error, false);
                return;
            }

            CheckoutLayoutToken.ItemInfo = infoToken;
            ViewData["CheckoutLayoutViewToken"] = CheckoutLayoutToken;
 
            var trackingId = ((HttpContext.Request.RequestContext.RouteData.Values["trackingId"] as string) ?? HttpContext.Request.QueryString["trackingId"]) ?? HttpContext.Request["TrackingID"];

            //if (String.IsNullOrEmpty(trackingId))
            //{
            //    //check wix instance
            //    if (!String.IsNullOrEmpty(HttpContext.Request.QueryString["instance"]))
            //    {
            //        var store = _widgetServices.GetWixInstanceStore(HttpContext.Request.QueryString["instance"]);

            //        if (store != null) trackingId = store.TrackingID;
            //    }
            //}

            CheckoutBase = new CheckoutBaseToken
            {
                IsValid      = true
                ,ItemInfo    = infoToken
                ,PriceLineId = priceLineid
                ,TrackingId  = trackingId
                ,WidgetSotre = String.IsNullOrEmpty(trackingId) ? null : _widgetServices.GetWidgetStoreDto(trackingId)
                ,Refferal    = CheckoutLayoutToken.Refferal
                ,ItemPageUrl = this.GenerateItemPageUrl(infoToken.Author.FullName,infoToken.ItemName,infoToken.ItemType,trackingId)
            }; 
            #endregion
        }

        //public CheckoutBaseController(int priceLineId, string referral, string trackingId)
        //{
        //   _widgetServices = DependencyResolver.Current.GetService<IWidgetServices>();

        //    #region Layout
        //    CheckoutLayoutToken = new CheckoutLayoutToken { Refferal = referral};
        //    ViewData["CheckoutLayoutViewToken"] = CheckoutLayoutToken; 
        //    #endregion


        //    #region checkout base
        //    string error;
        //    var infoToken = _widgetServices.GetItemInfoTokenByPriceLineId(priceLineId, out error);

        //    if (infoToken == null || !infoToken.IsValid)
        //    {
        //        CheckoutBase = new CheckoutBaseToken(error, false);
        //        return;
        //    }

        //    CheckoutLayoutToken.ItemInfo = infoToken;
        //    ViewData["CheckoutLayoutViewToken"] = CheckoutLayoutToken;
 
        //    CheckoutBase = new CheckoutBaseToken
        //    {
        //        IsValid      = true
        //        ,ItemInfo    = infoToken
        //        ,PriceLineId = priceLineId
        //        ,TrackingId  = trackingId
        //        ,Refferal    = referral
        //    }; 
        //    #endregion
        //}

    }
}
