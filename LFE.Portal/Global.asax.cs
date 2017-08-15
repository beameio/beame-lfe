using Autofac;
using Autofac.Integration.Mvc;
using LFE.Core.Enums;
using LFE.Core.Extensions;
using LFE.Core.Utils;
using LFE.Infrastructure.NLogger;
using LFE.Portal.App_Start;
using LFE.Portal.Controllers;
using System;
using System.IdentityModel.Claims;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac.Integration.WebApi;
using WebMatrix.WebData;
using Microsoft.AspNet.Mvc.Facebook;
using Newtonsoft.Json;

namespace LFE.Portal
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private readonly NLogLogger _logger = new NLogLogger(); 

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleConfigAuthorPortal.RegisterBundles(BundleTable.Bundles);
            BundleConfigPortalAdmin.RegisterBundles(BundleTable.Bundles);
            BundleConfigUserPortal.RegisterBundles(BundleTable.Bundles);
            BundleConfigWidget.RegisterBundles(BundleTable.Bundles);
            BundleConfigWix.RegisterBundles(BundleTable.Bundles);
            
            AuthConfig.RegisterAuth();

               //JSON Settings
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling           = NullValueHandling.Ignore
                ,ReferenceLoopHandling      = ReferenceLoopHandling.Ignore
                ,Formatting                 = Formatting.Indented
                ,PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = jsonSettings;

            // initializing dependency injection  container
            var builder = new ContainerBuilder();

            // registration all the existing controllers
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            //Repositories registration
            AppExtensions.RegisterRepositories(builder);

            // NLog service registration
            builder.Register(c => new NLogLogger()).AsSelf().InstancePerRequest();
            
            //Services
            ServicesConfig.Register(builder);
          
            // build the dependencies
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));


            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);


            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "Email", autoCreateTables: false);
            }

            FacebookConfig.Register(GlobalFacebookConfiguration.Configuration);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e){}


        void Application_PreSendRequestHeaders(Object sender, EventArgs e)
        {
            //HttpContext.Current.Response.Headers.Remove("X-Frame-Options");

            var browser = Request.Browser;
            if (browser.Browser.ToUpper() != "IE" && browser.Browser.ToUpper() != "INTERNETEXPLORER") return;
          
            //set cookie policy for iframe authorization cookie
            Response.Headers.Set("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
        }

        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app == null) return;

            var acceptEncoding = app.Request.Headers["Accept-Encoding"];
            var prevUncompressedStream = app.Response.Filter;

            if (app.Context.CurrentHandler == null)
                return;

            if (!(app.Context.CurrentHandler is System.Web.UI.Page ||
                  app.Context.CurrentHandler.GetType().Name == "SyncSessionlessHandler") ||
                app.Request["HTTP_X_MICROSOFTAJAX"] != null)
                return;

            if (string.IsNullOrEmpty(acceptEncoding))
                return;

            acceptEncoding = acceptEncoding.ToLower();

            if (acceptEncoding.Contains("deflate") || acceptEncoding == "*")
            {
                // deflate
                app.Response.Filter = new DeflateStream(prevUncompressedStream,
                    CompressionMode.Compress);
                app.Response.AppendHeader("Content-Encoding", "deflate");
            }
            else if (acceptEncoding.Contains("gzip"))
            {
                // gzip
                app.Response.Filter = new GZipStream(prevUncompressedStream,
                    CompressionMode.Compress);
                app.Response.AppendHeader("Content-Encoding", "gzip");
            }
        }

        protected void Application_BeginRequest()
        {

            var appDown = Boolean.Parse(Utils.GetKeyValue("appDown"));

            if (appDown)
            {
                if (Request.AppRelativeCurrentExecutionFilePath != "~/SystemInfo.html") {Response.Redirect("~/SystemInfo.html");}

                return;
                
            }
            
          //  var useSSL = bool.Parse(Utils.GetKeyValue("useSSL"));

            //            var isCheckout = false;
            //
            //            HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
            //            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            //            var routeData = urlHelper.RouteCollection.GetRouteData(currentContext);
            //            if (routeData != null)
            //            {
            //                var action = routeData.Values["action"] as string;
            //                var controller = routeData.Values["controller"] as string;
            //
            //                if (controller != null && controller.ToLower() == "checkout" && action != null && action.ToLower() == "index") isCheckout = true;
            //            }  

  
 //           if (Request.IsLocal || !useSSL || Request.IsSecureConnection) return;
//
//            _logger.Debug("is secure " + Request.IsSecureConnection);
//            Response.RedirectPermanent(Request.Url.ToString().Replace("http:", "https:"));

//            switch (Request.Url.Scheme)
//            {
//                case "https":
//                    Response.AddHeader("Strict-Transport-Security", "max-age=300");
//                    return;
//                case "http":
//                    var path = Request.Url.ToString().Replace("http:", "https:");//"https://" + Request.Url.Host + Request.Url.PathAndQuery;
//                    Response.Status = "301 Moved Permanently";
//                    Response.AddHeader("Location", path);
////                    Response.RedirectPermanent(path);
//                    return;
//            }

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();

            _logger.Error("application error::" +Utils.FormatError(ex),ex,CommonEnums.LoggerObjectTypes.Application);
            try
            {
                _logger.Fatal("application error::" + HttpContext.Current.Request.RawUrl + "::" + HttpContext.Current.Request.QueryString, ex);
            }
            catch (Exception x)
            {
                _logger.Error("application error::",x,CommonEnums.LoggerObjectTypes.Application);    
               
            }

            var exception = ex as HttpException;
            if (exception != null && exception.GetHttpCode() == 404)
            {
                try
                {
                    Response.StatusCode = 404;
                    var ctx = HttpContext.Current;

                    //var error = new KeyValuePair<string, object>("ErrorMessage", ctx.Server.GetLastError().ToString());

                    ctx.Response.Clear();

                    var rc = ((MvcHandler)ctx.CurrentHandler).RequestContext;

                    var factory = ControllerBuilder.Current.GetControllerFactory();

                    var controller = (HomeController)factory.CreateController(rc, "home");
                    var cc = new ControllerContext(rc, controller);

                    controller.ControllerContext = cc;

                    var s = controller.RenderRazorViewToString("Error404", null);

                    Response.Write(s);
                    ctx.Response.End();
                }
                catch (Exception) { }
            }
            else
            {
                var ee = Utils.FormatError(ex).OptimizedUrl();
                Response.Redirect("/Home/Error/" );
            }
        }
    }
}