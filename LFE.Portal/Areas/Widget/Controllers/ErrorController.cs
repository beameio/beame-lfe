using System.Web.Mvc;
using LFE.Core.Enums;
using LFE.Portal.Areas.Widget.Models;

namespace LFE.Portal.Areas.Widget.Controllers
{
    public class ErrorController : Controller
    {
        
        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

        public ActionResult Error()
        {            
            Response.TrySkipIisCustomErrors = true;
            return View();
        }


        public ActionResult NotFound(WebStoreEnums.StoreStatus? status)
        {
            NotFoundToken token;
            switch (status)
            {
                case WebStoreEnums.StoreStatus.Draft:
                    token = new NotFoundToken
                    {
                        Title = "The Webstore you were looking for is not public anymore.",
                        FirstMessage = "Please Contact the owner of this website for further information.",
                        SecondMessage = "If you are the owner: change the status of the webstore form draft to published",
                    };
                   
                    break;
                case WebStoreEnums.StoreStatus.Deleted:
                    token = new NotFoundToken
                    {
                        Title = "The Webstore you were looking for has deleted.",
                        FirstMessage = "Please Contact the owner of this website for further information.",
                        SecondMessage = "",
                    };
                    
                    break;
                default:
                    token = new NotFoundToken
                    {
                        Title = "The Webstore you were looking for has not found.",
                        FirstMessage = "",
                        SecondMessage = "",
                    };
                    
                    break;
            }

            return View("~/Areas/Widget/Views/Widget/NotFound.cshtml", token);


        }
    }
}
