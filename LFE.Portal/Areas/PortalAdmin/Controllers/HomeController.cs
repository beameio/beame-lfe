using LFE.Application.Services;
using LFE.Application.Services.Interfaces;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;

namespace LFE.Portal.Areas.PortalAdmin.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IAuthorAdminServices _authorAdminServices;
        private readonly IAdminDashboardServices _dashboardServices;

        public HomeController()
        {
            _authorAdminServices = DependencyResolver.Current.GetService<IAuthorAdminServices>();
            _dashboardServices = DependencyResolver.Current.GetService<IAdminDashboardServices>();
        }

        public ActionResult Index()
        {

            var token = _dashboardServices.GetAdminDashboardToken();
            return View(token);
        }

        public ActionResult SystemActions()
        {
            return CurrentUserId == 422 ? View() : View("Index");
        }

        //        public async Task ImportUsersDataToProvision()
        //        {
        //            var s = new PortalAdminServices();            
        //            await s.ImportUserDataToProvision(null);
        //        }

        //        public void ImportUserDataToProvision(int? userId)
        //        {
        //            var s = new PortalAdminServices();
        //            s.ImportUserDataToProvision(userId);
        //        }
        //
        //        public void ImportProductsToProvision()
        //        {
        //            var s = new PortalAdminServices();
        //            s.ImportProducts(null, null);
        //        }
        //
        //        public void ImportUserProductsToProvision(int? userId, int? courseId)
        //        {
        //            var s = new PortalAdminServices();
        //            s.ImportProducts(userId, courseId);
        //        }


        public void FindMissingRenditions()
        {
            var s = new PortalAdminServices();
            s.FindMissingRenditions();
        }

        public void UpdateStorePlatform()
        {
            var s = new PortalAdminServices();
            s.UpdateStoreRegistrationSources();
        }

        public void ImportFreePrices()
        {
            var s = new PortalAdminServices();

            s.UpdateFreeCourses();
        }

        public long GetTotalVideosDuration()
        {
            var s = new PortalAdminServices();

            return s.GetTotalVideosDuration();

        }
        public void UpdateVideosState()
        {
            var s = new PortalAdminServices();

            s.UpdateVideosState();

        }

        public string FixBundlePurchases()
        {
            string emails;
            var s = new PortalAdminServices();
            s.FixBundleCourseAccessProblem(out emails);
            return emails;
        }

        public ActionResult GetPlayer(long? id)
        {
            if (id == null) return Content("<h3>Id required</h3>");
            var token = _authorAdminServices.GetVideoToken((long)id);
            return PartialView("_Player", token);
        }
    }

    public class XmlDocumentResult : ContentResult
    {
        public XmlDocument XmlDocument { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (XmlDocument == null)
                return;

            Content = XmlDocument.InnerXml;
            ContentType = "text/xml";
            base.ExecuteResult(context);
        }
    }
}
