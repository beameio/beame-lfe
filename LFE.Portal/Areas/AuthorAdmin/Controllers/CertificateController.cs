using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using LFE.Application.Services.Interfaces;
using LFE.DataTokens;

namespace LFE.Portal.Areas.AuthorAdmin.Controllers
{
    public class CertificateController : BaseController
    {
        private readonly ICertificateAdminServices _certificateAdminServices;
        private readonly IStudentSertificateCervices _studentSertificateCervices;
        public CertificateController()
        {
            _certificateAdminServices = DependencyResolver.Current.GetService<ICertificateAdminServices>();
            _studentSertificateCervices = DependencyResolver.Current.GetService<IStudentSertificateCervices>();
        }
        public ActionResult Report()
        {
            return View();
        }

        public ActionResult StudentCertificateEmail(int id)
        {
            var cert = _studentSertificateCervices.GetStudentCertificate(id, CurrentUserId);

            return View(cert);
        }

        public ActionResult _CertificateManageForm(int id)
        {
            var token = _certificateAdminServices.GetCourseCertificate(id);

            return PartialView("Certificate/_CertificateManage", token);
        }

        #region posts
        [HttpPost]
        //[ValidateInput(false)]  
        public ActionResult SaveCertificate(CertificateDTO token)
        {
            string error;
            var saved = _certificateAdminServices.SaveCertificate(token, out error);
            return Json(new JsonResponseToken{success = saved,error = error,result = token.CertificateId}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteCertificate(int certId)
        {
            string error;
            var saved = _certificateAdminServices.DeleteCertificate(certId, out error);
            return Json(new JsonResponseToken { success = saved, error = error}, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region api
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCertReport([DataSourceRequest] DataSourceRequest request)
        {
            var list = _certificateAdminServices.FindStudentCertificatesByAuthor(CurrentUserId).OrderByDescending(x=>x.AddOn).ToArray();
            return Json(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTemplates()
        {
            var list = _certificateAdminServices.TempatesLOV;
            return Json(list,JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region send to student
        [HttpPost]
        public ActionResult ResendStudentCertificate(Guid id)
        {
            string error;
            
            var cert = _studentSertificateCervices.GetStudentCertificate(id);
            
            if(!cert.IsValid) return ErrorResponse(cert.Message);
            
            var body = RenderRazorViewToString("StudentCertificateEmail", cert);
         
            var sent = _studentSertificateCervices.SendStudentCertificate(cert, body, out error);

            return Json(new JsonResponseToken { success = sent, error = error }, JsonRequestBehavior.AllowGet);
        }
        #endregion 
    }
}
